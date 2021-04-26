namespace MultiplayerARPG
{
    public interface IHotkeyJoystickEventHandler
    {
        UICharacterHotkey UICharacterHotkey { get; }
        bool Interactable { get; }
        bool IsDragging { get; }
        AimPosition AimPosition { get; }
        void UpdateEvent();
    }
}
