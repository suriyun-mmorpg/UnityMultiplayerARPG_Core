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
        IsSprinting = 1 << 5,
        IsJump = 1 << 6,
    }
}
