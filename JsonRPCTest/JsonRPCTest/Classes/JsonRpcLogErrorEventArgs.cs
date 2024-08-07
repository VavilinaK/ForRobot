using System;
using System.Collections.Generic;
using System.Text;

namespace JsonRPCTest.Classes
{
    /// <summary>
    /// Класс логирования ошибок
    /// </summary>
    public class JsonRpcLogErrorEventArgs : JsonRpcLogEventArgs
    {
        #region Private variables

        private readonly Exception exception;

        #endregion

        #region Public variables

        public Exception Exception
        {
            get { return this.exception; }
        }

        #endregion

        #region Constructors

        public JsonRpcLogErrorEventArgs(string message)
            : base(message)
        { }

        public JsonRpcLogErrorEventArgs(string message, Exception exception)
            : base(message)
        {
            this.exception = exception;
        }

        #endregion
    }
}
