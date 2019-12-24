using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGameEntityComponent<T> : MonoBehaviour
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

        public GameInstance CurrentGameInstance { get { return CacheEntity.CurrentGameInstance; } }
        public BaseGameplayRule CurrentGameplayRule { get { return CacheEntity.CurrentGameplayRule; } }
        public BaseGameNetworkManager CurrentGameManager { get { return CacheEntity.CurrentGameManager; } }
        public Transform CacheTransform { get { return CacheEntity.CacheTransform; } }
        public bool IsOwnerClient { get { return CacheEntity.IsOwnerClient; } }
        public bool IsServer { get { return CacheEntity.IsServer; } }
        public bool IsClient { get { return CacheEntity.IsClient; } }
    }
}
