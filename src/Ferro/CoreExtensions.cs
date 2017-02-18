using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ferro {
    // Extensions methods on core types that we use internally.
    public static class CoreExtensions {
        public static void Write(this Stream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static ImmutableArray<byte> ToImmutable(this IList<byte> bytes) {
            return ImmutableArray.Create<byte>(bytes.ToArray());
        }

        // Produces a human developer-friendly respresentation of the bytes.
        public static string ToHuman(this IList<byte> bytes) {
            var result = new StringBuilder(bytes.Count);

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
        public static string FromASCII(this IList<byte> bytes) {
            return Encoding.ASCII.GetString(bytes.ToArray());
        }

        // Encodes a string to bytes as UTF-8.
        public static byte[] ToUTF8(this string s) {
            return Encoding.UTF8.GetBytes(s);
        }

        // Decodes bytes from a string as UTF-8.
        public static string FromUTF8(this IList<byte> bytes) {
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        // Returns the SHA-1 hash digest of these bytes.
        public static byte[] Sha1(this IList<byte> bytes) {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(bytes.ToArray());
            }
        }

        // Converts bytes to their lowercase hexadecimal representation.
        public static string ToHex(this IList<byte> bytes) {
            return BitConverter.ToString(bytes.ToArray()).Replace("-", "").ToLower();
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
    }
}
