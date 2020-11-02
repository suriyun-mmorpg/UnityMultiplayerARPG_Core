using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum MonsterCharacteristic
    {
        Normal,
        Aggressive,
        Assist,
    }

    [System.Serializable]
    public struct MonsterCharacterAmount
    {
        public MonsterCharacter monster;
        public short amount;
    }

    [CreateAssetMenu(fileName = "Monster Character", menuName = "Create GameData/Monster Character", order = -4998)]
    public partial class MonsterCharacter : BaseCharacter
    {
        [Header("Monster Data")]
        [SerializeField]
        [Tooltip("This will be used to adjust stats. If this value is 100, it means current stats which set to this character data it is stats for character level 100, it will be used to adjust stats for character level 1.")]
        private short defaultLevel = 1;
        public short DefaultLevel { get { return defaultLevel; } }
        [SerializeField]
        [Tooltip("`Normal` will attack when being attacked, `Aggressive` will attack when enemy nearby, `Assist` will attack when other with same `Ally Id` being attacked.")]
        private MonsterCharacteristic characteristic;
        public MonsterCharacteristic Characteristic { get { return characteristic; } }
        [SerializeField]
        [Tooltip("This will work with assist characteristic only, to detect ally")]
        private ushort allyId;
        public ushort AllyId { get { return allyId; } }
        [SerializeField]
        [Tooltip("This move speed will be applies when it's wandering. if it's going to chase enemy, stats'moveSpeed will be applies")]
        private float wanderMoveSpeed;
        public float WanderMoveSpeed { get { return wanderMoveSpeed; } }
        [SerializeField]
        [Tooltip("Range to see an enemies and allies")]
        private float visualRange = 5f;
        public float VisualRange { get { return visualRange; } }
        [SerializeField]
        private MonsterSkill[] monsterSkills;

        [Header("Weapon/Attack Abilities")]
        [SerializeField]
        private DamageInfo damageInfo;
        public DamageInfo DamageInfo { get { return damageInfo; } }
        [SerializeField]
        private DamageIncremental damageAmount;
        public DamageIncremental DamageAmount
        {
            get
            {
                // Adjust base stats by default level
                if (defaultLevel <= 1)
                {
                    return damageAmount;
                }
                else
                {
                    if (!adjustDamageAmount.HasValue)
                    {
                        adjustDamageAmount = new DamageIncremental()
                        {
                            damageElement = damageAmount.damageElement,
                            amount = new IncrementalMinMaxFloat()
                            {
                                baseAmount = damageAmount.amount.baseAmount + (damageAmount.amount.amountIncreaseEachLevel * -(defaultLevel - 1)),
                                amountIncreaseEachLevel = damageAmount.amount.amountIncreaseEachLevel,
                            }
                        };
                    }
                    return adjustDamageAmount.Value;
                }
            }
        }
        [SerializeField]
        private float moveSpeedRateWhileAttacking = 0f;
        public float MoveSpeedRateWhileAttacking { get { return moveSpeedRateWhileAttacking; } }


        [Header("Killing Rewards")]
        [SerializeField]
        private int randomExpMin;
        [SerializeField]
        private int randomExpMax;
        [SerializeField]
        private int randomGoldMin;
        [SerializeField]
        private int randomGoldMax;
        [Tooltip("Max kind of items that will be dropped in ground")]
        [SerializeField]
        private byte maxDropItems = 5;
        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemDrop[] randomItems;
        [SerializeField]
        private ItemDropTable itemDropTable;

        [System.NonSerialized]
        private CharacterStatsIncremental? adjustStats;
        [System.NonSerialized]
        private AttributeIncremental[] adjustAttributes;
        [System.NonSerialized]
        private ResistanceIncremental[] adjustResistances;
        [System.NonSerialized]
        private ArmorIncremental[] adjustArmors;
        [System.NonSerialized]
        private DamageIncremental? adjustDamageAmount;

        [System.NonSerialized]
        private List<ItemDrop> cacheRandomItems;
        public List<ItemDrop> CacheRandomItems
        {
            get
            {
                if (cacheRandomItems == null)
                {
                    int i;
                    cacheRandomItems = new List<ItemDrop>();
                    if (randomItems != null &&
                        randomItems.Length > 0)
                    {
                        for (i = 0; i < randomItems.Length; ++i)
                        {
                            if (randomItems[i].item == null ||
                                randomItems[i].amount <= 0 ||
                                randomItems[i].dropRate <= 0)
                                continue;
                            cacheRandomItems.Add(randomItems[i]);
                        }
                    }
                    if (itemDropTable != null &&
                        itemDropTable.randomItems != null &&
                        itemDropTable.randomItems.Length > 0)
                    {
                        for (i = 0; i < itemDropTable.randomItems.Length; ++i)
                        {
                            if (itemDropTable.randomItems[i].item == null ||
                                itemDropTable.randomItems[i].amount <= 0 ||
                                itemDropTable.randomItems[i].dropRate <= 0)
                                continue;
                            cacheRandomItems.Add(itemDropTable.randomItems[i]);
                        }
                    }
                }
                return cacheRandomItems;
            }
        }

        public override sealed CharacterStatsIncremental Stats
        {
            get
            {
                // Adjust base stats by default level
                if (defaultLevel <= 1)
                {
                    return base.Stats;
                }
                else
                {
                    if (!adjustStats.HasValue)
                    {
                        adjustStats = new CharacterStatsIncremental()
                        {
                            baseStats = base.Stats.baseStats + (base.Stats.statsIncreaseEachLevel * -(defaultLevel - 1)),
                            statsIncreaseEachLevel = base.Stats.statsIncreaseEachLevel,
                        };
                    }
                    return adjustStats.Value;
                }
            }
        }

        public override sealed AttributeIncremental[] Attributes
        {
            get
            {
                // Adjust base attributes by default level
                if (defaultLevel <= 1)
                {
                    return base.Attributes;
                }
                else
                {
                    if (adjustAttributes == null)
                    {
                        adjustAttributes = new AttributeIncremental[base.Attributes.Length];
                        AttributeIncremental tempValue;
                        for (int i = 0; i < base.Attributes.Length; ++i)
                        {
                            tempValue = base.Attributes[i];
                            adjustAttributes[i] = new AttributeIncremental()
                            {
                                attribute = tempValue.attribute,
                                amount = new IncrementalFloat()
                                {
                                    baseAmount = tempValue.amount.baseAmount + (tempValue.amount.amountIncreaseEachLevel * -(defaultLevel - 1)),
                                    amountIncreaseEachLevel = tempValue.amount.amountIncreaseEachLevel,
                                }
                            };
                        }
                    }
                    return adjustAttributes;
                }
            }
        }

        public override sealed ResistanceIncremental[] Resistances
        {
            get
            {
                // Adjust base resistances by default level
                if (defaultLevel <= 1)
                {
                    return base.Resistances;
                }
                else
                {
                    if (adjustResistances == null)
                    {
                        adjustResistances = new ResistanceIncremental[base.Resistances.Length];
                        ResistanceIncremental tempValue;
                        for (int i = 0; i < base.Resistances.Length; ++i)
                        {
                            tempValue = base.Resistances[i];
                            adjustResistances[i] = new ResistanceIncremental()
                            {
                                damageElement = tempValue.damageElement,
                                amount = new IncrementalFloat()
                                {
                                    baseAmount = (short)(tempValue.amount.baseAmount + (tempValue.amount.amountIncreaseEachLevel * -(defaultLevel - 1))),
                                    amountIncreaseEachLevel = tempValue.amount.amountIncreaseEachLevel,
                                }
                            };
                        }
                    }
                    return adjustResistances;
                }
            }
        }

        public override sealed ArmorIncremental[] Armors
        {
            get
            {
                // Adjust base armors by default level
                if (defaultLevel <= 1)
                {
                    return base.Armors;
                }
                else
                {
                    if (adjustArmors == null)
                    {
                        adjustArmors = new ArmorIncremental[base.Armors.Length];
                        ArmorIncremental tempValue;
                        for (int i = 0; i < base.Armors.Length; ++i)
                        {
                            tempValue = base.Armors[i];
                            adjustArmors[i] = new ArmorIncremental()
                            {
                                damageElement = tempValue.damageElement,
                                amount = new IncrementalFloat()
                                {
                                    baseAmount = (short)(tempValue.amount.baseAmount + (tempValue.amount.amountIncreaseEachLevel * -(defaultLevel - 1))),
                                    amountIncreaseEachLevel = tempValue.amount.amountIncreaseEachLevel,
                                }
                            };
                        }
                    }
                    return adjustArmors;
                }
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, short> cacheSkillLevels;
        public override Dictionary<BaseSkill, short> CacheSkillLevels
        {
            get
            {
                if (cacheSkillLevels == null)
                    cacheSkillLevels = GameDataHelpers.CombineSkills(monsterSkills, new Dictionary<BaseSkill, short>());
                return cacheSkillLevels;
            }
        }

        private readonly List<MonsterSkill> tempRandomSkills = new List<MonsterSkill>();

        public virtual int RandomExp()
        {
            int min = randomExpMin;
            int max = randomExpMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public virtual int RandomGold()
        {
            int min = randomGoldMin;
            int max = randomGoldMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public virtual void RandomItems(System.Action<BaseItem, short> onRandomItem)
        {
            if (CacheRandomItems.Count == 0)
                return;
            ItemDrop randomItem;
            for (int countDrops = 0; countDrops < CacheRandomItems.Count && countDrops < maxDropItems; ++countDrops)
            {
                randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                if (Random.value > randomItem.dropRate)
                    continue;
                onRandomItem.Invoke(randomItem.item, randomItem.amount);
            }
        }

        public virtual bool RandomSkill(BaseMonsterCharacterEntity entity, out BaseSkill skill, out short level)
        {
            skill = null;
            level = 1;

            if (!entity.CanUseSkill())
                return false;

            if (monsterSkills == null || monsterSkills.Length == 0)
                return false;

            if (tempRandomSkills.Count != monsterSkills.Length)
            {
                tempRandomSkills.Clear();
                tempRandomSkills.AddRange(monsterSkills);
            }

            float random = Random.value;
            foreach (MonsterSkill monsterSkill in tempRandomSkills)
            {
                if (monsterSkill.skill == null)
                    continue;

                if (random < monsterSkill.useRate && (monsterSkill.useWhenHpRate <= 0 || entity.HpRate <= monsterSkill.useWhenHpRate))
                {
                    skill = monsterSkill.skill;
                    level = monsterSkill.level;
                    // Shuffle for next random
                    tempRandomSkills.Shuffle();
                    return true;
                }
            }
            return false;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            DamageInfo.PrepareRelatesData();
            GameInstance.AddItems(CacheRandomItems);
        }
    }
}
