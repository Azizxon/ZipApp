using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace ZipperLibUnitTest
{
    public static class TestHelper
    {
        public static void CreateTestData(string fileName, long length)
        {
            Debug.WriteLine("Creating test data with length: {0}", length);
            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fileStream.SetLength(length);
            }
            Debug.WriteLine("Test data created");
        }

        public static string CalculateMd5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
