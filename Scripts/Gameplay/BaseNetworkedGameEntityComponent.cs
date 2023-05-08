using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class BaseNetworkedGameEntityComponent<T> : LiteNetLibBehaviour, IGameEntityComponent
        where T : BaseGameEntity
    {
        private bool isFoundEntity;
        private T cacheEntity;
        public T Entity
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
        [System.Obsolete("Keeping this for backward compatibility, use `Entity` instead.")]
        public T CacheEntity { get { return Entity; } }

        public GameInstance CurrentGameInstance { get { return Entity.CurrentGameInstance; } }
        public BaseGameplayRule CurrentGameplayRule { get { return Entity.CurrentGameplayRule; } }
        public BaseGameNetworkManager CurrentGameManager { get { return Entity.CurrentGameManager; } }
        public Transform CacheTransform { get { return Entity.EntityTransform; } }

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
