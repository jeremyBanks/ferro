using System;
using System.Net;

namespace Ferro {

    class CLInterface {
        // Releases should have a specific version, non-releases should be
        // tagged as -dev of the next reease.
        public static readonly string version = "v0.1-dev";

        public static int Main(string[] args)
        {
            var testAddress = IPAddress.Loopback;

            if (args.Length != 1) {
                Console.WriteLine("usage: ferro BOOTSTRAP_PEER_IP_ADDRESS");
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Error.WriteLine($"Ferro BitTorrent CLIent {version}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Error.WriteLine("https://banks.gitlab.io/ferro/");
            Console.Error.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine(
                "WARNING: This software is still in an experimental state. It may misbehave towards other peers on the network or your own system. Please limit your use.");
            Console.Error.WriteLine("");
            Console.ResetColor();

            testAddress = IPAddress.Parse(args[0]);

            using (var client = new Ferro.BitTorrent.Client()) {
                client.Example(testAddress).Wait();
            }

            return 0;
        }
    }
}