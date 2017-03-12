using System.IO;

using Ditto.Common;
using Ditto.Serialization;

namespace Ditto
{
    public class DataHandler
    {
        public static void SaveMetadata(byte[] bytes)
        {
            var info = Bencoding.DecodeDict(bytes);
            var name = info.GetBytes("name");
            var metainfo = Bencoding.Dict();
            metainfo.Set("info", info);
            var torrentFileData = Bencoding.Encode(metainfo);

            string[] pathStrings = { Directory.GetCurrentDirectory(), "..", "..", "our-state", "metadata", name.FromUTF8() + ".torrent" };
            var path = Path.Combine(pathStrings);

            // if the file already exists, completely overwrite it.
            using (var stream = File.Create(path))
            {
                stream.Write(torrentFileData, 0, torrentFileData.Length);
            }
        }
    }
}
