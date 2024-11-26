using GameData;
using Hikaria.QC.Extras;
using Hikaria.QC.UI;
using System.Reflection;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;
using TheArchive.Loader;
using UnityEngine;

namespace Hikaria.QC.Loader
{
    [EnableFeatureByDefault]
    [DisallowInGameToggle]
    [HideInModSettings]
    internal class QuantumConsoleLoader : Feature
    {
        public override string Name => "量子终端加载器";

        public static new ILocalizationService Localization { get; set; }

        private static Dictionary<string, UnityEngine.Object> s_AssetLookup = new();

        public static UnityEngine.Object GetLoadedAsset(string path)
        {
            return s_AssetLookup[path.ToUpper()];
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
                if (QuantumConsole.Instance == null)
                    return;

                if (QuantumConsole.Instance.IsActive)
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
                if (QuantumConsole.Instance == null)
                    return;

                if (QuantumConsole.Instance.IsActive)
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
                bool flag = Cursor.lockState > CursorLockMode.None;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    __result = 0f;
                    result = false;
                }
                return result;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButton))]
        private class InputMapper__DoGetButton__Patch
        {
            private static bool Prefix(bool __result)
            {
                bool flag = Cursor.lockState > CursorLockMode.None;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    __result = false;
                    result = false;
                }
                return result;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonDown))]
        private class InputMapper__DoGetButtonDown__Patch
        {
            private static bool Prefix(bool __result)
            {
                bool flag = Cursor.lockState > CursorLockMode.None;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    __result = false;
                    result = false;
                }
                return result;
            }
        }

        [ArchivePatch(typeof(InputMapper), nameof(InputMapper.DoGetButtonUp))]
        private class InputMapper__DoGetButtonUp__Patch
        {
            private static bool Prefix(bool __result)
            {
                bool flag = Cursor.lockState > CursorLockMode.None;
                bool result;
                if (flag)
                {
                    result = true;
                }
                else
                {
                    __result = false;
                    result = false;
                }
                return result;
            }
        }
    }
}
