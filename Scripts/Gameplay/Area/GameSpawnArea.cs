using System.Collections;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class GameSpawnArea<T> : GameArea where T : LiteNetLibBehaviour
    {
        [Header("Spawning Data")]
        public T asset;
        public short amount = 1;

        public virtual void RegisterAssets()
        {
            if (asset != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(asset.Identity);
        }

        public virtual void SpawnAll()
        {
            if (asset != null)
            {
                for (int i = 0; i < amount; ++i)
                {
                    Spawn(0);
                }
            }
        }

        public virtual void Spawn(float delay)
        {
            Invoke("SpawnInternal", delay);
        }

        protected abstract void SpawnInternal();
    }
}
