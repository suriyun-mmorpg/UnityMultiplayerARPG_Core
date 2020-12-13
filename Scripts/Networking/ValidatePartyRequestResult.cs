namespace MultiplayerARPG
{
    public struct ValidatePartyRequestResult
    {
        public bool IsSuccess { get; set; }
        public GameMessage.Type GameMessageType { get; set; }
        public int PartyId { get; set; }
        public PartyData Party { get; set; }

        public ValidatePartyRequestResult(bool isSuccess, GameMessage.Type gameMessageType)
        {
            IsSuccess = isSuccess;
            GameMessageType = gameMessageType;
            PartyId = 0;
            Party = null;
        }

        public ValidatePartyRequestResult(bool isSuccess, GameMessage.Type gameMessageType, int partyId, PartyData party)
        {
            IsSuccess = isSuccess;
            GameMessageType = gameMessageType;
            PartyId = partyId;
            Party = party;
        }
    }
}
