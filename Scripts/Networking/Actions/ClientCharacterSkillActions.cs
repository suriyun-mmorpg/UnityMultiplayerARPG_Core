using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientCharacterSkillActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseCharacterSkillLevelMessage> onResponseIncreaseCharacterSkillLevel;

        public static async UniTaskVoid ResponseIncreaseCharacterSkillLevel(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseCharacterSkillLevelMessage response)
        {
            await UniTask.Yield();
            if (onResponseIncreaseCharacterSkillLevel != null)
                onResponseIncreaseCharacterSkillLevel.Invoke(requestHandler, responseCode, response);
        }
    }
}
