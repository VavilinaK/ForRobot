using System;

using NLog;
using NLog.Targets;
using NLog.Config;

namespace ForRobot.Libr
{
    public class Logger
    {
        private NLog.Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        public Logger()
        {
            MethodCallTarget target = new MethodCallTarget();
            target.ClassName = typeof(Logger).AssemblyQualifiedName;
            target.MethodName = nameof(this.LogMethod);
            target.Parameters.Add(new MethodCallParameter("${time}"));
            target.Parameters.Add(new MethodCallParameter("${level}"));
            target.Parameters.Add(new MethodCallParameter("${logger}"));
            target.Parameters.Add(new MethodCallParameter("${message}"));
            target.Parameters.Add(new MethodCallParameter("${exception:maxInnerExceptionLevel=10:format=tostring}"));

            LogManager.ThrowExceptions = true;
            LogManager.ThrowConfigExceptions = true;
            LogManager.Setup().LoadConfiguration(c => c.ForLogger(LogLevel.Trace).WriteTo(target));

            this._logger = LogManager.GetLogger(nameof(Logger));
        }

        public void Trace<T>(T value) => this._logger.Trace(value);
        public void Error<T>(T value) => this._logger.Error(value);
        public void Error(Exception v1, string v2) => this._logger.Error(v1, v2);
        public void Info<T>(T value) => this._logger.Info(value);
        public void Info(Exception v1, string v2) => this._logger.Info(v1, v2);
        public void Fatal<T>(T value) => this._logger.Fatal(value);
        public void Fatal(Exception v1, string v2) => this._logger.Fatal(v1, v2);

        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public static event EventHandler<string[]> LoggingEvent;

        public static void LogMethod(string time, string level, string logger, string message, string exception)
        {
            LoggingEvent?.Invoke(typeof(Logger), new string[] { time, level, message, exception });
        }
    }
}
