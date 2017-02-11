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
        private byte[] handshakeGreeting = "19BitTorrent protocol".ToASCII();

        public PeerConnector(String ipAddress)
        {
            myIpAddress = IPAddress.Parse(ipAddress);
        }

        public PeerConnector()
        {
            myIpAddress = IPAddress.Parse("127.0.0.1");
        }

        public void Handshake(IPAddress peerIP, Int32 peerPort)
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

            NetworkStream stream = client.GetStream();
            stream.Write(handshakeGreeting);
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
