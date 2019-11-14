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
        public float HpRate { get { return (float)CurrentHp / (float)MaxHp; } }

        private readonly Queue<KeyValuePair<CombatAmountType, int>> spawningCombatTexts = new Queue<KeyValuePair<CombatAmountType, int>>();
        KeyValuePair<CombatAmountType, int> tempCombatTextData;
        private float tempTime;
        private float lastSpawnCombatTextTime;

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction<byte, int>(NetFuncCombatAmount);
        }

        protected override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
            if (spawningCombatTexts.Count == 0 || UISceneGameplay.Singleton == null)
                return;
            tempTime = Time.time;
            if (tempTime - lastSpawnCombatTextTime >= 0.1f)
            {
                lastSpawnCombatTextTime = tempTime;
                tempCombatTextData = spawningCombatTexts.Dequeue();
                UISceneGameplay.Singleton.SpawnCombatText(CombatTextTransform, tempCombatTextData.Key, tempCombatTextData.Value);
            }
        }

        /// <summary>
        /// This will be called on clients to display combat texts
        /// </summary>
        /// <param name="combatAmountType"></param>
        /// <param name="amount"></param>
        protected void NetFuncCombatAmount(byte byteCombatAmountType, int amount)
        {
            spawningCombatTexts.Enqueue(new KeyValuePair<CombatAmountType, int>((CombatAmountType)byteCombatAmountType, amount));
        }

        public virtual void RequestCombatAmount(CombatAmountType combatAmountType, int amount)
        {
            CallNetFunction(NetFuncCombatAmount, FunctionReceivers.All, (byte)combatAmountType, amount);
        }

        public bool IsDead()
        {
            return CurrentHp <= 0;
        }

        public virtual void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead())
                return;
            if (onReceiveDamage != null)
                onReceiveDamage.Invoke(attacker, weapon, damageAmounts, skill, skillLevel);
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

        public virtual void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            GameEffect[] effects = gameInstance.DefaultHitEffects.effects;
            if (skill != null && skill.GetHitEffect().effects != null && skill.GetHitEffect().effects.Length > 0)
            {
                // Set hit effects from skill's hit effects
                effects = skill.GetHitEffect().effects;
            }
            else
            {
                foreach (DamageElement element in damageElements)
                {
                    if (element.hitEffects.effects == null ||
                        element.hitEffects.effects.Length == 0)
                        continue;
                    effects = element.hitEffects.effects;
                    break;
                }
            }
            if (Model != null)
                Model.InstantiateEffect(effects);
        }
    }
}
