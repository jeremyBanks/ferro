using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ferro
{
    public class HelloWorld
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Would you like me to say 'Hello world?'");
            string ans = Console.ReadLine();
            if (ans.ToLower() == "yes")
            {
                Console.WriteLine("Hello world!");
            }
            else if (ans.ToLower() == "no")
            {
                Console.WriteLine("Too bad");
                Console.WriteLine("Hello world!");
            }
            else
            {
                Console.WriteLine("I didn't understand your answer, so...");
                Console.WriteLine("Hello world!");
            }

            Console.WriteLine("Hit a key to exit");
            Console.ReadLine();

        }
    }
}
