using System;
using System.Collections.Generic;
using System.Text;

namespace ForRobot.Libr.Client
{
    /// <summary>
    /// Класс статуса процесса на роботе
    /// </summary>
    public class StatusEventArgs : EventArgs
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

        public StatusEventArgs(string message)
            : base()
        {
            this.message = message;
        }

        #endregion
    }
}
