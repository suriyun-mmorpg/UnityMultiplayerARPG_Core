using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkey))]
    public class UICharacterHotkeyJoystickEventHandler : MonoBehaviour
    {
        public static readonly List<UICharacterHotkeyJoystickEventHandler> Joysticks = new List<UICharacterHotkeyJoystickEventHandler>();
        public UICharacterHotkey CacheHotkey { get; private set; }
        private MobileMovementJoystick joystick;
        private string hotkeyAxisNameX;
        private string hotkeyAxisNameY;
        private RectTransform hotkeyCancelArea;
        private Vector2 hotkeyAxes;
        private bool hotkeyCancel;
        public bool Interactable { get { return CacheHotkey.IsAssigned(); } }
        public bool IsDragging { get; private set; }
        public Vector3? AimPosition { get; private set; }

        private void Start()
        {
            CacheHotkey = GetComponent<UICharacterHotkey>();
            joystick = Instantiate(CacheHotkey.UICharacterHotkeys.hotkeyAimJoyStickPrefab, CacheHotkey.transform.parent);
            joystick.gameObject.SetActive(true);
            joystick.transform.localPosition = CacheHotkey.transform.localPosition;
            joystick.axisXName = hotkeyAxisNameX = UICharacterHotkeys.HOTKEY_AXIS_X + "_" + CacheHotkey.hotkeyId;
            joystick.axisYName = hotkeyAxisNameY = UICharacterHotkeys.HOTKEY_AXIS_Y + "_" + CacheHotkey.hotkeyId;
            joystick.SetAsLastSiblingOnDrag = true;
            joystick.HideWhileIdle = true;
            joystick.Interactable = true;
            CacheHotkey.UICharacterHotkeys.RegisterHotkeyJoystick(this);
            hotkeyCancelArea = CacheHotkey.UICharacterHotkeys.hotkeyCancelArea;
        }

        public void UpdateEvent()
        {
            joystick.Interactable = Interactable;

            if (!IsDragging && joystick.IsDragging)
            {
                CacheHotkey.UICharacterHotkeys.SetUsingHotkey(CacheHotkey);
                IsDragging = true;
            }

            // Can dragging only 1 hotkey each time, so check with latest dragging hotkey
            // If it's not this hotkey then set dragging state to false 
            // To check joystick's started dragging state next time
            if (UICharacterHotkeys.UsingHotkey != CacheHotkey)
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
                AimPosition = CacheHotkey.UpdateAimControls(hotkeyAxes);
            }

            if (IsDragging && !joystick.IsDragging)
            {
                CacheHotkey.UICharacterHotkeys.FinishHotkeyAimControls(hotkeyCancel);
                IsDragging = false;
            }
        }
    }
}
