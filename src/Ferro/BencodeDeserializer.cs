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

            try
            {
                return Int64.Parse(output.ToArray().FromASCII());
            }
            catch(OverflowException e)
            {
                throw new DeserializationException("Gigantic Integers are unsupported.", e);
            }
            
        }

        private static byte[] StringDeserialize(byte[] bytes)
        {
            var output = new MemoryStream();
            var numStore = new List<byte>();
            byte[] byteArray = bytes;
            
            foreach (var item in byteArray)
            {
                if (item == (byte) ':')
                {
                    byteArray = byteArray.Skip(1).ToArray();
                    break;
                }
                else
                {
                    numStore.Add(item);
                    byteArray = byteArray.Skip(1).ToArray();
                }
            }

            Int32 length;

            try
            {
                length = Int32.Parse(numStore.ToArray().FromASCII());
            }
            catch(OverflowException e)
            {
                throw new DeserializationException("The String length value is too large", e);
            }

            if (length < 0)
            {
                throw new DeserializationException("Strings cannot have a negative length.");
            }

            return byteArray.Take(length).ToArray();
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
                else if ((byte) '0' <= item && (byte) '9' >= item)
                {
                    return StringDeserialize(bytes);
                }
            }

            return output.ToArray().FromASCII();
        }
    }

    public class DeserializationException : Exception {
        public DeserializationException(string message) : 
            base(message) { }
        public DeserializationException(string message, Exception inner) : 
            base(message, inner) { }
    }
}
