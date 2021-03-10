using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerCharacterHandlers
    {
        bool IsCharacterOnline(string characterId);
        void HandleRequestOnlineCharacter(MessageHandlerData messageHandler);
        void MarkOnlineCharacter(string characterId);
        void ClearOnlineCharacters();
    }
}
