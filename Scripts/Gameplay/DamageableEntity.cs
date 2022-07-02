using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Events;
using LiteNetLib;

namespace MultiplayerARPG
{
    public abstract partial class DamageableEntity : BaseGameEntity, IDamageableEntity
    {
        [Category("Relative GameObjects/Transforms")]
        [Tooltip("This is transform where combat texts will be instantiates from")]
        [SerializeField]
        private Transform combatTextTransform;
        public Transform CombatTextTransform
        {
            get { return combatTextTransform; }
            set { combatTextTransform = value; }
        }

        [Tooltip("This is transform for other entities to aim to this entity")]
        [SerializeField]
        private Transform opponentAimTransform;
        public Transform OpponentAimTransform
        {
            get { return opponentAimTransform; }
            set { opponentAimTransform = value; }
        }

        [Category(4, "Hit Boxes")]
        [SerializeField]
        protected bool isStaticHitBoxes;

        [Category(99, "Events", false)]
        public UnityEvent onNormalDamageHit = new UnityEvent();
        public UnityEvent onCriticalDamageHit = new UnityEvent();
        public UnityEvent onBlockedDamageHit = new UnityEvent();
        public UnityEvent onDamageMissed = new UnityEvent();
        public event ReceiveDamageDelegate onReceiveDamage;
        public event ReceivedDamageDelegate onReceivedDamage;

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldBool isImmune = new SyncFieldBool();
        [SerializeField]
        protected SyncFieldInt currentHp = new SyncFieldInt();

        public virtual bool IsImmune { get { return isImmune.Value || IsInSafeArea; } set { isImmune.Value = value; } }
        public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
        public bool IsInSafeArea { get; set; }
        public abstract int MaxHp { get; }
        public float HpRate { get { return (float)CurrentHp / (float)MaxHp; } }
        public DamageableHitBox[] HitBoxes { get; protected set; }

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            // Cache components
            if (combatTextTransform == null)
                combatTextTransform = CacheTransform;
            if (opponentAimTransform == null)
                opponentAimTransform = CombatTextTransform;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            // Prepare hitboxes
            HitBoxes = GetComponentsInChildren<DamageableHitBox>(true);
            if (HitBoxes == null || HitBoxes.Length == 0)
                HitBoxes = CreateHitBoxes();
            // Assign index to hitboxes
            for (byte i = 0; i < HitBoxes.Length; ++i)
            {
                HitBoxes[i].Setup(i);
            }
            // Add to lag compensation manager
            if (!isStaticHitBoxes)
                CurrentGameManager.LagCompensationManager.AddHitBoxes(ObjectId, HitBoxes);
        }

        private DamageableHitBox[] CreateHitBoxes()
        {
            // Get colliders to calculate bounds
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                GameObject obj = new GameObject("_HitBoxes");
                obj.transform.parent = CacheTransform;
                Collider[] colliders = GetComponents<Collider>();
                Bounds bounds = default;
                for (int i = 0; i < colliders.Length; ++i)
                {
                    if (i > 0)
                    {
                        bounds.Encapsulate(colliders[i].bounds);
                    }
                    else
                    {
                        bounds = colliders[i].bounds;
                    }
                }
                BoxCollider newCollider = obj.AddComponent<BoxCollider>();
                newCollider.center = bounds.center - CacheTransform.position;
                newCollider.size = bounds.size;
                newCollider.isTrigger = true;
                obj.transform.localPosition = Vector3.zero;
                obj.layer = gameObject.layer;
                return new DamageableHitBox[] { obj.AddComponent<DamageableHitBox>() };
            }
            else
            {
                GameObject obj = new GameObject("_HitBoxes");
                obj.transform.parent = CacheTransform;
                Collider2D[] colliders = GetComponents<Collider2D>();
                Bounds bounds = default;
                for (int i = 0; i < colliders.Length; ++i)
                {
                    if (i > 0)
                    {
                        bounds.Encapsulate(colliders[i].bounds);
                    }
                    else
                    {
                        bounds = colliders[i].bounds;
                    }
                }
                BoxCollider2D newCollider = obj.AddComponent<BoxCollider2D>();
                newCollider.offset = bounds.center - CacheTransform.position;
                newCollider.size = bounds.size;
                newCollider.isTrigger = true;
                obj.transform.localPosition = Vector3.zero;
                obj.layer = gameObject.layer;
                return new DamageableHitBox[] { obj.AddComponent<DamageableHitBox>() };
            }
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            CurrentGameManager.LagCompensationManager.RemoveHitBoxes(ObjectId);
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (Model != null && Model is IDeadableModel)
            {
                // Update dead animation
                (Model as IDeadableModel).SetIsDead(this.IsDead());
            }
        }

        /// <summary>
        /// This will be called on clients to display combat texts, play hit effects, play hit animation
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="damageSource"></param>
        /// <param name="dataId"></param>
        /// <param name="amount"></param>
        [AllRpc]
        protected void AllAppendCombatText(CombatAmountType combatAmountType, DamageSource damageSource, int dataId, int amount)
        {
            switch (combatAmountType)
            {
                case CombatAmountType.NormalDamage:
                    onNormalDamageHit.Invoke();
                    break;
                case CombatAmountType.CriticalDamage:
                    onCriticalDamageHit.Invoke();
                    break;
                case CombatAmountType.BlockedDamage:
                    onBlockedDamageHit.Invoke();
                    break;
                case CombatAmountType.Miss:
                    onDamageMissed.Invoke();
                    break;
            }

            if (!IsClient)
                return;

            BaseUISceneGameplay.Singleton.PrepareCombatText(this, combatAmountType, amount);
            if (combatAmountType == CombatAmountType.NormalDamage ||
                combatAmountType == CombatAmountType.CriticalDamage ||
                combatAmountType == CombatAmountType.BlockedDamage)
            {
                if (Model != null)
                {
                    // Find effects to instantiate
                    GameEffect[] effects = CurrentGameInstance.DefaultDamageHitEffects;
                    switch (damageSource)
                    {
                        case DamageSource.Weapon:
                            DamageElement damageElement;
                            if (GameInstance.DamageElements.TryGetValue(dataId, out damageElement) &&
                                damageElement.DamageHitEffects != null &&
                                damageElement.DamageHitEffects.Length > 0)
                            {
                                effects = damageElement.DamageHitEffects;
                            }
                            break;
                        case DamageSource.Skill:
                            BaseSkill skill;
                            if (GameInstance.Skills.TryGetValue(dataId, out skill) &&
                                skill.DamageHitEffects != null &&
                                skill.DamageHitEffects.Length > 0)
                            {
                                effects = skill.DamageHitEffects;
                            }
                            break;
                    }
                    if (damageSource != DamageSource.None && Model is IHittableModel)
                        (Model as IHittableModel).PlayHitAnimation();
                    Model.InstantiateEffect(effects);
                }
            }
        }

        public void CallAllAppendCombatText(CombatAmountType combatAmountType, DamageSource damageSource, int dataId, int amount)
        {
            RPC(AllAppendCombatText, 0, DeliveryMethod.Unreliable, combatAmountType, damageSource, dataId, amount);
        }

        /// <summary>
        /// Applying damage to this entity
        /// </summary>
        /// <param name="position"></param>
        /// <param name="fromPosition"></param>
        /// <param name="instigator"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="randomSeed"></param>
        internal void ApplyDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed)
        {
            ReceivingDamage(position, fromPosition, instigator, damageAmounts, weapon, skill, skillLevel);
            CombatAmountType combatAmountType;
            int totalDamage;
            ApplyReceiveDamage(position, fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, randomSeed, out combatAmountType, out totalDamage);
            ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel);
        }

        /// <summary>
        /// This function will be called before apply receive damage
        /// </summary>
        /// <param name="position"></param>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivingDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            IGameEntity attacker;
            instigator.TryGetEntity(out attacker);
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(position, fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }

        /// <summary>
        /// Apply damage then return damage type and calculated damage amount
        /// </summary>
        /// <param name="position"></param>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        /// <param name="randomSeed">Random seed for damage randoming</param>
        /// <param name="combatAmountType">Result damage type</param>
        /// <param name="totalDamage">Result damage</param>
        protected abstract void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage);

        /// <summary>
        /// This function will be called after applied receive damage
        /// </summary>
        /// <param name="position">Which part?</param>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amount before total damage calculated</param>
        /// <param name="combatAmountType">Result damage type which receives from `ApplyReceiveDamage`</param>
        /// <param name="totalDamage">Result damage which receives from `ApplyReceiveDamage`</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            DamageSource damageSource = DamageSource.None;
            int dataId = 0;
            if (combatAmountType != CombatAmountType.Miss)
            {
                damageSource = skill == null ? DamageSource.Weapon : DamageSource.Skill;
                switch (damageSource)
                {
                    case DamageSource.Weapon:
                        if (damageAmounts != null)
                        {
                            foreach (DamageElement element in damageAmounts.Keys)
                            {
                                if (element != null && element != CurrentGameInstance.DefaultDamageElement &&
                                    element.DamageHitEffects != null && element.DamageHitEffects.Length > 0)
                                {
                                    dataId = element.DataId;
                                    break;
                                }
                            }
                        }
                        break;
                    case DamageSource.Skill:
                        dataId = skill.DataId;
                        break;
                }
            }
            CallAllAppendCombatText(combatAmountType, damageSource, dataId, totalDamage);
            IGameEntity attacker;
            instigator.TryGetEntity(out attacker);
            if (onReceivedDamage != null)
                onReceivedDamage.Invoke(position, fromPosition, attacker, combatAmountType, totalDamage, weapon, skill, skillLevel);
        }

        public virtual bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (IsImmune)
            {
                // If this entity is in safe area it will not receives damages
                return false;
            }

            if (string.IsNullOrEmpty(instigator.Id))
            {
                // If attacker is unknow entity, can receive damages
                return true;
            }

            if (instigator.IsInSafeArea)
            {
                // If attacker is in safe area, it will not receives damages
                return false;
            }

            return true;
        }
    }
}
