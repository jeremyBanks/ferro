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
        private byte[] handshakeHeader = "19BitTorrent protocol".ToASCII();
        private byte[] handshakeBuffer = "00000000".ToASCII();

        public PeerConnector(String ipAddress)
        {
            myIpAddress = IPAddress.Parse(ipAddress);
        }

        public PeerConnector()
        {
            myIpAddress = IPAddress.Parse("127.0.0.1");
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort, byte[] infoHash)
        {
            TcpListener connection = new TcpListener(myIpAddress, myPort);
            connection.Start();

            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();

            if (client.Connected)
            {
                Console.WriteLine("connected");
            } else
            {
                throw new Exception("Attempted to move on without connecting to peer");
            }

            // sha1 infohash will be 20 bytes -- so....
            byte[] handshake = new byte[49];
            Array.Copy(handshakeHeader, 0, handshake, 0, handshakeHeader.Length);
            Array.Copy(handshakeBuffer, 0, handshake, handshakeHeader.Length, handshakeBuffer.Length);
            Array.Copy(infoHash, 0, handshake, handshakeHeader.Length + handshakeBuffer.Length, infoHash.Length);

            Console.WriteLine(handshake.FromASCII());

            NetworkStream stream = client.GetStream();
            stream.Write(handshake);

            if (stream.CanRead)
            {
                Console.WriteLine("About to read...");
                byte[] response = new byte[256];
                stream.Read(response, 0, response.Length);
                Console.WriteLine(stream.DataAvailable.ToString());
                Console.WriteLine(response.FromASCII());
            }
            else
            {
                Console.WriteLine("Can't read from this stream, apparently");
            }
            
        }
    }
}
