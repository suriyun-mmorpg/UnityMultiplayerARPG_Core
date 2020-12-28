using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientOnlineCharacterHandlers
    {
        bool IsCharacterOnline(string characterId);
        void RequestOnlineCharacter(string characterId);
        void HandleNotifyOnlineCharacter(MessageHandlerData messageHandler);
        void ClearOnlineCharacters();
    }
}
