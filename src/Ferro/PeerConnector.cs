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
        private Int32 myPort = 8888;

        public TcpListener TCPConnection(IPAddress peerIP, Int32 peerPort)
        {
            var host = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;

            // get a useable IP address for us
            foreach (var ip in host.AddressList)
            {
                if (ip.ToString().EndsWith("1") == false)
                {
                    return new TcpListener(ip, myPort);
                }
            }

            throw new Exception("No valid IP addresses are available.");
        }


    }
}
