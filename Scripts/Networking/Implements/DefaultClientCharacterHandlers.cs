using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultClientCharacterHandlers : MonoBehaviour, IClientCharacterHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestIncreaseAttributeAmount(RequestIncreaseAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseAttributeAmountMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseAttributeAmount, data, responseDelegate: callback);
        }

        public bool RequestIncreaseSkillLevel(RequestIncreaseSkillLevelMessage data, ResponseDelegate<ResponseIncreaseSkillLevelMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseSkillLevel, data, responseDelegate: callback);
        }

        public bool RequestRespawn(RequestRespawnMessage data, ResponseDelegate<ResponseRespawnMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.Respawn, data, responseDelegate: callback);
        }
    }
}
