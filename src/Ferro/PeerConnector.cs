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
            TcpClient client = new TcpClient();
            client.ConnectAsync(peerIP, peerPort).Wait();

            if (client.Connected)
            {
                Console.WriteLine("connected");
            }
        }
    }
}
