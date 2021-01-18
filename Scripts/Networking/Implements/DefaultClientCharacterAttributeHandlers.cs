using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientCharacterAttributeHandlers : MonoBehaviour, IClientCharacterAttributeHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestIncreaseCharacterAttributeAmount(RequestIncreaseCharacterAttributeAmountMessage data, ResponseDelegate<ResponseIncreaseCharacterAttributeAmountMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.IncreaseCharacterAttributeAmount, data, responseDelegate: callback);
        }
    }
}
