using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientBankHandlers : MonoBehaviour, IClientBankHandlers
    {
        public bool RequestDepositGuildGold(RequestDepositGuildGoldMessage data, ResponseDelegate<ResponseDepositGuildGoldMessage> callback)
        {
            throw new System.NotImplementedException();
        }

        public bool RequestDepositUserGold(RequestDepositUserGoldMessage data, ResponseDelegate<ResponseDepositUserGoldMessage> callback)
        {
            throw new System.NotImplementedException();
        }

        public bool RequestWithdrawGuildGold(RequestWithdrawGuildGoldMessage data, ResponseDelegate<ResponseWithdrawGuildGoldMessage> callback)
        {
            throw new System.NotImplementedException();
        }

        public bool RequestWithdrawUserGold(RequestWithdrawUserGoldMessage data, ResponseDelegate<ResponseWithdrawUserGoldMessage> callback)
        {
            throw new System.NotImplementedException();
        }
    }
}
