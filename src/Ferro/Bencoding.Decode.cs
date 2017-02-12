using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Ferro  {
    public static partial class Bencoding {
        public static object Decode(IList<byte> bytes) {
            using (var stream = new MemoryStream(bytes.ToArray())) {
                var value = Decode(stream);
                if (stream.Position < stream.Length) {
                    throw new DecodingException("Unexpected data after input.");
                }
                return value;
            }
        }
        public static object DecodeFirst(IList<byte> bytes, out Int64 count) {
            using (var stream = new MemoryStream(bytes.ToArray())) {
                var value = Decode(stream);
                count = stream.Position;
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

                        if (valueDigits.Count == 0 && next == '0') {
                            // Leading zeros are not allowed, so this must be i0e.
                            var endOrNothing = stream.ReadByte();
                            if (endOrNothing == -1) {
                                throw new DecodingException("Unexpected end of stream while parsing zero integer.");
                            }
                            var end = (byte) endOrNothing;
                            if (end != 'e') {
                                throw new DecodingException($"Expected 'e' after 'i0', got: {end} '{char.ConvertFromUtf32(end)}'.");
                            }
                            return (Int64) 0;
                        }

                        if (valueDigits.Count == 1 && next == '0' && valueDigits[0] == '-') {
                            // Negative zero and leading zeros are not allowed.
                            throw new DecodingException("Unexpected '-0' parsing integer.");
                        }

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
                                $"Expected ASCII digit while parsing integer, got: {next} '{char.ConvertFromUtf32(next)}'.");
                        }

                        valueDigits.Add(next);
                    }

                    var valueDigitsString = valueDigits.ToArray().FromASCII();
                    try {
                        return Int64.Parse(valueDigitsString);
                    } catch (System.OverflowException exception) {
                        throw new DecodingException(
                            $"Integer out of supported 64-bit bounds: {valueDigitsString}", exception);
                    }
                
                case (byte) '0':
                    // Must be the empty string.
                    var secondOrNothing = stream.ReadByte();
                    if (secondOrNothing == -1) {
                        throw new DecodingException("Unexpected end of stream while parsing empty string.");
                    }
                    var second = (byte) secondOrNothing;
                    if (second != ':') {
                        throw new DecodingException($"Expected ':' after leading '0', got: {second} '{char.ConvertFromUtf32(second)}'.");
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

                    while (true) {
                        var nextOrNothing = stream.ReadByte();
                        if (nextOrNothing == -1) {
                            throw new DecodingException(
                                "Unexpected end of stream while parsing length.");
                        }
                        var next = (byte) nextOrNothing;

                        if (next == ':') {
                            break; // valid end of length
                        }

                        if (next == '-') {
                            if (lengthDigits.Count > 0) {
                                throw new DecodingException(
                                    "Unexpected hyphen-minus after first character of integer length.");
                            }
                        } else if (!('0' <= next && next <= '9')) {
                            throw new DecodingException(
                                $"Expected ASCII digit while parsing length, got: {next}");
                        }

                        lengthDigits.Add(next);
                    }

                    var lengthDigitsString = lengthDigits.ToArray().FromASCII();
                    Int32 length;
                    try {
                        length = Int32.Parse(lengthDigitsString);
                    } catch (System.OverflowException exception) {
                        throw new DecodingException(
                            $"Length out of supported 32-bit bounds: {lengthDigitsString}", exception);
                    }

                    var buffer = new byte[length];
                    var read = stream.Read(buffer, 0, (int) length);
                    if (read < length) {
                        throw new DecodingException(
                            "Unexpected end of stream while reading string content.");
                    }
                    return ImmutableArray.Create<byte>(buffer);
                
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
                    var dictionary = new Dictionary<ImmutableArray<byte>, object> {};
                    var hasPreviousKey = false;
                    ImmutableArray<byte> previousKey;

                    while (true) {
                        var key = Decode(stream, nullForCollectionEnd: true);
                        if (key == null) {
                            break; // valid end of dictionary
                        } else if (!(key is ImmutableArray<byte>)) {
                            throw new DecodingException(
                                $"Expected ImmutableArray<byte> for dictionary key, got: {key.GetType()}");
                        }
                        var typedKey = (ImmutableArray<byte>) key;
                        if (hasPreviousKey) {
                            if (!ByteListComparer<ImmutableArray<byte>>.Ascending(previousKey, typedKey)) {
                                throw new DecodingException(
                                    "Dictionary key was in the wrong order or duplicated.");
                            }
                        }
                        previousKey = typedKey;
                        hasPreviousKey = true;
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
                        $"Unexpected initial byte in value: {first} '{char.ConvertFromUtf32(first)}'.");
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
