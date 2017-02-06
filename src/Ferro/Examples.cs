using System;

namespace Ferro
{
    public class Examples
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Hello, world!");
            Console.WriteLine("Hello, world!".ToASCII().Sha1().ToHex());
            return 0;
        }
    }
}
