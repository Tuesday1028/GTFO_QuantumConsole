using Hikaria.QC.Utilities;
using UnityEngine;

namespace Hikaria.QC.Logging
{
    public static class LogLevelExtensions
    {
        /// <summary>
        ///     Gets the highest log level when there could potentially be multiple levels provided.
        /// </summary>
        /// <param name="levels">The log level(s).</param>
        /// <returns>The highest log level supplied.</returns>
        public static LogLevel GetHighestLevel(this LogLevel levels)
        {
            Array values = Enum.GetValues(typeof(LogLevel));
            Array.Sort(values);
            foreach (object obj in values)
            {
                LogLevel e = (LogLevel)obj;
                if ((levels & e) != LogLevel.None)
                {
                    return e;
                }
            }
            return LogLevel.None;
        }

        /// <summary>
        ///     Returns a translation of a log level to it's associated console colour.
        /// </summary>
        /// <param name="level">The log level(s).</param>
        /// <returns>A console color associated with the highest log level supplied.</returns>
        public static Color GetUnityColor(this LogLevel level, QuantumTheme theme)
        {
            level = level.GetHighestLevel();
            if (level <= LogLevel.Message)
            {
                switch (level)
                {
                    case LogLevel.Fatal:
                        return theme.FatalColor;
                    case LogLevel.Error:
                        return theme.ErrorColor;
                    case LogLevel.Message:
                        return theme.MessageColor;
                    case LogLevel.Info:
                        return theme.InfoColor;
                    case LogLevel.Debug:
                        return theme.DebugColor;
                    case LogLevel.Warning:
                        return theme.WarningColor;
                    default:
                        break;
                }
            }
            else
            {
                if (level == LogLevel.Info)
                {
                    return ColorExtensions.DarkGray;
                }
                if (level == LogLevel.Debug)
                {
                    return ColorExtensions.DarkGray;
                }
            }
            return Color.gray;
        }


        public static LoggingThreshold ToLoggingThreshold(this LogLevel logLevel)
        {
            LoggingThreshold severity = LoggingThreshold.Always;
            switch (logLevel)
            {
                case LogLevel.Fatal: severity = LoggingThreshold.Exception; break;
                case LogLevel.Error: severity = LoggingThreshold.Error; break;
                case LogLevel.Message: severity = LoggingThreshold.Error; break;
                case LogLevel.Warning: severity = LoggingThreshold.Warning; break;
                case LogLevel.Info: severity = LoggingThreshold.Always; break;
                case LogLevel.Debug: severity = LoggingThreshold.Always; break;
            }

            return severity;
        }
    }
}