using System;
using ZipperLib.Application;
using ZipperLib.Exceptions;

namespace Client
{
    class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length<3)
            {
                Console.WriteLine("Invalid parameters");
                return -1;
            }

            var mode = args[0];
            var input = args[1];
            var output = args[2];
            try
            {
                var zipApp = new ZipApp(mode, input, output);
                zipApp.Start();

                return 1;
            }
            catch (ZipperServiceConfigException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (ZipperServiceException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return -1;
        }
    }
}
