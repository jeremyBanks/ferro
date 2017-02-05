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

        public static object Decode(Stream input) {
            var firstOrNothing = input.ReadByte();
            if (firstOrNothing == -1) {
                throw new DecodingException("Unexpected end of stream.");
            }
            var first = (byte) firstOrNothing;

            switch (first) {
                case (byte) 'i':
                    return 11;
                
                case (byte) '0':
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
                    return Encoding.ASCII.GetBytes("NOT IMPLEMENTED");
                
                case (byte) 'l':
                    return new List<object> {
                        Encoding.ASCII.GetBytes("NOT IMPLEMENTED")
                    };
                
                case (byte) 'd':
                    return new Dictionary<byte[], object> {
                        {
                            Encoding.ASCII.GetBytes("NOT IMPLEMENTED"),
                            Encoding.ASCII.GetBytes("NOT IMPLEMENTED")
                        }
                    };
                
                default:
                    throw new DecodingException(
                        $"Unexpected initial byte in value ${first}.");
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
