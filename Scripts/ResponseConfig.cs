using Hikaria.QC.Loader;

namespace Hikaria.QC
{
    /// <summary>
    /// Configuration for requesting a response in the console.
    /// </summary>
    public struct ResponseConfig
    {
        // The prompt to display in the input field
        public string InputPrompt;

        // If the input should be logged back to the console
        public bool LogInput;

        public static readonly ResponseConfig Default = new ResponseConfig
        {
            InputPrompt = QuantumConsoleLoader.Localization.Get(90),
            LogInput = true
        };
    }
}