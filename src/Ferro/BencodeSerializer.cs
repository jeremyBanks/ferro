using System;
using System.Collections.Generic;
using System.Linq;
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
        public static string Serialize(string byteString)
        {
            return byteString.Length + ":" + byteString;
        }

        public static string Serialize(int integer)
        {
            return "i" + integer.ToString() + "e";
        }

        public static string Serialize(List<object> list)
        {
            var output = "";
            list.ForEach(item => output += BencodeSerializer.Serialize(item));

            return "l" + output + "e";
        }

        public static string Serialize(Dictionary<object, object> dict)
        {
            return "test";
        }
    }
}
