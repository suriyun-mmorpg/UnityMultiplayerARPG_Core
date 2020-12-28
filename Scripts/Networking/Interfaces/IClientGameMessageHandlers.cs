using LiteNetLibManager;

namespace MultiplayerARPG
{
    public interface IClientGameMessageHandlers
    {
        void HandleGameMessage(MessageHandlerData messageHandler);
        void HandleUpdatePartyMember(MessageHandlerData messageHandler);
        void HandleUpdateParty(MessageHandlerData messageHandler);
        void HandleUpdateGuildMember(MessageHandlerData messageHandler);
        void HandleUpdateGuild(MessageHandlerData messageHandler);
        void HandleUpdateFriends(MessageHandlerData messageHandler);
        void HandleNotifyRewardExp(MessageHandlerData messageHandler);
        void HandleNotifyRewardGold(MessageHandlerData messageHandler);
        void HandleNotifyRewardItem(MessageHandlerData messageHandler);
        void HandleNotifyStorageOpened(MessageHandlerData messageHandler);
        void HandleNotifyStorageClosed(MessageHandlerData messageHandler);
        void HandleNotifyStorageItems(MessageHandlerData messageHandler);
        void HandleNotifyPartyInvitation(MessageHandlerData messageHandler);
        void HandleNotifyGuildInvitation(MessageHandlerData messageHandler);
    }
}
