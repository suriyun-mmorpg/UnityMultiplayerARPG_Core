using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

public enum BuffTypes : byte
{
    SkillBuff,
    SkillDebuff,
    PotionBuff,
}

[System.Serializable]
public struct CharacterBuff
{
    public static readonly CharacterBuff Empty = new CharacterBuff();
    public string characterId;
    public string dataId;
    public BuffTypes type;
    public int level;
    public float buffRemainsDuration;
    [System.NonSerialized]
    private BuffTypes dirtyType;
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
            return;
        }
        if (string.IsNullOrEmpty(dirtyDataId) || !dirtyDataId.Equals(dataId) || type != dirtyType)
        {
            dirtyDataId = dataId;
            dirtyType = type;
            cacheSkill = null;
            cacheItem = null;
            cacheBuff = null;
            if (type == BuffTypes.SkillBuff || type == BuffTypes.SkillDebuff)
            {
                cacheSkill = GameInstance.Skills.TryGetValue(dataId, out cacheSkill) ? cacheSkill : null;
                if (cacheSkill != null)
                    cacheBuff = type == BuffTypes.SkillBuff ? cacheSkill.buff : cacheSkill.debuff;
            }
            if (type == BuffTypes.PotionBuff)
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

    public CharacterBuff Clone()
    {
        var newBuff = new CharacterBuff();
        newBuff.dataId = dataId;
        newBuff.level = level;
        newBuff.type = type;
        newBuff.buffRemainsDuration = buffRemainsDuration;
        return newBuff;
    }

    public string GetBuffId()
    {
        return GetBuffId(characterId, dataId, type);
    }

    public static CharacterBuff Create(string characterId, string dataId, BuffTypes type, int level)
    {
        var newBuff = new CharacterBuff();
        newBuff.characterId = characterId;
        newBuff.dataId = dataId;
        newBuff.type = type;
        newBuff.level = level;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }

    public static string GetBuffId(string characterId, string dataId, BuffTypes type)
    {
        return string.Format("<{0}>_<{1}>_<{2}>", characterId, dataId, type.ToString());
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.characterId = reader.GetString();
        newValue.dataId = reader.GetString();
        newValue.type = (BuffTypes)reader.GetByte();
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
    public int IndexOf(string characterId, string dataId, BuffTypes type)
    {
        CharacterBuff tempBuff;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (tempBuff.characterId.Equals(characterId) && tempBuff.dataId.Equals(dataId) && tempBuff.type == type)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
