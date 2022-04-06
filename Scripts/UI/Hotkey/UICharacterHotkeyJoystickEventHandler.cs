using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkey))]
    public class UICharacterHotkeyJoystickEventHandler : MonoBehaviour, IHotkeyJoystickEventHandler
    {
        public const string HOTKEY_AXIS_X = "HotkeyAxisX";
        public const string HOTKEY_AXIS_Y = "HotkeyAxisY";
        public UICharacterHotkey UICharacterHotkey { get; private set; }
        private MobileMovementJoystick joystick;
        private string hotkeyAxisNameX;
        private string hotkeyAxisNameY;
        private RectTransform hotkeyCancelArea;
        private Vector2 hotkeyAxes;
        private bool hotkeyCancel;
        public bool Interactable { get { return UICharacterHotkey.IsAssigned(); } }
        public bool IsDragging { get; private set; }
        public AimPosition AimPosition { get; private set; }

        private void Start()
        {
            UICharacterHotkey = GetComponent<UICharacterHotkey>();
            joystick = Instantiate(UICharacterHotkey.UICharacterHotkeys.hotkeyAimJoyStickPrefab, UICharacterHotkey.transform.parent);
            joystick.gameObject.SetActive(true);
            joystick.transform.localPosition = UICharacterHotkey.transform.localPosition;
            joystick.axisXName = hotkeyAxisNameX = HOTKEY_AXIS_X + "_" + UICharacterHotkey.hotkeyId;
            joystick.axisYName = hotkeyAxisNameY = HOTKEY_AXIS_Y + "_" + UICharacterHotkey.hotkeyId;
            joystick.SetAsLastSiblingOnDrag = true;
            joystick.HideWhileIdle = true;
            joystick.Interactable = true;
            UICharacterHotkey.UICharacterHotkeys.RegisterHotkeyJoystick(this);
            hotkeyCancelArea = UICharacterHotkey.UICharacterHotkeys.hotkeyCancelArea;
        }

        public void UpdateEvent()
        {
            joystick.Interactable = Interactable;

            if (!IsDragging && joystick.IsDragging)
            {
                UICharacterHotkeys.SetUsingHotkey(UICharacterHotkey);
                IsDragging = true;
            }

            // Can dragging only 1 hotkey each time, so check with latest dragging hotkey
            // If it's not this hotkey then set dragging state to false 
            // To check joystick's started dragging state next time
            if (UICharacterHotkeys.UsingHotkey == null || UICharacterHotkeys.UsingHotkey != UICharacterHotkey)
            {
                IsDragging = false;
                return;
            }

            hotkeyAxes = new Vector2(InputManager.GetAxis(hotkeyAxisNameX, false), InputManager.GetAxis(hotkeyAxisNameY, false));
            hotkeyCancel = false;

            if (hotkeyCancelArea != null)
            {
                if (hotkeyCancelArea.rect.Contains(hotkeyCancelArea.InverseTransformPoint(joystick.CurrentPosition)))
                {
                    // Cursor position is inside hotkey cancel area so cancel the hotkey
                    hotkeyCancel = true;
                }
            }

            if (IsDragging && joystick.IsDragging)
            {
                AimPosition = UICharacterHotkey.UpdateAimControls(hotkeyAxes);
            }

            if (IsDragging && !joystick.IsDragging)
            {
                UICharacterHotkeys.FinishHotkeyAimControls(hotkeyCancel);
                IsDragging = false;
            }
        }
    }
}
