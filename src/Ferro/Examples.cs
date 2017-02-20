﻿using System;
using System.Net;

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

            //dhtClient(testAddress);
            tcpPeerProtocol(testAddress);

            return 0;
        }

        static void dhtClient(IPAddress testAddress) {
            var dht = new DHTClient();
            dht.Ping(new IPEndPoint(testAddress, 9527)).Wait();
        }

        static void tcpPeerProtocol(IPAddress testAddress) {
            var connector = new PeerConnection(IPAddress.Any);
            var infoHash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();

            connector.Handshake(testAddress, 45566, infoHash);
            Console.WriteLine("Finished with Handshake");
        }
        
    }
}
