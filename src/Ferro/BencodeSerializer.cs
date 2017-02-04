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
    public class BencodeSerializer
    {
        public string Serialize(string byteString)
        {
            return "test";
        }

        public string Serialize(int integer)
        {
            return "test";
        }

        // Assuming our lists and dictionaries are going to 
        // consist of strings here
        public string Serialize(List<string> list)
        {
            return "test";
        }

        public string Serialize(Dictionary<string, string> dict)
        {
            return "test";
        }
    }
}
