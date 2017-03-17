using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto.PeerProtocol
{
    public class TorrentManager
    {
        Torrent torrent;
        Dictionary<IPEndPoint, PeerConnection> peers = new Dictionary<IPEndPoint, PeerConnection>();

        ILogger Logger { get; } = GlobalLogger.CreateLogger<TorrentManager>(); 
        
        public TorrentManager(byte[] infohash)
        {
            torrent = new Torrent(infohash);
        }

        public TorrentManager(byte[] infohash, byte[] metadata)
        {
            torrent = new Torrent(infohash, metadata);
        }

        public void AddPeer(IPEndPoint peer)
        {
            var connection = new PeerConnection(peer, torrent);
            peers[peer] = connection;
            Logger.LogInformation($"Connecting to peer {peer.Address} for {torrent.Infohash.ToHex()}");
            
            // for now, let's assume that all connections are outgoing connections.
            // we'll have a little conditional to direct incoming connections later.
            connection.InitiateHandshake(torrent.Infohash);
            Logger.LogInformation($"Metadata is now set to: {torrent.Metadata.ToHuman()}");
        }
    }
}
