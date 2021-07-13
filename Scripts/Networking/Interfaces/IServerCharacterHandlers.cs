using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial interface IServerCharacterHandlers
    {
        void HandleRequestOnlineCharacter(MessageHandlerData messageHandler);
        void MarkOnlineCharacter(string characterId);
        void ClearOnlineCharacters();
        void Respawn(int option, IPlayerCharacterData playerCharacter);
    }
}
