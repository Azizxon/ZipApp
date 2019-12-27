using System;
using ZipperLib.Common;
using ZipperLib.Domain;
using ZipperLib.Domain.ZipperService;
using ZipperLib.Exceptions;

namespace ZipperLib.Application
{
    public class ZipApp
    {
        public ZipperServiceConfig Config { get; set; }

        public ZipApp(string mode, string input, string output)
        {
            Configure(mode, input, output);
        }

        public void Start()
        {
            Config.ValidateInputFile();
            Config.ValidateOutputFile();
            _service = new ZipperService(Config);
            _service.Run();
        }

        private void Configure(string mode, string input, string output)
        {
            var isModeParsed=Enum.TryParse(mode,true, out ZipMode zipMode);
            if (isModeParsed)
            {
                Config = new ZipperServiceConfig(zipMode, input, output);
                Config.ValidateInputFile();
                Config.ValidateOutputFile();
            }
            else
            {
                throw new ZipperServiceConfigException($"Invalid mode\nAvailable modes:\n{ZipMode.Compress.ToString()}\n{ZipMode.Decompress.ToString()}");
            }
        }

        private ZipperService _service;

    }
}
