using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientBankActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDepositUserGoldMessage> onResponseDepositUserGold;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseWithdrawUserGoldMessage> onResponseWithdrawUserGold;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseDepositGuildGoldMessage> onResponseDepositGuildGold;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseWithdrawGuildGoldMessage> onResponseWithdrawGuildGold;

        public static async UniTaskVoid ResponseDepositUserGold(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDepositUserGoldMessage response)
        {
            await UniTask.Yield();
            if (onResponseDepositUserGold != null)
                onResponseDepositUserGold.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseWithdrawUserGold(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseWithdrawUserGoldMessage response)
        {
            await UniTask.Yield();
            if (onResponseWithdrawUserGold != null)
                onResponseWithdrawUserGold.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseDepositGuildGold(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseDepositGuildGoldMessage response)
        {
            await UniTask.Yield();
            if (onResponseDepositGuildGold != null)
                onResponseDepositGuildGold.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseWithdrawGuildGold(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseWithdrawGuildGoldMessage response)
        {
            await UniTask.Yield();
            if (onResponseWithdrawGuildGold != null)
                onResponseWithdrawGuildGold.Invoke(requestHandler, responseCode, response);
        }
    }
}
