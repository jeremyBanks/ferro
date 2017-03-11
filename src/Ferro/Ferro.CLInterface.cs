using System;
using System.Net;

using Ferro.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.CommandLineUtils;
using System.Linq;

namespace Ferro {

    class CLInterface {
        // Releases should have a specific version, non-releases should be
        // tagged as -dev of the next reease.
        public static readonly string version = "v0.1-dev";

        ILogger logger { get; } = GlobalLogger.CreateLogger<CLInterface>();
         
        public static int Main(string[] args)
        {
            // for testing functionality with a single controlled peer client -- pass target IP as arg
            var peerIP = IPAddress.Parse(args[0]);
            writeHeader();

            var cli = new CommandLineApplication();
            var verboseOption = cli.Option(
                "-v | --verbose",
                "Enables verbose logging",
                CommandOptionType.NoValue
            );
            var bootstrapAddressArgument = cli.Argument(
                "[bootstrap_addresses...]",
                "Optional IP addresses of DHT nodes for bootstrapping.");
            var helpOption = cli.HelpOption("-? | -h | --help");

            cli.OnExecute(() =>
            {
                var verbose = verboseOption.HasValue();

                GlobalLogger.LoggerFactory.AddConsole(
                    verbose ? LogLevel.Debug : LogLevel.Information, true);

                using (var client = new Ferro.BitTorrent.Client()) {
                    var bootstrapAddresses =
                        bootstrapAddressArgument.Values.Select(
                            s => IPAddress.Parse(s)
                        ).ToArray();
                    client.Example(bootstrapAddresses).Wait();
                }   

                return 0;
            });

            return cli.Execute(args);
        }

        static void writeHeader() {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Error.WriteLine($"Ferro BitTorrent CLIent {version}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Error.WriteLine("https://banks.gitlab.io/ferro/");
            Console.Error.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Error.WriteLine(
                "WARNING: This software is still in an experimental state. It may misbehave towards other peers on the network or your own system. Please limit your use.");
            Console.ResetColor();
        }
    }
}