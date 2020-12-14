namespace MultiplayerARPG
{
    public struct GuildInvitationData
    {
        public string InviterId { get; set; }
        public string InviterName { get; set; }
        public short InviterLevel { get; set; }
        public int GuildId { get; set; }
        public string GuildName { get; set; }
        public short GuildLevel { get; set; }
    }
}
