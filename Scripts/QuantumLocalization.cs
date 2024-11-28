using Hikaria.QC.Bootstrap;

namespace Hikaria.QC
{
    public class QuantumLocalization
    {
        public string Loading => QuantumConsoleBootstrap.Localization.Get(1);
        public string ExecutingAsyncCommand => QuantumConsoleBootstrap.Localization.Get(2);
        public string EnterCommand => QuantumConsoleBootstrap.Localization.Get(3);

        public string CommandError => QuantumConsoleBootstrap.Localization.Get(4);
        public string ConsoleError => QuantumConsoleBootstrap.Localization.Get(5);
        public string MaxLogSizeExceeded => QuantumConsoleBootstrap.Localization.Get(6);

        public string InitializationProgress => QuantumConsoleBootstrap.Localization.Get(7);

        public string InitializationComplete => QuantumConsoleBootstrap.Localization.Get(8);

        public string SubmitButtonText => QuantumConsoleBootstrap.Localization.Get(9);
        public string ClearButtonText => QuantumConsoleBootstrap.Localization.Get(10);
        public string CloseButtonText => QuantumConsoleBootstrap.Localization.Get(11);
    }
}
