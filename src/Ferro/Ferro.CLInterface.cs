using System;
using System.Net;

namespace Ferro {

    class CLInterface {

        public static int Main(string[] args)
        {
            var testAddress = IPAddress.Loopback;

            if (args.Length == 1) {
                testAddress = IPAddress.Parse(args[0]);
            } else {
                Console.WriteLine("usage: ferro BOOTSTRAP_PEER_IP_ADDRESS");
                return 1;
            }

            using (var client = new Ferro.BitTorrent.Client()) {
                client.Example(testAddress).Wait();
            }

            return 0;
        }
    }
}