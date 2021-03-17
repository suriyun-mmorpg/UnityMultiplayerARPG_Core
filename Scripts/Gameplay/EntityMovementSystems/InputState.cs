namespace MultiplayerARPG
{
    [System.Flags]
    public enum InputState : int
    {
        None = 0,
        IsKeyMovement = 1 << 0,
        PositionChanged = 1 << 1,
        RotationChanged = 1 << 2,
        IsJump = 1 << 3,
    }
}
