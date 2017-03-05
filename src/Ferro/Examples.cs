﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Ferro
{
    class Examples {
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
            var dht = new DHTClient();

            var testNode = new IPEndPoint(testAddress, 9527);
            var testNodeId = await dht.Ping(testNode);

            Console.WriteLine(
                $"Successfully pinged {testNode} and got response with node ID {testNodeId.ToHex()}");
            
            var infoHash = "ea45080eae6eab465f647e6366f775bf25f69a61".FromHex();

            var peers = await dht.GetPeers(infoHash);

            Console.WriteLine(
                $"Requested peers for {infoHash.ToHex()} and got some response!");
            
        }

        static void tcpPeerProtocol(IPAddress testAddress) {
            var connector = new PeerConnection(IPAddress.Any);
            var infoHash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();
            var infoHashMultiPiece = "68d22f0f856ca5056e009ac53597a66c0cb03068".FromHex();
            connector.InitiateHandshake(testAddress, 45566, infoHash);
            connector.InitiateHandshake(testAddress, 45566, infoHashMultiPiece);
            Console.WriteLine("Finished with Handshake");
        }
        
    }
}
