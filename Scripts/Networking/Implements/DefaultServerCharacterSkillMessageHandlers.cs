using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerCharacterSkillMessageHandlers : MonoBehaviour, IServerCharacterSkillMessageHandlers
    {
        public async UniTaskVoid HandleRequestIncreaseCharacterSkillLevel(RequestHandlerData requestHandler, RequestIncreaseCharacterSkillLevelMessage request, RequestProceedResultDelegate<ResponseIncreaseCharacterSkillLevelMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseCharacterSkillLevelMessage()
                {
                    error = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys gameMessage;
            if (!playerCharacter.AddSkill(out gameMessage, request.dataId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, gameMessage);
                result.Invoke(AckResponseCode.Error, new ResponseIncreaseCharacterSkillLevelMessage()
                {
                    error = gameMessage,
                });
                return;
            }
            playerCharacter.SkillPoint -= 1;
            result.Invoke(AckResponseCode.Success, new ResponseIncreaseCharacterSkillLevelMessage());
            await UniTask.Yield();
        }
    }
}
