using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientCharacterSkillHandlers : MonoBehaviour, IClientCharacterSkillHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestIncreaseCharacterSkillLevel(RequestIncreaseCharacterSkillLevelMessage data, ResponseDelegate<ResponseIncreaseCharacterSkillLevelMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseCharacterSkillLevel, data, responseDelegate: callback);
        }
    }
}
