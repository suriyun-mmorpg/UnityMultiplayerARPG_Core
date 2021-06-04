using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class DefaultClientCompanionHandlers : MonoBehaviour, IClientCompanionHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestGetCompanions(ResponseDelegate<ResponseGetCompanionsMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.GetCompanions, EmptyMessage.Value, responseDelegate: callback);
        }

        public bool RequestSelectCompanion(RequestSelectCompanionMessage data, ResponseDelegate<ResponseSelectCompanionMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.SelectCompanion, data, responseDelegate: callback);
        }
    }
}
