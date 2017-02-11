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
            
            return 0;
        }
    }
}
