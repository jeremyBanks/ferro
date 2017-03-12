using System;
using System.Net;

using Ditto.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.CommandLineUtils;

namespace Ditto {

    class CLInterface {
        // Releases should have a specific version, non-releases should be
        // tagged as -dev of the next reease.
        public static readonly string version = "v0.1-dev";

        static ILogger logger { get; } = GlobalLogger.CreateLogger<CLInterface>();
         
        public static int Main(string[] args)
        {
            var cli = new CommandLineApplication {
                Name = "ditto",
                FullName = "Ditto"
            };

            cli.HelpOption("-h | --help | -?");
            cli.VersionOption("--version", version);

            cli.OnExecute(() =>
            {
                GlobalLogger.LoggerFactory.AddConsole(LogLevel.Information, true);

                logger.LogWarning("This software is still in an experimental state. It may misbehave towards other peers on the network or your own system. Please limit your use.");

                cli.ShowHelp();
                return 0;
            });

            cli.Command("get-meta", sub => {
                sub.Description = "Fetches the metadata for the specified torrent, saving it as a .torrent file.";
                sub.HelpOption("-h | --help | -?");

                var verboseOption = sub.Option(
                    "-v | --verbose", "Enables verbose logging", CommandOptionType.NoValue);

                var infohashArgument = sub.Argument(
                    "infohash",
                    "The infohash of the torrent to fetch.");

                sub.OnExecute(() => {
                    GlobalLogger.LoggerFactory.AddConsole(
                        verboseOption.HasValue() ? LogLevel.Debug : LogLevel.Information, true);

                    using (var client = new Ditto.BitTorrent.Client()) {
                        client.Example(new IPAddress[] { IPAddress.Loopback }).Wait();
                    }

                    return 0; 
                });
            });

            cli.Command("connect", sub => {
                sub.Description = "Refreshes the DHT connection as neccessary.";
                sub.HelpOption("-h | --help | -?");

                var verboseOption = sub.Option(
                    "-v | --verbose", "Enables verbose logging", CommandOptionType.NoValue);

                var bootstrapArguments = sub.Argument(
                    "bootstrap",
                    "The address:port pairs for any bootstrap nodes to use if neccessary.");

                sub.OnExecute(() => {
                    GlobalLogger.LoggerFactory.AddConsole(
                        verboseOption.HasValue() ? LogLevel.Debug : LogLevel.Information, true);

                    using (var client = new Ditto.BitTorrent.Client()) {
                        client.Example(new IPAddress[] { IPAddress.Loopback }).Wait();
                    }

                    return 0; 
                });
            });

            return cli.Execute(args);
        }
    }
}