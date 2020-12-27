using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerGameMessageHandlers : MonoBehaviour, IServerGameMessageHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public void SendGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.GameMessage, message);
        }

        public void SendNotifyRewardExp(long connectionId, int exp)
        {
            if (exp <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardExp, (writer) =>
            {
                writer.PutPackedInt(exp);
            });
        }

        public void SendNotifyRewardGold(long connectionId, int gold)
        {
            if (gold <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardGold, (writer) =>
            {
                writer.PutPackedInt(gold);
            });
        }

        public void SendNotifyRewardItem(long connectionId, int dataId, short amount)
        {
            if (amount <= 0)
                return;
            Manager.ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardItem, (writer) =>
            {
                writer.PutPackedInt(dataId);
                writer.PutPackedShort(amount);
            });
        }
    }
}
