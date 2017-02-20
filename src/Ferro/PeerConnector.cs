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
        // We probably want to keep this private and potentially constant
        // This is the port we'll be listening on
        private Int32 myPort = 6881;
        private IPAddress myIpAddress;
        private byte[] fixedHeader = new byte[20];
        private byte[] extensionBitField = new byte[8];
        // TODO: Need to begin peer id with an implementation id -- format: `-FR1000-` (dash, callsign, version number, dash)
        private byte[] peerId = new byte[20];

        private bool extensionsEnabled = false;
        private bool theirExtensionsEnabled = false;

        public PeerConnection(IPAddress ipAddress)
        {
            fixedHeader[0] = Convert.ToByte(19);
            "BitTorrent protocol".ToASCII().CopyTo(fixedHeader, 1);
            myIpAddress = ipAddress;
            peerId.FillRandom();
        }

        // See BEP 10 http://www.bittorrent.org/beps/bep_0010.html
        private void EnableExtensions()
        {
            extensionBitField[5] = (byte) 16;
            extensionsEnabled = true;
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            EnableExtensions();

            //TcpListener connection = new TcpListener(myIpAddress, myPort);
            //connection.Start();

            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();


            if (!client.Connected)
            {
                throw new Exception("Failed to connect to peer.");
            }

            // Put all of our handshake data into a byte array
            var initialHandshake = new byte[68];
            fixedHeader.CopyTo(initialHandshake, 0);
            extensionBitField.CopyTo(initialHandshake, fixedHeader.Length);
            infoHash.CopyTo(initialHandshake, fixedHeader.Length + extensionBitField.Length);
            peerId.CopyTo(initialHandshake, fixedHeader.Length + extensionBitField.Length + infoHash.Length);

            Console.WriteLine("Sending our handshake: ");
            Console.WriteLine(initialHandshake.FromASCII());
            NetworkStream stream = client.GetStream();
            stream.Write(initialHandshake);

            var response = new byte[256];
            stream.Read(response, 0, response.Length);
            Console.WriteLine("Received response: ");
            Console.WriteLine(response.FromASCII());

            byte[] theirFixedHeader = new byte[20];
            byte[] theirBuffer = new byte[8];
            byte[] theirInfoHash = new byte[20];
            byte[] theirPeerId = new byte[20];

            Array.Copy(response, 0, theirFixedHeader, 0, 20);
            if (!theirFixedHeader.SequenceEqual(fixedHeader))
            {
                Console.WriteLine("Peer failed to return fixed header; aborting connection.");
                stream.Dispose();
                //connection.Stop();
            }

            Array.Copy(response, 20, theirBuffer, 0, 8);
            if (theirBuffer[5] == 16)
            {
                theirExtensionsEnabled = true;
            }

            Array.Copy(response, 28, theirInfoHash, 0, 20);
            Console.WriteLine("Peer's infohash is: " + theirInfoHash.FromASCII());
            if (!theirInfoHash.SequenceEqual(infoHash))
            {
                Console.WriteLine("Peer failed to return a matching infohash; aborting connection.");
                stream.Dispose();
                //connection.Stop();
            }

            Array.Copy(response, 48, theirPeerId, 0, 20);
            Console.WriteLine("The peer's peer ID is " + theirPeerId.FromASCII());

            if (extensionsEnabled && theirExtensionsEnabled)
            {
                var extensionDict = GenerateExtentionDict();
                var extensionHeader = new byte[extensionDict.Length + 2];
                extensionHeader[0] = 20;
                extensionHeader[1] = 0;
                extensionDict.CopyTo(extensionHeader, 2);
                stream.Write(extensionDict);

                // BitConverter assumes a little-endian byte array, and since we're getting it big-endian,
                // I'm using Linq to reverse the thing.
                // TODO: Write a more versitile method to check and do this if necessary.
                var length = 0;
                var lengthPrefix = new byte[4];
                Array.Copy(response, 68, lengthPrefix, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    length += BitConverter.ToInt32(lengthPrefix.Reverse().ToArray(), 0);
                }
                else
                {
                    length += BitConverter.ToInt32(lengthPrefix, 0);
                }

                var extensionResponse = new byte[length];
                Array.Copy(response, 72, extensionResponse, 0, length);
                if (extensionResponse[0] != 20)
                {
                    Console.WriteLine("Unexpected payload in handshake extension; Aborting.");
                    stream.Dispose();
                    //connection.Stop();
                }
                if (extensionResponse[1] != 0)
                {
                    Console.WriteLine("Derp derp write this later");
                    stream.Dispose();
                    // close this derps
                }

                var theirExtensionDict = new byte[length - 2];
                Array.Copy(extensionResponse, 2, theirExtensionDict, 0, length - 2);

                var decodedDict = Bencoding.Decode(theirExtensionDict);
                var humanReadableDict = Bencoding.ToHuman(theirExtensionDict);
                Console.WriteLine("Peer's handshake extension:");
                Console.WriteLine(humanReadableDict);
            }
            
            //connection.Stop();
        }

        private byte[] GenerateExtentionDict()
        {
            // byte[]
            var extensionDict = new Dictionary<byte[], object>();
            var supportedExtensions = new Dictionary<byte[], object>();
           
            // ut_metadata and metadata_size indicate support for BEP 9, which we will add later.
            // currently hardcoding metadata_size -- need to get it from actual source
            supportedExtensions.Add("ut_metadata".ToASCII(), (Int64) 3);
            extensionDict.Add("m".ToASCII(), supportedExtensions);
            extensionDict.Add("metadata_size".ToASCII(), (Int64) 16108);
            extensionDict.Add("p".ToASCII(), (Int64) myPort);
            extensionDict.Add("v".ToASCII(), "Ferro 0.1.0".ToASCII());

            return Bencoding.Encode(extensionDict);
        }
    }
}
