using BepInEx.Logging;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using Logger = BepInEx.Logging.Logger;

namespace Hikaria.QC.Loader
{
    [EnableFeatureByDefault]
    internal class BIELogListener : Feature, ILogListener
    {
        public override string Name => "BepInEx 日志监听";

        public override bool InlineSettingsIntoParentMenu => true;

        public override Type[] LocalizationExternalTypes => new[]
        {
            typeof(LogLevel)
        };

        [FeatureConfig]
        public static BIELogListenerSettings Settings { get; set; }

        public class BIELogListenerSettings
        {
            [FSDisplayName("监听级别")]
            public List<LogLevel> ListenLevel
            {
                get
                {
                    return _listenLevel;
                }
                set
                {
                    _listenLevel = value;

                    _logLevel = BepInEx.Logging.LogLevel.None;
                    foreach (var level in _listenLevel)
                    {
                        _logLevel |= (BepInEx.Logging.LogLevel)(int)level;
                    }
                }
            }

            private List<LogLevel> _listenLevel = new();
        }

        public override void Init()
        {
            if (!Settings.ListenLevel.Any())
            {
                Settings.ListenLevel.Add(LogLevel.Fatal);
                Settings.ListenLevel.Add(LogLevel.Error);
                Settings.ListenLevel.Add(LogLevel.Warning);
                Settings.ListenLevel.Add(LogLevel.Message);
            }
        }

        public override void OnEnable()
        {
            if (!Logger.Listeners.Contains(this))
            {
                Logger.Listeners.Add(this);
            }
        }

        public override void OnDisable()
        {
            if (Logger.Listeners.Contains(this))
            {
                Logger.Listeners.Remove(this);
            }
        }

        public BepInEx.Logging.LogLevel LogLevelFilter => _logLevel;

        private static BepInEx.Logging.LogLevel _logLevel = BepInEx.Logging.LogLevel.Message | BepInEx.Logging.LogLevel.Warning | BepInEx.Logging.LogLevel.Error | BepInEx.Logging.LogLevel.Fatal;

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

            if (eventArgs.Level == BepInEx.Logging.LogLevel.None)
                return;

            if (QuantumConsole.Instance == null)
                return;

            QuantumConsole.Instance.LogToConsole($"[{eventArgs.Source.SourceName}] {eventArgs.Data}", FromBIELogLevel(eventArgs.Level), true);
        }

        private LogLevel FromBIELogLevel(BepInEx.Logging.LogLevel level)
        {
            return level switch
            {
                BepInEx.Logging.LogLevel.Info => LogLevel.Info,
                BepInEx.Logging.LogLevel.Debug => LogLevel.Debug,
                BepInEx.Logging.LogLevel.Message => LogLevel.Message,
                BepInEx.Logging.LogLevel.Error => LogLevel.Error,
                BepInEx.Logging.LogLevel.Fatal => LogLevel.Error,
                BepInEx.Logging.LogLevel.Warning => LogLevel.Warning,
                _ => LogLevel.Debug,
            };
        }
    }
}
