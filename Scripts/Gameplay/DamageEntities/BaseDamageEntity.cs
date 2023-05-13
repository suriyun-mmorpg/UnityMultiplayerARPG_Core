using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseDamageEntity : PoolDescriptor
    {
        protected EntityInfo _instigator;
        protected CharacterItem _weapon;
        protected int _simulateSeed;
        protected byte _triggerIndex;
        protected byte _spreadIndex;
        protected Dictionary<DamageElement, MinMaxFloat> _damageAmounts;
        protected BaseSkill _skill;
        protected int _skillLevel;
        protected DamageHitDelegate _onHit;

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
        /// <param name="simulateSeed">Launch random seed</param>
        /// <param name="triggerIndex"></param>
        /// <param name="spreadIndex"></param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="onHit">Action when hit</param>
        public virtual void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            DamageHitDelegate onHit)
        {
            _instigator = instigator;
            _weapon = weapon;
            _simulateSeed = simulateSeed;
            _triggerIndex = triggerIndex;
            _spreadIndex = spreadIndex;
            _damageAmounts = damageAmounts;
            _skill = skill;
            _skillLevel = skillLevel;
            _onHit = onHit;
        }

        public virtual void ApplyDamageTo(DamageableHitBox target)
        {
            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(_instigator))
                return;
            if (_onHit != null)
                _onHit.Invoke(_simulateSeed, _triggerIndex, _spreadIndex, target.GetObjectId(), target.Index, target.CacheTransform.position);
            if (!IsServer)
                return;
            if (!CurrentGameManager.HitRegistrationManager.WillProceedHitRegByClient(this, _instigator))
                target.ReceiveDamage(CacheTransform.position, _instigator, _damageAmounts, _weapon, _skill, _skillLevel, _simulateSeed);
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
