using System;
using System.Net;

﻿namespace Ferro
{
    class Examples {
        public static void Main()
        {
            Console.WriteLine("Hello, world!");

            var connector = new PeerConnector(IPAddress.Parse("0.0.0.0"));
            var infoHash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();

            connector.Handshake(IPAddress.Parse("192.168.99.100"), 45566, infoHash);
            Console.WriteLine("Finished with Handshake");
                       
            dhtClient();
            tcpPeerProtocol();
            Console.ReadLine();
        }

        static void dhtClient() {
            var dht = new DHTClient();
            dht.Ping().Wait();
        }

        static void tcpPeerProtocol() {
            // TODO
        }
        
    }
}
