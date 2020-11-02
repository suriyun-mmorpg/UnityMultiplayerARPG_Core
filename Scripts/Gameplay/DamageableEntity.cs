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

        // Temp data
        private GameEffect[] pendingHitEffects;
        private bool playHitEffectsImmediately;

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
                HitBoxes = new DamageableHitBox[] { gameObject.AddComponent<DamageableHitBox>() };
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="amount"></param>
        [AllRpc]
        protected void AllAppendCombatAmount(CombatAmountType combatAmountType, int amount)
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
            switch (combatAmountType)
            {
                case CombatAmountType.NormalDamage:
                case CombatAmountType.CriticalDamage:
                case CombatAmountType.BlockedDamage:
                    if (pendingHitEffects == null || pendingHitEffects.Length == 0)
                    {
                        // Damage amount shown before hit effects prepared
                        // So it will play hit effects immediately when PlayHitEffects() call later
                        playHitEffectsImmediately = true;
                    }
                    else
                    {
                        if (Model != null)
                            Model.InstantiateEffect(pendingHitEffects);
                    }
                    break;
                case CombatAmountType.Miss:
                    break;
            }
            pendingHitEffects = null;
        }

        public void CallAllAppendCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            RPC(AllAppendCombatAmount, combatAmountType, amount);
        }

        /// <summary>
        /// Applying damage to this entity
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="attacker"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        internal void ApplyDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            ReceivingDamage(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
            CombatAmountType combatAmountType;
            int totalDamage;
            ApplyReceiveDamage(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel, out combatAmountType, out totalDamage);
            ReceivedDamage(fromPosition, attacker, combatAmountType, totalDamage, weapon, skill, skillLevel);
        }

        /// <summary>
        /// This function will be called before apply receive damage
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="attacker">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivingDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }

        /// <summary>
        /// Apply damage then return damage type and calculated damage amount
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="attacker">Who is attacking this?</param>
        /// <param name="damageAmounts">Damage amounts from attacker</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        /// <param name="combatAmountType">Result damage type</param>
        /// <param name="totalDamage">Result damage</param>
        protected abstract void ApplyReceiveDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, out CombatAmountType combatAmountType, out int totalDamage);

        /// <summary>
        /// This function will be called after applied receive damage
        /// </summary>
        /// <param name="fromPosition">Where is attacker?</param>
        /// <param name="attacker">Who is attacking this?</param>
        /// <param name="combatAmountType">Result damage type which receives from `ApplyReceiveDamage`</param>
        /// <param name="damage">Result damage which receives from `ApplyReceiveDamage`</param>
        /// <param name="weapon">Weapon which used to attack</param>
        /// <param name="skill">Skill which used to attack</param>
        /// <param name="skillLevel">Skill level which used to attack</param>
        public virtual void ReceivedDamage(Vector3 fromPosition, IGameEntity attacker, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            CallAllAppendCombatAmount(combatAmountType, damage);
            if (onReceivedDamage != null)
                onReceivedDamage.Invoke(fromPosition, attacker, combatAmountType, damage, weapon, skill, skillLevel);
        }

        public virtual bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            if (IsInSafeArea)
            {
                // If this entity is in safe area it will not receives damages
                return false;
            }

            if (attacker == null || attacker.Entity == null)
            {
                // If attacker is unknow entity, can receive damages
                return true;
            }

            if (attacker.Entity.IsInSafeArea)
            {
                // If attacker is in safe area, it will not receives damages
                return false;
            }

            return true;
        }

        public virtual void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            if (!IsClient)
                return;

            GameEffect[] effects = CurrentGameInstance.DefaultDamageHitEffects;
            if (skill != null && skill.GetDamageHitEffects() != null && skill.GetDamageHitEffects().Length > 0)
            {
                // Set hit effects from skill's hit effects
                effects = skill.GetDamageHitEffects();
            }
            else
            {
                foreach (DamageElement element in damageElements)
                {
                    if (element.GetDamageHitEffects() == null ||
                        element.GetDamageHitEffects().Length == 0)
                        continue;
                    effects = element.GetDamageHitEffects();
                    break;
                }
            }
            if (playHitEffectsImmediately)
            {
                // Play hit effects immediately because damage amount shown before client simulate hit
                if (Model != null)
                    Model.InstantiateEffect(effects);
                playHitEffectsImmediately = false;
            }
            else
            {
                // Prepare hit effects to play when damage amount show
                pendingHitEffects = effects;
            }
        }
    }
}
