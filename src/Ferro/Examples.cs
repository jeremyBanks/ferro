using System;
using System.Net;

namespace Ferro
{
    public class Examples
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, world!");
            Console.WriteLine("Hello, world!".ToASCII().Sha1().ToHex());
            Console.WriteLine("Hello, world!".ToASCII().Sha1().ToHuman());

            var connector = new PeerConnector();

            connector.Handshake(IPAddress.Parse("192.168.99.100"), 45566);

            Console.WriteLine("Finished with Handshake");
            Console.ReadLine();
                       
            return 0;
        }
    }
}
