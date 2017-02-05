using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ferro  {
    public static partial class Bencoding {
        public static object Decode(byte[] bytes) {
            using (var stream = new MemoryStream(bytes)) {
                var value = Decode(stream);
                if (stream.Position < stream.Length) {
                    throw new DecodingException("Unexpected data after input.");
                }
                return value;
            }
        }

        public static object Decode(Stream stream, bool nullForCollectionEnd = false) {
            var firstOrNothing = stream.ReadByte();
            if (firstOrNothing == -1) {
                throw new DecodingException("Unexpected end of stream while expecting next value.");
            }
            var first = (byte) firstOrNothing;

            switch (first) {
                case (byte) 'i':
                    var valueDigits = new List<byte>{};

                    while (true) {
                        var nextOrNothing = stream.ReadByte();
                        if (nextOrNothing == -1) {
                            throw new DecodingException(
                                "Unexpected end of stream while parsing integer.");
                        }
                        var next = (byte) nextOrNothing;

                        if (next == 'e') {
                            if (valueDigits.Count == 0) {
                                throw new DecodingException(
                                    "Unexpected 'e' before any digits while parsing integer.");
                            } else {
                                break; // valid end of integer
                            }
                        }

                        if (next == '-') {
                            if (valueDigits.Count > 0) {
                                throw new DecodingException(
                                    "Unexpected hyphen-minus after first character of integer value.");
                            }
                        } else if (!('0' <= next && next <= '9')) {
                            throw new DecodingException(
                                $"Expected ASCII digit while parsing integer, got: {next}");
                        }

                        valueDigits.Add(next);
                    }

                    var valueDigitsString = Encoding.ASCII.GetString(valueDigits.ToArray());
                    return Int64.Parse(valueDigitsString);
                
                case (byte) '0':
                    // Must be the empty string.
                    var secondOrNothing = stream.ReadByte();
                    if (secondOrNothing == -1) {
                        throw new DecodingException("Unexpected end of stream while parsing empty string.");
                    }
                    var second = (byte) secondOrNothing;
                    if (second != ':') {
                        throw new DecodingException($"Expected ':' after leading '0', got: {second}");
                    }
                    return new byte[]{};

                case (byte) '1':
                case (byte) '2':
                case (byte) '3':
                case (byte) '4':
                case (byte) '5':
                case (byte) '6':
                case (byte) '7':
                case (byte) '8':
                case (byte) '9':
                    var lengthDigits = new List<byte>{ first };
                    return Encoding.ASCII.GetBytes("NOT IMPLEMENTED");
                
                case (byte) 'l':
                    var list = new List<object> {};
                    while (true) {
                        var next = Decode(stream, nullForCollectionEnd: true);
                        if (next == null) {
                            break; // valid end of list
                        }
                        list.Add(next);
                    }
                    return list;
                
                case (byte) 'd':
                    var dictionary = new Dictionary<byte[], object> {};
                    while (true) {
                        var key = Decode(stream, nullForCollectionEnd: true);
                        if (key == null) {
                            break; // valid end of dictionary
                        } else if (!(key is byte[])) {
                            throw new DecodingException(
                                $"Expected byte string for dictionary key, got: {key.GetType()}");
                        }
                        var typedKey = (byte[]) key;
                        var value = Decode(stream);
                        dictionary.Add(typedKey, value);

                    }
                    return dictionary;

                case (byte) 'e':
                    if (nullForCollectionEnd) {
                        return null; // valid expected end of collection
                    } else {
                        throw new DecodingException(
                            $"Found end-of-collection indicator 'e' while expecting value.");
                    }
                
                default:
                    throw new DecodingException(
                        $"Unexpected initial byte in value: {first}.");
            }
        }

        public class DecodingException : Exception {
            public DecodingException(string message) :
                base(message) {}
            public DecodingException(string message, Exception inner) :
                base(message, inner) {}
        }
    }
}
