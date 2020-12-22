using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientBankHandlers : MonoBehaviour, IClientBankHandlers
    {
        public bool RequestDepositGuildGold(RequestDepositGuildGoldMessage data, ResponseDelegate<ResponseDepositGuildGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.DepositGuildGold, data, responseDelegate: callback);
        }

        public bool RequestDepositUserGold(RequestDepositUserGoldMessage data, ResponseDelegate<ResponseDepositUserGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.DepositUserGold, data, responseDelegate: callback);
        }

        public bool RequestWithdrawGuildGold(RequestWithdrawGuildGoldMessage data, ResponseDelegate<ResponseWithdrawGuildGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.WithdrawGuildGold, data, responseDelegate: callback);
        }

        public bool RequestWithdrawUserGold(RequestWithdrawUserGoldMessage data, ResponseDelegate<ResponseWithdrawUserGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(GameNetworkingConsts.WithdrawUserGold, data, responseDelegate: callback);
        }
    }
}
