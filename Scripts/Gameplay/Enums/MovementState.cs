namespace MultiplayerARPG
{
    [System.Flags]
    public enum MovementState : uint
    {
        None = 0U,
        Forward = 1U << 0, // 1
        Backward = 1U << 1, // 2
        Left = 1U << 2, // 4
        Right = 1U << 3, // 8
        IsGrounded = 1U << 4, // 16
        IsUnderWater = 1U << 5, // 32
        IsJump = 1U << 6, // 64
        IsTeleport = 1U << 7, // 128, end of byte
        Up = 1U << 8, // 256
        Down = 1U << 9, // 512
        IsClimbing = 1U << 10, // 1024
        IsDash = 1U << 11, // 2048
        IsEvenStep = 1U << 12, // 4096
        IsStarting = 1U << 13, // 8192
        IsEnding = 1U << 14, // 16384
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
