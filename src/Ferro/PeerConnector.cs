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

        public void Handshake(IPAddress peerIP, Int32 peerPort)
        {
            
            // TODO: figure out what port to use for my Docker container
            /*TcpListener listener = new TcpListener(myPort);*/


        }
    }
}
