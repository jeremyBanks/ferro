using Ditto.Common;

namespace Ditto.BitTorrent
{
    class KnownTorrents
    {
        // Torrents we expect to be loaded into our test peer.
        public static readonly byte[] veryTinyKnownInfohash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();
        public static readonly byte[] lessTinyKnownInfohash = "68d22f0f856ca5056e009ac53597a66c0cb03068".FromHex();
        // Torrents we do not expect to be loaded in our test peer, but which should have many peers online.
        public static readonly byte[] ubuntuUnknownInfohash = "34930674ef3bb9317fb5f263cca830f52685235b".FromHex();
        public static readonly byte[] netBSDInfohash = "a04028df0f94de0db51f3132dec47c754ffc20b1".FromHex();
    }
}
