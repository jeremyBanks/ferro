using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Ferro {
    // A client (not server) for the mainline BitTorrent DHT.
    // Only supporting BEP 5 at this point, none of the other extensions.
    public class DHTClient
    {
        readonly byte[] nodeId;
        readonly IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 6881);
<<<<<<< Updated upstream
=======
        readonly IPEndPoint knownTestNodeEndPoint = new IPEndPoint(IPAddress.Parse("192.168.99.100"), 9527);
>>>>>>> Stashed changes
        private UDPSocket socket;

        public DHTClient() {
            nodeId = new byte[20].FillRandom();

            socket = new UDPSocket(localEndPoint);
        }

        // Pings the DHT node at the given endpoint, or throws an error.
        public async Task Ping(IPEndPoint ep) {
            var token = new byte[4].FillRandom();
            sendPing(ep, token);

            Console.WriteLine("Waiting for packet...");

            while (true) {
                var response = await socket.ReceiveAsync();
                var value = (Dictionary<byte[], object>) Bencoding.Decode(response.Data);
                if (!response.Source.Equals(ep)) {
                    Console.WriteLine($"Got unexpected packet from a different source, {response.Source}: {Bencoding.ToHuman(response.Data)}");
                    continue;
                // } else if (!value["t".ToASCII()].Equals(token)) {
                //     Console.WriteLine($"Got patcket with unexpected token: {Bencoding.ToHuman(response.Data)}");
                //     continue;
                } else {
                    Console.WriteLine($"Got response packet: {Bencoding.ToHuman(response.Data)}");
                    break;
                }
            }
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
                    ["id".ToASCII()] = nodeId // only ping argument is own id
                },
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