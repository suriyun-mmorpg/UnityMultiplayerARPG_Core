using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultServerCharacterMessageHandlers : MonoBehaviour, IServerCharacterMessageHandlers
    {
        public async UniTaskVoid HandleRequestIncreaseAttributeAmount(RequestHandlerData requestHandler, RequestIncreaseAttributeAmountMessage request, RequestProceedResultDelegate<ResponseIncreaseAttributeAmountMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseAttributeAmountMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddAttribute(out gameMessage, request.dataId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseAttributeAmountMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            playerCharacter.StatPoint -= 1;
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseAttributeAmountMessage());
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestIncreaseSkillLevel(RequestHandlerData requestHandler, RequestIncreaseSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseSkillLevelMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseSkillLevelMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddSkill(out gameMessage, request.dataId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseSkillLevelMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            playerCharacter.SkillPoint -= 1;
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseSkillLevelMessage());
            await UniTask.Yield();
        }
    }
}
