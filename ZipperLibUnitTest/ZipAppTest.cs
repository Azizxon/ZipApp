using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipperLib.Application;
using ZipperLib.Exceptions;

namespace ZipperLibUnitTest
{
    [TestClass]
    public class ZipAppTest
    {
        [TestMethod]
        public void CompressFileWithZeroLength_ShouldReturnZipperServiceConfigException()
        {
            using (File.Create("testFile.txt")) { }
            Assert.ThrowsException<ZipperServiceConfigException>((() =>
            {
                var zipApp = new ZipApp("compress", "testFile.txt", "compressedFile.ai");
                zipApp.Start();
            }));

            File.Delete("testFile.txt");
            File.Delete("compressedFile.ai");
        }

        [TestMethod]
        public void CompressAndDecompressFilesBetweenEightByteAndOneGB_OriginalFileAndResultFileShouldBeSame()
        {
            var baseSize = 1024;
            // from 8 byte to 1 GB
            var fileSizes = new[]
            {
                8,
                baseSize,
                (int) Math.Pow(baseSize, 2),
                (int) Math.Pow(baseSize, 2)*10,
                (int) Math.Pow(baseSize, 2)*100,
                (int) Math.Pow(baseSize, 3)
            };
            foreach (var fileSize in fileSizes)
            {
                TestHelper.CreateTestData("testFile.txt", fileSize);
                var testFileInfo = new FileInfo("testFile.txt");
                Debug.WriteLine("Compressed start. File size: {0}", testFileInfo.Length);

                var compressor = new ZipApp("compress", "testFile.txt", "compressedFile.ai");
                compressor.Start();

                var compressedFileInfo = new FileInfo("compressedFile.ai");
                Debug.WriteLine("Decompressed start. File size: {0}", compressedFileInfo);
                var decompressor = new ZipApp("decompress", "compressedFile.ai", "originalFile.txt");
                decompressor.Start();

                Debug.WriteLine("Hash calculation start. File size: {0}", testFileInfo.Length);
                var originalFileHash = TestHelper.CalculateMd5("testFile.txt");
                var decompressedFileHash = TestHelper.CalculateMd5("originalFile.txt");
                
                Debug.WriteLine("Files hashes:\n{0}\n{1}",originalFileHash, decompressedFileHash);
                Assert.AreEqual(originalFileHash, decompressedFileHash);
            }

            File.Delete("testFile.txt");
            File.Delete("compressedFile.ai");
            File.Delete("originalFile.txt");
        }

        [TestMethod]
        public void CompressAndDecompressFilesBetweenOneGBAnd32GB_OriginalFileAndResultFileShouldBeSame()
        {
            var baseSize = 1024L;
            // from 1 GB to 32 GB
            var fileSizes = new[]
            {
                (long) Math.Pow(baseSize, 3),
                (long) Math.Pow(baseSize, 3)*2,
                (long) Math.Pow(baseSize, 3)*4,
                (long) Math.Pow(baseSize, 3)*8,
                (long) Math.Pow(baseSize, 3)*16,
                (long) Math.Pow(baseSize, 3)*32,
            };
            foreach (var fileSize in fileSizes)
            {
                TestHelper.CreateTestData("testFile.txt", fileSize);
                
                var compressor = new ZipApp("compress", "testFile.txt", "compressed.ai");
                compressor.Start();

                var decompressor = new ZipApp("decompress", "compressed.ai", "originalFile.txt");
                decompressor.Start();

                var originalFileHash = TestHelper.CalculateMd5("testFile.txt");
                var decompressedFileHash = TestHelper.CalculateMd5("originalFile.txt");

                Assert.AreEqual(originalFileHash, decompressedFileHash);

                File.Delete("testFile.txt");
                File.Delete("compressed.ai");
                File.Delete("originalFile.txt");
            }
        }
    }
}
