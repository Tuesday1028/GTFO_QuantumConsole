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

        public KeyCode SubmitCommandKey { get; set; } = KeyCode.Return;
        public ModifierKeyCombo ShowConsoleKey { get; set; } = KeyCode.None;
        public ModifierKeyCombo HideConsoleKey { get; set; } = KeyCode.None;
        public ModifierKeyCombo ToggleConsoleVisibilityKey { get; set; } = KeyCode.BackQuote;
        public ModifierKeyCombo ZoomInKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Equals, Ctrl = true };
        public ModifierKeyCombo ZoomOutKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Minus, Ctrl = true };
        public ModifierKeyCombo DragConsoleKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Mouse0, Shift = true };
        public ModifierKeyCombo SelectNextSuggestionKey { get; set; } = KeyCode.Tab;
        public ModifierKeyCombo SelectPreviousSuggestionKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.Tab, Shift = true };
        public KeyCode NextCommandKey { get; set; } = KeyCode.UpArrow;
        public KeyCode PreviousCommandKey { get; set; } = KeyCode.DownArrow;
        public ModifierKeyCombo CancelActionsKey { get; set; } = new ModifierKeyCombo { Key = KeyCode.C, Ctrl = true };
    }
}
