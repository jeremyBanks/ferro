using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ferro  {
    public static partial class Bencoding {
        public static byte[] Encode(object value) {
            using (var stream = new MemoryStream()) {
                Encode(stream, value);
                return stream.ToArray();
            }
        }

        public static void Encode(Stream stream, object value) {
            if (value is Int64) {
                Encode(stream, (Int64) value);
            } else if (value is byte[]) {
                Encode(stream, (byte[]) value);
            } else if (value is List<object>) {
                Encode(stream, (List<object>) value);
            } else if (value is Dictionary<byte[], object>) {
                Encode(stream, (Dictionary<byte[], object>) value);
            } else if (value is Int64) {
                Encode(stream, (Int64) value);
            } else {
                throw new EncodingException(
                    $"Cannot encode value of type ${value.GetType()}.");
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
            foreach (var key in keys) {
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
