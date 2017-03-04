using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Ferro
{
    public class DataHandler
    {
        public static void SaveMetadata(byte[] bytes)
        {
            string[] pathStrings = { Directory.GetCurrentDirectory(), "..", "..", "file-store", "metadata", "example.torrent" };
            var path = Path.Combine(pathStrings);

            File.Create(path, bytes.Length); // if the file already exists, completely overwrite it.
            using (var stream = File.OpenWrite(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
