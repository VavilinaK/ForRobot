using System;
using System.Collections.Generic;
using System.Text;

namespace ForRobot.Libr
{
    /// <summary>
    /// Класс логирования событий
    /// </summary>
    public class LogEventArgs : EventArgs
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

        public LogEventArgs(string message)
            : base()
        {
            this.message = message;
        }

        #endregion
    }
}
