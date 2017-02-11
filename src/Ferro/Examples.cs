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

            var host = Dns.GetHostEntryAsync(Dns.GetHostName()).Result;
            foreach (var ip in host.AddressList)
            {
                if (ip.ToString().EndsWith("1") == false)
                {
                    Console.WriteLine(ip.ToString());
                }
                
            }

            Console.ReadLine();
            return 0;
        }
    }
}
