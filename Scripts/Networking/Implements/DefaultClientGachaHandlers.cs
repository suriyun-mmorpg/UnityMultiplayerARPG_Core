using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultClientGachaHandlers : MonoBehaviour, IClientGachaHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        public bool RequestOpenGacha(RequestOpenGachaMessage data, ResponseDelegate<ResponseOpenGachaMessage> callback)
        {
            return Manager.ClientSendRequest(GameNetworkingConsts.OpenGacha, data, responseDelegate: callback);
        }
    }
}
