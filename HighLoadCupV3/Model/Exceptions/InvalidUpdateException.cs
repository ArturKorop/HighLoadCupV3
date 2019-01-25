using System;
using System.Runtime.Serialization;

namespace HighLoadCupV3.Model.Exceptions
{
    public class InvalidUpdateException : Exception
    {
        public InvalidUpdateException()
        {
        }

        public InvalidUpdateException(string invalidFilterName) : base($"Invalid update: [{invalidFilterName}]")
        {
        }

        public InvalidUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidUpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
