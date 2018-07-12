using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public abstract class DamageableNetworkEntity : RpgNetworkEntity
    {
        [SerializeField]
        protected SyncFieldInt currentHp = new SyncFieldInt();
        [SerializeField]
        protected Transform combatTextTransform;
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
        
        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction("CombatAmount", new LiteNetLibFunction<NetFieldByte, NetFieldInt>((combatAmountType, amount) => NetFuncCombatAmount((CombatAmountType)combatAmountType.Value, amount)));
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="amount"></param>
        protected void NetFuncCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            var uiSceneGameplay = UISceneGameplay.Singleton;
            if (uiSceneGameplay == null)
                return;
            uiSceneGameplay.SpawnCombatText(CombatTextTransform, combatAmountType, amount);
        }

        public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            CallNetFunction("CombatAmount", FunctionReceivers.All, combatAmountType, amount);
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public virtual void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
        {
            if (!IsServer || IsDead())
                return;
            this.InvokeClassAddOnMethods("ReceiveDamage", attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }

        public virtual void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            this.InvokeClassAddOnMethods("ReceivedDamage", attacker, combatAmountType, damage);
            RequestCombatAmount(combatAmountType, damage);
        }
    }
}
