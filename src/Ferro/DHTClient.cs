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

    class DHTNode {
        public IPEndPoint EP;
        public byte[] Id;
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
        readonly Task Connected;
        private TaskCompletionSource<bool> connectedSource;

        // A task that completes once we the main task has bound a port and initialized.
        readonly Task Started;
        private TaskCompletionSource<bool> startedSource;

        readonly Task MessageEventLoop;

        readonly Task ConnectionHealthLoop;
        
        // Endpoints that might be DHT nodes, but we haven't pinged yet.
        private HashSet<IPEndPoint> possibleNodes;

        // Nodes we've successfully pinged
        private Dictionary<IPEndPoint, DHTNode> knownNodes;

        byte[] lastToken = new byte[0];

        private Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>> pendingQueries;

        public DHTClient() {
            connectedSource = new TaskCompletionSource<bool>();
            Connected = connectedSource.Task;

            nodeId = new byte[20].FillRandom();
            possibleNodes = new HashSet<IPEndPoint>();
            knownNodes = new Dictionary<IPEndPoint, DHTNode>();
            pendingQueries = new Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>>();
            localEP = new IPEndPoint(IPAddress.Any, 6881);

            startedSource = new TaskCompletionSource<bool>();
            Started = startedSource.Task;

            MessageEventLoop = Task.Run(new Action(messageEventLoop));
            ConnectionHealthLoop = Task.Run(new Action(connectionHealthLoop));


            Task.Run(async () => {
                try {
                    await MessageEventLoop;
                    await ConnectionHealthLoop;
                } catch (Exception ex) {
                    // Mark other tasks as failed if the a main task fails.
                    canceled = true;
                    startedSource.TrySetException(ex);
                    connectedSource.TrySetException(ex);
                    throw ex;
                }
            });
        }

        public void AddNode(IPEndPoint ep) {
            possibleNodes.Add(ep);
        }

        private async void connectionHealthLoop() {
            while (!canceled) {
                Console.WriteLine($"DHT: {knownNodes.Count} good nodes, {possibleNodes.Count} potential nodes, {pendingQueries.Count} outstanding queries");

                if (possibleNodes.Count == 0 && knownNodes.Count == 0) {
                    await Task.Delay(2500);
                    continue;
                } 

                if (possibleNodes.Count > 0) {
                    var ep = possibleNodes.Pop();
                    Console.WriteLine($"Pinging possible node {ep} to check validity.");
                    Ping(ep).DoNotAwait();

                    await Task.Delay(1500);
                    continue;
                }

                if (knownNodes.Count > 0 && knownNodes.Count < 16) {
                    var id = new byte[20].FillRandom();
                    Console.WriteLine(
                        $"Searching for peers with random {id.ToHuman()} to improve DHT connection.");
                    GetPeers(id).DoNotAwait();

                    await Task.Delay(1000);
                    continue;
                }

                connectedSource.TrySetResult(true);

                Console.WriteLine("Our connection is good!");
                await Task.Delay(5000);
            }
        }

        private async void messageEventLoop() {
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
            var value = Bencoding.DecodeDict(message.Data);

            var type = value.GetString("y");

            switch (type) {
                case "r": {
                    var key = new DHTQueryKey {
                        Token = value.GetBytes("t"),
                        EP = message.Source
                    };

                    if (pendingQueries.ContainsKey(key)) {
                        var responseSource = pendingQueries[key];
                        pendingQueries.Remove(key);

                        responseSource.TrySetResult(new DHTMessage { Data = value });
                    } else {
                        Console.WriteLine("Got unexpected response message.");
                    }

                    break;
                }

                case "e": {
                    Console.WriteLine($"Got error mesage from {message.Source}.");

                    var key = new DHTQueryKey {
                        Token = value.GetBytes("t"),
                        EP = message.Source
                    };

                    Console.WriteLine("For query key: " + key);

                    if (pendingQueries.ContainsKey(key)) {
                        var responseSource = pendingQueries[key];
                        pendingQueries.Remove(key);

                        var errors = value.GetList("e");
                        var code = (Int64) errors[0];
                        var errorMessage = ((byte[]) errors[1]).FromASCII();
                        
                        var exception = new Exception($"{code} {errorMessage}");
                        Console.WriteLine("Rejecting pending task.");
                        responseSource.TrySetException(new Exception[] { exception });
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

        // Increments the token, as a big-endian unsigned integer, adding new bytes if it overflows.
        public static byte[] IncrementToken(byte[] token) {
            for (var i = token.Length - 1; i >= 0; i--) {
                if (token[i] < 0xFF) {
                    var newToken = token.Slice();
                    newToken[i]++;
                    for (var j = i + 1; j < token.Length; j++) {
                        newToken[j] = 0;
                    }
                    return newToken;
                }
            }
            // If we get here, everything is currently 0xFF, so we need to add grow into another byte.
            var expandedToken = new byte[token.Length + 1];
            expandedToken[0] = 1;
            return expandedToken;
        }

        // Pings the DHT node at the given endpoint and returns its id, or throws an error.
        // If the node is pinged successfully, it adds it to routing table.
        public async Task<byte[]> Ping(IPEndPoint ep) {
            // We won't ping it again if this fails.
            possibleNodes.Remove(ep);

            var token = (lastToken = IncrementToken(lastToken));

            var result = new TaskCompletionSource<DHTMessage>();
            var key = new DHTQueryKey { Token = token, EP = ep };
            pendingQueries.Add(key, result);
            Task.Run(async () => {
                await Task.Delay(5000);
                result.TrySetException(new Exception("request timed out"));
            }).DoNotAwait();

            Console.WriteLine($"Sending ping {key}...");
            sendPing(ep, token);

            var results = await result.Task;

            var nodeId = results.Data.GetDict("r").GetBytes("id");

            knownNodes[ep] = new DHTNode() {
                EP = ep,
                Id = nodeId
            };

            return nodeId;
        }

        private List<DHTNode> getKnownNodesByCloseness(byte[] target) {
            var nodes = knownNodes.Values.ToList();
            var comparer = new XorDistanceComparer(target);
            nodes.Sort((a, b) => comparer.Compare(a.Id, b.Id));
            return nodes;
        }

        public async Task<List<IPEndPoint>> GetPeers(byte[] infohash) {
            var visitedNodeAddresses = new HashSet<Int32>();

            var queries = 0;

            while (true) {
                var nodes = getKnownNodesByCloseness(infohash);

                DHTNode closestNode = null;

                foreach (var node in nodes) {
                    if (visitedNodeAddresses.Contains(node.EP.Address.GetAddressBytes().Decode32BitInteger())) {
                        continue;
                    } else {
                        visitedNodeAddresses.Add(node.EP.Address.GetAddressBytes().Decode32BitInteger());
                        closestNode = node;
                        break;
                    }
                }

                if (closestNode == null) {
                    Console.WriteLine($"Need new nodes to continue querying {infohash.ToHuman()} in the DHT (already visited {visitedNodeAddresses.Count}).");
                    await Task.Delay(5000);
                    continue;
                }

                if (queries++ > 32) {
                    throw new Exception("query count sanity limit exceeded");
                }

                var token = (lastToken = IncrementToken(lastToken));

                var result = new TaskCompletionSource<DHTMessage>();
                var key = new DHTQueryKey { Token = token, EP = closestNode.EP };
                pendingQueries.Add(key, result);
                Task.Run(async () => {
                    await Task.Delay(5000);
                    result.TrySetException(new Exception("get_peers timed out"));
                }).DoNotAwait();

                Console.WriteLine($"Sending get_peers {key}...");
                sendGetPeers(closestNode.EP, token, infohash);

                DHTMessage results = null;
                try {
                    results = await result.Task;
                } catch (Exception ex) {
                    Console.WriteLine("Query failed: " + ex);
                    continue;
                }

                var response = results.Data.GetDict("r");

                if (response.ContainsKey("nodes")) {
                    var compactNodes = response.GetBytes("nodes");
                    
                    Console.WriteLine("Got closer nodes.");

                    var nodeCount = compactNodes.Length % 26;
                    for (var i = 0; i < compactNodes.Length; i += 26) {
                        // we disregard the node ID here, since we'll ping all of them and get it then
                        var ep = new IPEndPoint(
                            new IPAddress(compactNodes.Slice(i + 20, i + 24)),
                            compactNodes.Slice(i + 24, i + 26).Decode16BitInteger());
                        Ping(ep).DoNotAwait();
                    }

                    await Task.Delay(2000);
                    continue;
                } else {
                    var compactPeers = response.GetList("values");

                    Console.WriteLine("Got peers!?");

                    var peers = new List<IPEndPoint> {};

                    foreach (var compactPeer_ in compactPeers) {
                        var compactPeer = (byte[]) compactPeer_;
                        peers.Add(new IPEndPoint(
                            new IPAddress(compactPeer.Slice(0, 4)),
                            compactPeer.Slice(4, 6).Decode16BitInteger()));
                    }

                    return peers;
                }
            }
            
            throw new Exception("this code path should not be possible");
        }

        void sendPing(IPEndPoint destination, byte[] token) {
            var dict = Bencoding.Dict();
            dict.Set("t", token);
            dict.Set("y", "q");
            dict.Set("q", "ping");
            dict.Set("ro", 1);
            var args = Bencoding.Dict();
            args.Set("id", nodeId);
            dict.Set("a", args);

            var encoded = Bencoding.Encode(dict);
            socket.SendTo(encoded, destination);
        }

        void sendGetPeers(IPEndPoint destination, byte[] token, byte[] infohash) {
            var dict = Bencoding.Dict();
            dict.Set("t", token);
            dict.Set("y", "q");
            dict.Set("q", "get_peers");
            dict.Set("ro", 1);
            var args = Bencoding.Dict();
            args.Set("id", nodeId);
            args.Set("info_hash", infohash);
            dict.Set("a", args);

            var encoded = Bencoding.Encode(dict);

            socket.SendTo(encoded, destination);
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

    // Defines an ordering of byte arrays based on their values after being xored with
    // a given byte array. (In the DHT, this can be used to sort nodes based on how
    // close they are to a target node/address).
    public class XorDistanceComparer : IComparer<byte[]> {
        // The target against which each value will be xored before ordering.
        byte[] target;
        public XorDistanceComparer(byte[] target) {
            this.target = target;
        }
        public int Compare(byte[] x, byte[] y) {
            if (target.Length != x.Length || target.Length != y.Length) {
                throw new Exception("target and values must all have same length");
            }
            for (var i = 0; i < target.Length; i++) {
                var xItem = x[i] ^ target[i];
                var yItem = y[i] ^ target[i];
                if (xItem > yItem) {
                    return +1; // x contains a greater item first
                } else if (yItem > xItem) {
                    return -1; // y contains a greater item first
                }
            }
            return 0;
        }
    }
}