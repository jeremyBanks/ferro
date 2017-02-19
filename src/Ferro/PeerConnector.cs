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
        private byte[] zeroBuffer = new byte[8];
        // Need to begin peer id with an implementation id -- format: `-FR1000-` (dash, callsign, version number, dash)
        private byte[] peerId = new byte[20];

        // Indicate whether extensions are enabled for both self and peer
        bool extensionsEnabled = false;
        bool theirExtensionsEnabled = false;

        public PeerConnection(IPAddress ipAddress)
        {
            fixedHeader[0] = Convert.ToByte(19);
            "BitTorrent protocol".ToASCII().CopyTo(fixedHeader, 1);
            myIpAddress = ipAddress;
            peerId.FillRandom();
        }

        // Generalized method to enable any extension we see fit.
        // See BEP 10 http://www.bittorrent.org/beps/bep_0010.html
        private void EnableExtensions()
        {
            zeroBuffer[5] = (byte) 16;
            extensionsEnabled = true;
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            EnableExtensions();

            TcpListener connection = new TcpListener(myIpAddress, myPort);
            connection.Start();

            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();


            if (!client.Connected)
            {
                throw new Exception("Failed to connect to peer.");
            }

            Console.WriteLine("Connected to peer.");

            // Put all of our handshake data into a byte array
            byte[] handshake = new byte[68];
            fixedHeader.CopyTo(handshake, 0);
            zeroBuffer.CopyTo(handshake, fixedHeader.Length);
            infoHash.CopyTo(handshake, fixedHeader.Length + zeroBuffer.Length);
            peerId.CopyTo(handshake, fixedHeader.Length + zeroBuffer.Length + infoHash.Length);

            Console.WriteLine(handshake.FromASCII());

            NetworkStream stream = client.GetStream();
            stream.Write(handshake);

            if (!stream.CanRead)
            {
                throw new Exception("Unable to read from the network stream.");
            }

            byte[] response = new byte[256];
            stream.Read(response, 0, response.Length);
            Console.WriteLine(response.FromASCII());

            byte[] peerFixedHeader = new byte[20];
            byte[] peerBuffer = new byte[8];
            byte[] peerInfoHash = new byte[20];
            byte[] theirPeerId = new byte[20];

            Array.Copy(response, 0, peerFixedHeader, 0, 20);
            // TODO: Replace byte[].SequenceEqual() with the more customized byte[] comparator written by 
            // @banks -- see ceb791f0f5f6067abb900bc32eb29c4ad54e1407
            if (!peerFixedHeader.SequenceEqual(fixedHeader))
            {
                Console.WriteLine("Peer failed to return fixed header; aborting connection.");
                stream.Dispose();
                connection.Stop();
            }

            Array.Copy(response, 20, peerBuffer, 0, 8);
            if (peerBuffer[5] == 16)
            {
                theirExtensionsEnabled = true;
            }

            Array.Copy(response, 28, peerInfoHash, 0, 20);
            Console.WriteLine("Peer's infohash is: " + peerInfoHash.FromASCII());
            if (!peerInfoHash.SequenceEqual(infoHash))
            {
                Console.WriteLine("Peer failed to return a matching infohash; aborting connection.");
                stream.Dispose();
                connection.Stop();
            }

            Array.Copy(response, 48, theirPeerId, 0, 20);
            Console.WriteLine("The peer's peer ID is " + theirPeerId.FromASCII());

            if (extensionsEnabled && theirExtensionsEnabled)
            { 
                byte[] extensionMessage = new byte[response.Length - 69];
                Array.Copy(response, 68, extensionMessage, 0, extensionMessage.Length);
                Console.WriteLine(extensionMessage.FromASCII());

                var lengthPrefix = new byte[4];
                Array.Copy(extensionMessage, 0, lengthPrefix, 0, 4);
                // BitConverter assumes a little-endian byte array, and since we're getting it big-endian,
                // I'm using Linq to reverse the thing.
                // TODO: Write a more versitile method to check and do this if necessary.
                var length = 0;
                if (BitConverter.IsLittleEndian)
                {
                    length += BitConverter.ToInt32(lengthPrefix.Reverse().ToArray(), 0);
                }
                else
                {
                    length += BitConverter.ToInt32(lengthPrefix, 0);
                }
                Console.WriteLine(length.ToString());

            }
            

            //Console.WriteLine("Protocol extension: " + Bencoding.ToHuman((byte[]) Bencoding.Decode(protocolExtension)).ToASCII());
            // we probably want to get rid of this in the future, when there's a proceding action
            connection.Stop();
        }
    }
}
