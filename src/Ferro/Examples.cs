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
        // Plug in your encoding and decoding functions here.
        object deserialize(byte[] bytes) {
            return BencodeDeserializer.Deserialize(bytes);
        }
        byte[] serialize(object value) {
            return BencodeDeserializer.Serialize(value);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Let's try some simple deserialization examples!");
            
            {
                Console.WriteLine("A positive integer");
                var input = Encoding.ASCII.GetBytes("i13e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(13 == typedResult);
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("A negative integer");
                var input = Encoding.ASCII.GetBytes("i-3153e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(-3153 == typedResult);
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("The zero integer");
                var input = Encoding.ASCII.GetBytes("i0e");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(0 == typedResult);
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("A large positive integer");
                var input = Encoding.ASCII.GetBytes("i42897244160");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Int64) == result.GetType());
                var typedResult = (Int64) result;
                assert(42897244160 == typedResult);
                assertRoundTrip(input);
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
                Console.WriteLine("An integer way too large for us to support (though technically valid)");
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
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("An empty string");
                var input = Encoding.ASCII.GetBytes("0:");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(string) == result.GetType());
                var typedResult = (string) result;
                assert("".Equals(typedResult));
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("Invalid leading 0s in string size");
                var input = Encoding.ASCII.GetBytes("05:hello");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
            }

            {
                Console.WriteLine("An empty list");
                var input = Encoding.ASCII.GetBytes("le");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(List<object>) == result.GetType());
                var typedResult = (List<object>) result;
                assert(0 == typedResult.Count);
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("A list of three integers");
                var input = Encoding.ASCII.GetBytes("li1ei2ei3ee");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(List<object>) == result.GetType());
                var typedResult = (List<object>) result;
                assert(typedResult.SequenceEqual(new List<object> {1, 2, 3}));
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("An empty dictionary");
                var input = Encoding.ASCII.GetBytes("de");
                var result = BencodeDeserializer.Deserialize(input);
                assert(typeof(Dictionary<byte[], object>) == result.GetType());
                var typedResult = (Dictionary<byte[], object>) result;
                assert(0 == typedResult.Count);
                assertRoundTrip(input);
            }

            {
                Console.WriteLine("An invalid integer-keyed dictionary");
                var input = Encoding.ASCII.GetBytes("di1ei2ee");
                assertThrows(() => BencodeDeserializer.Deserialize(input));
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

        // Asserts that deserializing and re-serializing the specified bytes
        // doesn't result in any change.
        protected static void assertRoundTrip(byte[] bytes) {
            // NOT IMPLEMENTED
            // TODO: Implement this once serializer is complete.
            // assert(bytes == BencodeSerializer.Serialize(BencodeDeserializer.Deserialize(bytes)));
        }
    }
}
