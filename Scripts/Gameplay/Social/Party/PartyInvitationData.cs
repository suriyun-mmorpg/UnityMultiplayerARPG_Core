namespace MultiplayerARPG
{
    public struct PartyInvitationData
    {
        public string InviterId { get; set; }
        public string InviterName { get; set; }
        public short InviterLevel { get; set; }
        public int PartyId { get; set; }
        public bool ShareExp { get; set; }
        public bool ShareItem { get; set; }
    }
}
