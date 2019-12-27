using System.IO;
using System.IO.Compression;

namespace ZipperLib.Domain
{
    public class Zipper
    {
        public byte[] Compress(byte[] uncompressedData)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    zipStream.Write(uncompressedData, 0, uncompressedData.Length);
                    zipStream.Close();

                   return compressedStream.ToArray();
                }
            }
        }

        public byte[] Decompress(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            {
                using (var resultStream = new MemoryStream())
                {
                    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(resultStream);
                        zipStream.Close();
                    }
                    return resultStream.ToArray();
                }
            }
        }
    }
}
