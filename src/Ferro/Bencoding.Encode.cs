using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Ferro  {
    public static partial class Bencoding {
        public static ImmutableArray<byte> Encode(object value) {
            using (var stream = new MemoryStream()) {
                Encode(stream, value);
                return stream.ToArray().ToImmutable();
            }
        }

        public static void Encode(Stream stream, object value) {
            if (value is Int64) {
                Encode(stream, (Int64) value);
            } else if (value is IList<byte>) {
                Encode(stream, ((IList<byte>) value).ToImmutable());
            } else if (value is byte[]) {
                // We also accept normal byte arrays as input.
                Encode(stream, ((IList<byte>) value).ToArray().ToImmutable());
            } else if (value is List<object>) {
                Encode(stream, (List<object>) value);
            } else if (value is Dictionary<ImmutableArray<byte>, object>) {
                Encode(stream, (Dictionary<ImmutableArray<byte>, object>) value);
            } else if (value is Dictionary<byte[], object>) {
                var dictionary =
                    ((Dictionary<byte[], object>) value)
                    .Select(kv =>
                        new KeyValuePair<ImmutableArray<byte>, object>(
                            kv.Key.ToImmutable(), kv.Value))
                   .ToDictionary(kv => kv.Key, kv => kv.Value);
                Encode(stream, dictionary);
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

        public static void Encode(Stream stream, ImmutableArray<byte> value) {
            stream.Write(value.Length.ToString().ToASCII());
            stream.WriteByte((byte) ':');
            stream.Write(value.ToArray());
        }

        public static void Encode(Stream stream, List<object> value) {
            stream.WriteByte((byte) 'l');
            foreach (var item in value) {
                Encode(stream, item);
            }
            stream.WriteByte((byte) 'e');
        }

        public static void Encode(Stream stream, Dictionary<ImmutableArray<byte>, object> value) {
            var keys = value.Keys.ToArray();
            Array.Sort(keys);
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
