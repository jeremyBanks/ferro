using System;
using System.IO;

namespace Ferro  {
    public static partial class Bencoding {
        public static object Decode(byte[] bytes) {
            return Decode(new MemoryStream(bytes));
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
