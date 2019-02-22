using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

public enum BuffType : byte
{
    SkillBuff,
    SkillDebuff,
    PotionBuff,
    GuildSkillBuff,
    SpawnBuff,
}

[System.Serializable]
public class CharacterBuff : INetSerializable
{
    public static readonly CharacterBuff Empty = new CharacterBuff();
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
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cacheItem;
    [System.NonSerialized]
    private GuildSkill cacheGuildSkill;
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
    private Dictionary<Attribute, short> cacheIncreaseAttributes;
    [System.NonSerialized]
    private Dictionary<DamageElement, float> cacheIncreaseResistances;
    [System.NonSerialized]
    private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;
    [System.NonSerialized]
    private Dictionary<DamageElement, MinMaxFloat> cacheDamageOverTimes;

    private void MakeCache()
    {
        if (dirtyDataId != dataId || dirtyType != type || dirtyLevel != level)
        {
            dirtyDataId = dataId;
            dirtyType = type;
            dirtyLevel = level;
            cacheSkill = null;
            cacheItem = null;
            cacheGuildSkill = null;
            cacheBuff = default(Buff);
            cacheDuration = 0;
            cacheRecoveryHp = 0;
            cacheRecoveryMp = 0;
            cacheRecoveryStamina = 0;
            cacheRecoveryFood = 0;
            cacheRecoveryWater = 0;
            cacheIncreaseStats = new CharacterStats();
            cacheIncreaseAttributes = null;
            cacheIncreaseResistances = null;
            cacheIncreaseDamages = null;
            cacheDamageOverTimes = null;
            switch (type)
            {
                case BuffType.SkillBuff:
                case BuffType.SkillDebuff:
                    if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill) && cacheSkill != null)
                        cacheBuff = type == BuffType.SkillBuff ? cacheSkill.buff : cacheSkill.debuff;
                    break;
                case BuffType.PotionBuff:
                    if (GameInstance.Items.TryGetValue(dataId, out cacheItem) && cacheItem != null)
                        cacheBuff = cacheItem.buff;
                    break;
                case BuffType.GuildSkillBuff:
                    if (GameInstance.GuildSkills.TryGetValue(dataId, out cacheGuildSkill) && cacheGuildSkill != null)
                        cacheBuff = cacheGuildSkill.buff;
                    break;
            }
            cacheDuration = cacheBuff.GetDuration(level);
            cacheRecoveryHp = cacheBuff.GetRecoveryHp(level);
            cacheRecoveryMp = cacheBuff.GetRecoveryMp(level);
            cacheRecoveryStamina = cacheBuff.GetRecoveryStamina(level);
            cacheRecoveryFood = cacheBuff.GetRecoveryFood(level);
            cacheRecoveryWater = cacheBuff.GetRecoveryWater(level);
            cacheIncreaseStats = cacheBuff.GetIncreaseStats(level);
            cacheIncreaseAttributes = cacheBuff.GetIncreaseAttributes(level);
            cacheIncreaseResistances = cacheBuff.GetIncreaseResistances(level);
            cacheIncreaseDamages = cacheBuff.GetIncreaseDamages(level);
            cacheDamageOverTimes = cacheBuff.GetDamageOverTimes(level);
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public Item GetItem()
    {
        MakeCache();
        return cacheItem;
    }

    public GuildSkill GetGuildSkill()
    {
        MakeCache();
        return cacheGuildSkill;
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

    public Dictionary<Attribute, short> GetIncreaseAttributes()
    {
        MakeCache();
        return cacheIncreaseAttributes;
    }

    public Dictionary<DamageElement, float> GetIncreaseResistances()
    {
        MakeCache();
        return cacheIncreaseResistances;
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

    public bool GetDisallowMove()
    {
        MakeCache();
        return cacheBuff.disallowMove;
    }

    public bool GetDisallowAttack()
    {
        MakeCache();
        return cacheBuff.disallowAttack;
    }

    public bool GetDisallowUseSkill()
    {
        MakeCache();
        return cacheBuff.disallowUseSkill;
    }

    public bool GetDisallowUseItem()
    {
        MakeCache();
        return cacheBuff.disallowUseItem;
    }

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0f;
    }

    public void Apply()
    {
        buffRemainsDuration = GetDuration();
    }

    public void Update(float deltaTime)
    {
        buffRemainsDuration -= deltaTime;
    }

    public string GetKey()
    {
        return type + "_" + dataId;
    }

    public static CharacterBuff Create(BuffType type, int dataId, short level = 1)
    {
        CharacterBuff newBuff = new CharacterBuff();
        newBuff.type = type;
        newBuff.dataId = dataId;
        newBuff.level = level;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)type);
        writer.Put(dataId);
        writer.Put(level);
        writer.Put(buffRemainsDuration);
    }

    public void Deserialize(NetDataReader reader)
    {
        type = (BuffType)reader.GetByte();
        dataId = reader.GetInt();
        level = reader.GetShort();
        buffRemainsDuration = reader.GetFloat();
    }
}

[System.Serializable]
public class SyncListCharacterBuff : LiteNetLibSyncList<CharacterBuff>
{
}
