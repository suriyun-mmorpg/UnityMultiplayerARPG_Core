namespace MultiplayerARPG
{
    public interface IServerGameMessageHandlers
    {
        void SendGameMessage(long connectionId, GameMessage.Type type);
        void SendNotifyRewardExp(long connectionId, int exp);
        void SendNotifyRewardGold(long connectionId, int gold);
        void SendNotifyRewardItem(long connectionId, int dataId, short amount);
    }
}
