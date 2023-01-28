using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkey))]
    public class UIStaticHotkeyJoystickEventHandler : MonoBehaviour, IHotkeyJoystickEventHandler
    {
        public UICharacterHotkey UICharacterHotkey { get; private set; }
        public bool Interactable { get { return UICharacterHotkey.IsAssigned(); } }
        public bool IsDragging { get { return false; } }
        public AimPosition AimPosition { get; private set; }

        private void Start()
        {
            UICharacterHotkey = GetComponent<UICharacterHotkey>();
            UICharacterHotkey.UICharacterHotkeys.RegisterHotkeyJoystick(this);
        }

        public void UpdateEvent()
        {
            if (UICharacterHotkeys.UsingHotkey == null || UICharacterHotkeys.UsingHotkey != UICharacterHotkey)
                return;
            AimPosition = UICharacterHotkey.UpdateAimControls(Vector2.zero);
            if (InputManager.GetButtonUp("Fire1"))
                UICharacterHotkeys.FinishHotkeyAimControls(false);
        }

        public void OnClickUse()
        {
            if (UICharacterHotkey.HasCustomAimControls())
            {
                // Set hotkey to aim
                UICharacterHotkeys.SetUsingHotkey(UICharacterHotkey);
            }
            else
            {
                // Use it immediately
                UICharacterHotkeys.SetUsingHotkey(null);
                UICharacterHotkey.OnClickUse();
            }
        }
    }
}
