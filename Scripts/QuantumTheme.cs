using Hikaria.QC.Loader;
using Hikaria.QC.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Hikaria.QC
{
    public class QuantumTheme
    {
        public static QuantumTheme DefaultTheme()
        {
            QuantumTheme quantumTheme = new QuantumTheme();
            TypeColorFormatter typeColorFormatter = new TypeColorFormatter(typeof(string));
            typeColorFormatter.Color = Color.white;
            quantumTheme.TypeFormatters.Add(typeColorFormatter);
            TypeColorFormatter typeColorFormatter2 = new TypeColorFormatter(typeof(IEnumerable));
            typeColorFormatter2.Color = ColorExt.Hex("FDF269");
            quantumTheme.TypeFormatters.Add(typeColorFormatter2);
            TypeColorFormatter typeColorFormatter3 = new TypeColorFormatter(typeof(KeyValuePair));
            typeColorFormatter3.Color = ColorExt.Hex("BAFFEE");
            quantumTheme.TypeFormatters.Add(typeColorFormatter3);
            TypeColorFormatter typeColorFormatter4 = new TypeColorFormatter(typeof(DictionaryEntry));
            typeColorFormatter4.Color = ColorExt.Hex("BAFFEE");
            quantumTheme.TypeFormatters.Add(typeColorFormatter4);
            TypeColorFormatter typeColorFormatter5 = new TypeColorFormatter(typeof(Enum));
            typeColorFormatter5.Color = ColorExt.Hex("C4FF8C");
            quantumTheme.TypeFormatters.Add(typeColorFormatter5);
            TypeColorFormatter typeColorFormatter6 = new TypeColorFormatter(typeof(object));
            typeColorFormatter6.Color = ColorExt.Hex("F799FF");
            quantumTheme.TypeFormatters.Add(typeColorFormatter6);
            CollectionFormatter collectionFormatter = new CollectionFormatter(typeof(Dictionary<,>));
            collectionFormatter.LeftScoper = string.Empty;
            collectionFormatter.SeperatorString = "\n";
            collectionFormatter.RightScoper = string.Empty;
            quantumTheme.CollectionFormatters.Add(collectionFormatter);
            CollectionFormatter collectionFormatter2 = new CollectionFormatter(typeof(ICollection));
            quantumTheme.CollectionFormatters.Add(collectionFormatter2);
            CollectionFormatter collectionFormatter3 = new CollectionFormatter(typeof(IEnumerable));
            collectionFormatter3.LeftScoper = string.Empty;
            collectionFormatter3.SeperatorString = "\n";
            collectionFormatter3.RightScoper = string.Empty;
            quantumTheme.CollectionFormatters.Add(collectionFormatter3);
            return quantumTheme;
        }

        public TMP_FontAsset Font => QuantumConsoleLoader.GetLoadedAsset(FontAssetPath).Cast<TMP_FontAsset>();
        public Material PanelMaterial => QuantumConsoleLoader.GetLoadedAsset(PanelMaterialAssetPath).Cast<Material>();

        public string FontAssetPath = "Assets/Plugins/QFSW/Quantum Console/Source/Fonts/TMP/OfficeCodePro-Regular SDF.asset";
        public string PanelMaterialAssetPath = "Assets/Plugins/QFSW/Quantum Console/Source/Materials/Blur Panel.mat";

        public Color PanelColor = Color.white;
        public Color CommandLogColor = Color.cyan;
        public Color SelectedSuggestionColor = new(1, 1, 0.55f);
        public Color SuggestionColor = Color.gray;
        public Color ErrorColor = Color.red;
        public Color FatalColor = Color.red;
        public Color WarningColor = Color.yellow;
        public Color MessageColor = Color.white;
        public Color DebugColor = ColorExtensions.DarkGray;
        public Color InfoColor = ColorExtensions.DarkGray;

        public Color SuccessColor = Color.green;

        public string TimestampFormat = "[{0:00}:{1:00}:{2:00}]";
        public string CommandLogFormat = "> {0}";

        public Color DefaultReturnValueColor = Color.white;
        public List<TypeColorFormatter> TypeFormatters = new List<TypeColorFormatter>(0);
        public List<CollectionFormatter> CollectionFormatters = new List<CollectionFormatter>(0);

        private T FindTypeFormatter<T>(List<T> formatters, Type type) where T : TypeFormatter
        {
            foreach (T formatter in formatters)
            {
                if (type == formatter.Type || type.IsGenericTypeOf(formatter.Type))
                {
                    return formatter;
                }
            }

            foreach (T formatter in formatters)
            {
                if (formatter.Type.IsAssignableFrom(type))
                {
                    return formatter;
                }
            }

            return null;
        }

        public string ColorizeReturn(string data, Type type)
        {
            TypeColorFormatter formatter = FindTypeFormatter(TypeFormatters, type);
            if (formatter == null) { return data.ColorText(DefaultReturnValueColor); }
            else { return data.ColorText(formatter.Color); }
        }

        public void GetCollectionFormatting(Type type, out string leftScoper, out string seperator, out string rightScoper)
        {
            CollectionFormatter formatter = FindTypeFormatter(CollectionFormatters, type);
            if (formatter == null)
            {
                leftScoper = "[";
                seperator = ",";
                rightScoper = "]";
            }
            else
            {
                leftScoper = formatter.LeftScoper.Replace("\\n", "\n");
                seperator = formatter.SeperatorString.Replace("\\n", "\n");
                rightScoper = formatter.RightScoper.Replace("\\n", "\n");
            }
        }
    }
}
