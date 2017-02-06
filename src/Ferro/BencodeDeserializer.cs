using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ferro
{
    public class BencodeDeserializer
    {
        public static object Deserialize(byte[] bytes) {
            // declare encodings for each type of delimiter
            var intBeginDelimiter = "i".ToASCII();
            var intEndDelimiter = "e".ToASCII();

            var output = new MemoryStream();

            return output.ToArray();
        }
    }

    public class DeserializationException : Exception {}
}
