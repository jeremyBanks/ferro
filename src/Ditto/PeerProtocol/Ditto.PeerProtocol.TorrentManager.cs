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
        byte[] Infohash;
        byte[] Metadata;
        byte[][] dataPieces; // will get the length of this from metadata
        Dictionary<IPEndPoint, PeerConnection> peers = new Dictionary<IPEndPoint, PeerConnection>();

        ILogger Logger { get; } = GlobalLogger.CreateLogger<TorrentManager>(); 
        
        public TorrentManager(byte[] infohash)
        {
            Infohash = infohash;
        }

        public TorrentManager(byte[] infohash, byte[] metadata)
        {
            Infohash = infohash;
            Metadata = metadata;
        }

        public void AddPeer(IPEndPoint peer)
        {
            var connection = new PeerConnection(peer);
            peers[peer] = connection;
            Logger.LogInformation($"Connecting to peer {peer.Address} for {Infohash.ToHex()}");
            
            // for now, let's assume that all connections are outgoing connections.
            // we'll have a little conditional to direct incoming connections later.
            connection.InitiateHandshake(Infohash);
        }
    }
}
