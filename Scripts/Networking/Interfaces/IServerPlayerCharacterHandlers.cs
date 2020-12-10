namespace MultiplayerARPG
{
    public interface IServerPlayerCharacterHandlers
    {
        bool TryGetPlayerCharacter(long connectionId, out IPlayerCharacterData playerCharacter);
    }
}
