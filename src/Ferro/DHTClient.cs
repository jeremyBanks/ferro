using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ferro {
    // A client (not server) for the mainline BitTorrent DHT.
    public class DHTClient
    {
        readonly byte[] NodeId;
        readonly IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Any, 6881);
        private UDPSocket socket;

        readonly Task Listening;

        // This is terrible.
        byte nextToken = 0;

        private Dictionary<
            Tuple<IPEndPoint, byte>,
            TaskCompletionSource<Dictionary<byte[], dynamic>>
        > pendingQueries;

        public DHTClient() {
            pendingQueries = new Dictionary<
                Tuple<IPEndPoint, byte>,
                TaskCompletionSource<Dictionary<byte[], dynamic>>
            >();

            NodeId = new byte[20].FillRandom();

            socket = new UDPSocket(LocalEndPoint);

            Listening = Task.Run(async () => {
                // TODO: don't crash when you get invalid data
                while (true) {
                    var response = await socket.ReceiveAsync();

                    var value = (Dictionary<byte[], object>) Bencoding.Decode(response.Data);

                    var type = ((byte[]) value["y".ToASCII()]).FromASCII();

                    switch (type) {
                        case "r":
                            Console.WriteLine($"Got response mesage from {response.Source}:\n{Bencoding.ToHuman(response.Data)}");

                            break;

                        case "e":
                            Console.WriteLine($"Got error mesage from {response.Source}:\n{Bencoding.ToHuman(response.Data)}");
                            break;

                        case "q": 
                            // do nothing because we're read-only
                            break;

                        default:
                            // maybe we could send an error?
                            break;
                    }
                }
            });
        }

        // Pings the DHT node at the given endpoint, or throws an error.
        public async Task Ping(IPEndPoint ep) {
            var token = nextToken++;

            Console.WriteLine("Waiting for packet...");

            var result = new TaskCompletionSource<Dictionary<byte[], dynamic>>();
            // TODO insert it
            sendPing(ep, new byte[]{token});

            var results = await result.Task;
        }

        public async Task<List<object>> GetPeers(byte[] infohash) {
            if ("".Length == 0) {
                throw new Exception("NOT IMPLEMENTED");
            }
            await Ping(null);
            return (List<object>) null;
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
            Console.WriteLine($"Sending ping to {destination}: {Bencoding.ToHuman(ping)}");

            socket.SendTo(ping, destination);
        }

        void sendGetPeers() {
            throw new Exception("NOT IMPLEMENTED");
        }

        void sendFindNode() {
            throw new Exception("NOT IMPLEMENTED");
        }
    }
}