﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Ferro
{
    class Examples {
        // Torrents we expect to be loaded into our test peer.
        readonly static byte[] veryTinyKnownInfohash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();
        readonly static byte[] lessTinyKnownInfohash = "68d22f0f856ca5056e009ac53597a66c0cb03068".FromHex();
        // Torrents we do not expect to be loaded in our test peer, but which should have many peers online.
        readonly static byte[] ubuntuUnknownInfohash = "34930674ef3bb9317fb5f263cca830f52685235b".FromHex();

        public static int Main(string[] args)
        {
            var testAddress = IPAddress.Loopback;

            if (args.Length == 1) {
                testAddress = IPAddress.Parse(args[0]);
            } else {
                Console.WriteLine("usage: ferro TEST_IP_ADDRESS");
                return 1;
            }

            dhtClient(testAddress).Wait();
            Console.Write("\n");
            tcpPeerProtocol(testAddress);
            

            return 0;
        }

        static async Task dhtClient(IPAddress testAddress) {
            using (var dht = new DHTClient()) {
                var testNode = new IPEndPoint(testAddress, 9527);
                var testNodeId = await dht.Ping(testNode);

                Console.WriteLine(
                    $"Successfully pinged {testNode} and got response with node ID {testNodeId.ToHex()}");
                
                var peers = await dht.GetPeers(veryTinyKnownInfohash);

                Console.WriteLine(
                    $"Requested peers for {veryTinyKnownInfohash.ToHex()} (known torrent) and got some response!");
                
                var ubuntuPeers = await dht.GetPeers(ubuntuUnknownInfohash);

                Console.WriteLine(
                    $"Requested peers for {ubuntuUnknownInfohash.ToHex()} (unknown torrent) and got some response!");
            }
        }

        static void tcpPeerProtocol(IPAddress testAddress) {
            var connector = new PeerConnection(IPAddress.Any);
            connector.InitiateHandshake(testAddress, 45566, veryTinyKnownInfohash);
            connector.InitiateHandshake(testAddress, 45566, lessTinyKnownInfohash);
            Console.WriteLine("Finished with Handshake");
        }
        
    }
}
