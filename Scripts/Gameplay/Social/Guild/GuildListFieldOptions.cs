namespace MultiplayerARPG
{
    [System.Flags]
    public enum GuildListFieldOptions : int
    {
        None = 0,
        GuildMessage = 1 << 0,
        GuildMessage2 = 1 << 1,
        Score = 1 << 2,
        OptionId1 = 1 << 3,
        OptionId2 = 1 << 4,
        OptionId3 = 1 << 5,
        OptionId4 = 1 << 6,
        OptionId5 = 1 << 7,
        AutoAcceptRequests = 1 << 8,
        Rank = 1 << 9,
        CurrentMembers = 1 << 10,
        MaxMembers = 1 << 11,
        All = GuildMessage | GuildMessage2 | Score | OptionId1 | OptionId2 | OptionId3 | OptionId4 | OptionId5 | AutoAcceptRequests | Rank | CurrentMembers | MaxMembers,
    }
}
