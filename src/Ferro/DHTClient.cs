using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ferro {
    // Identifier for a DHT query that can be used as a dictionary key.
    class DHTQueryKey {
        public IPEndPoint EP; // the ip and port of the dht node
        public byte[] Token; // the unique opaque token we sent with the query

        public override int GetHashCode() {
            // crappy but maybe adequate sum as hash. should be cached.
            return
                EP.Address.GetAddressBytes().Sum(x => x) +
                EP.Port +
                Token.Sum(x => x);
        }

        public override bool Equals(object obj) {
            if (!(obj != null && obj is DHTQueryKey)) {
                return false;
            }
            var other = obj as DHTQueryKey;
            return
                EP.Address.Equals(other.EP.Address) && 
                EP.Port.Equals(other.EP.Port) &&
                Token.SequenceEqual(other.Token);
        }

        public override string ToString() {
            return $"[{EP}/{String.Join(",", Token.Select(x => x.ToString()))}]";
        }
    }

    class DHTMessage {
        public Dictionary<byte[], dynamic> Data;
    }

    // A client (not server) for the mainline BitTorrent DHT.
    public class DHTClient : IDisposable
    {
        private byte[] nodeId;
        private IPEndPoint localEP;
        private UDPSocket socket;

        // Flag used to cancel main task.
        private bool canceled = false;

        // A task that completes once we have a healthy DHT connection.
        readonly Task Connecting;
        private TaskCompletionSource<bool> connectingSource;

        // A task that completes once we the main task has bound a port and initialized.
        readonly Task Started;
        private TaskCompletionSource<bool> startedSource;

        // A task for the DHT's background activites, that should keep runing for as long as it's active.
        readonly Task Running;
        
        private HashSet<IPEndPoint> knownGoodNodes;

        // This is terrible.
        byte nextToken = 0;

        private Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>> pendingQueries;

        public DHTClient() {
            connectingSource = new TaskCompletionSource<bool>();
            Connecting = connectingSource.Task;

            nodeId = new byte[20].FillRandom();
            knownGoodNodes = new HashSet<IPEndPoint>();
            pendingQueries = new Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>>();
            localEP = new IPEndPoint(IPAddress.Any, 6881);

            startedSource = new TaskCompletionSource<bool>();
            Started = startedSource.Task;

            Running = Task.Run(new Action(main));

            // Mark other tasks as failed if the main task fails.
            Task.Run(async () => {
                try {
                    await Running;
                } catch (Exception ex) {
                    startedSource.TrySetException(ex);
                    connectingSource.TrySetException(ex);
                    throw ex;
                }
            });
        }

        private async void main() {
            using (socket = new UDPSocket(localEP)) {
                startedSource.SetResult(true);

                while (!canceled) {
                    try {
                        handleMessage(await socket.ReceiveAsync());
                    } catch (Exception ex) {
                        Console.WriteLine("Exception! " + ex);  
                    }
                }
            }
        }

        private void handleMessage(UDPSocket.ReceivedPacket message) {
            var value = (Dictionary<byte[], object>) Bencoding.Decode(message.Data);

            var type = ((byte[]) value["y".ToASCII()]).FromASCII();

            switch (type) {
                case "r": {
                    Console.WriteLine($"Got response message from {message.Source}.");

                    var key = new DHTQueryKey {
                        Token = (byte[]) value["t".ToASCII()],
                        EP = message.Source
                    };

                    Console.WriteLine("For query key: " + key);

                    if (pendingQueries.ContainsKey(key)) {
                        var responseSource = pendingQueries[key];
                        pendingQueries.Remove(key);

                        responseSource.SetResult(new DHTMessage { Data = value });
                        Console.WriteLine("Resolved pending task.");
                    } else {
                        Console.WriteLine("But I wasn't expecting that!");
                    }

                    break;
                }

                case "e": {
                    Console.WriteLine($"Got error mesage from {message.Source}.");

                    var key = new DHTQueryKey {
                        Token = (byte[]) value["t".ToASCII()],
                        EP = message.Source
                    };

                    Console.WriteLine("For query key: " + key);

                    if (pendingQueries.ContainsKey(key)) {
                        var responseSource = pendingQueries[key];
                        pendingQueries.Remove(key);

                        var errors = (List<object>) value["e".ToASCII()];
                        var code = (Int64) errors[0];
                        var errorMessage = ((byte[]) errors[1]).FromASCII();
                        
                        var exception = new Exception($"{code} {errorMessage}");
                        Console.WriteLine("Rejecting pending task.");
                        responseSource.SetException(new Exception[] { exception });
                    } else {
                        Console.WriteLine("But I wasn't expecting that!");
                    }

                    break;
                }

                case "q": {
                    Console.WriteLine($"Ignored query mesage from {message.Source}:\n{Bencoding.ToHuman(message.Data)}");
                    // do nothing because we're read-only
                    break;
                }

                default: {
                    Console.WriteLine($"Got unknown mesage from {message.Source}:\n{Bencoding.ToHuman(message.Data)}");
                    // maybe we could send an error?
                    break;
                }
            }
        }

        // Pings the DHT node at the given endpoint and returns its id, or throws an error.
        // If the node is pinged successfully, it adds it to routing table.
        public async Task<byte[]> Ping(IPEndPoint ep) {
            var token = new byte[]{nextToken++};

            var result = new TaskCompletionSource<DHTMessage>();
            var key = new DHTQueryKey { Token = token, EP = ep };
            pendingQueries.Add(key, result);

            Console.WriteLine($"Sending ping {key}...");
            sendPing(ep, token);

            var results = await result.Task;

            var nodeId = (byte[]) results.Data["r".ToASCII()]["id".ToASCII()];

            knownGoodNodes.Add(ep);

            return nodeId;
        }

        public async Task<List<IPEndPoint>> GetPeers(byte[] infohash) {
            // TODO: This is not complete.
            // This should, up to like 10 times or something, ask the closest
            // node to the target infohash if it knows any nodes closer to that,
            // until it gets the result (a list of peers, not nodes) or gives up
            // and returns an empty list or throws an exception.
            foreach (var node in knownGoodNodes) {
                var token = new byte[]{nextToken++};

                var result = new TaskCompletionSource<DHTMessage>();
                var key = new DHTQueryKey { Token = token, EP = node };
                pendingQueries.Add(key, result);

                Console.WriteLine($"Sending get_peers {key}...");
                sendGetPeers(node, token, infohash);

                var results = await result.Task;

                var nodesData = (byte[]) results.Data["r".ToASCII()]["nodes".ToASCII()];

                Console.WriteLine("Got: " + Bencoding.ToHuman(Bencoding.Encode(results.Data)));

                // We need to parse `nodesData` as NODES and ping them.

                return new List<IPEndPoint> {};
            }

            throw new Exception("had no good nodes to query");
        }

        void sendPing(IPEndPoint destination, byte[] token) {
            var ping = Bencoding.Encode(new Dictionary<byte[], object>{
                ["t".ToASCII()] = token,
                    // unique identifier for this request/response
                ["y".ToASCII()] = "q".ToASCII(), // type is query
                ["q".ToASCII()] = "ping".ToASCII(), // query name is ping
                ["a".ToASCII()] = new Dictionary<byte[], object>{ // query arguments is a dict
                    ["id".ToASCII()] = nodeId // only ping argument is own id
                },
                ["ro".ToASCII()] = (Int64) 1 // indicates we're only a client, not an equal serving node
            });
            Console.WriteLine($"Sending ping to {destination}.");

            socket.SendTo(ping, destination);
        }

        void sendGetPeers(IPEndPoint destination, byte[] token, byte[] infohash) {
            var ping = Bencoding.Encode(new Dictionary<byte[], object>{
                ["t".ToASCII()] = token,
                    // unique identifier for this request/response
                ["y".ToASCII()] = "q".ToASCII(), // type is query
                ["q".ToASCII()] = "get_peers".ToASCII(), // query name is get_peers
                ["a".ToASCII()] = new Dictionary<byte[], object>{ // query arguments is a dict
                    ["id".ToASCII()] = nodeId, // own id
                    ["info_hash".ToASCII()] = infohash // own id
                },
                ["ro".ToASCII()] = (Int64) 1 // indicates we're only a client, not an equal serving node
            });
            Console.WriteLine($"Sending get_peers to {destination}.");

            socket.SendTo(ping, destination);
        }

        void sendFindNode() {
            throw new Exception("NOT IMPLEMENTED");
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                canceled = true;
                socket?.Dispose();

                disposedValue = true;
            }
        }

        ~DHTClient() {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}