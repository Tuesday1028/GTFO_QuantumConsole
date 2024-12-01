using Clonesoft.Json;
using System.Collections.Concurrent;
using System.Reflection;
using TheArchive;
using TheArchive.Core.Localization;
using TheArchive.Utilities;
using UnityEngine;

namespace Hikaria.QC
{
    internal static class CommandLocalizationHelper
    {
        public static void Init()
        {
            Application.add_quitting((Action)OnApplicationQuit);
        }

        public static void LoadCommandLocalizationData(this CommandData command)
        {
            var assembly = command.MethodData.DeclaringType.Assembly;
            if (_commandLocalizationLookup.TryAdd(assembly, new()))
            {
                var path = Path.Combine(Path.GetDirectoryName(assembly.Location), "Localization");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var file = Path.Combine(path, $"{assembly.GetName().Name}_QuantumCommands_Localization.json");
                if (File.Exists(file))
                {
                    foreach (var kvp in JsonConvert.DeserializeObject<Dictionary<string, Dictionary<Language, CommandLocalizationData>>>(File.ReadAllText(file), ArchiveMod.JsonSerializerSettings))
                        _commandLocalizationLookup[assembly][kvp.Key] = kvp.Value;
                }
            }

            if (!_commandLocalizationLookup[assembly].TryGetValue(command.LocalizationSignature, out var localization))
            {
                localization = GenerateCommandLocalizationData(command);
            }
            command.ApplyLocalization(localization);
            _commandLocalizationLookup[assembly][command.LocalizationSignature] = localization;
        }

        private static Dictionary<Language, CommandLocalizationData> GenerateCommandLocalizationData(CommandData command)
        {
            var localization = new Dictionary<Language, CommandLocalizationData>();
            foreach (var language in Enum.GetValues<Language>())
            {
                if (!command.TryGetLocalization(language, out var data))
                    data = new();
                data.Description = command.CommandDescription;
                data.ParameterDescriptions = new();
                foreach (var info in command.MethodParamData)
                    if (!data.ParameterDescriptions.ContainsKey(info.Name))
                        data.ParameterDescriptions[info.Name] = info.GetCustomAttribute<CommandParameterDescriptionAttribute>()?.Description ?? null;
                localization[language] = data;
            }
            return localization;
        }

        private static void OnApplicationQuit()
        {
            foreach (var kvp in _commandLocalizationLookup)
            {
                CheckAndSaveLocalization(kvp.Key);
            }
        }

        private static void CheckAndSaveLocalization(Assembly assembly)
        {
            var path = Path.Combine(Path.GetDirectoryName(assembly.Location), "Localization");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var file = Path.Combine(path, $"{assembly.GetName().Name}_QuantumCommands_Localization.json");
            if (!File.Exists(file))
            {
                if (!_commandLocalizationLookup.TryGetValue(assembly, out var lookup))
                    lookup = new();
                File.WriteAllText(file, JsonConvert.SerializeObject(lookup, ArchiveMod.JsonSerializerSettings));
                return;
            }
            var fileData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<Language, CommandLocalizationData>>>(File.ReadAllText(file), ArchiveMod.JsonSerializerSettings);
            var fjson = JsonConvert.SerializeObject(fileData, ArchiveMod.JsonSerializerSettings);
            var cdata = _commandLocalizationLookup[assembly].OrderBy(dict => dict.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var cjson = JsonConvert.SerializeObject(cdata, ArchiveMod.JsonSerializerSettings);
            if (cjson.HashString() != fjson.HashString())
                File.WriteAllText(file, cjson);
        }

        private static ConcurrentDictionary<Assembly, Dictionary<string, Dictionary<Language, CommandLocalizationData>>> _commandLocalizationLookup = new();
    }


    internal class CommandLocalizationData
    {
        public string Description { get; set; }

        public Dictionary<string, string> ParameterDescriptions { get; set; }
    }
}
