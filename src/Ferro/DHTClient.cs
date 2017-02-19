using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ferro {
    // Identifier for a DHT query that can be used as a dictionary key.
    public class DHTQueryKey {
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

    public class DHTMessage {
        public Dictionary<byte[], dynamic> Data;
    }

    // A client (not server) for the mainline BitTorrent DHT.
    public class DHTClient
    {
        readonly byte[] NodeId;
        readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 6881);
        private UDPSocket socket;

        readonly Task Listening;
        
        private HashSet<IPEndPoint> knownGoodNodes;

        // This is terrible.
        byte nextToken = 0;

        private Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>> pendingQueries;

        public DHTClient() {
            pendingQueries = new Dictionary<DHTQueryKey, TaskCompletionSource<DHTMessage>>();

            NodeId = new byte[20].FillRandom();

            socket = new UDPSocket(LocalEndPoint);

            knownGoodNodes = new HashSet<IPEndPoint>();

            Listening = Task.Run(async () => {
                while (true) {
                    await Task.Delay(10);

                    try {
                        var response = await socket.ReceiveAsync();

                        var value = (Dictionary<byte[], object>) Bencoding.Decode(response.Data);

                        var type = ((byte[]) value["y".ToASCII()]).FromASCII();

                        switch (type) {
                            case "r": {
                                Console.WriteLine($"Got response message from {response.Source}.");

                                var key = new DHTQueryKey {
                                    Token = (byte[]) value["t".ToASCII()],
                                    EP = response.Source
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
                                Console.WriteLine($"Got error mesage from {response.Source}.");

                                var key = new DHTQueryKey {
                                    Token = (byte[]) value["t".ToASCII()],
                                    EP = response.Source
                                };

                                Console.WriteLine("For query key: " + key);

                                if (pendingQueries.ContainsKey(key)) {
                                    var responseSource = pendingQueries[key];
                                    pendingQueries.Remove(key);

                                    var errors = (List<object>) value["e".ToASCII()];
                                    var code = (Int64) errors[0];
                                    var message = ((byte[]) errors[1]).FromASCII();
                                    
                                    var exception = new Exception($"{code} {message}");
                                    Console.WriteLine("Rejecting pending task.");
                                    responseSource.SetException(new Exception[] { exception });
                                } else {
                                    Console.WriteLine("But I wasn't expecting that!");
                                }

                                break;
                            }

                            case "q": {
                                Console.WriteLine($"Ignored query mesage from {response.Source}:\n{Bencoding.ToHuman(response.Data)}");
                                // do nothing because we're read-only
                                break;
                            }

                            default: {
                                Console.WriteLine($"Got unknown mesage from {response.Source}:\n{Bencoding.ToHuman(response.Data)}");
                                // maybe we could send an error?
                                break;
                            }
                        }
                    } catch (Exception ex) {
                        Console.WriteLine("Exception! " + ex);  
                    }
                }
            });
        }

        // Pings the DHT node at the given endpoint and returns its id, or throws an error.
        // If the node is pinged successfully, it adds it to routing table.
        public async Task<byte[]> Ping(IPEndPoint ep) {
            var token = new byte[]{nextToken++};

            var result = new TaskCompletionSource<DHTMessage>();
            var key = new DHTQueryKey { Token = token, EP = ep };
            pendingQueries[key] = result;

            Console.WriteLine($"Sending ping {key}...");
            sendPing(ep, token);

            var results = await result.Task;

            var nodeId = (byte[]) results.Data["r".ToASCII()]["id".ToASCII()];

            knownGoodNodes.Add(ep);

            return nodeId;
        }

        public async Task<List<object>> GetPeers(byte[] infohash) {
            foreach (var node in knownGoodNodes) {
                var token = new byte[]{nextToken++};

                var result = new TaskCompletionSource<DHTMessage>();
                var key = new DHTQueryKey { Token = token, EP = node };
                pendingQueries[key] = result;

                Console.WriteLine($"Sending get_peers {key}...");
                sendGetPeers(node, token, infohash);

                var results = await result.Task;

                var nodesData = (byte[]) results.Data["r".ToASCII()]["nodes".ToASCII()];

                // We need to parse this data instead of just returing a blob
                return new List<object> { nodesData };
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
                    ["id".ToASCII()] = NodeId // only ping argument is own id
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
                    ["id".ToASCII()] = NodeId, // own id
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
    }
}