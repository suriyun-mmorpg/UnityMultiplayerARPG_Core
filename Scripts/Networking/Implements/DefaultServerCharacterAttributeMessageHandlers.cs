using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerCharacterAttributeMessageHandlers : MonoBehaviour, IServerCharacterAttributeMessageHandlers
    {
        public async UniTaskVoid HandleRequestIncreaseCharacterAttributeAmount(RequestHandlerData requestHandler, RequestIncreaseCharacterAttributeAmountMessage request, RequestProceedResultDelegate<ResponseIncreaseCharacterAttributeAmountMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseCharacterAttributeAmountMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddAttribute(out gameMessage, request.dataId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseCharacterAttributeAmountMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            playerCharacter.StatPoint -= 1;
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseCharacterAttributeAmountMessage());
            await UniTask.Yield();
        }
    }
}
