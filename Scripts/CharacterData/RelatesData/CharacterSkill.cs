using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MultiplayerARPG;

[System.Serializable]
public class CharacterSkill : INetSerializableWithElement
{
    public static readonly CharacterSkill Empty = new CharacterSkill();
    public int dataId;
    public short level;

    [System.NonSerialized]
    private int dirtyDataId;
    [System.NonSerialized]
    private short dirtyLevel;

    [System.NonSerialized]
    private BaseSkill cacheSkill;
    [System.NonSerialized]
    private Buff cachePassiveBuff;
    [System.NonSerialized]
    private float cachePassiveBuffDuration;
    [System.NonSerialized]
    private int cachePassiveBuffRecoveryHp;
    [System.NonSerialized]
    private int cachePassiveBuffRecoveryMp;
    [System.NonSerialized]
    private int cachePassiveBuffRecoveryStamina;
    [System.NonSerialized]
    private int cachePassiveBuffRecoveryFood;
    [System.NonSerialized]
    private int cachePassiveBuffRecoveryWater;
    [System.NonSerialized]
    private CharacterStats cachePassiveBuffIncreaseStats;
    [System.NonSerialized]
    private CharacterStats cachePassiveBuffIncreaseStatsRate;
    [System.NonSerialized]
    private Dictionary<Attribute, float> cachePassiveBuffIncreaseAttributes;
    [System.NonSerialized]
    private Dictionary<Attribute, float> cachePassiveBuffIncreaseAttributesRate;
    [System.NonSerialized]
    private Dictionary<DamageElement, float> cachePassiveBuffIncreaseResistances;
    [System.NonSerialized]
    private Dictionary<DamageElement, float> cachePassiveBuffIncreaseArmors;
    [System.NonSerialized]
    private Dictionary<DamageElement, MinMaxFloat> cachePassiveBuffIncreaseDamages;

    [System.NonSerialized]
    private LiteNetLibElement element;
    public LiteNetLibElement Element
    {
        get { return element; }
        set { element = value; }
    }

    private void MakeCache()
    {
        if (dirtyDataId != dataId || dirtyLevel != level)
        {
            dirtyDataId = dataId;
            dirtyLevel = level;
            cacheSkill = null;
            cachePassiveBuff = default(Buff);
            cachePassiveBuffDuration = 0;
            cachePassiveBuffRecoveryHp = 0;
            cachePassiveBuffRecoveryMp = 0;
            cachePassiveBuffRecoveryStamina = 0;
            cachePassiveBuffRecoveryFood = 0;
            cachePassiveBuffRecoveryWater = 0;
            cachePassiveBuffIncreaseStats = new CharacterStats();
            cachePassiveBuffIncreaseStatsRate = new CharacterStats();
            cachePassiveBuffIncreaseAttributes = null;
            cachePassiveBuffIncreaseAttributesRate = null;
            cachePassiveBuffIncreaseResistances = null;
            cachePassiveBuffIncreaseArmors = null;
            cachePassiveBuffIncreaseDamages = null;
            if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill))
            {
                if (cacheSkill.GetSkillType() == SkillType.Passive)
                {
                    cachePassiveBuff = cacheSkill.GetBuff();
                    cachePassiveBuffDuration = cachePassiveBuff.GetDuration(level);
                    cachePassiveBuffRecoveryHp = cachePassiveBuff.GetRecoveryHp(level);
                    cachePassiveBuffRecoveryMp = cachePassiveBuff.GetRecoveryMp(level);
                    cachePassiveBuffRecoveryStamina = cachePassiveBuff.GetRecoveryStamina(level);
                    cachePassiveBuffRecoveryFood = cachePassiveBuff.GetRecoveryFood(level);
                    cachePassiveBuffRecoveryWater = cachePassiveBuff.GetRecoveryWater(level);
                    cachePassiveBuffIncreaseStats = cachePassiveBuff.GetIncreaseStats(level);
                    cachePassiveBuffIncreaseStatsRate = cachePassiveBuff.GetIncreaseStatsRate(level);
                    cachePassiveBuffIncreaseAttributes = cachePassiveBuff.GetIncreaseAttributes(level);
                    cachePassiveBuffIncreaseAttributesRate = cachePassiveBuff.GetIncreaseAttributesRate(level);
                    cachePassiveBuffIncreaseResistances = cachePassiveBuff.GetIncreaseResistances(level);
                    cachePassiveBuffIncreaseArmors = cachePassiveBuff.GetIncreaseArmors(level);
                    cachePassiveBuffIncreaseDamages = cachePassiveBuff.GetIncreaseDamages(level);
                }
            }
        }
    }

    public BaseSkill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public float GetPassiveBuffDuration()
    {
        MakeCache();
        return cachePassiveBuffDuration;
    }

    public int GetPassiveBuffRecoveryHp()
    {
        MakeCache();
        return cachePassiveBuffRecoveryHp;
    }

    public int GetPassiveBuffRecoveryMp()
    {
        MakeCache();
        return cachePassiveBuffRecoveryMp;
    }

    public int GetPassiveBuffRecoveryStamina()
    {
        MakeCache();
        return cachePassiveBuffRecoveryStamina;
    }

    public int GetPassiveBuffRecoveryFood()
    {
        MakeCache();
        return cachePassiveBuffRecoveryFood;
    }

    public int GetPassiveBuffRecoveryWater()
    {
        MakeCache();
        return cachePassiveBuffRecoveryWater;
    }

    public CharacterStats GetPassiveBuffIncreaseStats()
    {
        MakeCache();
        return cachePassiveBuffIncreaseStats;
    }

    public CharacterStats GetPassiveBuffIncreaseStatsRate()
    {
        MakeCache();
        return cachePassiveBuffIncreaseStatsRate;
    }

    public Dictionary<Attribute, float> GetPassiveBuffIncreaseAttributes()
    {
        MakeCache();
        return cachePassiveBuffIncreaseAttributes;
    }

    public Dictionary<Attribute, float> GetPassiveBuffIncreaseAttributesRate()
    {
        MakeCache();
        return cachePassiveBuffIncreaseAttributesRate;
    }

    public Dictionary<DamageElement, float> GetPassiveBuffIncreaseResistances()
    {
        MakeCache();
        return cachePassiveBuffIncreaseResistances;
    }

    public Dictionary<DamageElement, float> GetPassiveBuffIncreaseArmors()
    {
        MakeCache();
        return cachePassiveBuffIncreaseArmors;
    }

    public Dictionary<DamageElement, MinMaxFloat> GetPassiveBuffIncreaseDamages()
    {
        MakeCache();
        return cachePassiveBuffIncreaseDamages;
    }

    public static CharacterSkill Create(BaseSkill skill, short level)
    {
        CharacterSkill newSkill = new CharacterSkill();
        newSkill.dataId = skill.DataId;
        newSkill.level = level;
        return newSkill;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(dataId);
        writer.Put(level);
    }

    public void Deserialize(NetDataReader reader)
    {
        dataId = reader.GetInt();
        level = reader.GetShort();
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<CharacterSkill>
{
}
