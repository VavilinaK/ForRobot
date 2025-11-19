using System;
using System.Linq;

using NLog;

namespace ForRobot.Models
{
    public enum AppMessageTypes
    {
        Info = 0,

        Error = 1
    }

    public class AppMessage : BaseClass
    {
        #region Private variables

        //private string _source;

        #endregion

        #region Public variables

        public DateTime Time { get; set; }

        //public AppMessageTypes MessageType { get; set; }

        public NLog.LogLevel LogLevel { get; set; }

        public string Message { get; set; }

        public string Ditails { get; set; } = string.Empty;

        #endregion

        #region Constructor

        public AppMessage(string message, Exception exception = null)
        {
            this.Message = message;
            //this.Exception = exception;
        }

        public AppMessage(string[] values)
        {
            this.Time = Convert.ToDateTime(values[0]);
            this.LogLevel = NLog.LogLevel.AllLoggingLevels.Where(item => string.Equals(item.Name, values[1], StringComparison.InvariantCultureIgnoreCase)).First();
            this.Message = values[2];
            this.Ditails = values[3];
        }

        #endregion
    }
}
