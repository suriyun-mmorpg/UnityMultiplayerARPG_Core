using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class DamageableEntity : BaseGameEntity, IDamageableEntity
    {
        [SerializeField]
        protected SyncFieldInt currentHp = new SyncFieldInt();

        // Events / delegates
        public event ReceiveDamageDelegate onReceiveDamage;
        public event ReceivedDamage onReceivedDamage;

        [Tooltip("This is transform where combat texts will be instantiates from")]
        public Transform combatTextTransform;
        public Transform CombatTextTransform
        {
            get
            {
                if (combatTextTransform == null)
                    combatTextTransform = CacheTransform;
                return combatTextTransform;
            }
        }
        
        [Tooltip("This is transform for other entities to aim to this entity")]
        public Transform opponentAimTransform;
        public Transform OpponentAimTransform
        {
            get
            {
                if (opponentAimTransform == null)
                    opponentAimTransform = CombatTextTransform;
                return opponentAimTransform;
            }
        }

        public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
        public abstract int MaxHp { get; }
        public bool IsDead { get { return CurrentHp <= 0; } }

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
            UISceneGameplay uiSceneGameplay = UISceneGameplay.Singleton;
            if (uiSceneGameplay == null)
                return;
            uiSceneGameplay.SpawnCombatText(CombatTextTransform, (CombatAmountType)byteCombatAmountType, amount);
        }

        public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            CallNetFunction(NetFuncCombatAmount, FunctionReceivers.All, (byte)combatAmountType, amount);
        }

        public virtual void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead)
                return;
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }

        public virtual void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            RequestCombatAmount(combatAmountType, damage);
            if (onReceivedDamage != null)
                onReceivedDamage.Invoke(attacker, combatAmountType, damage);
        }

        public virtual bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return true;
        }
    }
}
