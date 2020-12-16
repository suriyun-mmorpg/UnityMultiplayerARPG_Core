using LiteNetLibManager;
using UnityEngine;
using ReqTypes = MultiplayerARPG.BaseGameNetworkManager.ReqTypes;

namespace MultiplayerARPG
{
    public class DefaultClientBankHandlers : MonoBehaviour, IClientBankHandlers
    {
        public bool RequestDepositGuildGold(RequestDepositGuildGoldMessage data, ResponseDelegate<ResponseDepositGuildGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.DepositGuildGold, data, responseDelegate: callback);
        }

        public bool RequestDepositUserGold(RequestDepositUserGoldMessage data, ResponseDelegate<ResponseDepositUserGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.DepositUserGold, data, responseDelegate: callback);
        }

        public bool RequestWithdrawGuildGold(RequestWithdrawGuildGoldMessage data, ResponseDelegate<ResponseWithdrawGuildGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.WithdrawGuildGold, data, responseDelegate: callback);
        }

        public bool RequestWithdrawUserGold(RequestWithdrawUserGoldMessage data, ResponseDelegate<ResponseWithdrawUserGoldMessage> callback)
        {
            return BaseGameNetworkManager.Singleton.ClientSendRequest(ReqTypes.WithdrawUserGold, data, responseDelegate: callback);
        }
    }
}
