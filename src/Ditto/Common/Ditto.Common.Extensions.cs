using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ditto.Common {
    public static class StreamExtensions {
        public static void Write(this Stream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static byte[] ReadBytes(this Stream stream, Int32 length) {
            var buffer = new byte[length];
            var total = 0;
            while (total < length) {
                var remaining = length - total;
                var pieceSize = stream.Read(buffer, total, remaining);
                if (pieceSize == 0) {
                    throw new Exception($"Unexpected end of stream reading {length} bytes.");
                }
                total += pieceSize;
            }
            return buffer;
        }

        public static void WriteBytes(this Stream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    
    public static class HashSetExtensions {
        private static Random random = new Random();

        // May not be thread-safe?
        public static T PopRandom<T>(this HashSet<T> hashSet) {
            var value = hashSet.ElementAt(random.Next(hashSet.Count));
            hashSet.Remove(value);
            return value;
        }
    }

    public static class ByteArrayExtensions {
        // Returns a copy of the array from start index (inclusive) to end index (exclusive).
        // Negative indicies are treated as offsets from the end of the string.
        // This should match the behaviour of the JavaScript Array..slice() method.
        public static byte[] Slice(this byte[] bytes, Int32? start = null, Int32? end = null) {
            var realEnd = end != null ? (Int32) end : bytes.Length;
            var realStart = start != null ? (Int32) start : 0;

            // Wrap negative indicies once.
            if (realStart < 0) {
                realStart = bytes.Length + realStart;
            }
            if (realEnd < 0) {
                realEnd = bytes.Length + realEnd;
            }

            // Clamp to array bounds.
            if (realStart < 0) {
                realStart = 0;
            }
            if (realEnd < 0) {
                realEnd = 0;
            }
            if (realStart > bytes.Length) {
                realStart = bytes.Length;
            }
            if (realEnd > bytes.Length) {
                realEnd = bytes.Length;
            }

            if (realEnd > realStart) {
                var result = new byte[realEnd - realStart];
                Array.Copy(bytes, realStart, result, 0, realEnd - realStart);
                return result;
            } else {
                return new byte[0];
            }
        }

        // Produces a human developer-friendly respresentation of the bytes.
        public static string ToHuman(this byte[] bytes) {
            var result = new StringBuilder(bytes.Length);

            foreach (var b in bytes) {
                if (' ' <= b && b <= '~' && b != '\\') {
                    result.Append(char.ConvertFromUtf32(b));
                } else {
                    result.AppendFormat("\\{0:x2}", b);
                }
            }

            return result.ToString();
        }

        // Encodes a string to bytes as ASCII.
        public static byte[] ToASCII(this string s) {
            return Encoding.ASCII.GetBytes(s);
        }

        // Decodes bytes from a string as ASCII.
        public static string FromASCII(this byte[] bytes) {
            return Encoding.ASCII.GetString(bytes);
        }

        // Encodes a string to bytes as UTF-8.
        public static byte[] ToUTF8(this string s) {
            return Encoding.UTF8.GetBytes(s);
        }

        // Decodes bytes from a string as UTF-8.
        public static string FromUTF8(this byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        // Returns the SHA-1 hash digest of these bytes.
        public static byte[] Sha1(this byte[] bytes) {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(bytes);
            }
        }

        // Converts bytes to their lowercase hexadecimal representation.
        public static string ToHex(this byte[] bytes) {
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // Converts a hexadecimal string into bytes
        public static byte[] FromHex (this string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new ArgumentException("Hexadecimal input must have an even number of members");
            }

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        // Fills the array with secure random data.
        public static byte[] FillRandom(this byte[] bytes)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        public static Int32 Decode32BitInteger(this byte[] bytes)
        {
            if (bytes.Length != 4) {
                throw new Exception($"bytes must have length 4, is {bytes.Length}");
            }
            return (
                (bytes[0] << (8 * 3)) |
                (bytes[1] << (8 * 2)) | 
                (bytes[2] << (8 * 1)) | 
                (bytes[3] << (8 * 0)));
        }

        public static byte[] EncodeBytes(this Int32 number) {
            return new byte[4] {
                (byte) ((number >> (8 * 3)) & 0xFF),
                (byte) ((number >> (8 * 2)) & 0xFF),
                (byte) ((number >> (8 * 1)) & 0xFF),
                (byte) ((number >> (8 * 0)) & 0xFF)
            };
        }

        public static Int32 Decode16BitInteger(this byte[] bytes)
        {
            if (bytes.Length != 2) {
                throw new Exception($"bytes must have length 2, is {bytes.Length}");
            }
            return (
                (((Int32) bytes[0]) << (8 * 1)) | 
                (((Int32) bytes[1]) << (8 * 0)));
        }

        public static byte[] EncodeBytes(this Int16 number) {
            return new byte[2] {
                (byte) ((number >> (8 * 1)) & 0xFF),
                (byte) ((number >> (8 * 0)) & 0xFF)
            };
        }

        public static dynamic Get(this Dictionary<byte[], object> bDict, string key) {
            return bDict[key.ToASCII()];
        }

        public static Dictionary<byte[], object> GetDict(this Dictionary<byte[], object> bDict, string key) {
            return (Dictionary<byte[], object>) bDict[key.ToASCII()];
        }
    }

    public static class BencodedDictExtensions {
        public static Int64 GetInt64(this Dictionary<byte[], object> bDict, string key) {
            return (Int64) bDict[key.ToASCII()];
        }

        public static BigInteger GetInt(this Dictionary<byte[], object> bDict, string key) {
            return (BigInteger) bDict[key.ToASCII()];
        }

        public static byte[] GetBytes(this Dictionary<byte[], object> bDict, string key) {
            return (byte[]) bDict[key.ToASCII()];
        }

        public static string GetString(this Dictionary<byte[], object> bDict, string key) {
            return bDict.GetBytes(key).FromUTF8();
        }

        public static List<object> GetList(this Dictionary<byte[], object> bDict, string key) {
            return (List<object>) bDict[key.ToASCII()];
        }

        public static bool ContainsKey(this Dictionary<byte[], object> bDict, string key) {
            return bDict.ContainsKey(key.ToASCII());
        }

        public static void Set(this Dictionary<byte[], object> bDict, string key, string value) {
            bDict[key.ToASCII()] = value.ToASCII();
        }

        public static void Set(this Dictionary<byte[], object> bDict, string key, Int32 value) {
            bDict[key.ToASCII()] = (BigInteger) value;
        }

        public static void Set(this Dictionary<byte[], object> bDict, string key, object value) {
            bDict[key.ToASCII()] = value;
        }
    }

    public static class TaskExtensions {
        // http://stackoverflow.com/a/29319061/1114
        public static void DoNotAwait(this Task task) { }
    }

    public static class KeyValuePairExtensions {
        public static void Deconstruct<T, U>(this KeyValuePair<T, U> pair, out T key, out U value) {
            key = pair.Key;
            value = pair.Value;
        }
    }
}
