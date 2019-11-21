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
    public sealed partial class MonsterCharacter : BaseCharacter
    {
        [Header("Monster Data")]
        public short defaultLevel = 1;
        [Tooltip("`Normal` will attack when being attacked, `Aggressive` will attack when enemy nearby, `Assist` will attack when other with same `Ally Id` being attacked.")]
        public MonsterCharacteristic characteristic;
        [Tooltip("This will work with assist characteristic only, to detect ally")]
        public ushort allyId;
        [Tooltip("This move speed will be applies when it's wandering. if it's going to chase enemy, stats'moveSpeed will be applies")]
        public float wanderMoveSpeed;
        [Tooltip("Range to see an enemy")]
        public float visualRange = 5f;
        [SerializeField]
        private MonsterSkill[] monsterSkills;

        [Header("Weapon/Attack Abilities")]
        public DamageInfo damageInfo;
        public DamageIncremental damageAmount;

        [Header("Killing Rewards")]
        public int randomExpMin;
        public int randomExpMax;
        public int randomGoldMin;
        public int randomGoldMax;
        [Tooltip("Max kind of items that will be dropped in ground")]
        public byte maxDropItems = 5;
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ItemDrop[] randomItems;
        public ItemDropTable itemDropTable;

        [System.NonSerialized]
        private CharacterStatsIncremental? adjustStats;
        [System.NonSerialized]
        private AttributeIncremental[] adjustAttributes;
        [System.NonSerialized]
        private ResistanceIncremental[] adjustResistances;
        [System.NonSerialized]
        private ArmorIncremental[] adjustArmors;

        [System.NonSerialized]
        private List<ItemDrop> cacheRandomItems;
        public List<ItemDrop> CacheRandomItems
        {
            get
            {
                if (cacheRandomItems == null)
                {
                    cacheRandomItems = new List<ItemDrop>(randomItems);
                    if (itemDropTable != null)
                        cacheRandomItems.AddRange(itemDropTable.randomItems);
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
                    if (adjustStats.HasValue)
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

        public int RandomExp()
        {
            int min = randomExpMin;
            int max = randomExpMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public int RandomGold()
        {
            int min = randomGoldMin;
            int max = randomGoldMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public void RandomItems(System.Action<Item, short> onRandomItem)
        {
            ItemDrop randomItem;

            for (int countDrops = 0; countDrops < CacheRandomItems.Count && countDrops < maxDropItems; ++countDrops)
            {
                randomItem = CacheRandomItems[Random.Range(0, CacheRandomItems.Count)];
                if (randomItem.item == null ||
                    randomItem.amount == 0 ||
                    Random.value > randomItem.dropRate)
                    continue;
                
                onRandomItem.Invoke(randomItem.item, randomItem.amount);
            }
        }

        public bool RandomSkill(BaseMonsterCharacterEntity entity, out BaseSkill skill, out short level)
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
    }
}
