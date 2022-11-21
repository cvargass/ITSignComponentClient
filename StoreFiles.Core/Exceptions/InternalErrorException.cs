using System;

namespace StoreFiles.Core.Exceptions
{
    public class InternalErrorException : Exception
    {
        public InternalErrorException()
        {

        }
        public InternalErrorException(string message) : base(message)
        {

        }
    }
}
