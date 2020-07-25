using UnityEngine;

namespace MultiplayerARPG
{
    public interface IHotkeyJoystickEventHandler
    {
        UICharacterHotkey UICharacterHotkey { get; }
        bool Interactable { get; }
        bool IsDragging { get; }
        Vector3? AimPosition { get; }
        void UpdateEvent();
    }
}
