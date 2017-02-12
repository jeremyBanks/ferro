using System;
using System.Net;

namespace Ferro
{
    public class Examples
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, world!");

            var connector = new PeerConnector();
            var infoHash = "ea45080eab61ab465f647e6366f775bf25f69a61".FromHex();

            connector.Handshake(IPAddress.Parse("192.168.99.100"), 45566, infoHash);

            Console.WriteLine("Finished with Handshake");
            Console.ReadLine();
                       
            return 0;
        }
    }
}
