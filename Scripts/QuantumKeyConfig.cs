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

        public KeyCode SubmitCommandKey = KeyCode.Return;
        public ModifierKeyCombo ShowConsoleKey = KeyCode.None;
        public ModifierKeyCombo HideConsoleKey = KeyCode.None;
        public ModifierKeyCombo ToggleConsoleVisibilityKey = KeyCode.BackQuote;

        public ModifierKeyCombo ZoomInKey = new ModifierKeyCombo { Key = KeyCode.Equals, Ctrl = true };
        public ModifierKeyCombo ZoomOutKey = new ModifierKeyCombo { Key = KeyCode.Minus, Ctrl = true };
        public ModifierKeyCombo DragConsoleKey = new ModifierKeyCombo { Key = KeyCode.Mouse0, Shift = true };

        public ModifierKeyCombo SelectNextSuggestionKey = KeyCode.Tab;
        public ModifierKeyCombo SelectPreviousSuggestionKey = new ModifierKeyCombo { Key = KeyCode.Tab, Shift = true };

        public KeyCode NextCommandKey = KeyCode.UpArrow;
        public KeyCode PreviousCommandKey = KeyCode.DownArrow;

        public ModifierKeyCombo CancelActionsKey = new ModifierKeyCombo { Key = KeyCode.C, Ctrl = true };
    }
}
