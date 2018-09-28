namespace MultiplayerARPG
{
    [System.Flags]
    public enum GuildMemberFlags : byte
    {
        None = 0,
        IsOnline = 1 << 0,
        IsLeader = 1 << 1,
        CanInvite = 1 << 2,
        CanKick = 1 << 3,
    }
}
