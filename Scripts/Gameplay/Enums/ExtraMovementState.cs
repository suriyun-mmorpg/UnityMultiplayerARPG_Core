namespace MultiplayerARPG
{
    /// <summary>
    /// Toggleable movement states
    /// </summary>
    public enum ExtraMovementState : byte
    {
        None,
        IsSprinting,
        IsWalking,
        IsCrouching,
        IsCrawling,
        IsFlying,
    }

    public static class ExtraMovementStateExtensions
    {
        public static bool IsStanding(this ExtraMovementState self)
        {
            return self == ExtraMovementState.None ||
                self == ExtraMovementState.IsSprinting ||
                self == ExtraMovementState.IsWalking ||
                self == ExtraMovementState.IsFlying;
        }
    }
}
