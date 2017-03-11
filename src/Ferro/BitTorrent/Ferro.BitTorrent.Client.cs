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

        private static ILogger logger { get; } = GlobalLogger.CreateLogger<Client>();

        public Client() {
            dht = new DHT.Client();
        }

        public async Task Example(IPAddress[] bootstrapAddresses, IPAddress peerIP)
        {
            //foreach (var address in bootstrapAddresses) {
            //    var bootstrapNode = new IPEndPoint(address, 9527);
            //    dht.AddNode(bootstrapNode);
            //}

            //var ubuntuPeers = await dht.GetPeers(ubuntuUnknownInfohash);
            //{
            //    logger.LogInformation(LoggingEvents.DHT_PROTOCOL_MSG,
            //        $"Requested peers for Ubuntu {ubuntuUnknownInfohash.ToHex()} and got {ubuntuPeers.Count}!");

            //    foreach (var ep in ubuntuPeers)
            //    {
            //        logger.LogInformation(LoggingEvents.ATTEMPT_CONNECTION, $"Attempting to connect to peer at {ep}.");

            //        try
            //        {
            //            var connection = new Ferro.PeerProtocol.PeerConnection(IPAddress.Any);
            //            connection.InitiateHandshake(ep.Address, ep.Port, ubuntuUnknownInfohash);
            //            break;
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.LogError(LoggingEvents.DHT_ERROR, "It failed: " + ex);
            //            await Task.Delay(1000);
            //            logger.LogError(LoggingEvents.DHT_ERROR, "Do I have another peer to try?");
            //            continue;
            //        }
            //    }

            //    logger.LogInformation("Done.");
            //}

            try
            {
                var connection = new Ferro.PeerProtocol.PeerConnection(IPAddress.Any);
                connection.InitiateHandshake(peerIP, 8999, ubuntuUnknownInfohash);
            }
            catch (Exception ex)
            {
                logger.LogError(LoggingEvents.DHT_ERROR, "It failed: " + ex);
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
