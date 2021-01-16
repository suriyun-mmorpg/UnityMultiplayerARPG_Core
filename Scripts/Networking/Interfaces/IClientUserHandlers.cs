namespace MultiplayerARPG
{
    public interface IClientUserHandlers
    {
        string UserId { get; set; }
        string UserToken { get; set; }
        string CharacterId { get; set; }
        IPlayerCharacterData Character { get; set; }
    }
}
