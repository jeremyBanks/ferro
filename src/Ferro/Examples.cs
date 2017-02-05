using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferro
{
    public class Examples
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Let's try some simple examples!");

            {
                // Let's start with a simple integer on its own.
                var input = Encoding.ASCII.GetBytes("i11e");
                var result = BencodeDeserializer.Deserialize(input);
                Debug.Assert(typeof(int) == result.GetType());
                var typedResult = (int) result;
                Debug.Assert(1 == typedResult);
            }

            {
                // Let's make sure it's not confused by more trailing characters.
                // (I mean, this should probably throw at some point, but not yet?)
                var input = Encoding.ASCII.GetBytes("i11e");
                var result = BencodeDeserializer.Deserialize(input);
                Debug.Assert(typeof(int) == result.GetType());
                var typedResult = (int) result;
                Debug.Assert(1 == typedResult);
            }

            Console.WriteLine("Done! We should now crash, or else we were't testing anything.");
            Debug.Assert(false);
        }
    }
}
