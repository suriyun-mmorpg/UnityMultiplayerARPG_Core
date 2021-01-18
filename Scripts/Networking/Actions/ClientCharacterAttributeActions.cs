using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientCharacterAttributeActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseCharacterAttributeAmountMessage> onResponseIncreaseCharacterAttributeAmount;

        public static async UniTaskVoid ResponseIncreaseCharacterAttributeAmount(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseCharacterAttributeAmountMessage response)
        {
            await UniTask.Yield();
            if (onResponseIncreaseCharacterAttributeAmount != null)
                onResponseIncreaseCharacterAttributeAmount.Invoke(requestHandler, responseCode, response);
        }
    }
}
