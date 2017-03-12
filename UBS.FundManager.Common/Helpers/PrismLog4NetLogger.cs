using log4net;
using Prism.Logging;
using System.Diagnostics;
using System.IO;

namespace UBS.FundManager.Common.Helpers
{
    /// <summary>
    /// Prism compatible implementation of the Log4Net logger.
    /// Used by the WPF application in writing logs. Totally different to
    /// the ILogging => Log4NetLogger
    /// </summary>
    public class PrismLog4NetLogger : ILoggerFacade
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(PrismLog4NetLogger));

        public PrismLog4NetLogger()
        {
            string logPath = ConfigManager.GetSetting("LogPath", "Logs");
            DirectoryInfo directoryInfo = new DirectoryInfo(logPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            GlobalContext.Properties["LogPath"] = logPath;
            log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Logs a message to file and also logs to system event log (in the Application category)
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="category">Category (Log level)</param>
        /// <param name="priority">Priority (not used)</param>
        public void Log(string message, Category category, Priority priority)
        {
            string src = "UBSFundManagerUI";
            using (EventLog eventLog = new EventLog())
            {
                if (!EventLog.SourceExists(src))
                {
                    EventLog.CreateEventSource(src, "UBSFundManager");
                }

                switch (category)
                {
                    case Category.Debug:
                        log.Debug(message);
                        EventLog.WriteEntry(src, message, EventLogEntryType.Information);
                        break;

                    case Category.Warn:
                        log.Warn(message);
                        EventLog.WriteEntry(src, message, EventLogEntryType.Warning);
                        break;

                    case Category.Exception:
                        log.Error(message);
                        EventLog.WriteEntry(src, message, EventLogEntryType.Error);
                        break;

                    case Category.Info:
                    default:
                        log.Info(message);
                        EventLog.WriteEntry(src, message, EventLogEntryType.Information);
                        break;
                }
            }
        }
    }
}
