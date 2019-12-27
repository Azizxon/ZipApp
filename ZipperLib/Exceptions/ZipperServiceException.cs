using System;
using System.Runtime.Serialization;

namespace ZipperLib.Exceptions
{
    [Serializable]
    public class ZipperServiceException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ZipperServiceException(string message) : base(message)
        {
        }

        public ZipperServiceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ZipperServiceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
