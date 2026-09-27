using Insthync.UnityEditorUtils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.ATTRIBUTE_FILE, menuName = GameDataMenuConsts.ATTRIBUTE_MENU, order = GameDataMenuConsts.ATTRIBUTE_ORDER)]
    public partial class Attribute : BaseGameData
    {

        [System.NonSerialized]
        private int _runtimeSlot = -1;
        [Newtonsoft.Json.JsonIgnore]
        public int RuntimeSlot { get { return _runtimeSlot; } internal set { _runtimeSlot = value; } }

        [Category("Attribute Settings")]
        [SerializeField]
        private float battlePointScore = 10;
        public float BattlePointScore
        {
            get { return battlePointScore; }
        }

        [SerializeField]
        private CharacterStats statsIncreaseEachLevel = new CharacterStats();
        public CharacterStats StatsIncreaseEachLevel
        {
            get { return statsIncreaseEachLevel; }
        }

        [SerializeField]
        private ResistanceIncremental[] increaseResistances = new ResistanceIncremental[0];
        public ResistanceIncremental[] IncreaseResistances
        {
            get { return increaseResistances; }
        }

        [SerializeField]
        private ArmorIncremental[] increaseArmors = new ArmorIncremental[0];
        public ArmorIncremental[] IncreaseArmors
        {
            get { return increaseArmors; }
        }

        [SerializeField]
        private DamageIncremental[] increaseDamages = new DamageIncremental[0];
        public DamageIncremental[] IncreaseDamages
        {
            get { return increaseDamages; }
        }

        [SerializeField]
        private StatusEffectResistanceIncremental[] increaseStatusEffectResistances = new StatusEffectResistanceIncremental[0];
        public StatusEffectResistanceIncremental[] IncreaseStatusEffectResistances
        {
            get { return increaseStatusEffectResistances; }
        }

        [SerializeField]
        [Tooltip("If this value more than 0 it will limit max amount of this attribute by this value")]
        private int maxAmount;
        public int MaxAmount
        {
            get { return maxAmount; }
        }

        [SerializeField]
        private bool cannotReset = false;
        public bool CannotReset
        {
            get { return cannotReset; }
        }

        public bool CanIncreaseAmount(IPlayerCharacterData character, int amount, out UITextKeys gameMessage, bool checkStatPoint = true)
        {
            gameMessage = UITextKeys.NONE;
            if (character == null)
                return false;

            if (maxAmount > 0 && amount >= MaxAmount)
            {
                gameMessage = UITextKeys.UI_ERROR_ATTRIBUTE_REACHED_MAX_AMOUNT;
                return false;
            }

            if (checkStatPoint && character.StatPoint <= 0)
            {
                gameMessage = UITextKeys.UI_ERROR_NOT_ENOUGH_STAT_POINT;
                return false;
            }

            return true;
        }

        public virtual CharacterStats GetStatsByLevel(float level)
        {
            return StatsIncreaseEachLevel * level;
        }

        public virtual void GetIncreaseResistancesByLevel(float level, Dictionary<DamageElement, float> result)
        {
            DamageElementFloatAmounts amounts = default;
            GameDataHelpers.CombineResistances(IncreaseResistances, ref amounts, Mathf.CeilToInt(level), 1f);
            amounts.CopyTo(result);
        }

        public virtual void GetIncreaseResistancesByLevel(float level, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            if (GetType() != typeof(Attribute))
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
                {
                    GetIncreaseResistancesByLevel(level, temporary);
                    result.Combine(temporary);
                }
                return;
            }
            if (IncreaseResistances == null)
                return;
            int roundedLevel = Mathf.CeilToInt(level);
            foreach (ResistanceIncremental entry in IncreaseResistances)
            {
                DamageElement element = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), entry.amount.GetAmount(roundedLevel));
            }
        }

        public virtual void GetIncreaseArmorsByLevel(float level, Dictionary<DamageElement, float> result)
        {
            DamageElementFloatAmounts amounts = default;
            GameDataHelpers.CombineArmors(IncreaseArmors, ref amounts, Mathf.CeilToInt(level), 1f);
            amounts.CopyTo(result);
        }

        public virtual void GetIncreaseArmorsByLevel(float level, ref DamageElementFloatAmounts result)
        {
            result.Clear();
            if (GetType() != typeof(Attribute))
            {
                using (CollectionPool<Dictionary<DamageElement, float>, KeyValuePair<DamageElement, float>>.Get(out Dictionary<DamageElement, float> temporary))
                {
                    GetIncreaseArmorsByLevel(level, temporary);
                    result.Combine(temporary);
                }
                return;
            }
            if (IncreaseArmors == null)
                return;
            int roundedLevel = Mathf.CeilToInt(level);
            foreach (ArmorIncremental entry in IncreaseArmors)
            {
                DamageElement element = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), entry.amount.GetAmount(roundedLevel));
            }
        }

        public virtual void GetIncreaseDamagesByLevel(float level, Dictionary<DamageElement, MinMaxFloat> result)
        {
            DamageElementMinMaxFloatAmounts amounts = default;
            GameDataHelpers.CombineDamages(IncreaseDamages, ref amounts, Mathf.CeilToInt(level), 1f);
            amounts.CopyTo(result);
        }

        public virtual void GetIncreaseDamagesByLevel(float level, ref DamageElementMinMaxFloatAmounts result)
        {
            result.Clear();
            if (GetType() != typeof(Attribute))
            {
                using (CollectionPool<Dictionary<DamageElement, MinMaxFloat>, KeyValuePair<DamageElement, MinMaxFloat>>.Get(out Dictionary<DamageElement, MinMaxFloat> temporary))
                {
                    GetIncreaseDamagesByLevel(level, temporary);
                    result.Combine(temporary);
                }
                return;
            }
            if (IncreaseDamages == null)
                return;
            int roundedLevel = Mathf.CeilToInt(level);
            foreach (DamageIncremental entry in IncreaseDamages)
            {
                DamageElement element = entry.damageElement == null ? GameInstance.Singleton.DefaultDamageElement : entry.damageElement;
                result.Add(RuntimeGameDataSlots.GetSlot(element), entry.amount.GetAmount(roundedLevel));
            }
        }

        public virtual void GetIncreaseStatusEffectResistancesByLevel(float level, Dictionary<StatusEffect, float> result)
        {
            result.Clear();
            GameDataHelpers.CombineStatusEffectResistances(IncreaseStatusEffectResistances, result, Mathf.CeilToInt(level), 1f);
        }
    }

    [System.Serializable]
    public struct AttributeAmount
    {
        public Attribute attribute;
        public float amount;
    }

    [System.Serializable]
    public struct AttributeRandomAmount
    {
        public Attribute attribute;
        public float minAmount;
        public float maxAmount;
        [Range(0, 1f)]
        public float applyRate;

        public bool Apply(System.Random random)
        {
            return random.NextDouble() <= applyRate;
        }

        public AttributeAmount GetRandomedAmount(System.Random random)
        {
            return new AttributeAmount()
            {
                attribute = attribute,
                amount = random.RandomFloat(minAmount, maxAmount),
            };
        }
    }

    [System.Serializable]
    public struct AttributeIncremental
    {
        public Attribute attribute;
        public IncrementalFloat amount;
    }
}
