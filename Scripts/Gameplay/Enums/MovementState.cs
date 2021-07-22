namespace MultiplayerARPG
{
    [System.Flags]
    public enum MovementState : int
    {
        None = 0,
        Forward = 1 << 0,
        Backward = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        IsGrounded = 1 << 4,
        IsUnderWater = 1 << 5,
        IsJump = 1 << 6,
        IsSprinting = 1 << 7,
        IsWalking = 1 << 8,
        IsCrouching = 1 << 9,
        IsCrawling = 1 << 10,
    }
}
