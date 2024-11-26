using Hikaria.QC.Loader;

namespace Hikaria.QC
{
    public class QuantumLocalization
    {
        public string Loading => QuantumConsoleLoader.Localization.Get(1);
        public string ExecutingAsyncCommand => QuantumConsoleLoader.Localization.Get(2);
        public string EnterCommand => QuantumConsoleLoader.Localization.Get(3);

        public string CommandError => QuantumConsoleLoader.Localization.Get(4);
        public string ConsoleError => QuantumConsoleLoader.Localization.Get(5);
        public string MaxLogSizeExceeded => QuantumConsoleLoader.Localization.Get(6);

        public string InitializationProgress => QuantumConsoleLoader.Localization.Get(7);

        public string InitializationComplete => QuantumConsoleLoader.Localization.Get(8);

        public string SubmitButtonText => QuantumConsoleLoader.Localization.Get(9);
        public string ClearButtonText => QuantumConsoleLoader.Localization.Get(10);
        public string CloseButtonText => QuantumConsoleLoader.Localization.Get(11);
    }
}
