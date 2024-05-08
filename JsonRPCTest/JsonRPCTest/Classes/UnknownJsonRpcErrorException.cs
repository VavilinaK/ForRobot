using System;


namespace JsonRPCTest.Classes
{
    public class UnknownJsonRpcErrorException : Exception
    {
        internal UnknownJsonRpcErrorException()
            : base()
        { }

        internal UnknownJsonRpcErrorException(string message)
            : base(message)
        { }

        internal UnknownJsonRpcErrorException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
