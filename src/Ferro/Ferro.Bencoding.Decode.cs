using System;
using System.IO;

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

        public static object Decode(Stream input) {
            // not implemented
            return null;
        }

        public class DecodingException : Exception {
            public DecodingException(string message) :
                base(message) {}
            public DecodingException(string message, Exception inner) :
                base(message, inner) {}
        }
    }
}
