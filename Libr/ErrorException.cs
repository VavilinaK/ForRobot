using System;

namespace ForRobot.Libr
{
    public class ErrorException : Exception
    {
        #region Private variables

        private int code;

        #endregion

        #region Public variables

        /// <summary>
        /// Код ошибки
        /// </summary>
        public int Code
        {
            get { return this.code; }
        }

        #endregion

        #region Constructors

        internal ErrorException(int code, string message)
            : this(code, message, null)
        { }

        internal ErrorException(int code, string message, Exception innerException)
            : base(message, innerException)
        {
            this.code = code;
        }

        #endregion
    }
}
