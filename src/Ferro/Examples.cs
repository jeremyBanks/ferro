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
            tcpPeerProtocol(testAddress);

            return 0;
        }

        static async Task dhtClient(IPAddress testAddress) {
            var dht = new DHTClient();
            var id = await dht.Ping(new IPEndPoint(testAddress, 9527));
            Console.WriteLine(
                $"Successfully pinged node and got response from node ID {id.ToHex()}");
        }

        static void tcpPeerProtocol(IPAddress testAddress) {
            var connector = new PeerConnector(IPAddress.Any);
            var infoHash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();

            connector.Handshake(testAddress, 45566, infoHash);
            Console.WriteLine("Finished with Handshake");
        }
        
    }
}
