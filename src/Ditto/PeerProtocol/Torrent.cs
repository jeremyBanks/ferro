using System;
using System.Collections.Generic;
using System.Text;

namespace Ditto.PeerProtocol
{
    public class Torrent
    {
        public byte[] Infohash { get; set; }
        public byte[] Metadata { get; set; }
        public byte[][] dataPieces { get; set; } // will get the length of this from metadata

        public Torrent(byte[] infohash)
        {
            Infohash = infohash;
        }

        public Torrent(byte[] infohash, byte[] metadata)
        {
            Infohash = infohash;
            Metadata = metadata;
        }
    }
}
