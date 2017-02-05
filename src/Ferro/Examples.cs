using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ferro
{
    public class Examples
    {
        // Plug in your encoding and decoding functions here.
        static object deserialize(byte[] bytes) {
            return BencodeDeserializer.Deserialize(bytes);
        }
        static byte[] serialize(object value) {
            return BencodeSerializer.Serialize(value);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Let's try some examples!");
            Console.WriteLine();

            test(() => {
                Console.WriteLine("A positive integer");
                var input = Encoding.ASCII.GetBytes("i13e");
                var result = deserialize(input);
                assertEquals(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEquals(13, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A negative integer");
                var input = Encoding.ASCII.GetBytes("i-3153e");
                var result = deserialize(input);
                assertEquals(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEquals(-3153, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("The zero integer");
                var input = Encoding.ASCII.GetBytes("i0e");
                var result = deserialize(input);
                assertEquals(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEquals(0, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A large positive integer");
                var input = Encoding.ASCII.GetBytes("i42897244160e");
                var result = deserialize(input);
                assertEquals(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEquals(42897244160, typedResult);
                assertRoundTrip(input);
            });
            
            test(() => {
                Console.WriteLine("Invalid leading 0s in a positive integer");
                var input = Encoding.ASCII.GetBytes("i05e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid multiple hyphen-minuses in integer");
                var input = Encoding.ASCII.GetBytes("i--33e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid non-initial hyphen-minus in integer");
                var input = Encoding.ASCII.GetBytes("i3-3e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid leading 0s in a negative integer");
                var input = Encoding.ASCII.GetBytes("i-03e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid negative zero integer");
                var input = Encoding.ASCII.GetBytes("i-0e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid empty integer value");
                var input = Encoding.ASCII.GetBytes("ie");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("An integer way too large for us to support (though technically valid)");
                var input = Encoding.ASCII.GetBytes(
                    "i" +
                    "012345678901234567890123456789012345678901234567890123456789" +
                    "012345678901234567890123456789012345678901234567890123456789" +
                    "012345678901234567890123456789012345678901234567890123456789" +
                    "012345678901234567890123456789012345678901234567890123456789" +
                    "e");
                assertThrows(() => deserialize(input));
            });
            
            test(() => {
                Console.WriteLine("An invalid leading character");
                var input = Encoding.ASCII.GetBytes("z0e");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A string");
                var input = Encoding.ASCII.GetBytes("5:hello");
                var result = deserialize(input);
                assertEquals(typeof(byte[]), result.GetType());
                var typedResult = (byte[]) result;
                assertSequenceEqual(Encoding.ASCII.GetBytes("hello"), typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("An empty string");
                var input = Encoding.ASCII.GetBytes("0:");
                var result = deserialize(input);
                assertEquals(typeof(byte[]), result.GetType());
                var typedResult = (byte[]) result;
                assertSequenceEqual(new byte[]{}, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("Invalid leading 0s in string size");
                var input = Encoding.ASCII.GetBytes("05:hello");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid string with length greater than remaining data");
                var input = Encoding.ASCII.GetBytes("50:hello");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("An empty list");
                var input = Encoding.ASCII.GetBytes("le");
                var result = deserialize(input);
                assertEquals(typeof(List<object>), result.GetType());
                var typedResult = (List<object>) result;
                assertEquals(0, typedResult.Count);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A list of three integers");
                var input = Encoding.ASCII.GetBytes("li1ei2ei3ee");
                var result = deserialize(input);
                assertEquals(typeof(List<object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("An empty dictionary");
                var input = Encoding.ASCII.GetBytes("de");
                var result = deserialize(input);
                assertEquals(typeof(Dictionary<byte[], object>), result.GetType());
                var typedResult = (Dictionary<byte[], object>) result;
                assertEquals(0, typedResult.Count);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A dictionary with two integer values");
                var input = Encoding.ASCII.GetBytes("d1:1i2e1:3i4ee");
                var result = deserialize(input);
                assertEquals(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A dictionary with two single-item integer list values");
                var input = Encoding.ASCII.GetBytes("d1:1li2ee1:3li4eee");
                var result = deserialize(input);
                assertEquals(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with two keys and one value");
                var input = Encoding.ASCII.GetBytes("di1ei2ei3ee");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with non-lexiconographically-ordered keys");
                var input = Encoding.ASCII.GetBytes("d1:3i4e1:1i2ee");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with duplicate keys");
                var input = Encoding.ASCII.GetBytes("d1:1i2e1:1i2ee");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("An invalid integer-keyed dictionary");
                var input = Encoding.ASCII.GetBytes("di1ei2ee");
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A pseudo-torrent (munged to fit in ASCII)!");
                var input = Encoding.ASCII.GetBytes(
                    "d8:announce35:udp://tracker.openbittorrent.com:8013:announce-list" +
                    "ll35:udp://tracker.openbittorrent.com:80el33:udp://tracker.opentrackr.org:1337ee" +
                    "4:infod6:lengthi7e4:name7:example12:piece lengthi7e6:pieces20:0I0')s000000v0-0o0?0" + 
                    "4:salt3:200e8:url-listl57:https://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/" + 
                    "56:http://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/ee"
                );
                var result = deserialize(input);
                assertEquals(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            reportResults();
        }

        class AssertionFailedException : Exception {
            public AssertionFailedException(string message) : base(message) {}
            public AssertionFailedException(string message, Exception inner) : base(message, inner) {}
        }
        
        // Specifies a condition that must be true.
        static void assert(bool condition, string message = null) {
            if (!condition) {
                throw new AssertionFailedException(message ?? "Assertion condition is false.");
            }
        }

        static void assertEquals<T>(T expected, T actual) {
            assert(expected.Equals(actual), $"Expected {expected}, got {actual}.");
        }

        static void assertSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null) {
            assert(
                expected.SequenceEqual(actual),
                $"{message}\nExpected [{String.Join(", ", expected.ToArray())}]\nGot      [{String.Join(", ", actual.ToArray())}].");
        }

        // Used to specify an action that must raise a DeserializationException.
        // If it raises a different type of Exception, that's a problem because
        // it means an error isn't being handled properly internally.
        public static void assertThrows(Action f) {
            try {
                f();
            } catch (DeserializationException) {
                return;
            }
            throw new AssertionFailedException("Expected exception, but none was thrown.");
        }

        // Asserts that deserializing and re-serializing the specified bytes
        // doesn't result in any change.
        static void assertRoundTrip(byte[] bytes) {
            return; // TODO: enable when you can
            var roundTripped = serialize(deserialize(bytes));
            assertSequenceEqual(bytes, roundTripped, "Serialization round-trip caused a change.");
        }

        static Int64 testsPassed = 0;
        static Int64 testsFailed = 0;
        static void test(Action f) {
            try {
                f();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Passed");
                Console.ResetColor();
                testsPassed += 1;
            } catch (AssertionFailedException e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine(e.ToString().Trim());
                Console.ResetColor();
                Console.WriteLine();
                testsFailed += 1;
            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Errored");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine(e.ToString().Trim());
                Console.ResetColor();
                Console.WriteLine();
                testsFailed += 1;
            }
            Console.WriteLine();
        }

        static void reportResults() {
            if (testsFailed > 0) {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"{testsFailed} tests failed. :(");
                Console.ResetColor();
            } else if (testsPassed > 0) {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine();
                Console.WriteLine($"Zero tests failed. :)");
                Console.ResetColor();
            }
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{testsPassed} tests passed.");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
