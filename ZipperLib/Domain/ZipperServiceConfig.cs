using System;
using System.IO;
using ZipperLib.Common;
using ZipperLib.Exceptions;

namespace ZipperLib.Domain
{
    public class ZipperServiceConfig
    {

        public readonly ZipMode Mode;
        public readonly FileInfo Input;
        public volatile FileInfo Output;
        public int BufferSize;
      
        public ZipperServiceConfig(ZipMode mode, string input, string output)
        {
            Mode = mode;
            Input = new FileInfo(input);
            Output = new FileInfo(output);
        }

        public void ValidateInputFile()
        {
            if (!Input.Exists)
            {
                throw new ZipperServiceConfigException("Input file does not exists");
            }

            if (Input.Length < 1)
            {
                throw new ZipperServiceConfigException("Input file empty");
            }
            var maxInputFileSize = Math.Pow(1024, 3) * 32;
            if (Input.Length > maxInputFileSize)
            {
                throw new ZipperServiceConfigException("Input file too big");
            }

            switch (Mode)
            {
                case ZipMode.Compress:
                    if (Input.Extension == ".ai")
                    {
                        throw new ZipperServiceConfigException("Invalid input file type");
                    }

                    if (Output.Extension != ".ai")
                    {
                        throw new ZipperServiceConfigException("Invalid output file type");

                    }

                    CheckAvailableFreeSpace(Input.FullName, Input.Length);
                    break;
                case ZipMode.Decompress:
                    if (Input.Extension != ".ai")
                    {
                        throw new ZipperServiceConfigException("Invalid input file type");
                    }

                    break;
            }
        }

        public void ValidateOutputFile()
        {
            if (Output.Directory != null && !Output.Directory.Exists)
            {
                throw new ZipperServiceConfigException("Output file directory does not exists");
            }

            try
            {
                using (File.Create(Output.FullName)) { }
            }
            catch (Exception e)
            {
                throw new ZipperServiceConfigException(e.Message, e);
            }
            switch (Mode)
            {
                case ZipMode.Compress:
                    if (Output.Extension != ".ai")
                    {
                        throw new ZipperServiceConfigException("Invalid output file type");
                    }

                    break;
                case ZipMode.Decompress:
                    if (Output.Extension == ".ai")
                    {
                        throw new ZipperServiceConfigException("Invalid output file type");
                    }
                    break;
            }
        }

        public void CalculateBufferSize(long length)
        {
            var baseSize = 1024;
            switch (length)
            {
                // when size
                // less than 1024 bytes
                case long size when size < baseSize:
                    BufferSize = 1; // 1 B
                    break;
                // between 1KB and 1MB
                case long size when size >= baseSize && size < Math.Pow(baseSize, 2):
                    BufferSize = 256; // 256 B
                    break;
                // between 1MB and 10 MB
                case long size when size >= Math.Pow(baseSize, 2) && size < Math.Pow(baseSize, 2) * 10:
                    BufferSize = baseSize; // 1 KB
                    break;
                // between 10 MB and 100 MB
                case long size when size >= Math.Pow(1024, 2) * 10 && size < Math.Pow(baseSize, 2) * 100:
                    BufferSize = baseSize*512; // 512 KB
                    break;
                // between 100 MB and 1 Gb
                case long size when size >= Math.Pow(baseSize, 2) * 100 && size < Math.Pow(baseSize, 3):
                    BufferSize = (int)Math.Pow(baseSize, 2); // 1 MB
                    break;
                // greater 1 GB MB 
                case long size when size >= Math.Pow(1024, 3):
                    BufferSize = (int)(Math.Pow(baseSize, 2) * 1.5); // 1.5 MB
                    break;
            }
        }

        public void CheckAvailableFreeSpace(string fullName, long length)
        {
            var drive = Path.GetPathRoot(fullName);
            var driveInfo = new DriveInfo(drive ?? throw new ZipperServiceConfigException("Drive not found"));
            if (driveInfo.AvailableFreeSpace < length)
            {
                throw new ZipperServiceConfigException(
                    "There is not enough space available on disk to complete this operation");

            }
        }
    }
}
