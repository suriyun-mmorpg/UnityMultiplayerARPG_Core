using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum BuffType : byte
    {
        SkillBuff,
        SkillDebuff,
        PotionBuff,
        GuildSkillBuff,
        StatusEffect,
    }

    [System.Serializable]
    public class CharacterBuff : INetSerializable
    {
        public static readonly CharacterBuff Empty = new CharacterBuff();
        public string id;
        public BuffType type;
        public int dataId;
        public short level;
        public float buffRemainsDuration;

        [System.NonSerialized]
        private BuffType dirtyType;
        [System.NonSerialized]
        private int dirtyDataId;
        [System.NonSerialized]
        private short dirtyLevel;

        [System.NonSerialized]
        private BaseSkill cacheSkill;
        [System.NonSerialized]
        private BaseItem cacheItem;
        [System.NonSerialized]
        private GuildSkill cacheGuildSkill;
        [System.NonSerialized]
        private StatusEffect cacheStatusEffect;
        [System.NonSerialized]
        private Buff cacheBuff;
        [System.NonSerialized]
        private float cacheDuration;
        [System.NonSerialized]
        private int cacheRecoveryHp;
        [System.NonSerialized]
        private int cacheRecoveryMp;
        [System.NonSerialized]
        private int cacheRecoveryStamina;
        [System.NonSerialized]
        private int cacheRecoveryFood;
        [System.NonSerialized]
        private int cacheRecoveryWater;
        [System.NonSerialized]
        private CharacterStats cacheIncreaseStats;
        [System.NonSerialized]
        private CharacterStats cacheIncreaseStatsRate;
        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheIncreaseAttributes;
        [System.NonSerialized]
        private Dictionary<Attribute, float> cacheIncreaseAttributesRate;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> cacheIncreaseResistances;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> cacheIncreaseArmors;
        [System.NonSerialized]
        private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;
        [System.NonSerialized]
        private Dictionary<DamageElement, MinMaxFloat> cacheDamageOverTimes;
        [System.NonSerialized]
        private int cacheMaxStack;
        [System.NonSerialized]
        private string cacheKey;

        public EntityInfo BuffApplier { get; private set; }

        private void MakeCache()
        {
            if (dirtyDataId != dataId || dirtyType != type || dirtyLevel != level)
            {
                cacheKey = type + "_" + dataId;
                dirtyDataId = dataId;
                dirtyType = type;
                dirtyLevel = level;
                cacheSkill = null;
                cacheItem = null;
                cacheGuildSkill = null;
                cacheBuff = Buff.Empty;
                cacheDuration = 0;
                cacheRecoveryHp = 0;
                cacheRecoveryMp = 0;
                cacheRecoveryStamina = 0;
                cacheRecoveryFood = 0;
                cacheRecoveryWater = 0;
                cacheIncreaseStats = CharacterStats.Empty;
                cacheIncreaseStatsRate = CharacterStats.Empty;
                cacheIncreaseAttributes = null;
                cacheIncreaseAttributesRate = null;
                cacheIncreaseResistances = null;
                cacheIncreaseArmors = null;
                cacheIncreaseDamages = null;
                cacheDamageOverTimes = null;
                cacheMaxStack = 0;
                switch (type)
                {
                    case BuffType.SkillBuff:
                    case BuffType.SkillDebuff:
                        if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill) && cacheSkill != null)
                            cacheBuff = type == BuffType.SkillBuff ? cacheSkill.Buff : cacheSkill.Debuff;
                        break;
                    case BuffType.PotionBuff:
                        if (GameInstance.Items.TryGetValue(dataId, out cacheItem) && cacheItem != null && cacheItem.IsPotion())
                            cacheBuff = (cacheItem as IPotionItem).Buff;
                        break;
                    case BuffType.GuildSkillBuff:
                        if (GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill) && cacheGuildSkill != null)
                            cacheBuff = cacheGuildSkill.Buff;
                        break;
                    case BuffType.StatusEffect:
                        if (GameInstance.StatusEffects.TryGetValue(dataId, out cacheStatusEffect) && cacheStatusEffect != null)
                            cacheBuff = cacheStatusEffect.Buff;
                        break;
                }
                cacheDuration = cacheBuff.GetDuration(level);
                cacheRecoveryHp = cacheBuff.GetRecoveryHp(level);
                cacheRecoveryMp = cacheBuff.GetRecoveryMp(level);
                cacheRecoveryStamina = cacheBuff.GetRecoveryStamina(level);
                cacheRecoveryFood = cacheBuff.GetRecoveryFood(level);
                cacheRecoveryWater = cacheBuff.GetRecoveryWater(level);
                cacheIncreaseStats = cacheBuff.GetIncreaseStats(level);
                cacheIncreaseStatsRate = cacheBuff.GetIncreaseStatsRate(level);
                cacheIncreaseAttributes = cacheBuff.GetIncreaseAttributes(level);
                cacheIncreaseAttributesRate = cacheBuff.GetIncreaseAttributesRate(level);
                cacheIncreaseResistances = cacheBuff.GetIncreaseResistances(level);
                cacheIncreaseArmors = cacheBuff.GetIncreaseArmors(level);
                cacheIncreaseDamages = cacheBuff.GetIncreaseDamages(level);
                cacheDamageOverTimes = cacheBuff.GetDamageOverTimes(level);
                cacheMaxStack = cacheBuff.GetMaxStack(level);
            }
        }

        public BaseSkill GetSkill()
        {
            MakeCache();
            return cacheSkill;
        }

        public BaseItem GetItem()
        {
            MakeCache();
            return cacheItem;
        }

        public GuildSkill GetGuildSkill()
        {
            MakeCache();
            return cacheGuildSkill;
        }

        public StatusEffect GetStatusEffect()
        {
            MakeCache();
            return cacheStatusEffect;
        }

        public Buff GetBuff()
        {
            MakeCache();
            return cacheBuff;
        }

        public float GetDuration()
        {
            MakeCache();
            return cacheDuration;
        }

        public int GetRecoveryHp()
        {
            MakeCache();
            return cacheRecoveryHp;
        }

        public int GetRecoveryMp()
        {
            MakeCache();
            return cacheRecoveryMp;
        }

        public int GetRecoveryStamina()
        {
            MakeCache();
            return cacheRecoveryStamina;
        }

        public int GetRecoveryFood()
        {
            MakeCache();
            return cacheRecoveryFood;
        }

        public int GetRecoveryWater()
        {
            MakeCache();
            return cacheRecoveryWater;
        }

        public CharacterStats GetIncreaseStats()
        {
            MakeCache();
            return cacheIncreaseStats;
        }

        public CharacterStats GetIncreaseStatsRate()
        {
            MakeCache();
            return cacheIncreaseStatsRate;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributes()
        {
            MakeCache();
            return cacheIncreaseAttributes;
        }

        public Dictionary<Attribute, float> GetIncreaseAttributesRate()
        {
            MakeCache();
            return cacheIncreaseAttributesRate;
        }

        public Dictionary<DamageElement, float> GetIncreaseResistances()
        {
            MakeCache();
            return cacheIncreaseResistances;
        }

        public Dictionary<DamageElement, float> GetIncreaseArmors()
        {
            MakeCache();
            return cacheIncreaseArmors;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages()
        {
            MakeCache();
            return cacheIncreaseDamages;
        }

        public Dictionary<DamageElement, MinMaxFloat> GetDamageOverTimes()
        {
            MakeCache();
            return cacheDamageOverTimes;
        }

        public int MaxStack()
        {
            MakeCache();
            return cacheMaxStack;
        }

        public string GetKey()
        {
            MakeCache();
            return cacheKey;
        }

        public bool ShouldRemove()
        {
            return buffRemainsDuration <= 0f;
        }

        public void Apply(EntityInfo buffApplier)
        {
            BuffApplier = buffApplier;
            buffRemainsDuration = GetDuration();
        }

        public void Update(float deltaTime)
        {
            buffRemainsDuration -= deltaTime;
        }

        public CharacterBuff Clone(bool generateNewId = false)
        {
            return new CharacterBuff()
            {
                id = generateNewId ? GenericUtils.GetUniqueId() : id,
                type = type,
                dataId = dataId,
                level = level,
                buffRemainsDuration = buffRemainsDuration,
            };
        }

        public static CharacterBuff Create(BuffType type, int dataId, short level = 1)
        {
            return new CharacterBuff()
            {
                id = GenericUtils.GetUniqueId(),
                type = type,
                dataId = dataId,
                level = level,
                buffRemainsDuration = 0f,
            };
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put((byte)type);
            writer.PutPackedInt(dataId);
            writer.PutPackedShort(level);
            writer.Put(buffRemainsDuration);
        }

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            type = (BuffType)reader.GetByte();
            dataId = reader.GetPackedInt();
            level = reader.GetPackedShort();
            buffRemainsDuration = reader.GetFloat();
        }
    }

    [System.Serializable]
    public class SyncListCharacterBuff : LiteNetLibSyncList<CharacterBuff>
    {
        protected override CharacterBuff DeserializeValueForSetOrDirty(int index, NetDataReader reader)
        {
            CharacterBuff result = this[index];
            result.buffRemainsDuration = reader.GetFloat();
            return result;
        }

        protected override void SerializeValueForSetOrDirty(int index, NetDataWriter writer, CharacterBuff value)
        {
            writer.Put(value.buffRemainsDuration);
        }
    }
}
