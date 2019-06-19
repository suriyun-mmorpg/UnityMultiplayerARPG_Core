using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGameComponent<T> : MonoBehaviour
        where T : BaseGameEntity
    {
        private T cacheEntity;
        public T CacheCharacterEntity
        {
            get
            {
                if (cacheEntity == null)
                    cacheEntity = GetComponent<T>();
                return cacheEntity;
            }
        }

        public GameInstance gameInstance { get { return CacheCharacterEntity.gameInstance; } }
        public BaseGameplayRule gameplayRule { get { return CacheCharacterEntity.gameplayRule; } }
        public BaseGameNetworkManager gameManager { get { return CacheCharacterEntity.gameManager; } }
        public Transform CacheTransform { get { return CacheCharacterEntity.CacheTransform; } }
        public bool IsOwnerClient { get { return CacheCharacterEntity.IsOwnerClient; } }
        public bool IsServer { get { return CacheCharacterEntity.IsServer; } }
        public bool IsClient { get { return CacheCharacterEntity.IsClient; } }

        public virtual void EntityOnSetup(T entity) { }
        public virtual void EntityOnDestroy(T entity) { }
    }
}
