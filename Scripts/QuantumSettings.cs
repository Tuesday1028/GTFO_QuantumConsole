using TheArchive.Core.Attributes.Feature.Settings;

namespace Hikaria.QC
{
    public class QuantumSettings
    {
        public static QuantumSettings DefaultSettings()
        {
            var settings = new QuantumSettings();

            settings.MaxLogs = 768;
            settings.MaxLogSize = 5120;
            settings.LogFontSize = 14;
            settings.SuggetionFontSize = 16;

            return settings;
        }

        [FSDisplayName("Max Log Size")]
        public int MaxLogSize { get; set; } = 5120;

        [FSDisplayName("Max Logs")]
        public int MaxLogs { get; set; } = 768;

        [FSDisplayName("Log Font Size")]
        public int LogFontSize { get; set; } = 14;

        [FSDisplayName("Log Font Size")]
        public int SuggetionFontSize { get; set; } = 16;
    }
}
