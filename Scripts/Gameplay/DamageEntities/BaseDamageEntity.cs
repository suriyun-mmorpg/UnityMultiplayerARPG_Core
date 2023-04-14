using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseDamageEntity : PoolDescriptor
    {
        protected EntityInfo _instigator;
        protected CharacterItem _weapon;
        protected Dictionary<DamageElement, MinMaxFloat> _damageAmounts;
        protected BaseSkill _skill;
        protected int _skillLevel;

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
        private FxCollection fxCollection;
        public FxCollection FxCollection
        {
            get
            {
                if (fxCollection == null)
                    fxCollection = new FxCollection(gameObject);
                return fxCollection;
            }
        }
        private bool playFxOnEnable;

        protected virtual void Awake()
        {
            CacheTransform = transform;
        }

        protected virtual void OnEnable()
        {
            if (playFxOnEnable)
                PlayFx();
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="instigator">Weapon's or skill's instigator who to spawn this to attack enemy</param>
        /// <param name="weapon">Weapon which was used to attack enemy</param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        public virtual void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel)
        {
            _instigator = instigator;
            _weapon = weapon;
            _damageAmounts = damageAmounts;
            _skill = skill;
            _skillLevel = skillLevel;
        }

        public virtual void ApplyDamageTo(DamageableHitBox target)
        {
            if (!IsServer || target == null || target.IsDead() || !target.CanReceiveDamageFrom(_instigator))
                return;
            target.ReceiveDamage(CacheTransform.position, _instigator, _damageAmounts, _weapon, _skill, _skillLevel, Random.Range(0, 255));
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Base Damage Entity is null, this should not happens");
                return;
            }
            FxCollection.InitPrefab();
            base.InitPrefab();
        }

        public override void OnGetInstance()
        {
            PlayFx();
            base.OnGetInstance();
        }

        protected override void OnPushBack()
        {
            StopFx();
            base.OnPushBack();
        }

        public virtual void PlayFx()
        {
            if (!gameObject.activeInHierarchy)
            {
                playFxOnEnable = true;
                return;
            }
            FxCollection.Play();
            playFxOnEnable = false;
        }

        public virtual void StopFx()
        {
            FxCollection.Stop();
        }
    }
}
