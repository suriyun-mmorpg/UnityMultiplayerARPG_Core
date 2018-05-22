using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibManager;

public enum BuffType : byte
{
    SkillBuff,
    SkillDebuff,
    PotionBuff,
}

[System.Serializable]
public struct CharacterBuff
{
    public static readonly CharacterBuff Empty = new CharacterBuff();
    // Use id as primary key
    public string id;
    public string characterId;
    public string dataId;
    public BuffType type;
    public int level;
    public float buffRemainsDuration;
    [System.NonSerialized]
    private BuffType dirtyType;
    [System.NonSerialized]
    private string dirtyDataId;
    [System.NonSerialized]
    private Skill cacheSkill;
    [System.NonSerialized]
    private Item cacheItem;
    [System.NonSerialized]
    private Buff cacheBuff;
    [System.NonSerialized]
    private float cacheDuration;
    [System.NonSerialized]
    private int cacheRecoveryHp;
    [System.NonSerialized]
    private int cacheRecoveryMp;
    [System.NonSerialized]
    private CharacterStats cacheIncreaseStats;
    [System.NonSerialized]
    private Dictionary<Attribute, int> cacheIncreaseAttributes;
    [System.NonSerialized]
    private Dictionary<DamageElement, float> cacheIncreaseResistances;
    [System.NonSerialized]
    private Dictionary<DamageElement, MinMaxFloat> cacheIncreaseDamages;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(dataId))
        {
            cacheSkill = null;
            cacheItem = null;
            cacheBuff = null;
            cacheDuration = 0f;
            cacheRecoveryHp = 0;
            cacheRecoveryMp = 0;
            cacheIncreaseStats = new CharacterStats();
            cacheIncreaseAttributes = new Dictionary<Attribute, int>();
            cacheIncreaseResistances = new Dictionary<DamageElement, float>();
            cacheIncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            return;
        }
        if (string.IsNullOrEmpty(dirtyDataId) || !dirtyDataId.Equals(dataId) || type != dirtyType)
        {
            dirtyDataId = dataId;
            dirtyType = type;
            cacheSkill = null;
            cacheItem = null;
            cacheBuff = null;
            cacheDuration = 0;
            cacheRecoveryHp = 0;
            cacheRecoveryMp = 0;
            cacheIncreaseStats = new CharacterStats();
            cacheIncreaseAttributes = null;
            cacheIncreaseResistances = null;
            cacheIncreaseDamages = null;
            if (type == BuffType.SkillBuff || type == BuffType.SkillDebuff)
            {
                cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
                if (cacheSkill != null)
                    cacheBuff = type == BuffType.SkillBuff ? cacheSkill.buff : cacheSkill.debuff;
            }
            if (type == BuffType.PotionBuff)
            {
                cacheItem = GameInstance.Items.TryGetValue(dataId, out cacheItem) ? cacheItem : null;
                if (cacheItem != null)
                    cacheBuff = cacheItem.buff;
            }
            if (cacheBuff != null)
            {
                cacheDuration = cacheBuff.GetDuration(level);
                cacheRecoveryHp = cacheBuff.GetRecoveryHp(level);
                cacheRecoveryMp = cacheBuff.GetRecoveryMp(level);
                cacheIncreaseStats = cacheBuff.GetIncreaseStats(level);
                cacheIncreaseAttributes = cacheBuff.GetIncreaseAttributes(level);
                cacheIncreaseResistances = cacheBuff.GetIncreaseResistances(level);
                cacheIncreaseDamages = cacheBuff.GetIncreaseDamages(level);
            }
        }
    }

    public bool IsEmpty()
    {
        return Equals(Empty);
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

    public int GetBuffRecoveryHp()
    {
        MakeCache();
        return cacheRecoveryHp;
    }

    public int GetBuffRecoveryMp()
    {
        MakeCache();
        return cacheRecoveryMp;
    }

    public CharacterStats GetIncreaseStats()
    {
        MakeCache();
        return cacheIncreaseStats;
    }

    public Dictionary<Attribute, int> GetIncreaseAttributes()
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

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0f;
    }

    public void Added()
    {
        buffRemainsDuration = GetDuration();
    }

    public void Update(float deltaTime)
    {
        buffRemainsDuration -= deltaTime;
    }

    public void ClearDuration()
    {
        buffRemainsDuration = 0;
    }

    public static CharacterBuff Create(string characterId, string dataId, BuffType type, int level)
    {
        var newBuff = new CharacterBuff();
        newBuff.characterId = characterId;
        newBuff.dataId = dataId;
        newBuff.type = type;
        newBuff.level = level;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.characterId = reader.GetString();
        newValue.dataId = reader.GetString();
        newValue.type = (BuffType)reader.GetByte();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.characterId);
        writer.Put(Value.dataId);
        writer.Put((byte)Value.type);
        writer.Put(Value.level);
        writer.Put(Value.buffRemainsDuration);
    }

    public override bool IsValueChanged(CharacterBuff newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterBuff : LiteNetLibSyncList<NetFieldCharacterBuff, CharacterBuff>
{
}
