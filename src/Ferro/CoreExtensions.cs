using System.IO;

namespace Ferro {
    // Extensions methods on core types that we use internally.
    public static class CoreExtensions {
        // Make the offset and length on MemoryStream.Write() optional if you're writing the whole thing.
        public static void Write(this MemoryStream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
