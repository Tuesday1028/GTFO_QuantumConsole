using UnityEngine;

namespace Hikaria.QC.Suggestors
{
    public class GameObjectSuggestor : BasicCachedQcSuggestor<string>
    {
        protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
        {
            return context.TargetType == typeof(GameObject);
        }

        protected override IQcSuggestion ItemToSuggestion(string name)
        {
            return new RawSuggestion(name, true);
        }

        protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
        {
            return UnityEngine.Object.FindObjectsOfType<GameObject>()
                .Select(obj => obj.name);
        }
    }
}