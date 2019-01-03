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

        public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }
        public abstract int MaxHp { get; }
        
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
            var uiSceneGameplay = UISceneGameplay.Singleton;
            if (uiSceneGameplay == null)
                return;
            uiSceneGameplay.SpawnCombatText(CombatTextTransform, (CombatAmountType)byteCombatAmountType, amount);
        }

        public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            CallNetFunction(NetFuncCombatAmount, FunctionReceivers.All, (byte)combatAmountType, amount);
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public virtual void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead())
                return;
            this.InvokeInstanceDevExtMethods("ReceiveDamage", attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }

        public virtual void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            this.InvokeInstanceDevExtMethods("ReceivedDamage", attacker, combatAmountType, damage);
            RequestCombatAmount(combatAmountType, damage);
        }

        public virtual bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return true;
        }
    }
}
