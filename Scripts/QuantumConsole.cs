using Hikaria.QC.Bootstrap;
using Hikaria.QC.Logging;
using Hikaria.QC.Pooling;
using Hikaria.QC.UI;
using Hikaria.QC.Utilities;
using Il2CppInterop.Runtime;
using System.Text;
using TheArchive.Core.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hikaria.QC
{
    /// <summary>
    /// Provides the UI and I/O interface for the QuantumConsoleProcessor. Invokes commands on the processor and displays the output.
    /// </summary>
    public class QuantumConsole : MonoBehaviour, ILocalizedTextUpdater
    {
        /// <summary>
        /// Singleton reference to the console. Only valid and set if the singleton option is enabled for the console.
        /// </summary>
        public static QuantumConsole Instance { get; private set; }

#pragma warning disable 0414, 0067, 0649
        private RectTransform _containerRect;
        private ScrollRect _scrollRect;
        private RectTransform _suggestionPopupRect;
        private RectTransform _jobCounterRect;
        private Image[] _panels;

        private QuantumTheme _theme;
        private QuantumKeyConfig _keyConfig;
        private QuantumLocalization _localization;
        private QuantumConsolePreferences _preferences;

        public QuantumTheme Theme
        {
            get => _theme;
            set => _theme = value;
        }

        public QuantumKeyConfig KeyConfig
        {
            get => _keyConfig;
            set => _keyConfig = value;
        }

        public QuantumLocalization Localization
        {
            get => _localization;
            set
            {
                _localization = value;
                ApplyLocalization(value);
            }
        }

        public QuantumConsolePreferences Preferences
        {
            get => _preferences;
            set
            {
                _preferences = value;
                ApplyPreferences(value);
            }
        }

        [Command("verbose-errors", "If errors caused by the Quantum Console Processor or commands should be logged in verbose mode.", MonoTargetType.Registry)]
        private bool _verboseErrors = false;

        [Command("verbose-logging", "Log levels required to use verbose logging.", MonoTargetType.Registry)]
        private LogLevel _verboseLogging = LogLevel.None;

        [Command("logging-level", "Log levels required to intercept and display the log.", MonoTargetType.Registry)]
        private LogLevel _loggingLevel = LogLevel.All & ~LogLevel.Warning;

        private LogLevel _openOnLogLevel = LogLevel.None;
        private bool _interceptDebugLogger = true;
        private bool _interceptWhilstInactive = true;
        private bool _prependTimestamps = true;

        private SupportedState _supportedState = SupportedState.Always;
        private bool _activateOnStartup = false;
        private bool _initialiseOnStartup = true;
        private bool _focusOnActivate = true;
        private bool _closeOnSubmit = false;
        private bool _singletonMode = true;
        private AutoScrollOptions _autoScroll = AutoScrollOptions.OnInvoke;

        private bool _enableAutocomplete = true;
        private bool _showPopupDisplay = true;
        private SortOrder _suggestionDisplayOrder = SortOrder.Descending;
        private int _maxSuggestionDisplaySize = 20;
        private bool _useFuzzySearch = true;
        private bool _caseSensitiveSearch = false;
        private bool _collapseSuggestionOverloads = true;

        private bool _showCurrentJobs = true;
        private bool _blockOnAsync = false;

        private bool _storeCommandHistory = true;
        private bool _storeDuplicateCommands = true;
        private bool _storeAdjacentDuplicateCommands = false;
        private int _commandHistorySize = 30;

        private int _maxStoredLogs = 768;
        private int _maxLogSize = 8192;
        private bool _showInitLogs = true;


        private TMP_InputField _consoleInput;
        private TextMeshProUGUI _inputPlaceholderText;
        private TextMeshProUGUI _consoleLogText;
        private TextMeshProUGUI _consoleSuggestionText;
        private TextMeshProUGUI _suggestionPopupText;
        private TextMeshProUGUI _jobCounterText;
        private TextMeshProUGUI _submitButtonText;
        private TextMeshProUGUI _clearButtonText;
        private TextMeshProUGUI _closeButtonText;

        private Application.LogCallback _logCallback;
        /// <summary>
        /// The maximum number of logs that may be stored in the log storage before old logs are removed.
        /// </summary>
        [Command("max-logs", MonoTargetType.Registry)]
        [CommandDescription("The maximum number of logs that may be stored in the log storage before old logs are removed.")]
        public int MaxStoredLogs
        {
            get => _maxStoredLogs;
            set
            {
                _maxStoredLogs = value;
                if (_logStorage != null) { _logStorage.MaxStoredLogs = value; }
                if (_logQueue != null) { _logQueue.MaxStoredLogs = value; }
            }
        }
#pragma warning restore 0414, 0067, 0649

        #region Callbacks
        /// <summary>Callback executed when the QC state changes.</summary>
        public event Action OnStateChange;

        /// <summary>Callback executed when the QC invokes a command.</summary>
        public event Action<string> OnInvoke;

        /// <summary>Callback executed when the QC is cleared.</summary>
        public event Action OnClear;

        /// <summary>Callback executed when text has been logged to the QC.</summary>
        public event Action<ILog> OnLog;

        /// <summary>Callback executed when the QC is activated.</summary>
        public event Action OnActivate;

        /// <summary>Callback executed when the QC is deactivated.</summary>
        public event Action OnDeactivate;

        /// <summary>Callback executed when a new suggestion set is created. The user may modify the suggestion set.</summary>
        public event Action<SuggestionSet> OnSuggestionSetGenerated;
        #endregion

        private bool IsBlockedByAsync => (_blockOnAsync
                                         && _currentTasks.Count > 0
                                         || _currentActions.Count > 0)
                                         && !_isHandlingUserResponse;

        private readonly QuantumSerializer _serializer = new QuantumSerializer();

        private SuggestionStack _suggestionStack;
        private ILogStorage _logStorage;
        private ILogQueue _logQueue;

        public bool IsActive { get; private set; }
        public bool IsFocused => IsActive && _consoleInput && _consoleInput.isFocused;

        /// <summary>
        /// If any actions are currently executing
        /// </summary>
        public bool AreActionsExecuting => _currentActions.Count > 0;

        private readonly List<string> _previousCommands = new List<string>();
        private readonly List<Task> _currentTasks = new List<Task>();
        private readonly List<IEnumerator<ICommandAction>> _currentActions = new List<IEnumerator<ICommandAction>>();
        private readonly StringBuilderPool _stringBuilderPool = new StringBuilderPool();

        private int _selectedPreviousCommandIndex = -1;
        private string _currentInput;
        private string _previousInput;
        private bool _isGeneratingTable;
        private bool _consoleRequiresFlush;
        private bool _isHandlingUserResponse = false;
        private ResponseConfig _currentResponseConfig;
        private Action<string> _onSubmitResponseCallback;

        private TextMeshProUGUI[] _textComponents;

        private readonly Type _voidTaskType = typeof(Task<>).MakeGenericType(Type.GetType("System.Threading.Tasks.VoidTaskResult"));


        private void ApplyLocalization(QuantumLocalization localization)
        {
            _localization = localization;
            if (localization != null)
            {
                _submitButtonText.text = localization.SubmitButtonText;
                _clearButtonText.text = localization.ClearButtonText;
                _closeButtonText.text = localization.CloseButtonText;
            }
        }

        private void ApplyPreferences(QuantumConsolePreferences pref)
        {
            _suggestionPopupText.fontSize = pref.SuggestionFontSize;
            _consoleLogText.fontSize = pref.LogFontSize;

            _verboseErrors = pref.VerboseErrors;
            _verboseLogging = pref.VerboseLogging;
            _loggingLevel = pref.LoggingLevel;

            _openOnLogLevel = pref.OpenOnLogLevel;
            _interceptDebugLogger = pref.InterceptDebugLogger;
            _interceptWhilstInactive = pref.InterceptWhilstInactive;
            _prependTimestamps = pref.PrependTimestamps;

            _activateOnStartup = pref.ActivateOnStartup;
            _initialiseOnStartup = pref.InitialiseOnStartup;
            _focusOnActivate = pref.FocusOnActivate;
            _closeOnSubmit = pref.CloseOnSubmit;
            _autoScroll = pref.AutoScroll;

            _enableAutocomplete = pref.EnableAutocomplete;
            _showPopupDisplay = pref.ShowPopupDisplay;
            _suggestionDisplayOrder = pref.SuggestionDisplayOrder;
            _maxSuggestionDisplaySize = pref.MaxSuggestionDisplaySize;
            _useFuzzySearch = pref.UseFuzzySearch;
            _caseSensitiveSearch = pref.CaseSensitiveSearch;
            _collapseSuggestionOverloads = pref.CollapseSuggestionOverloads;

            _showCurrentJobs = pref.ShowCurrentJobs;
            _blockOnAsync = pref.BlockOnAsync;

            _storeCommandHistory = pref.StoreCommandHistory;
            _storeDuplicateCommands = pref.StoreDuplicateCommands;
            _storeAdjacentDuplicateCommands = pref.StoreAdjacentDuplicateCommands;
            _commandHistorySize = pref.CommandHistorySize;

            _maxStoredLogs = pref.MaxStoredLogs;
            _maxLogSize = pref.MaxLogSize;
            _showInitLogs = pref.ShowInitLogs;
        }

        /// <summary>Applies a theme to the Quantum Console.</summary>
        /// <param name="theme">The desired theme to apply.</param>
        public void ApplyTheme(QuantumTheme theme, bool forceRefresh = false)
        {
            _theme = theme;
            if (theme != null)
            {
                if (_textComponents == null || forceRefresh) { _textComponents = GetComponentsInChildren<TextMeshProUGUI>(true); }
                foreach (TextMeshProUGUI text in _textComponents)
                {
                    if (theme.Font)
                    {
                        text.font = theme.Font;
                    }
                }

                foreach (Image panel in _panels)
                {
                    panel.material = theme.PanelMaterial;
                    panel.color = theme.PanelColor;
                }
            }
        }

        protected virtual void Update()
        {
            if (!IsActive)
            {
                if (_keyConfig.ShowConsoleKey.IsPressed() || _keyConfig.ToggleConsoleVisibilityKey.IsPressed())
                {
                    Activate();
                }
            }
            else
            {
                ProcessAsyncTasks();
                ProcessActions();
                HandleAsyncJobCounter();

                if (_keyConfig.HideConsoleKey.IsPressed() || _keyConfig.ToggleConsoleVisibilityKey.IsPressed())
                {
                    Deactivate();
                    return;
                }

                if (QuantumConsoleProcessor.TableIsGenerating)
                {
                    _consoleInput.interactable = false;
                    string consoleText = $"{_logStorage.GetLogString()}\n{GetTableGenerationText()}".Trim();
                    if (consoleText != _consoleLogText.text)
                    {
                        if (_showInitLogs)
                        {
                            OnStateChange?.Invoke();
                            _consoleLogText.text = consoleText;
                        }
                        if (_inputPlaceholderText) { _inputPlaceholderText.text = _localization.Loading; }
                    }

                    return;
                }
                else if (IsBlockedByAsync)
                {
                    OnStateChange?.Invoke();
                    _consoleInput.interactable = false;
                    if (_inputPlaceholderText) { _inputPlaceholderText.text = _localization.ExecutingAsyncCommand; }
                }
                else if (!_consoleInput.interactable)
                {
                    OnStateChange?.Invoke();
                    _consoleInput.interactable = true;
                    if (_inputPlaceholderText) { _inputPlaceholderText.text = _localization.EnterCommand; }
                    OverrideConsoleInput(string.Empty);

                    if (_isGeneratingTable)
                    {
                        if (_showInitLogs)
                        {
                            AppendLog(new Log(GetTableGenerationText()));
                            _consoleLogText.text = _logStorage.GetLogString();
                        }

                        _isGeneratingTable = false;
                        ScrollConsoleToLatest();
                    }
                }

                _previousInput = _currentInput;
                _currentInput = _consoleInput.text;
                if (_currentInput != _previousInput)
                {
                    OnInputChange();
                }

                else if (!IsBlockedByAsync)
                {
                    if (InputHelper.GetKeyDown(_keyConfig.SubmitCommandKey)) { InvokeCommand(); }
                    if (_storeCommandHistory) { ProcessCommandHistory(); }
                    ProcessAutocomplete();
                }
            }
        }

        private void LateUpdate()
        {
            if (IsActive)
            {
                FlushQueuedLogs();
                FlushToConsoleText();
            }
        }

        private string GetTableGenerationText()
        {
            string text = string.Format(_localization.InitializationProgress, QuantumConsoleProcessor.LoadedCommandCount);

            if (QuantumConsoleProcessor.TableIsGenerating)
            {
                text += "...";
            }
            else
            {
                string completionText =
                    _theme == null
                        ? _localization.InitializationComplete
                        : _localization.InitializationComplete.ColorText(_theme.SuccessColor);

                text += $"\n{completionText}";
            }

            return text;
        }

        private void ProcessCommandHistory()
        {
            if (InputHelper.GetKeyDown(_keyConfig.NextCommandKey) || InputHelper.GetKeyDown(_keyConfig.PreviousCommandKey))
            {
                if (InputHelper.GetKeyDown(_keyConfig.NextCommandKey)) { _selectedPreviousCommandIndex++; }
                else if (_selectedPreviousCommandIndex > 0) { _selectedPreviousCommandIndex--; }
                _selectedPreviousCommandIndex = Mathf.Clamp(_selectedPreviousCommandIndex, -1, _previousCommands.Count - 1);

                if (_selectedPreviousCommandIndex > -1)
                {
                    string command = _previousCommands[_previousCommands.Count - _selectedPreviousCommandIndex - 1];
                    OverrideConsoleInput(command);
                }
            }
        }

        private void UpdateSuggestions()
        {
            // don't show suggestions when the user is responding to a prompt
            if (_isHandlingUserResponse)
            {
                ClearSuggestions();
                ClearPopup();
                return;
            }

            SuggestorOptions options = new SuggestorOptions
            {
                CaseSensitive = _caseSensitiveSearch,
                Fuzzy = _useFuzzySearch,
                CollapseOverloads = _collapseSuggestionOverloads,
            };

            _suggestionStack.UpdateStack(_currentInput, options);

            UpdateSuggestionText();
            if (_showPopupDisplay)
            {
                UpdatePopupDisplay();
            }
        }

        private void ProcessAutocomplete()
        {
            if (!_enableAutocomplete)
            {
                return;
            }

            if (_keyConfig.SelectNextSuggestionKey.IsPressed() || _keyConfig.SelectPreviousSuggestionKey.IsPressed())
            {
                SuggestionSet set = _suggestionStack.TopmostSuggestionSet;
                if (set != null && set.Suggestions.Count > 0)
                {
                    if (_keyConfig.SelectNextSuggestionKey.IsPressed()) { set.SelectionIndex++; }
                    if (_keyConfig.SelectPreviousSuggestionKey.IsPressed()) { set.SelectionIndex--; }

                    set.SelectionIndex += set.Suggestions.Count;
                    set.SelectionIndex %= set.Suggestions.Count;
                    SetSuggestion(set.SelectionIndex);
                }
            }
        }

        private void FormatSuggestion(IQcSuggestion suggestion, bool selected, StringBuilder buffer)
        {
            if (_theme == null)
            {
                buffer.Append(suggestion.FullSignature);
                return;
            }

            Color primaryColor = Color.white;
            Color secondaryColor = _theme.SuggestionColor;
            if (selected)
            {
                primaryColor *= _theme.SelectedSuggestionColor;
                secondaryColor *= _theme.SelectedSuggestionColor;
            }

            buffer.AppendColoredText(suggestion.PrimarySignature, primaryColor);
            buffer.AppendColoredText(suggestion.SecondarySignature, secondaryColor);
        }

        private string GetFormattedSuggestions(SuggestionSet suggestionSet)
        {
            StringBuilder buffer = _stringBuilderPool.GetStringBuilder();
            GetFormattedSuggestions(suggestionSet, buffer);
            return _stringBuilderPool.ReleaseAndToString(buffer);
        }

        private void GetFormattedSuggestions(SuggestionSet suggestionSet, StringBuilder buffer)
        {
            int displaySize = suggestionSet.Suggestions.Count;
            if (_maxSuggestionDisplaySize > 0)
            {
                displaySize = Mathf.Min(displaySize, _maxSuggestionDisplaySize + 1);
            }

            for (int i = 0; i < displaySize; i++)
            {
                if (_maxSuggestionDisplaySize > 0 && i >= _maxSuggestionDisplaySize)
                {
                    const string remainingSuggestion = "...";
                    if (_theme != null && suggestionSet.SelectionIndex >= _maxSuggestionDisplaySize)
                    {
                        buffer.AppendColoredText(remainingSuggestion, _theme.SelectedSuggestionColor);
                    }
                    else
                    {
                        buffer.Append(remainingSuggestion);
                    }
                }
                else
                {
                    bool selected = i == suggestionSet.SelectionIndex;

                    buffer.Append("<link=");
                    buffer.Append(i);
                    buffer.Append('>');
                    FormatSuggestion(suggestionSet.Suggestions[i], selected, buffer);
                    buffer.AppendLine("</link>");
                }
            }
        }

        private void UpdatePopupDisplay()
        {
            SuggestionSet suggestionSet = _suggestionStack.TopmostSuggestionSet;
            if (suggestionSet == null || suggestionSet.Suggestions.Count == 0)
            {
                ClearPopup();
            }
            else
            {
                if (_suggestionPopupRect && _suggestionPopupText)
                {
                    string formattedSuggestions = GetFormattedSuggestions(suggestionSet);
                    if (_suggestionDisplayOrder == SortOrder.Ascending)
                    {
                        formattedSuggestions = formattedSuggestions.ReverseItems('\n');
                    }

                    _suggestionPopupRect.gameObject.SetActive(true);
                    _suggestionPopupText.text = formattedSuggestions;
                }
            }
        }

        /// <summary>
        /// Sets the selected suggestion on the console.
        /// </summary>
        /// <param name="suggestionIndex">The index of the suggestion to set.</param>
        public void SetSuggestion(int suggestionIndex)
        {
            if (!_suggestionStack.SetSuggestionIndex(suggestionIndex))
            {
                throw new ArgumentException(QuantumConsoleBootstrap.Localization.Format(92, suggestionIndex));
            }

            OverrideConsoleInput(_suggestionStack.GetCompletion());
            UpdateSuggestionText();
        }

        private void UpdateSuggestionText()
        {
            Color suggestionColor = _theme != null
                ? _theme.SuggestionColor
                : Color.gray;

            StringBuilder buffer = _stringBuilderPool.GetStringBuilder();
            buffer.AppendColoredText(_currentInput, Color.clear);
            buffer.AppendColoredText(_suggestionStack.GetCompletionTail(), suggestionColor);

            _consoleSuggestionText.text = _stringBuilderPool.ReleaseAndToString(buffer);
        }

        /// <summary>
        /// Overrides the console input field.
        /// </summary>
        /// <param name="newInput">The text to override the current input with.</param>
        /// <param name="shouldFocus">If the input field should be automatically focused.</param>
        public void OverrideConsoleInput(string newInput, bool shouldFocus = true)
        {
            _currentInput = newInput;
            _previousInput = newInput;
            _consoleInput.text = newInput;

            if (shouldFocus)
            {
                FocusConsoleInput();
            }

            OnInputChange();
        }

        /// <summary>
        /// Selects and focuses the input field for the console.
        /// </summary>
        public void FocusConsoleInput()
        {
            _consoleInput.Select();
            _consoleInput.caretPosition = _consoleInput.text.Length;
            _consoleInput.selectionAnchorPosition = _consoleInput.text.Length;
            _consoleInput.MoveTextEnd(false);
            _consoleInput.ActivateInputField();
        }

        private void OnInputChange()
        {
            if (_selectedPreviousCommandIndex >= 0 && _currentInput.Trim() !=
                _previousCommands[_previousCommands.Count - _selectedPreviousCommandIndex - 1])
            {
                ClearHistoricalSuggestions();
            }

            if (_enableAutocomplete)
            {
                UpdateSuggestions();
            }
        }

        private void ClearHistoricalSuggestions()
        {
            _selectedPreviousCommandIndex = -1;
        }

        private void ClearSuggestions()
        {
            _suggestionStack.Clear();
            _consoleSuggestionText.text = string.Empty;
        }

        private void ClearPopup()
        {
            if (_suggestionPopupRect) { _suggestionPopupRect.gameObject.SetActive(false); }
            if (_suggestionPopupText) { _suggestionPopupText.text = string.Empty; }
        }

        /// <summary>
        /// Invokes the command currently inputted into the Quantum Console.
        /// </summary>
        public void InvokeCommand()
        {
            string userInput = _consoleInput.text;

            // invoke command
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                string command = userInput.Trim();

                if (_isHandlingUserResponse)
                {
                    HandleUserResponse(command);
                }
                else
                {
                    InvokeCommand(command);
                    OverrideConsoleInput(string.Empty);
                    StoreCommand(command);
                }
            }
        }

        private void HandleUserResponse(string command)
        {
            if (_currentResponseConfig.LogInput)
            {
                LogUserInput(command);
                StoreCommand(command);
            }

            // reset state to accept a command as usual
            _onSubmitResponseCallback(command); // pushes the input back to user code
            _onSubmitResponseCallback = null;
            _consoleInput.interactable = false;
            _isHandlingUserResponse = false;

            OnStateChange?.Invoke();
        }

        private void LogUserInput(string input)
        {
            ILog commandLog = GenerateCommandLog(input);
            LogToConsole(commandLog);
        }

        protected ILog GenerateCommandLog(string command)
        {
            string format = _theme?.CommandLogFormat ?? "> {0}";

            if (command.Contains('<'))
            {
                // Add no parse as rich tags are possible
                command = $"<noparse>{command}</noparse>";
            }

            string logValue = string.Format(format, command);
            if (_theme != null)
            {
                logValue = logValue.ColorText(_theme.CommandLogColor);
            }

            return new Log(logValue);
        }

        /// <summary>
        /// Invokes the given command.
        /// </summary>
        /// <param name="command">The command to invoke.</param>
        /// <returns>The return value, if any, of the invoked command.</returns>
        public object InvokeCommand(string command)
        {
            object commandResult = null;
            if (!string.IsNullOrWhiteSpace(command))
            {
                LogUserInput(command);

                string logTrace = string.Empty;
                try
                {
                    commandResult = QuantumConsoleProcessor.InvokeCommand(command);

                    switch (commandResult)
                    {
                        case Task task: _currentTasks.Add(task); break;
                        case IEnumerator<ICommandAction> action: StartAction(action); break;
                        case IEnumerable<ICommandAction> action: StartAction(action.GetEnumerator()); break;
                        default: logTrace = Serialize(commandResult); break;
                    }
                }
                catch (System.Reflection.TargetInvocationException e) { logTrace = GetInvocationErrorMessage(e.InnerException); }
                catch (Exception e) { logTrace = GetErrorMessage(e); }

                LogToConsole(logTrace, LogLevel.Message, false);
                OnInvoke?.Invoke(command);

                if (_autoScroll == AutoScrollOptions.OnInvoke) { ScrollConsoleToLatest(); }
                if (_closeOnSubmit) { Deactivate(); }
            }
            else { OverrideConsoleInput(string.Empty); }

            return commandResult;
        }

        [Command("qc-script-extern", "Executes an external source of QC script file, where each line is a separate QC command.", MonoTargetType.Registry, Platform.AllPlatforms ^ Platform.WebGLPlayer)]
        public async Task InvokeExternalCommandsAsync(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string command = await reader.ReadLineAsync();
                    if (InvokeCommand(command) is Task ret)
                    {
                        await ret;
                        ProcessAsyncTasks();
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a sequence of commands, only starting a new command when the previous is complete.
        /// </summary>
        /// <param name="commands">The commands to invoke.</param>
        public async Task InvokeCommandsAsync(IEnumerable<string> commands)
        {
            foreach (string command in commands)
            {
                if (InvokeCommand(command) is Task ret)
                {
                    await ret;
                    ProcessAsyncTasks();
                }
            }
        }

        private string GetErrorMessage(Exception e)
        {
            return GetErrorMessage(e, _localization.ConsoleError);
        }

        private string GetInvocationErrorMessage(Exception e)
        {
            return GetErrorMessage(e, _localization.CommandError);
        }

        private string GetErrorMessage(Exception e, string label)
        {
            string message = _verboseErrors
                ? $"{label} ({e.GetType()}): {e.Message}\n{e.StackTrace}"
                : $"{label}: {e.Message}";

            return _theme != null
                ? message.ColorText(_theme.ErrorColor)
                : message;
        }

        /// <summary>Thread safe API to format and log text to the Quantum Console.</summary>
        /// <param name="logText">Text to be logged.</param>
        /// <param name="logLevel">The type of the log to be logged.</param>
        public void LogToConsoleAsync(string logText, LogLevel logLevel = LogLevel.Message)
        {
            if (!string.IsNullOrWhiteSpace(logText))
            {
                Log log = new Log(logText, logLevel);
                LogToConsoleAsync(log);
            }
        }

        /// <summary>Thread safe API to format and log text to the Quantum Console.</summary>
        /// <param name="log">Log to be logged.</param>
        public void LogToConsoleAsync(ILog log)
        {
            OnLog?.Invoke(log);
            _logQueue.QueueLog(log);
        }

        private void FlushQueuedLogs()
        {
            bool scroll = false;
            bool open = false;

            while (_logQueue.TryDequeue(out ILog log))
            {
                AppendLog(log);
                scroll |= _autoScroll == AutoScrollOptions.Always;
                open |= _openOnLogLevel.HasFlag(log.Level.GetHighestLevel());
            }

            if (scroll) { ScrollConsoleToLatest(); }
            if (open) { Activate(false); }
        }

        private void ProcessAsyncTasks()
        {
            for (int i = _currentTasks.Count - 1; i >= 0; i--)
            {
                if (_currentTasks[i].IsCompleted)
                {
                    if (_currentTasks[i].IsFaulted)
                    {
                        foreach (Exception e in _currentTasks[i].Exception.InnerExceptions)
                        {
                            string error = GetInvocationErrorMessage(e);
                            LogToConsole(error, LogLevel.Fatal, false);
                        }
                    }
                    else
                    {
                        Type taskType = _currentTasks[i].GetType();
                        if (taskType.IsGenericTypeOf(typeof(Task<>)) && !_voidTaskType.IsAssignableFrom(taskType))
                        {
                            System.Reflection.PropertyInfo resultProperty = _currentTasks[i].GetType().GetProperty("Result");
                            object result = resultProperty.GetValue(_currentTasks[i]);
                            string log = _serializer.SerializeFormatted(result, _theme);
                            LogToConsole(log, LogLevel.Message, false);
                        }
                    }

                    _currentTasks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Begin accepting text input from the user as a response.
        /// </summary>
        /// <param name="onSubmitResponseCallback">Method that will provide the submitted response text when it is submitted.</param>
        /// <param name="config">The configuration to use for this response.</param>
        /// <exception cref="ArgumentNullException"/>
        public void BeginResponse(Action<string> onSubmitResponseCallback, ResponseConfig config)
        {
            // validate args
            if (onSubmitResponseCallback == null)
            {
                throw new ArgumentNullException(nameof(onSubmitResponseCallback));
            }

            // update internal state
            _onSubmitResponseCallback = onSubmitResponseCallback;
            _currentResponseConfig = config;
            _isHandlingUserResponse = true;

            OnStateChange?.Invoke();

            // change state of input text to allow for response
            _consoleInput.interactable = true;
            if (_inputPlaceholderText)
            {
                _inputPlaceholderText.text = _currentResponseConfig.InputPrompt;
            }

            FocusConsoleInput();
        }

        /// <summary>
        /// Starts executing an action.
        /// </summary>
        /// <param name="action">The action to start.</param>
        public void StartAction(IEnumerator<ICommandAction> action)
        {
            _currentActions.Add(action);
            ProcessActions();
        }

        /// <summary>
        /// Cancels any actions currently executing.
        /// </summary>
        public void CancelAllActions()
        {
            _currentActions.Clear();
        }

        private void ProcessActions()
        {
            if (_keyConfig.CancelActionsKey.IsPressed())
            {
                CancelAllActions();
                return;
            }

            ActionContext context = new ActionContext
            {
                Console = this
            };

            for (int i = _currentActions.Count - 1; i >= 0; i--)
            {
                IEnumerator<ICommandAction> action = _currentActions[i];

                try
                {
                    if (action.Execute(context) != ActionState.Running)
                    {
                        _currentActions.RemoveAt(i);
                    }
                }
                catch (Exception e)
                {
                    _currentActions.RemoveAt(i);
                    string error = GetInvocationErrorMessage(e);
                    LogToConsole(error, LogLevel.Fatal, false);
                    break;
                }
            }
        }

        private void HandleAsyncJobCounter()
        {
            if (_showCurrentJobs)
            {
                if (_jobCounterRect && _jobCounterText)
                {
                    if (_currentTasks.Count == 0) { _jobCounterRect.gameObject.SetActive(false); }
                    else
                    {
                        _jobCounterRect.gameObject.SetActive(true);
                        _jobCounterText.text = $"{_currentTasks.Count} job{(_currentTasks.Count == 1 ? "" : "s")} in progress";
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a value using the current serializer and theme.
        /// </summary>
        /// <param name="value">The value to the serialize.</param>
        /// <returns>The serialized value.</returns>
        public string Serialize(object value)
        {
            return _serializer.SerializeFormatted(value, _theme);
        }

        /// <summary>
        /// Logs text to the Quantum Console.
        /// </summary>
        /// <param name="logText">Text to be logged.</param>
        /// <param name="newLine">If a newline should be ins</param>
        public void LogToConsole(string logText, LogLevel logLevel = LogLevel.Message, bool prependTimestamps = false, bool newLine = true)
        {
            bool logExists = !string.IsNullOrEmpty(logText);
            if (logExists)
            {
                if (prependTimestamps && _prependTimestamps)
                {
                    DateTime now = DateTime.Now;
                    string format = _theme != null
                        ? _theme.TimestampFormat
                        : "[{0:00}:{1:00}:{2:00}]";

                    logText = $"{string.Format(format, now.Hour, now.Minute, now.Second)} {logText}";
                }
                logText = logText.ColorText(logLevel.GetUnityColorFromTheme(_theme));
                LogToConsole(new Log(logText, logLevel, newLine));
            }
        }

        /// <summary>
        /// Logs text to the Quantum Console.
        /// </summary>
        /// <param name="log">Log to be logged.</param>
        public void LogToConsole(ILog log)
        {
            FlushQueuedLogs();
            AppendLog(log);
            OnLog?.Invoke(log);

            if (_autoScroll == AutoScrollOptions.Always)
            {
                ScrollConsoleToLatest();
            }
        }

        private float _prePos = 0f;
        private float _preSize = 0f;
        private Vector2 _anchoredPos = Vector2.zero;
        private void FlushToConsoleText()
        {
            if (_consoleRequiresFlush)
            {
                _consoleRequiresFlush = false;
                if (_scrollRect.verticalNormalizedPosition == 0f)
                {
                    _consoleLogText.text = _logStorage.GetLogString();
                }
                else if (_scrollRect.verticalNormalizedPosition == 1f)
                {
                    _consoleLogText.text = _logStorage.GetLogString();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

                    _anchoredPos.Set(0f, -_scrollRect.content.sizeDelta.y);
                    _scrollRect.SetContentAnchoredPosition(_anchoredPos);
                }
                else
                {
                    _prePos = _scrollRect.content.anchoredPosition.y;
                    _preSize = _scrollRect.content.sizeDelta.y;

                    _consoleLogText.text = _logStorage.GetLogString();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

                    _anchoredPos.Set(0f, _prePos + _preSize - _scrollRect.content.sizeDelta.y);
                    _scrollRect.SetContentAnchoredPosition(_anchoredPos);


                    //TODO: δ�����Ѵﵽ����С�����
                }
            }
        }

        private ILog TruncateLog(ILog log)
        {
            if (log.Text.Length <= _maxLogSize || _maxLogSize < 0)
            {
                return log;
            }

            string msg = string.Format(_localization.MaxLogSizeExceeded, log.Text.Length, _maxLogSize);
            if (_theme != null)
            {
                msg = msg.ColorText(_theme.ErrorColor);
            }

            return new Log(msg, LogLevel.Error);
        }

        protected void AppendLog(ILog log)
        {
            _logStorage.AddLog(TruncateLog(log));
            RequireFlush();
        }

        protected void RequireFlush()
        {
            _consoleRequiresFlush = true;
        }

        /// <summary>
        /// Removes the last log from the console.
        /// </summary>
        public void RemoveLogTrace()
        {
            _logStorage.RemoveLog();
            RequireFlush();
        }

        private void ScrollConsoleToLatest()
        {
            if (_scrollRect)
            {
                _scrollRect.verticalNormalizedPosition = 0;
            }
        }

        private void StoreCommand(string command)
        {
            if (_storeCommandHistory)
            {
                if (!_storeDuplicateCommands) { _previousCommands.Remove(command); }
                if (_storeAdjacentDuplicateCommands || _previousCommands.Count == 0 || _previousCommands[_previousCommands.Count - 1] != command) { _previousCommands.Add(command); }
                if (_commandHistorySize > 0 && _previousCommands.Count > _commandHistorySize) { _previousCommands.RemoveAt(0); }
            }
        }

        /// <summary>
        /// Clears the Quantum Console.
        /// </summary>
        [Command("clear", "Clears the Quantum Console", MonoTargetType.Registry)]
        public void ClearConsole()
        {
            _logStorage.Clear();
            _logQueue.Clear();
            _consoleLogText.text = string.Empty;
            _consoleLogText.SetLayoutDirty();
            ClearBuffers();
            OnClear?.Invoke();
        }

        [Command("close", "Closes the Quantum Console", MonoTargetType.Registry)]
        public void CloseConsole()
        {
            Deactivate();
        }

        public string GetConsoleText()
        {
            return _consoleLogText.text;
        }

        protected virtual void ClearBuffers()
        {
            ClearHistoricalSuggestions();
            ClearSuggestions();
            ClearPopup();
        }

        private void SetupComponents()
        {
            _theme = QuantumTheme.DefaultTheme();
            var consoleRect = transform.FindChild("ConsoleRect");
            _containerRect = consoleRect.GetComponent<RectTransform>();
            var dynamicCanvasScaler = gameObject.AddComponent<DynamicCanvasScaler>();
            dynamicCanvasScaler.Setup(GetComponent<CanvasScaler>(), _containerRect);
            var console = consoleRect.FindChild("Console");
            var blurShaderController = gameObject.AddComponent<BlurShaderController>();
            blurShaderController.Setup(_theme.PanelMaterial);
            _scrollRect = console.GetComponent<ScrollRect>();
            var resizeableUI = console.FindChild("Resize Anchor").gameObject.AddComponent<ResizableUI>();
            resizeableUI.Setup(_containerRect, gameObject.GetComponent<Canvas>());
            var consoleView = console.FindChild("Console View");
            var draggableUI = consoleView.gameObject.AddComponent<DraggableUI>();
            draggableUI.Setup(_containerRect, this, _scrollRect);
            _consoleLogText = consoleView.FindChild("View Port/Text").GetComponent<TextMeshProUGUI>();
            _consoleLogText.fontSize = 14;
            _consoleLogText.maxVisibleLines = int.MaxValue;
            _consoleLogText.maxVisibleWords = int.MaxValue;
            _consoleLogText.maxVisibleCharacters = int.MaxValue;
            var popup = console.FindChild("Popup");
            _suggestionPopupRect = popup.GetComponent<RectTransform>();
            _suggestionPopupText = popup.FindChild("Text").GetComponent<TextMeshProUGUI>();
            _suggestionPopupText.fontSize = 16;
            var suggestionDisplay = popup.gameObject.AddComponent<SuggestionDisplay>();
            suggestionDisplay.Setup(this, _suggestionPopupText);
            var ioBar = consoleRect.FindChild("IOBar");
            var jobCounter = ioBar.FindChild("JobCounter");
            _jobCounterRect = jobCounter.GetComponent<RectTransform>();
            _jobCounterText = jobCounter.FindChild("Text").GetComponent<TextMeshProUGUI>();
            var inputField = ioBar.FindChild("InputField");
            _consoleInput = inputField.GetComponent<TMP_InputField>();
            _inputPlaceholderText = inputField.FindChild("Text Area/Placeholder").GetComponent<TextMeshProUGUI>();
            _consoleSuggestionText = inputField.FindChild("Text Area/Backing Text").GetComponent<TextMeshProUGUI>();
            var uiControlTab = console.FindChild("UI Controls Tab");
            var zoomUIController = uiControlTab.gameObject.AddComponent<ZoomUIController>();
            var zoomSizeUpButton = uiControlTab.FindChild("Zoom+").GetComponent<Button>();
            zoomSizeUpButton.onClick.AddListener(new Action(zoomUIController.ZoomUp));
            var zoomSizeDownButton = uiControlTab.FindChild("Zoom-").GetComponent<Button>();
            zoomSizeDownButton.onClick.AddListener(new Action(zoomUIController.ZoomDown));
            zoomUIController.Setup(zoomSizeDownButton, zoomSizeUpButton, dynamicCanvasScaler, this, uiControlTab.FindChild("Text").GetComponent<TextMeshProUGUI>());
            _submitButtonText = ioBar.FindChild("Submit/Text").GetComponent<TextMeshProUGUI>();
            _clearButtonText = ioBar.FindChild("Clear/Text").GetComponent<TextMeshProUGUI>();
            _closeButtonText = ioBar.FindChild("Close/Text").GetComponent<TextMeshProUGUI>();

            _panels = new Image[5];
            _panels[0] = console.GetComponent<Image>();
            _panels[1] = inputField.GetComponent<Image>();
            _panels[2] = popup.GetComponent<Image>();
            _panels[3] = jobCounter.GetComponent<Image>();
            _panels[4] = uiControlTab.GetComponent<Image>();

            var inputEventTrigger = _consoleInput.GetComponent<EventTrigger>();
            var inputEventEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Submit
            };
            inputEventEntry.callback.AddListener(new Action<BaseEventData>((data) => { InvokeCommand(); }));
            inputEventTrigger.triggers.Add(inputEventEntry);

            var submitButton = ioBar.FindChild("Submit").GetComponent<Button>();
            submitButton.onClick.AddListener(new Action(InvokeCommand));

            var clearButton = ioBar.FindChild("Clear").GetComponent<Button>();
            clearButton.onClick.AddListener(new Action(ClearConsole));

            var closeButton = ioBar.FindChild("Close").GetComponent<Button>();
            closeButton.onClick.AddListener(new Action(Deactivate));
        }

        private void Awake()
        {
            QuantumConsoleBootstrap.Localization.AddTextUpdater(this);
            SetupComponents();
            InitializeLogging();
            _logCallback = DelegateSupport.ConvertDelegate<Application.LogCallback>(DebugIntercept);
            Application.s_LogCallbackHandlerThreaded += _logCallback;
        }

        private void OnEnable()
        {
            QuantumRegistry.RegisterObject(this);

            if (IsSupportedState())
            {
                if (_singletonMode)
                {
                    if (Instance == null)
                    {
                        Instance = this;
                        DontDestroyOnLoad(gameObject);
                    }
                    else if (Instance != this)
                    {
                        Destroy(gameObject);
                    }
                }

                if (_activateOnStartup)
                {
                    bool shouldFocus = SystemInfo.deviceType == DeviceType.Desktop;
                    Activate(shouldFocus);
                }
                else
                {
                    if (_initialiseOnStartup) { Initialize(); }
                    Deactivate();
                }
            }
            else { DisableQC(); }
        }

        private bool IsSupportedState()
        {
#if QC_DISABLED
            return false;
#endif
            SupportedState currentState = SupportedState.Always;
#if DEVELOPMENT_BUILD
            currentState = SupportedState.Development;
#elif UNITY_EDITOR
            currentState = SupportedState.Editor;
#endif
            return _supportedState <= currentState;
        }

        private void OnDisable()
        {
            QuantumRegistry.DeregisterObject(this);
            Deactivate();
        }

        private void OnDestroy()
        {
            QuantumConsoleBootstrap.Localization.RemoveTextUpdater(this);
            Application.s_LogCallbackHandlerThreaded -= _logCallback;
        }

        private void DisableQC()
        {
            Deactivate();
            enabled = false;
        }

        private bool _initialized;
        private void Initialize()
        {
            if (_initialized)
                return;

            if (!QuantumConsoleProcessor.TableGenerated)
            {
                QuantumConsoleProcessor.GenerateCommandTable(true);
                _consoleInput.interactable = false;
                _isGeneratingTable = true;
            }

            InitializeSuggestionStack();
            InitializeLogging();

            _consoleLogText.richText = true;
            _consoleSuggestionText.richText = true;

            _theme ??= QuantumTheme.DefaultTheme();
            _keyConfig ??= QuantumKeyConfig.DefaultKeyConfig();
            _localization ??= new();
            _preferences ??= new();

            ApplyTheme(_theme);
            ApplyLocalization(_localization);
            ApplyPreferences(_preferences);

            _initialized = true;
        }

        private void InitializeSuggestionStack()
        {
            if (_suggestionStack == null)
            {
                _suggestionStack = CreateSuggestionStack();
                _suggestionStack.OnSuggestionSetCreated += OnSuggestionSetGenerated;
            }
        }

        private void InitializeLogging()
        {
            _logStorage ??= CreateLogStorage();
            _logQueue ??= CreateLogQueue();
        }

        protected virtual ILogStorage CreateLogStorage() => new LogStorage(_maxStoredLogs);
        protected virtual ILogQueue CreateLogQueue() => new LogQueue(_maxStoredLogs);
        protected virtual SuggestionStack CreateSuggestionStack() => new SuggestionStack();
        
        /// <summary>
        /// Toggles the Quantum Console.
        /// </summary>
        public void Toggle()
        {
            if (IsActive) { Deactivate(); }
            else { Activate(); }
        }

        /// <summary>
        /// Activates the Quantum Console.
        /// </summary>
        public void Activate()
        {
            Activate(_focusOnActivate);
        }

        /// <summary>
        /// Activates the Quantum Console.
        /// </summary>
        /// <param name="shouldFocus">If the input field should be automatically focused.</param>
        public void Activate(bool shouldFocus)
        {
            if (PlayerChatManager.InChatMode)
                return;

            Initialize();
            IsActive = true;
            _containerRect.gameObject.SetActive(true);
            OverrideConsoleInput(string.Empty, shouldFocus);

            if (!EventSystem.current)
            {
                Logs.LogWarning("Quantum Console's UI requires an EventSystem in the scene but there were none present.");
            }

            OnActivate?.Invoke();
        }

        /// <summary>
        /// Deactivates the Quantum Console.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            _containerRect.gameObject.SetActive(false);

            OnDeactivate?.Invoke();
        }

        private void DebugIntercept(string condition, string stackTrace, LogType type)
        {
            if (_interceptDebugLogger && (IsActive || _interceptWhilstInactive) && _loggingLevel.HasFlag(type.ToLogLevel()))
            {
                bool appendStackTrace = _verboseLogging.HasFlag(type.ToLogLevel());
                ILog log = ConstructDebugLog(condition, stackTrace, type, _prependTimestamps, appendStackTrace);
                LogToConsoleAsync(log);
            }
        }

        protected virtual ILog ConstructDebugLog(string condition, string stackTrace, LogType type, bool prependTimeStamp, bool appendStackTrace)
        {
            if (prependTimeStamp)
            {
                DateTime now = DateTime.Now;
                condition = $"{string.Format(_theme?.TimestampFormat ?? "[{0:00}:{1:00}:{2:00}]", now.Hour, now.Minute, now.Second)} {condition}";
            }

            if (appendStackTrace)
            {
                condition += $"\n{stackTrace}";
            }

            var level = LogLevel.Debug;
            if (_theme != null)
            {
                switch (type)
                {
                    case LogType.Log:
                        {
                            condition = ColorExtensions.ColorText(condition, _theme.MessageColor);
                            break;
                        }
                    case LogType.Warning:
                        {
                            condition = ColorExtensions.ColorText(condition, _theme.WarningColor);
                            level = LogLevel.Warning;
                            break;
                        }
                    case LogType.Error:
                        {
                            condition = ColorExtensions.ColorText(condition, _theme.ErrorColor);
                            level = LogLevel.Error;
                            break;
                        }
                    case LogType.Assert:
                    case LogType.Exception:
                        {
                            condition = ColorExtensions.ColorText(condition, _theme.FatalColor);
                            level = LogLevel.Fatal;
                            break;
                        }
                }
            }

            return new Log(condition, level, true);
        }

        protected virtual void OnValidate()
        {
            MaxStoredLogs = _maxStoredLogs;
        }

        public void UpdateText()
        {
            ApplyLocalization(_localization);
        }
    }
}
