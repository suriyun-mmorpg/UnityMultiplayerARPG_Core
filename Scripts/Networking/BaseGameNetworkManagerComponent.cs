using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
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

        /// <summary>
        /// This function will be called to reader something after map info update data, when received from game-server
        /// You may use it to read seed for "Procedural Map Generation" system.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="reader"></param>
        public virtual void ReadMapInfoExtra(BaseGameNetworkManager networkManager, NetDataReader reader)
        {

        }

        /// <summary>
        /// This function will be called to write something after map info update data, which written before send to clients
        /// You may use it to write seed for "Procedural Map Generation" system.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="writer"></param>
        public virtual void WriteMapInfoExtra(BaseGameNetworkManager networkManager, NetDataWriter writer)
        {

        }

        /// <summary>
        /// This function will be called to update `readyToInstantiateObjectsStates`, if all `readyToInstantiateObjectsStates`'s values are `TRUE` the manager will determined that it is ready to instantiates objects (such as monster, harvestable and so on)
        /// You may use it in case that your game have an "Procedural Map Generation" system, which have to be proceeded before instantiates objects.
        /// </summary>
        /// <param name="networkManager"></param>
        /// <param name="readyToInstantiateObjectsStates"></param>
        public virtual void UpdateReadyToInstantiateObjectsStates(BaseGameNetworkManager networkManager, Dictionary<string, bool> readyToInstantiateObjectsStates)
        {

        }
    }
}
