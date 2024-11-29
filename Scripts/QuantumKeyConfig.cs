using TheArchive.Core.Attributes.Feature.Settings;
using UnityEngine;

namespace Hikaria.QC
{
    public class QuantumKeyConfig
    {
        public static QuantumKeyConfig DefaultKeyConfig()
        {
            QuantumKeyConfig config = new();
            config.SubmitCommandKey = KeyCode.Return;
            config.ShowConsoleKey = KeyCode.None;
            config.HideConsoleKey = KeyCode.None;
            config.ToggleConsoleVisibilityKey = KeyCode.BackQuote;
            config.ZoomInKey = new ModifierKeyCombo { Key = KeyCode.Equals, Ctrl = true };
            config.ZoomOutKey = new ModifierKeyCombo { Key = KeyCode.Minus, Ctrl = true };
            config.DragConsoleKey = new ModifierKeyCombo { Key = KeyCode.Mouse0, Shift = true };
            config.SelectNextSuggestionKey = KeyCode.Tab;
            config.SelectPreviousSuggestionKey = new ModifierKeyCombo { Key = KeyCode.Tab, Shift = true };
            config.NextCommandKey = KeyCode.UpArrow;
            config.PreviousCommandKey = KeyCode.DownArrow;
            config.CancelActionsKey = new ModifierKeyCombo { Key = KeyCode.C, Ctrl = true };
            return config;
        }

        [FSDisplayName("Submit Command Key")]
        public KeyCode SubmitCommandKey { get; set; } = KeyCode.Return;
        [FSDisplayName("Show Console Key")]
        public ModifierKeyCombo ShowConsoleKey { get; set; } = KeyCode.None;
        [FSDisplayName("Hide Console Key")]
        public ModifierKeyCombo HideConsoleKey { get; set; } = KeyCode.None;
        [FSDisplayName("Toggle Console Key")]
        public ModifierKeyCombo ToggleConsoleVisibilityKey { get; set; } = KeyCode.BackQuote;

        [FSDisplayName("Zoom In Key")]
        public ModifierKeyCombo ZoomInKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Equals, Ctrl = true };
        [FSDisplayName("Zoom Out Key")]
        public ModifierKeyCombo ZoomOutKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Minus, Ctrl = true };
        [FSDisplayName("Drag Console Key")]
        public ModifierKeyCombo DragConsoleKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Mouse0, Shift = true };

        [FSDisplayName("Select Next Suggestion Key")]
        public ModifierKeyCombo SelectNextSuggestionKey { get; set; } = KeyCode.Tab;
        [FSDisplayName("Select Previous Suggestion Key")]
        public ModifierKeyCombo SelectPreviousSuggestionKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Tab, Shift = true };

        [FSDisplayName("Next Command Key")]
        public KeyCode NextCommandKey { get; set; } = KeyCode.UpArrow;
        [FSDisplayName("Previous Command Key")]
        public KeyCode PreviousCommandKey { get; set; } = KeyCode.DownArrow;

        [FSDisplayName("Cancel Actions Key")]
        public ModifierKeyCombo CancelActionsKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.C, Ctrl = true };
    }
}
