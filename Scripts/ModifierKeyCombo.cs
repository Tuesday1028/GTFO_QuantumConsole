using TheArchive.Core.Attributes.Feature.Settings;
using UnityEngine;

namespace Hikaria.QC
{
    public struct ModifierKeyCombo
    {
        [FSDisplayName("Key")]
        public KeyCode Key { get; set; }
        [FSDisplayName("Ctrl")]
        public bool Ctrl { get; set; }
        [FSDisplayName("Alt")]
        public bool Alt { get; set; }
        [FSDisplayName("Shift")]
        public bool Shift { get; set; }

        public bool ModifiersActive
        {
            get
            {
                bool ctrlDown = !Ctrl ^
                                (InputHelper.GetKey(KeyCode.LeftControl)  ||
                                 InputHelper.GetKey(KeyCode.RightControl) ||
                                 InputHelper.GetKey(KeyCode.LeftCommand)  ||
                                 InputHelper.GetKey(KeyCode.RightCommand));

                bool altDown = !Alt ^ (InputHelper.GetKey(KeyCode.LeftAlt) || InputHelper.GetKey(KeyCode.RightAlt));
                bool shiftDown =
                    !Shift ^ (InputHelper.GetKey(KeyCode.LeftShift) || InputHelper.GetKey(KeyCode.RightShift));

                return ctrlDown && altDown && shiftDown;
            }
        }

        public bool IsHeld()
        {
            return ModifiersActive && InputHelper.GetKey(Key);
        }

        public bool IsPressed()
        {
            return ModifiersActive && InputHelper.GetKeyDown(Key);
        }

        public static implicit operator ModifierKeyCombo(KeyCode key)
        {
            return new ModifierKeyCombo { Key = key };
        }
    }
}
