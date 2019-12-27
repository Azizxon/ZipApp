using System;
using System.IO;
using ZipperLib.Common;

namespace ZipperLib.Domain
{
    public sealed class HeaderReaderWriter
    {
        public BlockHeader CreateBlockHeader(FileStream reader)
        {
            var indexBuffer = new byte[sizeof(long)];
            reader.Read(indexBuffer, 0, indexBuffer.Length);

            var index = BitConverter.ToInt64(indexBuffer, 0);

            var compressedDataLengthBuffer = new byte[sizeof(int)];
            reader.Read(compressedDataLengthBuffer, 0, compressedDataLengthBuffer.Length);
            var compressedDataLength = BitConverter.ToInt32(compressedDataLengthBuffer, 0);

            var uncompressedDataLengthBuffer = new byte[sizeof(int)];
            reader.Read(uncompressedDataLengthBuffer, 0, uncompressedDataLengthBuffer.Length);
            var uncompressedDataLength = BitConverter.ToInt32(uncompressedDataLengthBuffer, 0);

            return new BlockHeader(index, uncompressedDataLength, compressedDataLength);
        }

        public byte[] CreateBlockHeader(BlockHeader header)
        {
            using (var headerStream = new MemoryStream())
            {
                var blockIndexInfo = BitConverter.GetBytes(header.Index);
                headerStream.Write(blockIndexInfo, 0, blockIndexInfo.Length);

                var compressedDataLengthInfo = BitConverter.GetBytes(header.CompressedDataLength);
                headerStream.Write(compressedDataLengthInfo, 0, compressedDataLengthInfo.Length);

                var uncompressedDataLengthInfo = BitConverter.GetBytes(header.DecompressedDataLength);
                headerStream.Write(uncompressedDataLengthInfo, 0, uncompressedDataLengthInfo.Length);

                headerStream.Close();

                return headerStream.ToArray();
            }
        }
    }
}
