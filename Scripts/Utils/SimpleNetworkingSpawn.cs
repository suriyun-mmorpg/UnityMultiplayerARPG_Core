using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class SimpleNetworkingSpawn : MonoBehaviour
    {
        public LiteNetLibIdentity prefab;

        public void Spawn()
        {
            if (prefab == null)
                return;
            LiteNetLibGameManager manager = FindObjectOfType<LiteNetLibGameManager>();
            if (manager == null || !manager.IsServer)
                return;
            manager.Assets.NetworkSpawn(prefab.HashAssetId, transform.position, transform.rotation);
        }
    }
}