using System;
using System.Net;

using Ditto.Common;
using Microsoft.Extensions.Logging;
namespace Ditto
{
    // This class exists temporarily, and will definitely NOT make it into production.
    // Its sole purpose is to determine if we're using the CLInterface or not
    // until CLInterface is 100% functional/works with Chris's workflow.
    class Runner
    {

        ILogger logger { get; } = GlobalLogger.CreateLogger<Runner>();
        public static int Main(string[] args)
        {
            var useCLI = true;

            GlobalLogger.LoggerFactory.AddConsole(LogLevel.Information, true);

            if (useCLI)
            {
                CLInterface.RunCLI(args);
            }
            else
            {
                using (var client = new Ditto.BitTorrent.Client())
                {
                    var peerIP = IPAddress.Parse(args[0]);
                    var peerEndpoint = new IPEndPoint(peerIP, Int32.Parse(args[1]));
                    client.Example(new IPAddress[] { IPAddress.Loopback }, peerEndpoint).Wait();
                }
            }
            return 0;
        }
    }
}
