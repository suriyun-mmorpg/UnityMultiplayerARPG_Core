using UnityEngine;

namespace MultiplayerARPG
{
    public interface IGameEntityComponent
    {
        bool Enabled { get; set; }
        void EntityOnSetup();
        void EntityAwake();
        void EntityStart();
        void EntityUpdate();
        void EntityLateUpdate();
        void EntityFixedUpdate();
        void EntityOnDestroy();
    }

    public abstract class BaseGameEntityComponent<T> : MonoBehaviour, IGameEntityComponent
        where T : BaseGameEntity
    {
        private bool isFoundEntity;
        private T cacheEntity;
        public T CacheEntity
        {
            get
            {
                if (!isFoundEntity)
                {
                    cacheEntity = GetComponent<T>();
                    isFoundEntity = cacheEntity != null;
                }
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

        private bool isEnabled;
        public bool Enabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value)
                    return;
                isEnabled = value;
                if (isEnabled)
                    ComponentOnEnable();
                else
                    ComponentOnDisable();
            }
        }

        public virtual void EntityOnSetup()
        {
        }

        public virtual void EntityAwake()
        {
        }

        public virtual void EntityStart()
        {
        }

        public virtual void EntityUpdate()
        {
        }

        public virtual void EntityLateUpdate()
        {
        }

        public virtual void EntityFixedUpdate()
        {
        }

        public virtual void EntityOnDestroy()
        {
        }

        public virtual void ComponentOnEnable()
        {
        }

        public virtual void ComponentOnDisable()
        {
        }
    }
}
