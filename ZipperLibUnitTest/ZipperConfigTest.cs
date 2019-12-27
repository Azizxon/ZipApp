using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZipperLib.Common;
using ZipperLib.Domain;
using ZipperLib.Exceptions;

namespace ZipperLibUnitTest
{
    [TestClass]
    public class ZipperConfigTest
    {
        [TestMethod]
        public void CorrectInputOutputFileForCompress_ShouldReturnTrue()
        {
            TestHelper.CreateTestData("testFile.txt", 1024 * 1024);
            var zipperConfigCompress = new ZipperServiceConfig( ZipMode.Compress, "testFile.txt", "compressedFile.ai");
            zipperConfigCompress.ValidateOutputFile();
            zipperConfigCompress.ValidateInputFile();

            Assert.IsTrue(File.Exists("compressedFile.ai"));

            File.Delete("testFile.txt");
            File.Delete("compressedFile.txt");
        }


        [TestMethod]
        public void NotExistsInputFileForCompress_ShouldThrowZipperServiceConfigException()
        {
            var zipperConfigCompress = new ZipperServiceConfig( ZipMode.Compress, "someIncorrectFileName", "compressedFile.ai");

            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateInputFile());
        }

        [TestMethod]
        public void NotExistsDirectoryOutputFileForCompress_ShouldThrowZipperServiceConfigException()
        {
            TestHelper.CreateTestData("testFile.txt", 1024);
            var zipperConfigCompress = new ZipperServiceConfig( ZipMode.Compress, "testFile.txt", "Z:\\NotExistDirectory\\compressedFile.ai");

            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateOutputFile());
        }

        [TestMethod]
        public void InputEmptyFileForCompress_ShouldThrowZipperServiceConfigException()
        {
            using (File.Create("testFile.txt")) ;
            var zipperConfigCompress = new ZipperServiceConfig( ZipMode.Compress, "testFile.txt", "compressedFile.ai");

            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateInputFile());
            File.Delete("testFile.txt");
        }

        [TestMethod]
        public void InputAndOutputFileIncorrectExtensionForCompress_ShouldThrowZipperServiceConfigException()
        {
            TestHelper.CreateTestData("testFile.ai", 1024 * 1024);
            var zipperConfigCompress = new ZipperServiceConfig( ZipMode.Compress, "testFile.ai", "compressedFile.7z");

            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateInputFile());
            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateOutputFile());

            File.Delete("testFile.ai");
            File.Delete("compressedFile.7z");
        }

      
        [TestMethod]
        public void InputFileIncorrectExtensionForDecompress_ShouldThrowZipperServiceConfigException()
        {
            TestHelper.CreateTestData("testFile.sz", 1024);
            var zipperConfigCompress =
                new ZipperServiceConfig(ZipMode.Decompress, "testFile.sz", "originalFile.txt");

            Assert.ThrowsException<ZipperServiceConfigException>(() => zipperConfigCompress.ValidateInputFile());
            File.Delete("testFile.sz");
            File.Delete("originalFile.txt");
        }
    }
}
