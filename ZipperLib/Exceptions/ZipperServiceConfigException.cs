using System;
using System.Runtime.Serialization;

namespace ZipperLib.Exceptions
{
    [Serializable]
    public class ZipperServiceConfigException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
      
        public ZipperServiceConfigException(string message) : base(message)
        {
        }

        public ZipperServiceConfigException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ZipperServiceConfigException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
