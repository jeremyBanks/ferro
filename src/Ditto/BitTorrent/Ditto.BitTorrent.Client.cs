﻿using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto.BitTorrent
{
    class Client : IDisposable
    {    
        readonly private IPAddress myIpAddress;
        public static readonly Int32 myPort = 6881;
        public static readonly byte[] peerId = new byte[20].FillRandom();

        public static bool extensionsEnabled = true; // Extensions enabled by default -- option to disable?

        private DHT.Client dht;

        private static ILogger logger { get; } = GlobalLogger.CreateLogger<Client>();

        public Client() {
            dht = new DHT.Client();
            myIpAddress = IPAddress.Any;
            "ditto.to#".ToASCII().CopyTo(peerId, 0);
        }

        public async Task Example(IPAddress[] bootstrapAddresses, IPEndPoint peer=null)
        {
            if (peer == null)
            {
                foreach (var address in bootstrapAddresses)
                {
                    var bootstrapNode = new IPEndPoint(address, 9527);
                    dht.AddNode(bootstrapNode);
                }

                var ubuntuPeers = await dht.GetPeers(KnownTorrents.ubuntuUnknownInfohash);
                {
                    logger.LogInformation(LoggingEvents.DHT_PROTOCOL_MSG,
                        $"Requested peers for Ubuntu {KnownTorrents.ubuntuUnknownInfohash.ToHex()} and got {ubuntuPeers.Count}!");

                    foreach (var ep in ubuntuPeers)
                    {
                        logger.LogInformation(LoggingEvents.ATTEMPT_CONNECTION, $"Attempting to connect to peer at {ep}.");

                        try
                        {
                            var testTorrent = new Ditto.PeerProtocol.TorrentManager(KnownTorrents.ubuntuUnknownInfohash);
                            testTorrent.AddPeer(peer); // this will need to be managed more systematically
                            break;
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(LoggingEvents.DHT_ERROR, "It failed: " + ex);
                            await Task.Delay(1000);
                            logger.LogError(LoggingEvents.DHT_ERROR, "Do I have another peer to try?");
                            continue;
                        }
                    }

                    logger.LogInformation("Done.");
                }
            }
            else
            {
                try
                {
                    var ubuntuTorrent = new Ditto.PeerProtocol.TorrentManager(KnownTorrents.netBSDInfohash);
                    ubuntuTorrent.AddPeer(peer); // this will need to be managed more systematically
                }
                catch (Exception ex)
                {
                    logger.LogError(LoggingEvents.DHT_ERROR, "It failed: " + ex);
                }
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
