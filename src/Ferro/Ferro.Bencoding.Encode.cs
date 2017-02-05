using System;
using System.IO;

namespace Ferro  {
    public static partial class Bencoding {
        public static byte[] Encode(object value) {
            var stream = new MemoryStream();
            Encode(stream, value);
            return stream.ToArray();
        }

        public static void Encode(Stream stream, object value) {
            // not implemented
            return;
        }

        public class EncodingException : Exception {
            public EncodingException(string message) :
                base(message) {}
            public EncodingException(string message, Exception inner) :
                base(message, inner) {}
        }
    }
}
