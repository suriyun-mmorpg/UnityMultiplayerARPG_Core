using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseGameNetworkManagerComponent : MonoBehaviour
    {
        public virtual void RegisterMessages(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void Clean(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void OnStartServer(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void OnStopServer(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void OnStartClient(BaseGameNetworkManager networkManager, LiteNetLibClient client)
        {

        }

        public virtual void OnStopClient(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void InitPrefabs(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void OnClientOnlineSceneLoaded(BaseGameNetworkManager networkManager)
        {

        }

        public virtual void OnServerOnlineSceneLoaded(BaseGameNetworkManager networkManager)
        {

        }
    }
}
