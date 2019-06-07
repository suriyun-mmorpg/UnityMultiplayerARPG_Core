using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseCharacterComponent : MonoBehaviour
    {
        private BaseCharacterEntity cacheCharacterEntity;
        public BaseCharacterEntity CacheCharacterEntity
        {
            get
            {
                if (cacheCharacterEntity == null)
                    cacheCharacterEntity = GetComponent<BaseCharacterEntity>();
                return cacheCharacterEntity;
            }
        }

        public GameInstance gameInstance { get { return CacheCharacterEntity.gameInstance; } }
        public BaseGameplayRule gameplayRule { get { return CacheCharacterEntity.gameplayRule; } }
        public BaseGameNetworkManager gameManager { get { return CacheCharacterEntity.gameManager; } }
        public Transform CacheTransform { get { return CacheCharacterEntity.CacheTransform; } }
        public bool IsOwnerClient { get { return CacheCharacterEntity.IsOwnerClient; } }
        public bool IsServer { get { return CacheCharacterEntity.IsServer; } }
        public bool IsClient { get { return CacheCharacterEntity.IsClient; } }

        public virtual void EntityOnSetup(BaseCharacterEntity entity) { }

        public virtual void EntityOnDestroy(BaseCharacterEntity entity) { }

        public bool IsDead()
        {
            return CacheCharacterEntity.IsDead();
        }
    }
}
