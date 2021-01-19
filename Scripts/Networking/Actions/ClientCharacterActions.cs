using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientCharacterActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseAttributeAmountMessage> onResponseIncreaseCharacterAttributeAmount;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseSkillLevelMessage> onResponseIncreaseCharacterSkillLevel;

        public static async UniTaskVoid ResponseIncreaseCharacterAttributeAmount(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseAttributeAmountMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseIncreaseCharacterAttributeAmount != null)
                onResponseIncreaseCharacterAttributeAmount.Invoke(requestHandler, responseCode, response);
        }

        public static async UniTaskVoid ResponseIncreaseCharacterSkillLevel(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseSkillLevelMessage response)
        {
            await UniTask.Yield();
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseIncreaseCharacterSkillLevel != null)
                onResponseIncreaseCharacterSkillLevel.Invoke(requestHandler, responseCode, response);
        }
    }
}
