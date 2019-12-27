using System.Collections.Concurrent;
using ZipperLib.Common;

namespace ZipperLib.Domain.ZipperService
{
    public partial class ZipperService
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
                    CompressFile();
                    break;
                case ZipMode.Decompress:
                    DecompressFile();
                    break;
            }
        }

        private static int _onProcessingBlockCount;
        private ConcurrentDictionary<long, DataBlock> _blocks;
        private ConcurrentDictionary<long, InputBlock> _inputBlocks;
        private volatile ZipperServiceConfig _config;
    }
}
