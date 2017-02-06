using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Ferro
{
    public class BencodeDeserializer
    {
        // DECLARE DELIMITERS
        private static byte endDelimiter = "e".ToASCII()[0];
        private static byte intBeginDelimiter = "i".ToASCII()[0];
        
        private static Int64 IntDeserialize(byte[] bytes)
        {
            var output = new MemoryStream();
            foreach (var item in bytes)
            {
                if (item != endDelimiter)
                {
                    output.WriteByte(item);
                }
            }

            return Int64.Parse(output.ToArray().FromASCII());
        }

        public static object Deserialize(byte[] bytes)
        {
            var output = new MemoryStream();
            foreach (var item in bytes)
            {
                if (item == intBeginDelimiter)
                {
                   return IntDeserialize(bytes.Skip(1).ToArray());
                }
            }

            return output.ToArray().FromASCII();
        }
    }

    public class DeserializationException : Exception {}
}
