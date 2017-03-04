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
            var info = Bencoding.Decode(bytes);
            var metainfo = new Dictionary<byte[], object> {
                {"info".ToASCII(), info}
            };
            var torrentFileData = Bencoding.Encode(metainfo);

            string[] pathStrings = { Directory.GetCurrentDirectory(), "..", "..", "file-store", "metadata", "example.torrent" };
            var path = Path.Combine(pathStrings);

            // if the file already exists, completely overwrite it.
            using (var stream = File.Create(path))
            {
                stream.Write(torrentFileData, 0, torrentFileData.Length);
            }
        }
    }
}
