using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IServerOnlineCharacterHandlers
    {
        bool IsCharacterOnline(string characterId);
        void HandleRequestOnlineCharacter(MessageHandlerData messageHandler);
        void MarkOnlineCharacter(string characterId);
        void ClearOnlineCharacters();
    }
}
