using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseDamageEntity : MonoBehaviour, IPoolDescriptor
    {
        protected IGameEntity attacker;
        protected CharacterItem weapon;
        protected Dictionary<DamageElement, MinMaxFloat> damageAmounts;
        protected BaseSkill skill;
        protected short skillLevel;

        public GameInstance CurrentGameInstance
        {
            get { return GameInstance.Singleton; }
        }

        public BaseGameplayRule CurrentGameplayRule
        {
            get { return CurrentGameInstance.GameplayRule; }
        }

        public BaseGameNetworkManager CurrentGameManager
        {
            get { return BaseGameNetworkManager.Singleton; }
        }

        public bool IsServer
        {
            get { return CurrentGameManager.IsServer; }
        }

        public bool IsClient
        {
            get { return CurrentGameManager.IsClient; }
        }
        
        public Transform CacheTransform { get; private set; }

        [SerializeField]
        private int poolSize = 30;
        public int PoolSize
        {
            get { return poolSize; }
        }

        public IPoolDescriptor ObjectPrefab
        {
            get; set;
        }

        protected virtual void Awake()
        {
            CacheTransform = transform;
        }

        protected virtual void PushBack(float delay)
        {
            Invoke("PushBack", delay);
        }

        protected virtual void PushBack()
        {
            OnPushBack();
            PoolSystem.PushBack(this);
        }

        protected virtual void OnPushBack()
        {

        }

        public virtual void Setup(
            IGameEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel)
        {
            this.attacker = attacker;
            this.weapon = weapon;
            this.damageAmounts = damageAmounts;
            this.skill = skill;
            this.skillLevel = skillLevel;
        }

        public virtual void ApplyDamageTo(IDamageableEntity target)
        {
            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(attacker))
                return;
            if (IsClient)
                target.PlayHitEffects(damageAmounts.Keys, skill);
            if (IsServer)
                target.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
        }
    }
}
