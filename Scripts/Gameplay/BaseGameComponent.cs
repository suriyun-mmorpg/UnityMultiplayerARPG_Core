using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGameComponent<T> : MonoBehaviour
        where T : BaseGameEntity
    {
        private T cacheEntity;
        public T CacheEntity
        {
            get
            {
                if (cacheEntity == null)
                    cacheEntity = GetComponent<T>();
                return cacheEntity;
            }
        }

        public GameInstance gameInstance { get { return CacheEntity.gameInstance; } }
        public BaseGameplayRule gameplayRule { get { return CacheEntity.gameplayRule; } }
        public BaseGameNetworkManager gameManager { get { return CacheEntity.gameManager; } }
        public Transform CacheTransform { get { return CacheEntity.CacheTransform; } }
        public bool IsOwnerClient { get { return CacheEntity.IsOwnerClient; } }
        public bool IsServer { get { return CacheEntity.IsServer; } }
        public bool IsClient { get { return CacheEntity.IsClient; } }

        public virtual void EntityOnSetup(T entity) { }
        public virtual void EntityOnDestroy(T entity) { }
    }
}
