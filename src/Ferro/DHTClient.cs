using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ferro {
    // A client (not server) for the mainline BitTorrent DHT.
    // Only supporting BEP 5 at this point, none of the other extensions.
    public class DHTClient
    {
        readonly byte[] nodeId;
        readonly IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 6881);
        readonly IPEndPoint knownTestNodeEndPoint = new IPEndPoint(IPAddress.Loopback, 9527);
        private UDPSocket socket;

        public DHTClient() {
            nodeId = "[An Example Node ID]".ToASCII();

            socket = new UDPSocket(localEndPoint);
        }

        // Pings any DHT node, to confirm we have some connection.
        public async Task Ping() {
            sendPing(knownTestNodeEndPoint);

            Console.WriteLine("Waiting for packet...");

            var response = await socket.ReceiveAsync();

            Console.WriteLine($"Got packet from {response.Source}: {Bencoding.ToHuman(response.Data)}");

            Console.WriteLine("I hop that's the response we were looking for! Should check...");
        }

        public async Task<List<object>> GetPeers(byte[] infohash) {
            if ("".Length == 0) {
                throw new Exception("NOT IMPLEMENTED");
            }
            await Ping();
            return (List<object>) null;
        }

        void sendPing(IPEndPoint destination) {
            var ping = Bencoding.Encode(new Dictionary<byte[], object>{
                ["t".ToASCII()] = "t2".ToASCII(), // unique identifier for this request/response
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