namespace ZipperLib.Common
{
    public struct InputBlock
    {
        public long Index { get; }
        public byte[] Data { get; }
        public int Length { get; }
        public InputBlock(long index, int length, byte[] data)
        {
            Index = index;
            Data = data;
            Length = length;
        }

        public override string ToString()
        {
            return $"Index: {Index}\n" +
                   $"Data Length: {Data.Length}\n" +
                   $"Length: {Length}\n" +
                   $"Data first two items: {Data[0]} {Data[1]}\n";
        }
    }
}
