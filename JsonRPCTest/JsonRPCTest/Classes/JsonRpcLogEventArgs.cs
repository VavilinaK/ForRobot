using System;
using System.Collections.Generic;
using System.Text;

namespace JsonRPCTest.Classes
{
    /// <summary>
    /// Класс логирования событий
    /// </summary>
    public class JsonRpcLogEventArgs : EventArgs
    {
        #region Private variables

        private readonly string message;

        #endregion

        #region Public variables

        public string Message
        {
            get { return this.message; }
        }

        #endregion

        #region Constructors

        public JsonRpcLogEventArgs(string message)
            : base()
        {
            this.message = message;
        }

        #endregion
    }
}
