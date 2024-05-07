using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseGameEntityComponent<T> : MonoBehaviour, IGameEntityComponent
        where T : BaseGameEntity
    {
        private bool _isFoundEntity;
        private T _cacheEntity;
        public T Entity
        {
            get
            {
                if (!_isFoundEntity)
                {
                    _cacheEntity = GetComponent<T>();
                    _isFoundEntity = _cacheEntity != null;
                }
                return _cacheEntity;
            }
        }
        [System.Obsolete("Keeping this for backward compatibility, use `Entity` instead.")]
        public T CacheEntity { get { return Entity; } }

        public GameInstance CurrentGameInstance { get { return Entity.CurrentGameInstance; } }
        public BaseGameplayRule CurrentGameplayRule { get { return Entity.CurrentGameplayRule; } }
        public BaseGameNetworkManager CurrentGameManager { get { return Entity.CurrentGameManager; } }
        public Transform CacheTransform { get { return Entity.EntityTransform; } }

        private bool _isEnabled;
        public bool Enabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value)
                    return;
                _isEnabled = value;
                if (_isEnabled)
                    ComponentOnEnable();
                else
                    ComponentOnDisable();
            }
        }

        public bool AlwaysUpdate { get; protected set; }

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

        public virtual void Clean()
        {
            _cacheEntity = null;
        }
    }
}