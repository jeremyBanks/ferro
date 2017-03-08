﻿using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Ferro.Common;

namespace Ferro.BitTorrent
{
    class Client : IDisposable {
        // Torrents we expect to be loaded into our test peer.
        readonly byte[] veryTinyKnownInfohash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();
        readonly byte[] lessTinyKnownInfohash = "68d22f0f856ca5056e009ac53597a66c0cb03068".FromHex();
        // Torrents we do not expect to be loaded in our test peer, but which should have many peers online.
        readonly byte[] ubuntuUnknownInfohash = "34930674ef3bb9317fb5f263cca830f52685235b".FromHex();

        private DHT.Client dht;

        static ILogger Logger { get; } = ApplicationLogging.CreateLogger<Client>();

        public Client() {
            dht = new DHT.Client();
        }

        public async Task Example(IPAddress testAddress) {
            ApplicationLogging.LoggerFactory.AddConsole();

            var bootstrapNode = new IPEndPoint(testAddress, 9527);
            dht.AddNode(bootstrapNode);

            var ubuntuPeers = await dht.GetPeers(ubuntuUnknownInfohash);
            using (Logger.BeginScope($"{nameof(Client)}"))
            {
                Logger.LogInformation(
                    $"Requested peers for Ubuntu {ubuntuUnknownInfohash.ToHex()} and got {ubuntuPeers.Count}!");

                foreach (var ep in ubuntuPeers)
                {
                    Logger.LogInformation($"Attempting to connect to peer at {ep}.");

                    try
                    {
                        var connection = new Ferro.PeerProtocol.PeerConnection(IPAddress.Any);
                        connection.InitiateHandshake(ep.Address, ep.Port, ubuntuUnknownInfohash);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("It failed: " + ex);
                        await Task.Delay(1000);
                        Logger.LogError("Do I have another peer to try?");
                        continue;
                    }
                }

                Logger.LogInformation("Done.");
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                dht?.Dispose();

                disposedValue = true;
            }
        }

        ~Client() {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
