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
                Console.WriteLine("usage: ferro BOOTSTRAP_PEER_IP_ADDRESS");
                return 1;
            }

            main(testAddress).Wait();

            return 0;
        }

        static async Task main(IPAddress testAddress) {
            using (var dht = new DHTClient()) {
                var bootstrapNode = new IPEndPoint(testAddress, 9527);
                dht.AddNode(bootstrapNode);

                var ubuntuPeers = await dht.GetPeers(ubuntuUnknownInfohash);

                Console.WriteLine(
                    $"Requested peers for Ubuntu {ubuntuUnknownInfohash.ToHex()} and got some response!");

                foreach (var ep in ubuntuPeers) {
                    Console.WriteLine("Attempting to connect to peer at ${ep}.");

                    var connection = new PeerConnection(IPAddress.Any);
                    connection.InitiateHandshake(ep.Address, ep.Port, ubuntuUnknownInfohash);

                    break;
                }

                Console.WriteLine(ubuntuPeers);

            }
        }
    }
}
