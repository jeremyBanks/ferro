using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Ditto.Common;
using Microsoft.Extensions.Logging;

namespace Ditto  {
    public static partial class Bencoding {
        public static byte[] Encode(object value) {
            using (var stream = new MemoryStream()) {
                Encode(stream, value);
                return stream.ToArray();
            }
        }

        public static void Encode(Stream stream, object value) {
            switch (value) {
                case Int64 x:
                    logger.LogWarning("Using deprecated support for Int64 in Ditto.Bencoding.Encode.");
                    Encode(stream, (BigInteger) x);
                    break;
                case BigInteger x:
                    Encode(stream, x);
                    break;
                case byte[] x:
                    Encode(stream, x);
                    break;
                case List<object> x:
                    Encode(stream, x);
                    break;
                case Dictionary<byte[], object> x:
                    Encode(stream, x);
                    break;
                default:
                    throw new EncodingException(
                        $"Cannot encode value of unexpected type {value.GetType()}.");
            }
        }

        public static void Encode(Stream stream, Int64 value) {
            stream.WriteByte((byte) 'i');
            stream.Write(value.ToString().ToASCII());
            stream.WriteByte((byte) 'e');
        }

        public static void Encode(Stream stream, byte[] value) {
            stream.Write(value.Length.ToString().ToASCII());
            stream.WriteByte((byte) ':');
            stream.Write(value);
        }

        public static void Encode(Stream stream, List<object> value) {
            stream.WriteByte((byte) 'l');
            foreach (var item in value) {
                Encode(stream, item);
            }
            stream.WriteByte((byte) 'e');
        }

        public static void Encode(Stream stream, Dictionary<byte[], object> value) {
            var keys = value.Keys.ToArray();
            Array.Sort(keys, ByteArrayComparer.Instance);
            stream.WriteByte((byte) 'd');
            byte[] previousKey = null;
            foreach (var key in keys) {
                if (ByteArrayComparer.Instance.Equals(previousKey, key)) {
                    // Because it's sorted, duplicate keys will be consecutive.
                    throw new EncodingException(
                        "Unexpected duplicate key when encoding dictionary");
                }
                previousKey = key;

                Encode(stream, key);
                Encode(stream, value[key]);
            }
            stream.WriteByte((byte) 'e');
        }

        public class EncodingException : Exception {
            public EncodingException(string message) :
                base(message) {}
            public EncodingException(string message, Exception inner) :
                base(message, inner) {}
        }
    }
}
