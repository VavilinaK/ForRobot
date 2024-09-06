using System;
using System.Collections.Generic;
using System.Text;

namespace ForRobot.Libr
{
    /// <summary>
    /// Класс логирования ошибок
    /// </summary>
    public class LogErrorEventArgs : LogEventArgs
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

        public LogErrorEventArgs(string message)
            : base(message)
        { }

        public LogErrorEventArgs(string message, Exception exception)
            : base(message)
        {
            this.exception = exception;
        }

        #endregion
    }
}
