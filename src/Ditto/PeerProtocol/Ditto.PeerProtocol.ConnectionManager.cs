using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto.PeerProtocol
{
    public class ConnectionManager
    {
        public static readonly Int32 myPort = 6881;
        readonly private IPAddress myIpAddress;
        public static readonly byte[] peerId = new byte[20].FillRandom();

        public static bool extensionsEnabled = false;
        
        ILogger Logger { get; } = GlobalLogger.CreateLogger<ConnectionManager>();

        public ConnectionManager(IPAddress ipAddress)
        {
            myIpAddress = ipAddress;
            "-FR0001-".ToASCII().CopyTo(peerId, 0);
        }

        public Task InitiateConnection(IPEndPoint peer)
        {
            var connection = new TcpClient();
        }
        // uses the TPL to listen for and manage multiple incoming requests from peers
        // concurrently. This needs to be set up more rigorously and tested.
        public void HandleIncomingConnections()
        {
            var listener = new TcpListener(myIpAddress, myPort);

            while (true)
            {
                var connectionTask = new Task(() => listener.AcceptTcpClientAsync().Wait());
                connectionTask.Start();
            }
        }
    }
}
