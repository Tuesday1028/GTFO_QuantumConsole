using BepInEx.Logging;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Hikaria.QC.Loader
{
    internal class BIELogLisener : ILogListener
    {
        public static void Init()
        {
            if (Instance != null)
                return;

            Instance = new BIELogLisener();

            if (!Logger.Listeners.Contains(Instance) && _displayBepInExLog)
            {
                Logger.Listeners.Add(Instance);
            }
        }

        public LogLevel LogLevelFilter => _logLevel;

        private static LogLevel _logLevel = LogLevel.Fatal | LogLevel.Error | LogLevel.Warning | LogLevel.Message;

        private static bool _displayBepInExLog = true;

        public void Dispose()
        {
            if (Logger.Listeners.Contains(this))
            {
                Logger.Listeners.Remove(this);
            }
        }

        public void LogEvent(object sender, LogEventArgs eventArgs)
        {
            if (eventArgs.Source.SourceName == "Unity")
                return;

            if (eventArgs.Level == LogLevel.None)
                return;

            QuantumConsole.Instance.LogToConsole($"{eventArgs.Source.SourceName}: {eventArgs.Data}", FromLogLevel(eventArgs.Level), true);
        }

        private LogType FromLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Info => LogType.Log,
                LogLevel.Debug => LogType.Log,
                LogLevel.Message => LogType.Log,
                LogLevel.Error => LogType.Error,
                LogLevel.Fatal => LogType.Exception,
                LogLevel.Warning => LogType.Warning,
                _ => LogType.Log,
            };
        }

        public static BIELogLisener Instance { get; private set; }
    }
}
