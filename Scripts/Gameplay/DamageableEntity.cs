using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public abstract partial class DamageableEntity : BaseGameEntity, IDamageableEntity
    {
        [Header("Damageable Settings")]
        [Tooltip("This is transform where combat texts will be instantiates from")]
        [SerializeField]
        private Transform combatTextTransform;
        public Transform CombatTextTransform
        {
            get { return combatTextTransform; }
        }

        [Tooltip("This is transform for other entities to aim to this entity")]
        [SerializeField]
        private Transform opponentAimTransform;
        public Transform OpponentAimTransform
        {
            get { return opponentAimTransform; }
        }

        [Header("Damageable Entity Events")]
        public UnityEvent onNormalDamageHit = new UnityEvent();
        public UnityEvent onCriticalDamageHit = new UnityEvent();
        public UnityEvent onBlockedDamageHit = new UnityEvent();
        public UnityEvent onDamageMissed = new UnityEvent();
        public event ReceiveDamageDelegate onReceiveDamage;
        public event ReceivedDamageDelegate onReceivedDamage;

        [Header("Damageable Sync Fields")]
        [SerializeField]
        protected SyncFieldInt currentHp = new SyncFieldInt();

        public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
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
            HitBoxes = GetComponentsInChildren<DamageableHitBox>(true);
            if (HitBoxes == null || HitBoxes.Length == 0)
                HitBoxes = CreateHitBoxes();
        }

        private DamageableHitBox[] CreateHitBoxes()
        {
            GameObject obj = new GameObject("_HitBoxes");
            obj.transform.parent = CacheTransform;
            // Get colliders to calculate bounds
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
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
                return new DamageableHitBox[] { obj.AddComponent<DamageableHitBox>() };
            }
            else
            {
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
                return new DamageableHitBox[] { obj.AddComponent<DamageableHitBox>() };
            }
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="damageSource"></param>
        /// <param name="dataId"></param>
        /// <param name="amount"></param>
        [AllRpc]
        protected void AllAppendCombatAmount(CombatAmountType combatAmountType, DamageSource damageSource, int dataId, int amount)
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
                                damageElement.GetDamageHitEffects() != null &&
                                damageElement.GetDamageHitEffects().Length > 0)
                            {
                                effects = damageElement.GetDamageHitEffects();
                            }
                            break;
                        case DamageSource.Skill:
                            BaseSkill skill;
                            if (GameInstance.Skills.TryGetValue(dataId, out skill) &&
                                skill.GetDamageHitEffects() != null &&
                                skill.GetDamageHitEffects().Length > 0)
                            {
                                effects = skill.GetDamageHitEffects();
                            }
                            break;
                    }
                    Model.InstantiateEffect(effects);
                }
            }
        }

        public void CallAllAppendCombatAmount(CombatAmountType combatAmountType, DamageSource damageSource, int dataId, int amount)
        {
            RPC(AllAppendCombatAmount, combatAmountType, damageSource, dataId, amount);
        }

        /// <summary>
        /// Applying damage to this entity
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="instigator"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        internal void ApplyDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            ReceivingDamage(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel);
            CombatAmountType combatAmountType;
            int totalDamage;
            ApplyReceiveDamage(fromPosition, instigator, damageAmounts, weapon, skill, skillLevel, out combatAmountType, out totalDamage);
            ReceivedDamage(fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel);
        }

        /// <summary>
        /// This function will be called before apply receive damage
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivingDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            IGameEntity attacker;
            if (onReceiveDamage != null && instigator.TryGetEntity(out attacker))
                onReceiveDamage.Invoke(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }

        /// <summary>
        /// Apply damage then return damage type and calculated damage amount
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        /// <param name="combatAmountType">Result damage type</param>
        /// <param name="totalDamage">Result damage</param>
        protected abstract void ApplyReceiveDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, out CombatAmountType combatAmountType, out int totalDamage);

        /// <summary>
        /// This function will be called after applied receive damage
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="instigator">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amount before total damage calculated</param>
        /// <param name="combatAmountType">Result damage type which receives from `ApplyReceiveDamage`</param>
        /// <param name="totalDamage">Result damage which receives from `ApplyReceiveDamage`</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivedDamage(Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel)
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
                                    element.GetDamageHitEffects() != null && element.GetDamageHitEffects().Length > 0)
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
            CallAllAppendCombatAmount(combatAmountType, damageSource, dataId, totalDamage);
            IGameEntity attacker;
            if (onReceivedDamage != null && instigator.TryGetEntity(out attacker))
                onReceivedDamage.Invoke(fromPosition, attacker, combatAmountType, totalDamage, weapon, skill, skillLevel);
        }

        public virtual bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (IsInSafeArea)
            {
                // If this entity is in safe area it will not receives damages
                return false;
            }

            if (string.IsNullOrEmpty(instigator.id))
            {
                // If attacker is unknow entity, can receive damages
                return true;
            }
            
            if (instigator.isInSafeArea)
            {
                // If attacker is in safe area, it will not receives damages
                return false;
            }

            return true;
        }
    }
}
