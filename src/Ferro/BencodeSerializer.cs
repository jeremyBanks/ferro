using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferro
{
    /// <summary>
    /// <para>Helper class, containing methods to serialize data into 
    /// Bencode format</para>
    /// </summary>
    // I'm going to want to change the name of this eventually
    public static class BencodeSerializer
    {
        public static byte[] Serialize(object value) {
            // TODO: make this a switch when upgrading to C# 7.0
            // https://blogs.msdn.microsoft.com/dotnet/2016/08/24/whats-new-in-csharp-7-0/
            if (value is Int64) {
                return Serialize((Int64) value);
            } else if (value is byte[]) {
                return Serialize((byte[]) value);
            } else if (value is List<object>) {
                return Serialize((List<object>) value);
            } else if (value is Dictionary<byte[], object>) {
                return Serialize((Dictionary<byte[], object>) value);
            } else if (value is Int64) {
                return Serialize((Int64) value);
            } else {
                throw new Exception("Cannot Serialize value of that type!");
            }
        }

        public static byte[] Serialize(byte[] byteArray)
        {
            var output = new MemoryStream();
            output.Write(Encoding.ASCII.GetBytes(byteArray.Length.ToString()));
            output.Write(Encoding.ASCII.GetBytes(":"));
            output.Write(Encoding.ASCII.GetBytes(byteArray.ToString()));
            return output.ToArray();   
        }

        public static byte[] Serialize(Int64 integer)
        {
           var output = new MemoryStream();
           output.Write(Encoding.ASCII.GetBytes("i"));
           output.Write(Encoding.ASCII.GetBytes(integer.ToString()));
           output.Write(Encoding.ASCII.GetBytes("e"));
           return output.ToArray();
        }

        public static byte[] Serialize(List<object> list)
        {
            var output = new MemoryStream();
            output.Write(Encoding.ASCII.GetBytes("l"));
            foreach (byte[] item in list)
            {
                output.Write(BencodeSerializer.Serialize(item));
            }
            output.Write(Encoding.ASCII.GetBytes("e"));
            return output.ToArray();
        }

        public static byte[] Serialize(Dictionary<byte[], object> dict)
        {
            var output = new MemoryStream();
            output.Write(Encoding.ASCII.GetBytes("d"));
            foreach(var pair in dict)
            {
                output.Write(BencodeSerializer.Serialize(pair.Key));
                output.Write(BencodeSerializer.Serialize(pair.Value));
            }
            output.Write(Encoding.ASCII.GetBytes("e"));
            return output.ToArray();
        }
    }
}
