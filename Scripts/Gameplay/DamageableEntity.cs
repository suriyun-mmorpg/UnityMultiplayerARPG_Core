using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class DamageableEntity : BaseGameEntity, IDamageableEntity
    {
        // Events / delegates
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

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            // Cache components
            if (combatTextTransform == null)
                combatTextTransform = CacheTransform;
            if (opponentAimTransform == null)
                opponentAimTransform = CombatTextTransform;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction<byte, int>(NetFuncCombatAmount);
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="amount"></param>
        protected void NetFuncCombatAmount(byte byteCombatAmountType, int amount)
        {
            if (!IsClient)
                return;

            CombatAmountType combatAmountType = (CombatAmountType)byteCombatAmountType;
            BaseUISceneGameplay.Singleton.PrepareCombatText(this, combatAmountType, amount);
            switch (combatAmountType)
            {
                case CombatAmountType.NormalDamage:
                case CombatAmountType.CriticalDamage:
                case CombatAmountType.BlockedDamage:
                    if (Model != null)
                        Model.InstantiateEffect(pendingHitEffects);
                    pendingHitEffects = null;
                    break;
                case CombatAmountType.Miss:
                    pendingHitEffects = null;
                    break;
            }
        }

        public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            CallNetFunction(NetFuncCombatAmount, FunctionReceivers.All, (byte)combatAmountType, amount);
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public virtual void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead())
                return;
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(attacker, weapon, damageAmounts, skill, skillLevel);
        }

        public virtual void ReceivedDamage(IGameEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            RequestCombatAmount(combatAmountType, damage);
            if (onReceivedDamage != null)
                onReceivedDamage.Invoke(attacker, combatAmountType, damage);
        }

        public virtual bool CanReceiveDamageFrom(IGameEntity attacker)
        {
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
            pendingHitEffects = effects;
        }
    }
}
