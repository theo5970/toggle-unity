using System.IO;
using System.IO.Compression;

namespace Toggle.Utils
{
    public static class CompressionUtils
    {
        public static byte[] Compress(byte[] bytes)
        {
            byte[] compressedBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(bytes, 0, bytes.Length);
                }

                compressedBytes = memoryStream.ToArray();
            }

            return compressedBytes;
        }

        public static byte[] Decompress(byte[] compressedBytes)
        {
            byte[] decompressedBytes;
            MemoryStream decompressResultStream = new MemoryStream();
            using (MemoryStream memoryStream = new MemoryStream(compressedBytes))
            {
                using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressResultStream);
                    deflateStream.Close();
                }

                decompressedBytes = decompressResultStream.ToArray();
                decompressResultStream.Dispose();
            }

            return decompressedBytes;
        }
    }
}