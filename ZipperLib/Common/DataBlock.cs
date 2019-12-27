namespace ZipperLib.Common
{
    public struct DataBlock
    {
        public BlockHeader Header { get; }
        public byte[] Data { get; set; }
        public DataBlock(BlockHeader header, byte[] data)
        {
            Data = data;
            Header = header;
        }

        public override string ToString()
        {
            return $"Index : {Header.Index}\n" +
                   $"Compressed Data Length: {Header.CompressedDataLength}\n" +
                   $"Decompressed Data Length: {Header.DecompressedDataLength}\n" +
                   $"Data Length: {Data.Length}\n";
        }
    }
}
