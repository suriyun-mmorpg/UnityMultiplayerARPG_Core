using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerOnlineCharacterMessageHandlers : MonoBehaviour, IServerOnlineCharacterMessageHandlers
    {
        public async UniTaskVoid HandleRequestGetOnlineCharacterData(RequestHandlerData requestHandler, RequestGetOnlineCharacterDataMessage request, RequestProceedResultDelegate<ResponseGetOnlineCharacterDataMessage> result)
        {
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(request.characterId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetOnlineCharacterDataMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND,
                });
                return;
            }
            PlayerCharacterData resultPlayerCharacter = playerCharacter.CloneTo(new PlayerCharacterData(), true, false, false, false, false, true, false, false, false, false, false);
            result.Invoke(AckResponseCode.Success, new ResponseGetOnlineCharacterDataMessage()
            {
                character = resultPlayerCharacter,
            });
            await UniTask.Yield();
        }
    }
}