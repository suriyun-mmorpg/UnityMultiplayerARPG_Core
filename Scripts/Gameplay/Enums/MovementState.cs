namespace MultiplayerARPG
{
    [System.Flags]
    public enum MovementState : byte
    {
        None = 0,
        Forward = 1 << 0,
        Backward = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        IsGrounded = 1 << 4,
        IsUnderWater = 1 << 5,
        IsJump = 1 << 6,
        IsTeleport = 1 << 7,
    }

    public static class MovementStateExtensions
    {
        public static bool Has(this MovementState self, MovementState flag)
        {
            return (self & flag) == flag;
        }

        public static bool HasDirectionMovement(this MovementState self)
        {
            return self.Has(MovementState.Forward) ||
                self.Has(MovementState.Backward) ||
                self.Has(MovementState.Right) ||
                self.Has(MovementState.Left);
        }
    }
}
