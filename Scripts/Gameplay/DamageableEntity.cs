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
        private GameEffect[] pendingHitEffects;

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
            switch ((CombatAmountType)byteCombatAmountType)
            {
                case CombatAmountType.NormalDamage:
                case CombatAmountType.CriticalDamage:
                case CombatAmountType.BlockedDamage:
                    if (Model != null)
                        Model.InstantiateEffect(pendingHitEffects);
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
            GameEffect[] effects = gameInstance.DefaultDamageHitEffects;
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
