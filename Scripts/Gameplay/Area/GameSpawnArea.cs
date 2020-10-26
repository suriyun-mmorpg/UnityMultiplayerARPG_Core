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
        public float respawnPendingEntitiesDelay = 5f;
        protected float respawnPendingEntitiesTimer = 0f;
        protected int pending = 0;

        protected virtual void LateUpdate()
        {
            if (pending > 0)
            {
                respawnPendingEntitiesTimer += Time.deltaTime;
                if (respawnPendingEntitiesTimer >= respawnPendingEntitiesDelay)
                {
                    respawnPendingEntitiesTimer = 0f;
                    Logging.LogWarning(ToString(), "Spawning pending entities, " + pending);
                    int spawnAmount = pending;
                    for (int i = 0; i < spawnAmount; ++i)
                    {
                        pending--;
                        Spawn(0);
                    }
                }
            }
        }

        public virtual void RegisterAssets()
        {
            if (asset != null)
                BaseGameNetworkManager.Singleton.Assets.RegisterPrefab(asset.Identity);
        }

        public virtual void SpawnAll()
        {
            SpawnByAmount(amount);
        }

        public virtual void SpawnByAmount(int amount)
        {
            for (int i = 0; i < amount; ++i)
            {
                Spawn(0);
            }
        }

        public virtual void Spawn(float delay)
        {
            if (asset != null)
                Invoke(nameof(SpawnInternal), delay);
        }

        protected abstract void SpawnInternal();
    }
}
