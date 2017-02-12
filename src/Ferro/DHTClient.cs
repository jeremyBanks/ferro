using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ferro {
    public class DHTClient
    {
        static byte[] randomId() {
            using(RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] id = new byte[20];
                rng.GetBytes(id);
                return id;
            }
        }

        const Int32 DEFAULT_PORT = 6881;
        const Int32 DOCKTORRENT_DHT_PORT = 9527;

        // Pings any DHT peer, to confirm we have some connection.
        public async Task Ping() {
            var myId = "[An Example Peer ID]".ToASCII();

            var localEndPoint = new IPEndPoint(IPAddress.Any, DEFAULT_PORT);
            var peerDhtEndpoint = new IPEndPoint(IPAddress.Loopback, DOCKTORRENT_DHT_PORT);

            var socket = new UDPSocket(localEndPoint);

            var ping = Bencoding.Encode(new Dictionary<byte[], object>{
                ["t".ToASCII()] = "t2".ToASCII(), // unique identifier for this request/response
                ["y".ToASCII()] = "q".ToASCII(), // type is query
                ["q".ToASCII()] = "ping".ToASCII(), // query name is ping
                ["a".ToASCII()] = new Dictionary<byte[], object>{ // query arguments is a dict
                    ["id".ToASCII()] = myId // only ping argument is own id
                },
            });
            Console.WriteLine($"Sending ping: {Bencoding.ToHuman(ping)}");

            socket.SendTo(ping, peerDhtEndpoint);

            Console.WriteLine("Waiting for packet...");

            var response = await socket.Receive();

            Console.WriteLine($"Got packet from {response.Source}: {Bencoding.ToHuman(response.Data)}");
        }
    }
}