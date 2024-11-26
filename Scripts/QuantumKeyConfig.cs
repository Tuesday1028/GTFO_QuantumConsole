﻿using UnityEngine;

namespace Hikaria.QC
{
    public class QuantumKeyConfig
    {
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
