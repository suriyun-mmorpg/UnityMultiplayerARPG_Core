namespace MultiplayerARPG
{
    public enum ExtraMovementState : byte
    {
        None,
        IsSprinting,
        IsWalking,
        IsCrouching,
        IsCrawling,
        IsStandToCrouchChanging,
        IsCrouchToStandChanging,
        IsStandToCrawlChanging,
        IsCrawlToStandChanging,
        IsCrouchToCrawlChanging,
        IsCrawlToCrouchChanging,
        IsFlying,
        IsStandToFlyChanging,
        IsFlyToStandChanging,
    }
}
