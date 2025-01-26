﻿using BepInEx.Logging;
using Globals;
using Hikaria.QC.Extras;
using Hikaria.QC.UI;
using Hikaria.QC.Utilities;
using LevelGeneration;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Core.Models;
using TheArchive.Loader;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Hikaria.QC.Bootstrap
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    [DoNotSaveToConfig]
    public class QuantumConsoleBootstrap : Feature
    {
        public override string Name => "量子终端加载器";

        public static new ILocalizationService Localization { get; set; }

        public override Type[] LocalizationExternalTypes => new[]
        {
            typeof(BepInEx.Logging.LogLevel), typeof(LogLevel), typeof(AutoScrollOptions), typeof(SortOrder)
        };

        public override bool RequiresRestart => true;

        public override bool InlineSettingsIntoParentMenu => true;

        [FeatureConfig]
        public static QuantumConsoleSettings Settings { get; set; }
        public class QuantumConsoleSettings
        {
            [FSDisplayName("BepInEx 日志监听级别")]
            public List<BepInEx.Logging.LogLevel> BIEListenLevel
            {
                get
                {
                    return _bieListenLevel;
                }
                set
                {
                    _bieListenLevel = value;

                    _bieLogLevel = (BepInEx.Logging.LogLevel)(ulong)_bieListenLevel.ToFlags();
                }
            }
            [FSDisplayName("按键绑定")]
            public QuantumKeySettings KeySettings { get; set; } = new();
            [FSDisplayName("主题设置")]
            public QuantumThemeSettings ConsoleThemeSettings { get; set; } = new();
            [FSDisplayName("控制台设置")]
            public QuantumConsolePreferenceSettings ConsolePrefSettings { get; set; } = new();

            public static BepInEx.Logging.LogLevel BIELogLevel => _bieLogLevel;
            private static BepInEx.Logging.LogLevel _bieLogLevel = BepInEx.Logging.LogLevel.Message | BepInEx.Logging.LogLevel.Warning | BepInEx.Logging.LogLevel.Error | BepInEx.Logging.LogLevel.Fatal;
            private List<BepInEx.Logging.LogLevel> _bieListenLevel = new();
        }

        public class QuantumThemeSettings
        {
            [FSDisplayName("字体路径")]
            public string FontAssetPath { get; set; } = "Assets/Plugins/QFSW/Quantum Console/Source/Fonts/TMP/OfficeCodePro-Regular SDF.asset";
            [FSDisplayName("面板材质路径")]
            public string PanelMaterialAssetPath { get; set; } = "Assets/Plugins/QFSW/Quantum Console/Source/Materials/Blur Panel.mat";
            [FSDisplayName("时间戳格式")]
            public string TimestampFormat { get; set; } = "[{0:00}:{1:00}:{2:00}]";
            [FSDisplayName("指令日志格式")]
            public string CommandLogFormat { get; set; } = "> {0}";

            [FSHeader("颜色设置")]
            [FSDisplayName("面板")]
            public SColor PanelColor { get; set; } = Color.white;
            [FSDisplayName("指令日志")]
            public SColor CommandLogColor { get; set; } = Color.cyan;
            [FSDisplayName("指令建议选中")]
            public SColor SelectedSuggestionColor { get; set; } = new(1, 1, 0.55f);
            [FSDisplayName("指令")]
            public SColor SuggestionColor { get; set; } = Color.gray;
            [FSDisplayName("关键错误")]
            public SColor FatalColor { get; set; } = Color.red;
            [FSDisplayName("错误")]
            public SColor ErrorColor { get; set; } = Color.red;
            [FSDisplayName("警告")]
            public SColor WarningColor { get; set; } = Color.yellow;
            [FSDisplayName("消息")]
            public SColor MessageColor { get; set; } = Color.white;
            [FSDisplayName("信息")]
            public SColor InfoColor { get; set; } = ColorExtensions.DarkGray;
            [FSDisplayName("调试")]
            public SColor DebugColor { get; set; } = ColorExtensions.DarkGray;


            public static implicit operator QuantumTheme(QuantumThemeSettings settings)
            {
                var theme = QuantumTheme.DefaultTheme();
                theme.PanelColor = settings.PanelColor;
                theme.CommandLogColor = settings.CommandLogColor;
                theme.SelectedSuggestionColor = settings.SelectedSuggestionColor;
                theme.SuggestionColor = settings.SuggestionColor;
                theme.ErrorColor = settings.ErrorColor;
                theme.FatalColor = settings.FatalColor;
                theme.WarningColor = settings.WarningColor;
                theme.MessageColor = settings.MessageColor;
                theme.DebugColor = settings.DebugColor;
                theme.InfoColor = settings.InfoColor;
                theme.FontAssetPath = settings.FontAssetPath;
                theme.PanelMaterialAssetPath = settings.PanelMaterialAssetPath;
                theme.TimestampFormat = settings.TimestampFormat;
                theme.CommandLogFormat = settings.CommandLogFormat;
                return theme;
            }
        }

        public class QuantumConsolePreferenceSettings
        {
            [FSHeader("日志设置")]
            [FSDisplayName("详细错误日志")]
            public bool VerboseErrors { get; set; } = false;
            [FSDisplayName("详细记录日志级别")]
            public List<LogLevel> VerboseLogging { get; set; } = new();
            [FSDisplayName("Unity日志监听级别")]
            public List<LogLevel> LoggingLevel { get; set; } = new();

            [FSDisplayName("自动激活日志级别")]
            public List<LogLevel> OpenOnLogLevel { get; set; } = new();
            [FSDisplayName("监听Unity日志")]
            public bool InterceptDebugLogger { get; set; } = true;
            [FSDisplayName("非激活时监听")]
            public bool InterceptWhilstInactive { get; set; } = true;
            [FSDisplayName("添加时间戳前缀")]
            public bool PrependTimestamps { get; set; } = true;

            [FSDisplayName("日志最大容量")]
            public int MaxStoredLogs { get; set; } = 768;
            [FSDisplayName("日志最大长度")]
            public int MaxLogSize { get; set; } = 8192;
            [FSDisplayName("日志文本大小")]
            public int LogFontSize { get; set; } = 14;
            [FSDisplayName("显示初始化日志")]
            public bool ShowInitLogs { get; set; } = true;

            [FSHeader("控制台设置")]
            [FSDisplayName("启动时激活")]
            public bool ActivateOnStartup { get; set; } = false;
            [FSDisplayName("激活时自动追焦")]
            public bool FocusOnActivate { get; set; } = true;
            [FSDisplayName("提交后关闭")]
            public bool CloseOnSubmit { get; set; } = false;
            [FSDisplayName("自动滚动模式")]
            public AutoScrollOptions AutoScroll { get; set; } = AutoScrollOptions.OnInvoke;

            [FSHeader("指令设置")]
            [FSDisplayName("自动补全")]
            public bool EnableAutocomplete { get; set; } = true;
            [FSDisplayName("指令提示")]
            public bool ShowPopupDisplay { get; set; } = true;
            [FSDisplayName("指令提示排序")]
            public SortOrder SuggestionDisplayOrder { get; set; } = SortOrder.Descending;
            [FSDisplayName("指令提示文本大小")]
            public int SuggestionFontSize { get; set; } = 16;
            [FSDisplayName("指令提示最大容量")]
            public int MaxSuggestionDisplaySize { get; set; } = 20;
            [FSDisplayName("模糊搜索")]
            public bool UseFuzzySearch { get; set; } = true;
            [FSDisplayName("搜索区分大小写")]
            public bool CaseSensitiveSearch { get; set; } = false;
            [FSDisplayName("折叠重载指令建议")]
            public bool CollapseSuggestionOverloads { get; set; } = true;

            [FSHeader("任务设置")]
            [FSDisplayName("显示当前任务")]
            public bool ShowCurrentJobs { get; set; } = true;
            [FSDisplayName("异步时阻塞")]
            public bool BlockOnAsync { get; set; } = false;

            [FSHeader("指令历史设置")]
            [FSDisplayName("保存指令历史")]
            public bool StoreCommandHistory { get; set; } = true;
            [FSDisplayName("保存重复指令")]
            public bool StoreDuplicateCommands { get; set; } = true;
            [FSDisplayName("保存相邻重复指令")]
            public bool StoreAdjacentDuplicateCommands { get; set; } = false;
            [FSDisplayName("指令历史大小")]
            public int CommandHistorySize { get; set; } = 30;

            public static implicit operator QuantumConsolePreferences(QuantumConsolePreferenceSettings settings)
            {
                var pref = new QuantumConsolePreferences();

                pref.VerboseErrors = settings.VerboseErrors;
                pref.VerboseLogging = settings.VerboseLogging.ToFlags();
                pref.LoggingLevel = settings.LoggingLevel.ToFlags();

                pref.OpenOnLogLevel = settings.OpenOnLogLevel.ToFlags();
                pref.InterceptDebugLogger = settings.InterceptDebugLogger;
                pref.InterceptWhilstInactive = settings.InterceptWhilstInactive;
                pref.PrependTimestamps = settings.PrependTimestamps;

                pref.ActivateOnStartup = settings.ActivateOnStartup; ;
                pref.InitialiseOnStartup = settings.InterceptWhilstInactive;
                pref.FocusOnActivate = settings.FocusOnActivate;
                pref.CloseOnSubmit = settings.CloseOnSubmit;
                pref.AutoScroll = settings.AutoScroll;

                pref.EnableAutocomplete = settings.EnableAutocomplete;
                pref.ShowPopupDisplay = settings.ShowPopupDisplay;
                pref.SuggestionDisplayOrder = settings.SuggestionDisplayOrder;
                pref.MaxSuggestionDisplaySize = settings.MaxSuggestionDisplaySize;
                pref.UseFuzzySearch = settings.UseFuzzySearch;
                pref.CaseSensitiveSearch = settings.CaseSensitiveSearch;
                pref.CollapseSuggestionOverloads = settings.CollapseSuggestionOverloads;

                pref.ShowCurrentJobs = settings.ShowCurrentJobs;
                pref.BlockOnAsync = settings.BlockOnAsync;

                pref.StoreCommandHistory = settings.StoreCommandHistory;
                pref.StoreDuplicateCommands = settings.StoreDuplicateCommands;
                pref.StoreAdjacentDuplicateCommands = settings.StoreAdjacentDuplicateCommands;
                pref.CommandHistorySize = settings.CommandHistorySize;

                pref.MaxStoredLogs = settings.MaxStoredLogs;
                pref.MaxLogSize = settings.MaxLogSize;
                pref.ShowInitLogs = settings.ShowInitLogs;

                pref.LogFontSize = settings.LogFontSize;
                pref.SuggestionFontSize = settings.SuggestionFontSize;
                return pref;
            }
        }

        public class QuantumKeySettings
        {
            [FSDisplayName("提交命令")]
            public KeyCode SubmitCommandKey { get; set; } = KeyCode.Return;
            [FSDisplayName("显示控制台")]
            public KeyCombo ShowConsoleKey { get; set; } = KeyCode.None;
            [FSDisplayName("隐藏控制台")]
            public KeyCombo HideConsoleKey { get; set; } = KeyCode.None;
            [FSDisplayName("切换控制台状态")]
            public KeyCombo ToggleConsoleVisibilityKey { get; set; } = KeyCode.BackQuote;
            [FSDisplayName("放大")]
            public KeyCombo ZoomInKey { get; set; } = new KeyCombo { Key = KeyCode.Equals, Ctrl = true };
            [FSDisplayName("缩小")]
            public KeyCombo ZoomOutKey { get; set; } = new KeyCombo { Key = KeyCode.Minus, Ctrl = true };
            [FSDisplayName("拖动")]
            public KeyCombo DragConsoleKey { get; set; } = new KeyCombo { Key = KeyCode.Mouse0, Shift = true };
            [FSDisplayName("选择下一条建议")]
            public KeyCombo SelectNextSuggestionKey { get; set; } = KeyCode.Tab;
            [FSDisplayName("选择上一条建议")]
            public KeyCombo SelectPreviousSuggestionKey { get; set; } = new KeyCombo { Key = KeyCode.Tab, Shift = true };
            [FSDisplayName("下一条命令")]
            public KeyCode NextCommandKey { get; set; } = KeyCode.UpArrow;
            [FSDisplayName("上一条命令")]
            public KeyCode PreviousCommandKey { get; set; } = KeyCode.DownArrow;
            [FSDisplayName("取消执行")]
            public KeyCombo CancelActionsKey { get; set; } = new KeyCombo { Key = KeyCode.C, Ctrl = true };

            public static implicit operator QuantumKeyConfig(QuantumKeySettings settings)
            {
                var config = new QuantumKeyConfig();
                config.SubmitCommandKey = settings.SubmitCommandKey;
                config.ShowConsoleKey = settings.ShowConsoleKey;
                config.HideConsoleKey = settings.HideConsoleKey;
                config.ToggleConsoleVisibilityKey = settings.ToggleConsoleVisibilityKey;
                config.ZoomInKey = settings.ZoomInKey;
                config.ZoomOutKey = settings.ZoomOutKey;
                config.DragConsoleKey = settings.DragConsoleKey;
                config.SelectNextSuggestionKey = settings.SelectNextSuggestionKey;
                config.SelectPreviousSuggestionKey = settings.SelectPreviousSuggestionKey;
                config.NextCommandKey = settings.NextCommandKey;
                config.PreviousCommandKey = settings.PreviousCommandKey;
                config.CancelActionsKey = settings.CancelActionsKey;
                return config;
            }
        }

        public class KeyCombo
        {
            [FSDisplayName("按键")]
            public KeyCode Key { get; set; } = KeyCode.None;
            [FSDisplayName("Ctrl")]
            public bool Ctrl { get; set; } = false;
            [FSDisplayName("Alt")]
            public bool Alt { get; set; } = false;
            [FSDisplayName("Shift")]
            public bool Shift { get; set; } = false;

            public static implicit operator KeyCombo(KeyCode key)
            {
                return new KeyCombo { Key = key };
            }

            public static implicit operator ModifierKeyCombo(KeyCombo combo)
            {
                return new ModifierKeyCombo() { Key = combo.Key, Alt = combo.Alt, Ctrl = combo.Ctrl, Shift = combo.Shift };
            }
        }

        public override void Init()
        {
            BIELogListener.Init();
            CommandLocalizationHelper.Init();
        }

        public override void OnEnable()
        {
            BIELogListener.Instance.OnEnable();
        }

        public override void OnDisable()
        {
            BIELogListener.Instance.OnDisable();
        }

        private static Dictionary<string, UnityEngine.Object> s_AssetLookup = new();

        public static UnityEngine.Object GetLoadedAsset(string path)
        {
            return s_AssetLookup[path.ToUpper()];
        }


        private class BIELogListener : ILogListener
        {
            public static BIELogListener Instance { get; private set; }

            public static void Init()
            {
                if (Instance != null)
                    return;

                Instance = new();
            }

            public void OnEnable()
            {
                if (!Logger.Listeners.Contains(this))
                {
                    Logger.Listeners.Add(this);
                }
            }

            public void OnDisable()
            {
                if (Logger.Listeners.Contains(this))
                {
                    Logger.Listeners.Remove(this);
                }
            }

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
                return level.GetHighestLevel() switch
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

            public BepInEx.Logging.LogLevel LogLevelFilter => QuantumConsoleSettings.BIELogLevel;
        }

        [ArchivePatch(typeof(GlobalSetup), nameof(GlobalSetup.Awake))]
        private class GlobalSetup__Awake__Patch
        {
            static bool _inited = false;

            private static void Prefix()
            {
                if (!_inited)
                {
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CoroutineCommands>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<KeyBinderModule>();

                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<QuantumConsole>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<DraggableUI>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<BlurShaderController>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<DynamicCanvasScaler>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<ResizableUI>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<ZoomUIController>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<SuggestionDisplay>();

                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<TypeFormatter>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<TypeColorFormatter>();
                    LoaderWrapper.ClassInjector.RegisterTypeInIl2Cpp<CollectionFormatter>();

                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets/quantumconsole");
                    AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                    string[] array = assetBundle.AllAssetNames();
                    foreach (string text in array)
                    {
                        UnityEngine.Object obj = assetBundle.LoadAsset(text);
                        if (obj != null)
                        {
                            s_AssetLookup.Add(text.ToUpper(), obj);
                        }
                    }
                    var console = UnityEngine.Object.Instantiate(GetLoadedAsset("Assets/Plugins/QFSW/Quantum Console/Source/Prefabs/Quantum Console.prefab").Cast<GameObject>()).AddComponent<QuantumConsole>();
                    console.Theme = Settings.ConsoleThemeSettings;
                    console.KeyConfig = Settings.KeySettings;
                    console.Preferences = Settings.ConsolePrefSettings;

                    KeyBinderModule.Init();

                    _inited = true;
                }
            }
        }

        [ArchivePatch(typeof(PlayerChatManager), nameof(PlayerChatManager.UpdateTextChatInput))]
        private class PlayerChatManager__UpdateTextChatInput__Patch
        {
            private static bool Prefix()
            {
                if (QuantumConsole.Instance?.IsActive ?? false)
                {
                    PlayerChatManager.ExitChatIfInChatMode();
                    return false;
                }
                return true;
            }
        }

        [ArchivePatch(typeof(LG_TERM_PlayerInteracting), nameof(LG_TERM_PlayerInteracting.ParseInput))]
        private class LG_TERM_PlayerInteracting__ParseInput__Patch
        {
            private static bool Prefix()
            {
                if (QuantumConsole.Instance?.IsActive ?? false)
                {
                    return false;
                }
                return true;
            }
        }

        [ArchivePatch(typeof(Cursor), nameof(Cursor.visible), null, ArchivePatch.PatchMethodType.Setter)]
        private class Cursor__set_visible__Patch
        {
            private static void Prefix(ref bool value)
            {
                if (QuantumConsole.Instance?.IsActive ?? false)
                {
                    value = true;
                }
            }
        }

        [ArchivePatch(typeof(Cursor), nameof(Cursor.lockState), null, ArchivePatch.PatchMethodType.Setter)]
        private class Cursor__set_lockState__Patch
        {
            private static void Prefix(ref CursorLockMode value)
            {
                if (QuantumConsole.Instance?.IsActive ?? false)
                {
                    value = CursorLockMode.None;
                }
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetAxis))]
        private class InputMapper__DoGetAxis__Patch
        {
            private static bool Prefix(ref float __result)
            {
                if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButton))]
        private class InputMapper__DoGetButton__Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonDown))]
        private class InputMapper__DoGetButtonDown__Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonUp))]
        private class InputMapper__DoGetButtonUp__Patch
        {
            private static bool Prefix(ref bool __result)
            {
                if (Cursor.lockState == CursorLockMode.None || (QuantumConsole.Instance?.IsActive ?? false))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}