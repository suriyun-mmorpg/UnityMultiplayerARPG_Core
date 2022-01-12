using UnityEngine;

namespace MultiplayerARPG
{
    public class BaseBuffEntity : PoolDescriptor
    {
        [Tooltip("If this is `TRUE` buffs will applies to everyone including with an enemies")]
        public bool applyBuffToEveryone;

        protected EntityInfo buffApplier;
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

        public virtual void Setup(
            EntityInfo buffApplier,
            BaseSkill skill,
            short skillLevel)
        {
            this.buffApplier = buffApplier;
            this.skill = skill;
            this.skillLevel = skillLevel;
        }

        public virtual void ApplyBuffTo(BaseCharacterEntity target)
        {
            if (!IsServer || target == null || target.IsDead() || (!applyBuffToEveryone && !target.IsAlly(buffApplier)))
                return;
            target.ApplyBuff(skill.DataId, BuffType.SkillBuff, skillLevel, buffApplier);
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Base Bufff Entity is null, this should not happens");
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
