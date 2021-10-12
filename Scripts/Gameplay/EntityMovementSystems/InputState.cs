namespace MultiplayerARPG
{
    [System.Flags]
    public enum InputState : byte
    {
        None = 0,
        IsKeyMovement = 1 << 0,
        PositionChanged = 1 << 1,
        RotationChanged = 1 << 2,
        IsJump = 1 << 3,
    }

    public static class InputStateExtensions
    {
        public static bool Has(this InputState self, InputState flag)
        {
            return (self & flag) == flag;
        }
    }
}
