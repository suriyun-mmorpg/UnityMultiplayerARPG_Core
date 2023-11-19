using UnityEngine;

namespace MultiplayerARPG
{
    public class TagHotkeyJoystickForDialogControlling : MonoBehaviour
    {
        public IHotkeyJoystickEventHandler HotkeyJoystickEventHandler { get; private set; }

        private void Awake()
        {
            HotkeyJoystickEventHandler = GetComponent<IHotkeyJoystickEventHandler>();
            if (HotkeyJoystickEventHandler != null)
                UICharacterHotkeys.s_hotkeyJoystickForDialogControlling = HotkeyJoystickEventHandler;
        }
    }
}