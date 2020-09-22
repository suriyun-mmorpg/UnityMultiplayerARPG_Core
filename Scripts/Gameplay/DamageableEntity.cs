using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract partial class DamageableEntity : BaseGameEntity, IDamageableEntity
    {
        // Events
        public event ReceiveDamageDelegate onReceiveDamage;
        public event ReceivedDamageDelegate onReceivedDamage;

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

        [Header("Damageable Sync Fields")]
        [SerializeField]
        protected SyncFieldInt currentHp = new SyncFieldInt();

        public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
        public abstract int MaxHp { get; }
        public float HpRate { get { return (float)CurrentHp / (float)MaxHp; } }

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
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="amount"></param>
        [AllRpc]
        protected void AllAppendCombatAmount(CombatAmountType combatAmountType, int amount)
        {
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

        public virtual void ReceiveDamage(Vector3 fromPosition, IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || this.IsDead())
                return;
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(fromPosition, attacker, damageAmounts, weapon, skill, skillLevel);
        }

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
