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
    private Skill cacheSkill;
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
    private Dictionary<Attribute, short> cachePassiveBuffIncreaseAttributes;
    [System.NonSerialized]
    private Dictionary<DamageElement, float> cachePassiveBuffIncreaseResistances;
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
            cachePassiveBuffIncreaseAttributes = null;
            cachePassiveBuffIncreaseResistances = null;
            cachePassiveBuffIncreaseDamages = null;
            if (GameInstance.Skills.TryGetValue(dataId, out cacheSkill))
            {
                if (cacheSkill.skillType == SkillType.Passive)
                {
                    cachePassiveBuff = cacheSkill.buff;
                    cachePassiveBuffDuration = cachePassiveBuff.GetDuration(level);
                    cachePassiveBuffRecoveryHp = cachePassiveBuff.GetRecoveryHp(level);
                    cachePassiveBuffRecoveryMp = cachePassiveBuff.GetRecoveryMp(level);
                    cachePassiveBuffRecoveryStamina = cachePassiveBuff.GetRecoveryStamina(level);
                    cachePassiveBuffRecoveryFood = cachePassiveBuff.GetRecoveryFood(level);
                    cachePassiveBuffRecoveryWater = cachePassiveBuff.GetRecoveryWater(level);
                    cachePassiveBuffIncreaseStats = cachePassiveBuff.GetIncreaseStats(level);
                    cachePassiveBuffIncreaseAttributes = cachePassiveBuff.GetIncreaseAttributes(level);
                    cachePassiveBuffIncreaseResistances = cachePassiveBuff.GetIncreaseResistances(level);
                    cachePassiveBuffIncreaseDamages = cachePassiveBuff.GetIncreaseDamages(level);
                }
            }
        }
    }

    public Skill GetSkill()
    {
        MakeCache();
        return cacheSkill;
    }

    public bool CanLevelUp(IPlayerCharacterData character, bool checkSkillPoint = true)
    {
        return GetSkill().CanLevelUp(character, level, checkSkillPoint);
    }

    public bool CanUse(ICharacterData character)
    {
        return GetSkill().CanUse(character, level);
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

    public Dictionary<Attribute, short> GetPassiveBuffIncreaseAttributes()
    {
        MakeCache();
        return cachePassiveBuffIncreaseAttributes;
    }

    public Dictionary<DamageElement, float> GetPassiveBuffIncreaseResistances()
    {
        MakeCache();
        return cachePassiveBuffIncreaseResistances;
    }

    public Dictionary<DamageElement, MinMaxFloat> GetPassiveBuffIncreaseDamages()
    {
        MakeCache();
        return cachePassiveBuffIncreaseDamages;
    }

    public static CharacterSkill Create(Skill skill, short level)
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
