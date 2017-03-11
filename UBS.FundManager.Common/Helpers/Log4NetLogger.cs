using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace UBS.FundManager.Common.Helpers
{
    public enum LogLevel { Debug, Info, Warning, Error }

    /// <summary>
    /// Exposes operations possible on a logger
    /// </summary>
    public interface ILogging
    {
        void Log(Exception exception, string additionalMessage);
        void Log(LogLevel logLevel, string message);
    }

    /// <summary>
    /// Log4Net Impl of a logger (can be replaced with any logging framework with further code change)
    /// </summary>
    public class Log4NetLogger : ILogging
    {
        private ILog _log;

        public Log4NetLogger()
        {
            string logPath = ConfigManager.GetSetting("LogPath", "Logs");
            DirectoryInfo directoryInfo = new DirectoryInfo(logPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            GlobalContext.Properties["LogPath"] = logPath;
            _log = LogManager.GetLogger("ClientLogger");
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Logs a message to file and also to the system event logs (application category)
        /// </summary>
        /// <param name="logLevel">i.e Debug, Info</param>
        /// <param name="msg">Message to log</param>
        public void Log(LogLevel logLevel, string msg)
        {
            if(_log == null)
            {
                throw new InvalidOperationException("Logger not yet initialised.");
            }

            msg = this.GetType().Name + " : " + msg;

            using (EventLog eventLog = new EventLog(""))
            {
                eventLog.Source = "Application";

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        _log.Debug(msg);
                        eventLog.WriteEntry(msg, EventLogEntryType.Information);
                        break;

                    case LogLevel.Info:
                        _log.Info(msg);
                        eventLog.WriteEntry(msg, EventLogEntryType.Information);
                        break;

                    case LogLevel.Warning:
                        _log.Warn(msg);
                        eventLog.WriteEntry(msg, EventLogEntryType.Warning);
                        break;

                    case LogLevel.Error:
                        _log.Error(msg);
                        eventLog.WriteEntry(msg, EventLogEntryType.Error);
                        break;

                    default:
                        throw new ArgumentException($"No support for logging level '{ logLevel }' is implemented. Failed to persist message \"{ msg }\"");
                }
            }
        }

        /// <summary>
        /// Logs a message to file and also to the system event logs (under the application category)
        /// </summary>
        /// <param name="ex">Exception message</param>
        /// <param name="additionalMsg">additional message to log</param>
        public void Log(Exception ex, string additionalMsg)
        {
            string exceptionGraph = GetFullExceptionMessage(ex, true);

            if (!string.IsNullOrWhiteSpace(additionalMsg))
            {
                exceptionGraph = $"{ additionalMsg + Environment.NewLine + exceptionGraph }";
            }

            this.Log(LogLevel.Error, exceptionGraph);
        }

        /// <summary>
        /// Extracts the full exception message graph from the exception object
        /// </summary>
        /// <param name="e">Exception object</param>
        /// <param name="includeStacktrace">Flag indicating the inclusion of the stacktrace</param>
        /// <returns></returns>
        private string GetFullExceptionMessage(Exception e, bool includeStacktrace = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Message);

            if (e.InnerException != null)
            {
                sb.AppendLine(GetFullExceptionMessage(e.InnerException, includeStacktrace: false));
            }

            if (includeStacktrace)
            {
                sb.AppendLine(e.StackTrace);
            }

            return sb.ToString().Trim();
        }
    }
}
