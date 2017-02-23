using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Ferro
{
    public class PeerConnection
    {
        readonly private Int32 myPort = 6881;
        readonly private IPAddress myIpAddress;
        // TODO: Need to begin peer id with an implementation id -- format: `-FR1000-` (dash, callsign, version number, dash)
        readonly private byte[] peerId = new byte[20];

        private bool extensionsEnabled = false;
        private bool theirExtensionsEnabled = false;

        public PeerConnection(IPAddress ipAddress)
        {
            myIpAddress = ipAddress;
            peerId.FillRandom();
        }

        public void InitiateHandshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            var fixedHeader = new byte[20];
            fixedHeader[0] = (byte) 19;
            "BitTorrent protocol".ToASCII().CopyTo(fixedHeader, 1);

            var bufferBitfield = new byte[8];
            bufferBitfield[5] = (byte) 16;
            extensionsEnabled = true;

            TcpClient connection = new TcpClient();
            connection.ConnectAsync(peerIP, peerPort).Wait();

            if (!connection.Connected)
            {
                throw new Exception("Failed to connect to peer.");
            }

            var initialHandshake = new byte[68];
            fixedHeader.CopyTo(initialHandshake, 0);
            bufferBitfield.CopyTo(initialHandshake, fixedHeader.Length);
            infoHash.CopyTo(initialHandshake, fixedHeader.Length + bufferBitfield.Length);
            peerId.CopyTo(initialHandshake, fixedHeader.Length + bufferBitfield.Length + infoHash.Length);

            Console.WriteLine("Sending our handshake to " + peerIP + ":" + peerPort);
            NetworkStream stream = connection.GetStream();
            stream.Write(initialHandshake);

            Console.WriteLine("Received response from peer.");
            byte[] theirFixedHeader = new byte[20];
            byte[] theirBuffer = new byte[8];
            byte[] theirInfoHash = new byte[20];
            byte[] theirPeerId = new byte[20];

            stream.Read(theirFixedHeader, 0, 20);
            if (!theirFixedHeader.SequenceEqual(fixedHeader))
            {
                stream.Dispose();
                throw new Exception("Peer failed to return fixed header; aborting connection.");
            }

            stream.Read(theirBuffer, 0, 8);
            if (theirBuffer[5] == 16)
            {
                theirExtensionsEnabled = true;
            }

            stream.Read(theirInfoHash, 0, 20);
            Console.WriteLine("Peer's infohash is: " + theirInfoHash.FromASCII());
            if (!theirInfoHash.SequenceEqual(infoHash))
            {
                stream.Dispose();
                throw new Exception("Peer failed to return a matching infohash; aborting connection.");
            }

            stream.Read(theirPeerId, 0, 20);
            Console.WriteLine("The peer's peer ID is " + theirPeerId.FromASCII());

            if (extensionsEnabled && theirExtensionsEnabled)
            {
                var theirExtensionHeader = GetPeerExtensionHeader(stream);
                var decodedExtension = Bencoding.Decode(theirExtensionHeader);
                Console.WriteLine("Peer's extension header:");
                Console.WriteLine(Bencoding.ToHuman(theirExtensionHeader));

                Console.WriteLine("Sending our extension header...");

                var extensionDict = GenerateExtentionDict();
                var extensionHeader = new byte[extensionDict.Length + 2];
                extensionHeader[0] = (byte)20;
                extensionHeader[1] = (byte)0;
                extensionDict.CopyTo(extensionHeader, 2);
                stream.Write(extensionDict);

                Console.WriteLine(Bencoding.ToHuman(extensionDict));
            }

            stream.Dispose();
        }

        private byte[] GetPeerExtensionHeader(NetworkStream stream)
        {
            // BitConverter assumes a little-endian byte array, and since we're getting it big-endian,
            // I'm using Linq to reverse the thing.
            // TODO: Write a more versitile method to check and do this if necessary.
            var length = 0;
            var lengthPrefix = new byte[4];
            stream.Read(lengthPrefix, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                length += BitConverter.ToInt32(lengthPrefix.Reverse().ToArray(), 0);
            }
            else
            {
                length += BitConverter.ToInt32(lengthPrefix, 0);
            }

            var extensionResponse = new byte[length];
            stream.Read(extensionResponse, 0, length);
            if (extensionResponse[0] != 20)
            {
                stream.Dispose();
                throw new Exception("Unexpected payload in handshake extension; Aborting.");
            }
            if (extensionResponse[1] != 0)
            {
                stream.Dispose();
                throw new Exception("Derp derp write this later");
            }

            var theirExtensionDict = new byte[length - 2];
            Array.Copy(extensionResponse, 2, theirExtensionDict, 0, length - 2);

            return theirExtensionDict;
        }

        private byte[] GenerateExtentionDict()
        {
            var extensionDict = new Dictionary<byte[], object>();
            var supportedExtensions = new Dictionary<byte[], object>();

            // ut_metadata and metadata_size indicate support for BEP 9, which we will add later.
            // currently hardcoding metadata_size -- need to get it from actual source
            supportedExtensions["ut_metadata".ToASCII()] = (Int64) 3;
            extensionDict["m".ToASCII()] = supportedExtensions;
            extensionDict["metadata_size".ToASCII()] = (Int64) 16108;
            extensionDict["p".ToASCII()] = (Int64) myPort;
            extensionDict["v".ToASCII()] = "Ferro 0.1.0".ToASCII();

            return Bencoding.Encode(extensionDict);
        }
    }
}
