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
                Console.WriteLine("A positive integer");
                var input = Encoding.ASCII.GetBytes("i13e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(13 == typedResult);
            }

            {
                Console.WriteLine("A negative integer");
                var input = Encoding.ASCII.GetBytes("i-3153e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(-3153 == typedResult);
            }

            {
                Console.WriteLine("The zero integer");
                var input = Encoding.ASCII.GetBytes("i0e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(0 == typedResult);
            }

            {
                Console.WriteLine("A large positive integer");
                var input = Encoding.ASCII.GetBytes("i42897244160");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(42897244160 == typedResult);
            }
            
            {
                Console.WriteLine("Invalid leading 0s in an integer");
                var input = Encoding.ASCII.GetBytes("i-01e");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
            }

            {
                Console.WriteLine("Invalid negative zero integer");
                var input = Encoding.ASCII.GetBytes("i-0e");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
            }

            {
                Console.WriteLine("An integer way too large for us to handle");
                var input = Encoding.ASCII.GetBytes(
                    "i" +
                    "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF" +
                    "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF" +
                    "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF" +
                    "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF" +
                    "e");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
            }
            
            {
                Console.WriteLine("An invalid leading character");
                var input = Encoding.ASCII.GetBytes("z0e");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
            }

            {
                Console.WriteLine("A string");
                var input = Encoding.ASCII.GetBytes("5:hello");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(string) == result.GetType());
                var typedResult = (string) result;
                assert("hello".Equals(typedResult));
            }

            {
                Console.WriteLine("An empty list");
                var input = Encoding.ASCII.GetBytes("le");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(List<object>) == result.GetType());
                var typedResult = (List<object>) result;
                assert(0 == typedResult.Count);
            }

            {
                Console.WriteLine("An empty dictionary");
                var input = Encoding.ASCII.GetBytes("le");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Dictionary<byte[], object>) == result.GetType());
                var typedResult = (Dictionary<byte[], object>) result;
                assert(0 == typedResult.Count);
            }
        }
        
        // Specifies a condition that must be true.
        protected static void assert(bool condition) {
            if (!condition) {
                throw new Exception("Assertion failed.");
            }
        }

        // Used to specify an action that must raise a DeserializationException.
        // If it raises a different type of Exception, that's a problem because
        // it means an error isn't being handled properly internally.
        protected static void assertThrows(Action f) {
            try {
                f();
            } catch (DeserializationException) {
                return;
            }
            throw new Exception("Expected exception, but none was thrown.");
        }
    }
}
