using LiteNetLibManager;

namespace MultiplayerARPG
{
    public static class ClientCharacterActions
    {
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseAttributeAmountMessage> onResponseIncreaseAttributeAmount;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseIncreaseSkillLevelMessage> onResponseIncreaseSkillLevel;
        public static System.Action<ResponseHandlerData, AckResponseCode, ResponseRespawnMessage> onResponseRespawn;

        public static void ResponseIncreaseAttributeAmount(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseAttributeAmountMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseIncreaseAttributeAmount != null)
                onResponseIncreaseAttributeAmount.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseIncreaseSkillLevel(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseIncreaseSkillLevelMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseIncreaseSkillLevel != null)
                onResponseIncreaseSkillLevel.Invoke(requestHandler, responseCode, response);
        }

        public static void ResponseRespawn(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRespawnMessage response)
        {
            ClientGenericActions.ClientReceiveGameMessage(response.message);
            if (onResponseRespawn != null)
                onResponseRespawn.Invoke(requestHandler, responseCode, response);
        }
    }
}
