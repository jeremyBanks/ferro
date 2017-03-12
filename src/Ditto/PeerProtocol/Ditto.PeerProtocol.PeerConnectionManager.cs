using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using Ditto.Common;

namespace Ditto.PeerProtocol
{
    public class PeerConnectionManager
    {
        public static readonly Int32 myPort = 6881;
        readonly private IPAddress myIpAddress;
        public static readonly byte[] peerId = new byte[20].FillRandom();

        public static bool extensionsEnabled = false;
        
        ILogger Logger { get; } = GlobalLogger.CreateLogger<PeerConnectionManager>();

        public PeerConnectionManager(IPAddress ipAddress)
        {
            myIpAddress = ipAddress;
            "-FR0001-".ToASCII().CopyTo(peerId, 0);
        }

    }
}
