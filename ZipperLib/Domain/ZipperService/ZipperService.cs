using ZipperLib.Common;

namespace ZipperLib.Domain.ZipperService
{
    public class ZipperService
    {
        public ZipperService(ZipperServiceConfig config)
        {
            _config = config;
        }

        public void Run()
        {
            switch (_config.Mode)
            {
                case ZipMode.Compress:
                    _config.CalculateBufferSize(_config.Input.Length);
                    var compressor = new ZipperServiceCompressor(_config);
                    compressor.CompressFile();
                    break;
                case ZipMode.Decompress:
                    var decompressor = new ZipperServiceDecompressor(_config);
                    decompressor.DecompressFile();
                    break;
            }
        }

        private volatile ZipperServiceConfig _config;
    }
}
