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
        IsJump = 1 << 5,
        IsSwimming = 1 << 6,
        IsFlying = 1 << 7,
    }

    [System.Flags]
    public enum ExtraMovementState : byte
    {
        None = 0,
        IsSprinting = 1 << 0,
        IsCrouching = 1 << 1,
        IsCrawling = 1 << 2,
    }
}
