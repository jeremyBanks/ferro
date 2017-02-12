using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Ferro
{

    public class Examples
    {
        /**
         * Returns 20 random bytes, as for a BitTorrent peer id or infohash.
         */
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

        public static async Task MainAsync() {
            var myId = "example peer id 2234".ToASCII();


            var socket = new Socket(
                AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var peerDhtEndpoint = new IPEndPoint(IPAddress.Loopback, DOCKTORRENT_DHT_PORT);

            socket.Bind(new IPEndPoint(IPAddress.Loopback, DEFAULT_PORT));

            Console.WriteLine($"Sending data to {peerDhtEndpoint}.");

            var ping = Bencoding.Encode(new Dictionary<byte[], object>{
                ["t".ToASCII()] = "t2".ToASCII(), // unique identifier for this request/response
                ["y".ToASCII()] = "q".ToASCII(), // type is query
                ["q".ToASCII()] = "ping".ToASCII(), // query name is ping
                ["a".ToASCII()] = new Dictionary<byte[], object>{ // query arguments is a dict
                    ["id".ToASCII()] = myId // only ping argument is own id
                },
            });
            Console.WriteLine(ping.FromASCII());

            // d1:ad2:id20:example peer id 2234e1:q4:ping1:t2:t21:y1:qe
            // d1:ad2:id20:abcdefghij0123456789e1:q4:ping1:t2:aa1:y1:qe

            socket.SendTo(ping, peerDhtEndpoint);

            Console.WriteLine("waiting for response");

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            byte[] buffer = new byte[1024];
            args.SetBuffer(buffer, 0, buffer.Length);
            args.RemoteEndPoint = peerDhtEndpoint;
            args.Completed += (sender, results) => {
                Console.WriteLine($"got response: {results.Buffer.FromASCII()}");
                var decoded = Bencoding.Decode(results.Buffer);
                Console.WriteLine($"got response: {decoded}");
                stopWaitHandle.Reset();
            };

            var pending = socket.ReceiveFromAsync(args);
            if (!pending) {
                Console.WriteLine($"got response: {args.Buffer.FromASCII()}");
                var decoded = Bencoding.Decode(args.Buffer);
                Console.WriteLine($"got response: {decoded}");
                stopWaitHandle.Reset();
            } else {
                Console.WriteLine("Keep waiting?");
                stopWaitHandle.WaitOne();
            }
        }

        static ManualResetEvent stopWaitHandle = new ManualResetEvent(false);

        public static void Main()
        {
            MainAsync().Wait();
            stopWaitHandle.Set();
        }
    }
}
