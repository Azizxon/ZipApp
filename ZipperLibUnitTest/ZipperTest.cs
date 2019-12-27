using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipperLib.Domain;

namespace ZipperLibUnitTest
{
    [TestClass]
    public class ZipperTest
    {
        [TestMethod]
        public void CompressDataByteArray_DecompressCompressedDataByteArray_DecompressedAndOriginalDataShouldBeSame()
        {
            var random=new Random();
            var text = new StringBuilder();
            for (int i = 0; i < 1024*1024; i++)
            {
                text.Append(random.Next(256).ToString());
            }
            var data = Encoding.ASCII.GetBytes(text.ToString());
            var zipper = new Zipper();
            
            var compressedData=zipper.Compress(data);
            var decompressedData = zipper.Decompress(compressedData);

            Assert.IsTrue(compressedData.Length<=data.Length);
            Assert.IsTrue(data.SequenceEqual(decompressedData));
        }
    }
}
