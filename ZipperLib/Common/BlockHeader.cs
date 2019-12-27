namespace ZipperLib.Common
{
    public struct BlockHeader
    {
        public long Index { get; set; }
        public int DecompressedDataLength { get; set; }
        public int CompressedDataLength { get; set; }

        public BlockHeader(long index, int decompressedDataLength, int compressedDataLength)
        {
            Index = index;
            DecompressedDataLength = decompressedDataLength;
            CompressedDataLength = compressedDataLength;
        }
    }
}
