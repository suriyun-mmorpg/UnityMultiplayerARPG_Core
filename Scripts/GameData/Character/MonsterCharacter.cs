using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public enum MonsterCharacteristic
    {
        Normal,
        Aggressive,
        Assist,
        NoHarm,
    }

    [System.Serializable]
    public struct MonsterCharacterAmount
    {
        public MonsterCharacter monster;
        public int amount;
    }

    [CreateAssetMenu(fileName = GameDataMenuConsts.MONSTER_CHARACTER_FILE, menuName = GameDataMenuConsts.MONSTER_CHARACTER_MENU, order = GameDataMenuConsts.MONSTER_CHARACTER_ORDER)]
    public partial class MonsterCharacter : BaseCharacter
    {
        [Category(2, "Monster Settings")]
        [Header("Monster Data")]
        [SerializeField]
        [Tooltip("This will be used to adjust stats. If this value is 100, it means current stats which set to this character data is stats for character level 100, it will be used to adjust stats for character level 1.")]
        private int defaultLevel = 1;
        public int DefaultLevel { get { return defaultLevel; } }
        [SerializeField]
        [Tooltip("`Normal` will attack when being attacked, `Aggressive` will attack when enemy nearby, `Assist` will attack when other with same `Ally Id` being attacked, `NoHarm` won't attack.")]
        private MonsterCharacteristic characteristic = MonsterCharacteristic.Normal;
        public MonsterCharacteristic Characteristic { get { return characteristic; } }
        [SerializeField]
        [Tooltip("This will work with assist characteristic only, to detect ally")]
        private ushort allyId = 0;
        public ushort AllyId { get { return allyId; } }
        [SerializeField]
        [Tooltip("This move speed will be applies when it's wandering. if it's going to chase enemy, stats'moveSpeed will be applies")]
        private float wanderMoveSpeed = 1f;
        public float WanderMoveSpeed { get { return wanderMoveSpeed; } }
        [SerializeField]
        [Tooltip("Range to see an enemies and allies")]
        private float visualRange = 5f;
        public float VisualRange { get { return visualRange; } }
        [SerializeField]
        [Tooltip("Range to see an enemies and allies while summoned")]
        private float summonedVisualRange = 10f;
        public float SummonedVisualRange { get { return summonedVisualRange; } }

        [Category(3, "Character Stats")]
        [SerializeField]
        [FormerlySerializedAs("monsterSkills")]
        private MonsterSkill[] skills = new MonsterSkill[0];
        [SerializeField]
        private Buff summonerBuff = Buff.Empty;
        public Buff SummonerBuff { get { return summonerBuff; } }

        [Category(4, "Attacking")]
        [SerializeField]
        private DamageInfo damageInfo = default;
        public DamageInfo DamageInfo { get { return damageInfo; } }
        [SerializeField]
        private DamageIncremental damageAmount = default;
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
                    if (!_adjustDamageAmount.HasValue)
                    {
                        _adjustDamageAmount = new DamageIncremental()
                        {
                            damageElement = damageAmount.damageElement,
                            amount = new IncrementalMinMaxFloat()
                            {
                                baseAmount = damageAmount.amount.baseAmount + (damageAmount.amount.amountIncreaseEachLevel * -(defaultLevel - 1)),
                                amountIncreaseEachLevel = damageAmount.amount.amountIncreaseEachLevel,
                            }
                        };
                    }
                    return _adjustDamageAmount.Value;
                }
            }
        }
        [SerializeField]
        private float moveSpeedRateWhileAttacking = 0f;
        public float MoveSpeedRateWhileAttacking { get { return moveSpeedRateWhileAttacking; } }

        [Category(5, "Killing Rewards")]
        [SerializeField]
        private IncrementalMinMaxInt randomExp = default;
        [SerializeField]
        private IncrementalMinMaxInt randomGold = default;
        [SerializeField]
        [ArrayElementTitle("currency")]
        public CurrencyRandomAmount[] randomCurrencies = new CurrencyRandomAmount[0];
        public ItemDropManager itemDropManager = new ItemDropManager();
        public ItemDropManager ItemDropManager { get { return itemDropManager; } }

        #region Being deprecated
        [HideInInspector]
        [SerializeField]
        [ArrayElementTitle("item")]
        private ItemDrop[] randomItems = new ItemDrop[0];

        [HideInInspector]
        [SerializeField]
        private ItemDropTable[] itemDropTables = new ItemDropTable[0];

        [HideInInspector]
        [SerializeField]
        private ItemRandomByWeightTable[] itemRandomByWeightTables = new ItemRandomByWeightTable[0];

        [HideInInspector]
        [SerializeField]
        [Tooltip("Max kind of items that will be dropped in ground")]
        private byte maxDropItems = 5;

        [HideInInspector]
        [SerializeField]
        private int randomExpMin;

        [HideInInspector]
        [SerializeField]
        private int randomExpMax;

        [HideInInspector]
        [SerializeField]
        private int randomGoldMin;

        [HideInInspector]
        [SerializeField]
        private int randomGoldMax;

        [HideInInspector]
        [SerializeField]
        private ItemDropTable itemDropTable = null;
        #endregion

        [System.NonSerialized]
        private CharacterStatsIncremental? _adjustStats = null;
        [System.NonSerialized]
        private AttributeIncremental[] _adjustAttributes = null;
        [System.NonSerialized]
        private ResistanceIncremental[] _adjustResistances = null;
        [System.NonSerialized]
        private ArmorIncremental[] _adjustArmors = null;
        [System.NonSerialized]
        private DamageIncremental? _adjustDamageAmount = null;
        [System.NonSerialized]
        private IncrementalMinMaxInt? _adjustRandomExp = null;
        [System.NonSerialized]
        private IncrementalMinMaxInt? _adjustRandomGold = null;

        [System.NonSerialized]
        private List<CurrencyRandomAmount> _cacheRandomCurrencies = null;
        public List<CurrencyRandomAmount> CacheRandomCurrencies
        {
            get
            {
                if (_cacheRandomCurrencies == null)
                {
                    int i;
                    _cacheRandomCurrencies = new List<CurrencyRandomAmount>();
                    if (randomCurrencies != null &&
                        randomCurrencies.Length > 0)
                    {
                        for (i = 0; i < randomCurrencies.Length; ++i)
                        {
                            if (randomCurrencies[i].currency == null ||
                                randomCurrencies[i].maxAmount <= 0)
                                continue;
                            _cacheRandomCurrencies.Add(randomCurrencies[i]);
                        }
                    }
                    if (itemDropTables != null &&
                        itemDropTables.Length > 0)
                    {
                        foreach (ItemDropTable itemDropTable in itemDropTables)
                        {
                            if (itemDropTable != null &&
                                itemDropTable.randomCurrencies != null &&
                                itemDropTable.randomCurrencies.Length > 0)
                            {
                                for (i = 0; i < itemDropTable.randomCurrencies.Length; ++i)
                                {
                                    if (itemDropTable.randomCurrencies[i].currency == null ||
                                        itemDropTable.randomCurrencies[i].maxAmount <= 0)
                                        continue;
                                    _cacheRandomCurrencies.Add(itemDropTable.randomCurrencies[i]);
                                }
                            }
                        }
                    }
                }
                return _cacheRandomCurrencies;
            }
        }

        public override CharacterStatsIncremental Stats
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
                    if (!_adjustStats.HasValue)
                    {
                        _adjustStats = new CharacterStatsIncremental()
                        {
                            baseStats = base.Stats.baseStats + (base.Stats.statsIncreaseEachLevel * -(defaultLevel - 1)),
                            statsIncreaseEachLevel = base.Stats.statsIncreaseEachLevel,
                        };
                    }
                    return _adjustStats.Value;
                }
            }
        }

        public override AttributeIncremental[] Attributes
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
                    if (_adjustAttributes == null)
                    {
                        _adjustAttributes = new AttributeIncremental[base.Attributes.Length];
                        AttributeIncremental tempValue;
                        for (int i = 0; i < base.Attributes.Length; ++i)
                        {
                            tempValue = base.Attributes[i];
                            _adjustAttributes[i] = new AttributeIncremental()
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
                    return _adjustAttributes;
                }
            }
        }

        public override ResistanceIncremental[] Resistances
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
                    if (_adjustResistances == null)
                    {
                        _adjustResistances = new ResistanceIncremental[base.Resistances.Length];
                        ResistanceIncremental tempValue;
                        for (int i = 0; i < base.Resistances.Length; ++i)
                        {
                            tempValue = base.Resistances[i];
                            _adjustResistances[i] = new ResistanceIncremental()
                            {
                                damageElement = tempValue.damageElement,
                                amount = new IncrementalFloat()
                                {
                                    baseAmount = tempValue.amount.baseAmount + (tempValue.amount.amountIncreaseEachLevel * -(defaultLevel - 1)),
                                    amountIncreaseEachLevel = tempValue.amount.amountIncreaseEachLevel,
                                }
                            };
                        }
                    }
                    return _adjustResistances;
                }
            }
        }

        public override ArmorIncremental[] Armors
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
                    if (_adjustArmors == null)
                    {
                        _adjustArmors = new ArmorIncremental[base.Armors.Length];
                        ArmorIncremental tempValue;
                        for (int i = 0; i < base.Armors.Length; ++i)
                        {
                            tempValue = base.Armors[i];
                            _adjustArmors[i] = new ArmorIncremental()
                            {
                                damageElement = tempValue.damageElement,
                                amount = new IncrementalFloat()
                                {
                                    baseAmount = tempValue.amount.baseAmount + (tempValue.amount.amountIncreaseEachLevel * -(defaultLevel - 1)),
                                    amountIncreaseEachLevel = tempValue.amount.amountIncreaseEachLevel,
                                }
                            };
                        }
                    }
                    return _adjustArmors;
                }
            }
        }

        [System.NonSerialized]
        private Dictionary<BaseSkill, int> _cacheSkillLevels = null;
        public override Dictionary<BaseSkill, int> CacheSkillLevels
        {
            get
            {
                if (_cacheSkillLevels == null)
                    _cacheSkillLevels = GameDataHelpers.CombineSkills(skills, new Dictionary<BaseSkill, int>());
                return _cacheSkillLevels;
            }
        }

        public IncrementalMinMaxInt AdjustedRandomExp
        {
            get
            {
                // Adjust base stats by default level
                if (defaultLevel <= 1)
                {
                    return randomExp;
                }
                else
                {
                    if (!_adjustRandomExp.HasValue)
                    {
                        MinMaxFloat adjustBaseAmount = new MinMaxFloat()
                        {
                            min = randomExp.baseAmount.min,
                            max = randomExp.baseAmount.max,
                        };
                        adjustBaseAmount += randomExp.amountIncreaseEachLevel * -(defaultLevel - 1);
                        _adjustRandomExp = new IncrementalMinMaxInt()
                        {
                            baseAmount = new MinMaxInt()
                            {
                                min = (int)adjustBaseAmount.min,
                                max = (int)adjustBaseAmount.max,
                            },
                            amountIncreaseEachLevel = randomExp.amountIncreaseEachLevel,
                        };
                    }
                    return _adjustRandomExp.Value;
                }
            }
        }

        public IncrementalMinMaxInt AdjustedRandomGold
        {
            get
            {
                // Adjust base stats by default level
                if (defaultLevel <= 1)
                {
                    return randomGold;
                }
                else
                {
                    if (!_adjustRandomGold.HasValue)
                    {
                        MinMaxFloat adjustBaseAmount = new MinMaxFloat()
                        {
                            min = randomExp.baseAmount.min,
                            max = randomExp.baseAmount.max,
                        };
                        adjustBaseAmount += randomGold.amountIncreaseEachLevel * -(defaultLevel - 1);
                        _adjustRandomGold = new IncrementalMinMaxInt()
                        {
                            baseAmount = new MinMaxInt()
                            {
                                min = (int)adjustBaseAmount.min,
                                max = (int)adjustBaseAmount.max,
                            },
                            amountIncreaseEachLevel = randomGold.amountIncreaseEachLevel,
                        };
                    }
                    return _adjustRandomGold.Value;
                }
            }
        }

        private readonly List<MonsterSkill> tempRandomSkills = new List<MonsterSkill>();

        public virtual int RandomExp(int level)
        {
            return AdjustedRandomExp.GetAmount(level).Random();
        }

        public virtual int RandomGold(int level)
        {
            return AdjustedRandomGold.GetAmount(level).Random();
        }

        public virtual void RandomItems(System.Action<BaseItem, int> onRandomItem, float rate = 1f)
        {
            ItemDropManager.RandomItems(onRandomItem, rate);
        }

        public virtual CurrencyAmount[] RandomCurrencies()
        {
            if (CacheRandomCurrencies.Count == 0)
                return new CurrencyAmount[0];
            List<CurrencyAmount> currencies = new List<CurrencyAmount>();
            CurrencyRandomAmount randomCurrency;
            for (int count = 0; count < CacheRandomCurrencies.Count; ++count)
            {
                randomCurrency = CacheRandomCurrencies[count];
                currencies.Add(new CurrencyAmount()
                {
                    currency = randomCurrency.currency,
                    amount = Random.Range(randomCurrency.minAmount, randomCurrency.maxAmount),
                });
            }
            return currencies.ToArray();
        }

        public virtual bool RandomSkill(BaseMonsterCharacterEntity entity, out BaseSkill skill, out int level)
        {
            skill = null;
            level = 1;

            if (!entity.CanUseSkill())
                return false;

            if (skills == null || skills.Length == 0)
                return false;

            if (tempRandomSkills.Count != skills.Length)
            {
                tempRandomSkills.Clear();
                tempRandomSkills.AddRange(skills);
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
            ItemDropManager.PrepareRelatesData();
        }

        public override bool Validate()
        {
            bool hasChanges = false;
            if (randomExpMin != 0 ||
                randomExpMax != 0)
            {
                hasChanges = true;
                if (randomExp.baseAmount.min == 0 &&
                    randomExp.baseAmount.max == 0 &&
                    randomExp.amountIncreaseEachLevel.min == 0 &&
                    randomExp.amountIncreaseEachLevel.max == 0)
                {
                    IncrementalMinMaxInt result = randomExp;
                    result.baseAmount.min = randomExpMin;
                    result.baseAmount.max = randomExpMax;
                    randomExp = result;
                }
                randomExpMin = 0;
                randomExpMax = 0;
            }
            if (randomGoldMin != 0 ||
                randomGoldMax != 0)
            {
                hasChanges = true;
                if (randomGold.baseAmount.min == 0 &&
                    randomGold.baseAmount.max == 0 &&
                    randomGold.amountIncreaseEachLevel.min == 0 &&
                    randomGold.amountIncreaseEachLevel.max == 0)
                {
                    IncrementalMinMaxInt result = randomGold;
                    result.baseAmount.min = randomGoldMin;
                    result.baseAmount.max = randomGoldMax;
                    randomGold = result;
                }
                randomGoldMin = 0;
                randomGoldMax = 0;
            }
            if (randomItems != null && randomItems.Length > 0)
            {
                hasChanges = true;
                List<ItemDrop> list = new List<ItemDrop>(itemDropManager.randomItems);
                list.AddRange(randomItems);
                itemDropManager.randomItems = list.ToArray();
                randomItems = null;
            }
            if (itemDropTable != null)
            {
                hasChanges = true;
                List<ItemDropTable> list = new List<ItemDropTable>(itemDropManager.itemDropTables)
                {
                    itemDropTable
                };
                itemDropManager.itemDropTables = list.ToArray();
                itemDropTable = null;
            }
            if (itemDropTables != null && itemDropTables.Length > 0)
            {
                hasChanges = true;
                List<ItemDropTable> list = new List<ItemDropTable>(itemDropManager.itemDropTables);
                list.AddRange(itemDropTables);
                itemDropManager.itemDropTables = list.ToArray();
                itemDropTables = null;
            }
            if (itemRandomByWeightTables != null && itemRandomByWeightTables.Length > 0)
            {
                hasChanges = true;
                List<ItemRandomByWeightTable> list = new List<ItemRandomByWeightTable>(itemDropManager.itemRandomByWeightTables);
                list.AddRange(itemRandomByWeightTables);
                itemDropManager.itemRandomByWeightTables = list.ToArray();
                itemRandomByWeightTables = null;
            }
            if (maxDropItems > 0)
            {
                hasChanges = true;
                itemDropManager.maxDropItems = maxDropItems;
                maxDropItems = 0;
            }
            return hasChanges || base.Validate();
        }
    }
}
