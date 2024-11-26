using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hikaria.QC
{
    public class SuggestionDisplay : MonoBehaviour
    {
        private QuantumConsole _quantumConsole = null;
        private TextMeshProUGUI _textArea = null;

        internal static void Setup(SuggestionDisplay self, QuantumConsole console, TextMeshProUGUI textArea)
        {
            self._quantumConsole = console;
            self._textArea = textArea;

            var eventTrigger = self.gameObject.AddComponent<EventTrigger>();

            var onPointerClickEntry = new EventTrigger.Entry();
            onPointerClickEntry.eventID = EventTriggerType.PointerClick;
            onPointerClickEntry.callback.AddListener(new Action<BaseEventData>((data) => { self.OnPointerClick(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onPointerClickEntry);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textArea, eventData.position, null);
            if (linkIndex >= 0)
            {
                TMP_LinkInfo link = _textArea.textInfo.linkInfo[linkIndex];
                if (int.TryParse(link.GetLinkID(), out int suggestionIndex))
                {
                    _quantumConsole.SetSuggestion(suggestionIndex);
                }
            }
        }
    }
}