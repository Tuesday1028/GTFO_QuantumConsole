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
        public int MaxLogSize { get; set; } = 8192;
        public int MaxLogs { get; set; } = 768;
        public int LogFontSize { get; set; } = 14;
        public int SuggetionFontSize { get; set; } = 16;
    }
}
