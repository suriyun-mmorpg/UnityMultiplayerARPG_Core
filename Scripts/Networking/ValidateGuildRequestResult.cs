namespace MultiplayerARPG
{
    public struct ValidateGuildRequestResult
    {
        public bool IsSuccess { get; set; }
        public GameMessage.Type GameMessageType { get; set; }
        public int GuildId { get; set; }
        public GuildData Guild { get; set; }

        public ValidateGuildRequestResult(bool isSuccess, GameMessage.Type gameMessageType)
        {
            IsSuccess = isSuccess;
            GameMessageType = gameMessageType;
            GuildId = 0;
            Guild = null;
        }

        public ValidateGuildRequestResult(bool isSuccess, GameMessage.Type gameMessageType, int partyId, GuildData party)
        {
            IsSuccess = isSuccess;
            GameMessageType = gameMessageType;
            GuildId = partyId;
            Guild = party;
        }
    }
}
