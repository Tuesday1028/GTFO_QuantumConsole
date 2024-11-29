using BepInEx.Logging;
using GameData;
using Hikaria.QC.Extras;
using Hikaria.QC.UI;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.Attributes.Feature.Settings;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Loader;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Hikaria.QC.Bootstrap
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    internal class QuantumConsoleBootstrap : Feature
    {
        public override string Name => "量子终端引导";

        public static new ILocalizationService Localization { get; set; }

        public override Type[] LocalizationExternalTypes => new[]
        {
            typeof(LogLevel)
        };

        [FeatureConfig]
        public static QuantumConsoleSettings Settings { get; set; }
        public class QuantumConsoleSettings
        {
            [FSDisplayName("BepInEx 日志监听级别")]
            public List<LogLevel> BIEListenLevel
            {
                get
                {
                    return _bieListenLevel;
                }
                set
                {
                    _bieListenLevel = value;

                    _bieLogLevel = BepInEx.Logging.LogLevel.None;
                    foreach (var level in _bieListenLevel)
                    {
                        _bieLogLevel |= (BepInEx.Logging.LogLevel)(int)level;
                    }
                }
            }

            [FSDisplayName("按键设置")]

            public QuantumKeyConfig KeyConfig { get; set; } = QuantumKeyConfig.DefaultKeyConfig();

            [FSDisplayName("终端设置")]
            public QuantumSettings ConsoleSettings { get;set; } = QuantumSettings.DefaultSettings();

            public static BepInEx.Logging.LogLevel BIELogLevel => _bieLogLevel;
            private static BepInEx.Logging.LogLevel _bieLogLevel = BepInEx.Logging.LogLevel.Message | BepInEx.Logging.LogLevel.Warning | BepInEx.Logging.LogLevel.Error | BepInEx.Logging.LogLevel.Fatal;
            private List<LogLevel> _bieListenLevel = new();
        }

        public override void Init()
        {
            BIELogListener.Init();
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

                if (!Settings.BIEListenLevel.Any())
                {
                    Settings.BIEListenLevel.Add(LogLevel.Fatal);
                    Settings.BIEListenLevel.Add(LogLevel.Error);
                    Settings.BIEListenLevel.Add(LogLevel.Warning);
                    Settings.BIEListenLevel.Add(LogLevel.Message);
                }
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

        [ArchivePatch(typeof(GameDataInit), nameof(GameDataInit.Initialize))]
        private class GameDataInit__Initialize__Patch
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
                    UnityEngine.Object.Instantiate(GetLoadedAsset("Assets/Plugins/QFSW/Quantum Console/Source/Prefabs/Quantum Console.prefab").Cast<GameObject>()).AddComponent<QuantumConsole>();
                    _inited = true;
                }
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