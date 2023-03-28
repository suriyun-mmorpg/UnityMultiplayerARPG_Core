using LiteNetLib;
using System.Net.Sockets;

namespace MultiplayerARPG
{
    public static class ClientGenericActions
    {
        public static System.Action onClientConnected;
        public static System.Action<DisconnectReason, SocketError, UITextKeys> onClientDisconnected;
        public static System.Action onClientStopped;
        public static System.Action onClientWarp;
        public static System.Action<ChatMessage> onClientReceiveChatMessage;
        public static System.Action<UITextKeys> onClientReceiveGameMessage;
        public static System.Action<RewardGivenType, int> onNotifyRewardExp;
        public static System.Action<RewardGivenType, int> onNotifyRewardGold;
        public static System.Action<RewardGivenType, int, int> onNotifyRewardItem;
        public static System.Action<RewardGivenType, int, int> onNotifyRewardCurrency;
        public static System.Action<int> onNotifyBattlePointsChanged;

        public static void ClientConnected()
        {
            if (onClientConnected != null)
                onClientConnected.Invoke();
        }

        public static void ClientDisconnected(DisconnectReason reason, SocketError socketError, UITextKeys message)
        {
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(reason, socketError, message);
        }

        public static void ClientStopped()
        {
            if (onClientStopped != null)
                onClientStopped.Invoke();
        }

        public static void ClientWarp()
        {
            if (onClientWarp != null)
                onClientWarp.Invoke();
        }

        public static void ClientReceiveChatMessage(ChatMessage message)
        {
            if (onClientReceiveChatMessage != null)
                onClientReceiveChatMessage.Invoke(message);
        }

        public static void ClientReceiveGameMessage(UITextKeys message)
        {
            if (message == UITextKeys.NONE)
                return;
            if (onClientReceiveGameMessage != null)
                onClientReceiveGameMessage.Invoke(message);
        }

        public static void NotifyRewardExp(RewardGivenType givenType, int exp)
        {
            if (onNotifyRewardExp != null)
                onNotifyRewardExp.Invoke(givenType, exp);
        }

        public static void NotifyRewardGold(RewardGivenType givenType, int gold)
        {
            if (onNotifyRewardGold != null)
                onNotifyRewardGold.Invoke(givenType, gold);
        }

        public static void NotifyRewardItem(RewardGivenType givenType, int dataId, int amount)
        {
            if (onNotifyRewardItem != null)
                onNotifyRewardItem.Invoke(givenType, dataId, amount);
        }

        public static void NotifyRewardCurrency(RewardGivenType givenType, int dataId, int amount)
        {
            if (onNotifyRewardCurrency != null)
                onNotifyRewardCurrency.Invoke(givenType, dataId, amount);
        }

        public static void NotifyBattlePointsChanged(int amount)
        {
            if (onNotifyBattlePointsChanged != null)
                onNotifyBattlePointsChanged.Invoke(amount);
        }
    }
}
