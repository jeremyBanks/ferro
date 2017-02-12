using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Ferro
{
    public class PeerConnector
    {
        // We probably want to keep this private and potentially constant
        // This is the port we'll be listening on
        private Int32 myPort = 6881;
        private IPAddress myIpAddress;
        private byte[] handshakeHeader = new byte[20];
        private byte[] handshakeBuffer = new byte[8];
        private byte[] peerId = new byte[20];

        public PeerConnector(IPAddress ipAddress)
        {
            handshakeHeader[0] = Convert.ToByte(19);
            "BitTorrent protocol".ToASCII().CopyTo(handshakeHeader, 1);
            myIpAddress = ipAddress;
            peerId.FillRandom();
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            TcpListener connection = new TcpListener(myIpAddress, myPort);
            connection.Start();

            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();

            if (!client.Connected)
            {
                throw new Exception("Attempted to move on without connecting to peer");
            } else
            {
                Console.WriteLine("connected");
            }
            

            // Put all of our handshake data into a byte array
            byte[] handshake = new byte[68];
            handshakeHeader.CopyTo(handshake, 0);
            handshakeBuffer.CopyTo(handshake, handshakeHeader.Length);
            infoHash.CopyTo(handshake, handshakeHeader.Length + handshakeBuffer.Length);
            peerId.CopyTo(handshake, handshakeHeader.Length + handshakeBuffer.Length + infoHash.Length);

            Console.WriteLine(handshake.FromASCII());

            NetworkStream stream = client.GetStream();
            stream.Write(handshake);

            if (!stream.CanRead)
            {
                throw new Exception("Unable to read from the current network stream");
            } else
            {
                byte[] response = new byte[256];
                stream.Read(response, 0, response.Length);
                Console.WriteLine(response.FromASCII());
            }

            // we probably want to get rid of this in the future, when there's a proceding action
            connection.Stop();
        }
    }
}
