﻿using TheArchive.Core;
using TheArchive.Core.Attributes;
using TheArchive.Core.FeaturesAPI;
using TheArchive.Core.Localization;

namespace Hikaria.QC
{
    [ArchiveModule(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class EntryPoint : IArchiveModule
    {
        public void Init()
        {
            Logs.LogMessage("OK");
        }

        public void OnExit()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
        }

        public bool ApplyHarmonyPatches => false;

        public bool UsesLegacyPatches => false;

        public ArchiveLegacyPatcher Patcher { get; set; }

        public string ModuleGroup => FeatureGroups.GetOrCreateModuleGroup("Quantum Console", new()
        {
            { Language.English, "Quantum Console" }, { Language.Chinese, "量子终端" }
        });
    }
}
