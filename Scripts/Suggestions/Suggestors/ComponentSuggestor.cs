using Hikaria.QC.Utilities;
using Il2CppInterop.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hikaria.QC.Suggestors
{
    public class ComponentSuggestor : BasicCachedQcSuggestor<string>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            Type targetType = context.TargetType;
            return targetType != null
                && targetType.IsDerivedTypeOf(typeof(Component))
                && !targetType.IsGenericParameter;
        }

        protected override IQcSuggestion ItemToSuggestion(string name)
        {
            return new RawSuggestion(name, true);
        }

        protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return Object.FindObjectsOfType(Il2CppType.From(context.TargetType))
                .Select(cmp => cmp.Cast<Component>())
                .Select(cmp => cmp.gameObject.name);
        }
    }
}