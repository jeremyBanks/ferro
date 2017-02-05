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
            return Bencoding.Decode(bytes);
        }
        static byte[] serialize(object value) {
            return Bencoding.Encode(value);
        }

        public static int Main(string[] args)
        {
            Console.WriteLine("Let's try some examples!");
            Console.WriteLine();

            test(() => {
                Console.WriteLine("A positive integer from C#");
                Int64 value = 12345;
                var encoded = serialize(value);
                assertSequencesEqual("i12345e".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("A positive integer");
                var input = "i13e".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEqual(13, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A negative integer");
                var input = "i-3153e".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEqual(-3153, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A negative integer from C#");
                Int64 value = -9876;
                var encoded = serialize(value);
                assertSequencesEqual("i-9876e".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("The zero integer");
                var input = "i0e".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEqual(0, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A large positive integer");
                var input = "i42897244160e".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Int64), result.GetType());
                var typedResult = (Int64) result;
                assertEqual(42897244160, typedResult);
                assertRoundTrip(input);
            });
            
            test(() => {
                Console.WriteLine("Invalid leading 0s in a positive integer");
                var input = "i05e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid multiple hyphen-minuses in integer");
                var input = "i--33e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid non-initial hyphen-minus in integer");
                var input = "i3-3e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid leading 0s in a negative integer");
                var input = "i-03e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid negative zero integer");
                var input = "i-0e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid empty integer value");
                var input = "ie".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("An integer way too large for us to support (though technically valid)");
                var input = (
                    "i" +
                    "123456789012345678901234567890123456789012345678901234567890" +
                    "123456789012345678901234567890123456789012345678901234567890" +
                    "123456789012345678901234567890123456789012345678901234567890" +
                    "123456789012345678901234567890123456789012345678901234567890" +
                    "e").ToASCII();
                assertThrows(() => deserialize(input));
            });
            
            test(() => {
                Console.WriteLine("An invalid leading character");
                var input = "z0e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A string");
                var input = "5:hello".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(byte[]), result.GetType());
                var typedResult = (byte[]) result;
                assertSequencesEqual("hello".ToASCII(), typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A byte string from C#");
                var value = "hello world".ToASCII();
                var encoded = serialize(value);
                assertSequencesEqual("11:hello world".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("An empty string");
                var input = "0:".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(byte[]), result.GetType());
                var typedResult = (byte[]) result;
                assertSequencesEqual(new byte[]{}, typedResult);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("Invalid leading 0s in string size");
                var input = "05:hello".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid string with negative length");
                var input = "-5:hello".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid string with length greater than remaining data");
                var input = "50:hello".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("Invalid string with length greater than ever possible");
                var input =
                    "987654332198765433219876543321987654332198765433219876543321:hello".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("An empty list");
                var input = "le".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(List<object>), result.GetType());
                var typedResult = (List<object>) result;
                assertEqual(0, typedResult.Count);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A list of three integers");
                var input = "li1ei2ei3ee".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(List<object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A list of strings and integers from C#");
                var value = new List<object> { (Int64) 1234, "hello".ToASCII(), (Int64) (-5678), "world".ToASCII() };
                var encoded = serialize(value);
                assertSequencesEqual("li1234e5:helloi-5678e5:worlde".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("An empty dictionary");
                var input = "de".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Dictionary<byte[], object>), result.GetType());
                var typedResult = (Dictionary<byte[], object>) result;
                assertEqual(0, typedResult.Count);
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A dictionary with two integer values");
                var input = "d1:1i2e1:3i4ee".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A dictionary of integers from C#");
                var value = new Dictionary<byte[], object> {
                    {"hello".ToASCII(), (Int64) 1234},
                    {"world".ToASCII(), (Int64) (-5678)}
                };
                var encoded = serialize(value);
                assertSequencesEqual("d5:helloi1234e5:worldi-5678ee".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("A dictionary with two single-item integer list values");
                var input = "d1:1li2ee1:3li4eee".ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            test(() => {
                Console.WriteLine("A dictionary containing a list containing a dictionary from C#");
                var value = new Dictionary<byte[], object> {
                    {
                        "hello".ToASCII(),
                        new List<object> {
                            new Dictionary<byte[], object> {
                                {"world".ToASCII(), (Int64) 102436}
                            }
                        }
                    }
                };
                var encoded = serialize(value);
                Console.WriteLine(encoded.FromASCII());
                assertSequencesEqual("d5:hellold5:worldi102436eeee".ToASCII(), encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with two keys and one value");
                var input = "d1:1i2e1:3e".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with non-lexiconographically-ordered keys");
                var input = "d1:3i4e1:1i2ee".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A invalid dictionary with duplicate keys");
                var input = "d1:1i2e1:1i2ee".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A dictionary from C# whose keys are initially disordered");
                var value = new Dictionary<byte[], object> {
                    {"zzz".ToASCII(), "last".ToASCII()},
                    {"zz".ToASCII(), "dictionary".ToASCII()},
                    {"zza".ToASCII(), "whose".ToASCII()},
                    {"".ToASCII(), "disordered".ToASCII()},
                    {"a".ToASCII(), "with".ToASCII()},
                    {"az".ToASCII(), "keys".ToASCII()},
                    {"azb".ToASCII(), "invalid".ToASCII()},
                    {"aza".ToASCII(), "two".ToASCII()}
                };
                var encoded = serialize(value);
                assertSequencesEqual(
                    ("d0:10:disordered1:a4:with2:az4:keys3:aza3:two3:azb" + 
                        "7:invalid2:zz10:dictionary3:zza5:whose3:zzz4:laste").ToASCII(),
                    encoded);
                assertRoundTrip(encoded);
            });

            test(() => {
                Console.WriteLine("An invalid integer-keyed dictionary");
                var input = "di1ei2ee".ToASCII();
                assertThrows(() => deserialize(input));
            });

            test(() => {
                Console.WriteLine("A pseudo-torrent (munged to fit in ASCII)!");
                var input = (
                    "d8:announce35:udp://tracker.openbittorrent.com:8013:announce-list" +
                    "ll35:udp://tracker.openbittorrent.com:80el33:udp://tracker.opentrackr.org:1337ee" +
                    "4:infod6:lengthi7e4:name7:example12:piece lengthi7e6:pieces20:0I0')s000000v0-0o0?0" + 
                    "4:salt3:200e8:url-listl57:https://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/" + 
                    "56:http://mgnt.ca/123456fc77d23aca05a8b58066bb55fe06c72f8e/ee").ToASCII();
                var result = deserialize(input);
                assertEqual(typeof(Dictionary<byte[], object>), result.GetType());
                assertRoundTrip(input);
            });

            return reportResults();
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

        static void assertEqual<T>(T expected, T actual) {
            assert(expected.Equals(actual), $"Expected {expected}, got {actual}.");
        }

        static void assertSequencesEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message = null) {
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
            } catch (DeserializationException error) {
                Console.WriteLine($"Expected exception and got one: {error.GetType()}: {error.Message}");
                return;
            } catch (Bencoding.DecodingException error) {
                Console.WriteLine($"Expected exception and got one: {error.GetType()}: {error.Message}");
                return;
            } catch (Bencoding.EncodingException error) {
                Console.WriteLine($"Expected exception and got one: {error.GetType()}: {error.Message}");
                return;
            } catch (Bencoding.DecodingException) {
                return;
            } catch (Bencoding.EncodingException) {
                return;
            }
            throw new AssertionFailedException("Expected exception, but none was thrown.");
        }

        // Asserts that deserializing and re-serializing the specified bytes
        // doesn't result in any change.
        static void assertRoundTrip(byte[] bytes) {
            var roundTripped = serialize(deserialize(bytes));
            assertSequencesEqual(bytes, roundTripped, "Serialization round-trip caused a change.");
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

        static int reportResults() {
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
            return (testsFailed > 0) ? 1 : 0;
        }
    }
}
