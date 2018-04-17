using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterBuff
{
    public static readonly CharacterBuff Empty = new CharacterBuff();
    public string characterId;
    public string skillId;
    public bool isDebuff;
    public int level;
    public float buffRemainsDuration;
    [System.NonSerialized]
    private string dirtySkillId;
    [System.NonSerialized]
    private Skill cacheSkill;
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
        if (string.IsNullOrEmpty(skillId))
        {
            cacheSkill = null;
            cacheDuration = 0f;
            cacheRecoveryHp = 0;
            cacheRecoveryMp = 0;
            cacheIncreaseStats = new CharacterStats();
            cacheIncreaseAttributes = new Dictionary<Attribute, int>();
            cacheIncreaseResistances = new Dictionary<DamageElement, float>();
            return;
        }
        if (string.IsNullOrEmpty(dirtySkillId) || !dirtySkillId.Equals(skillId))
        {
            dirtySkillId = skillId;
            cacheSkill = GameInstance.Skills.TryGetValue(skillId, out cacheSkill) ? cacheSkill : null;
            if (cacheSkill != null)
            {
                cacheDuration = !isDebuff ? cacheSkill.buff.GetDuration(level) : cacheSkill.debuff.GetDuration(level);
                cacheRecoveryHp = !isDebuff ? cacheSkill.buff.GetRecoveryHp(level) : cacheSkill.debuff.GetRecoveryHp(level);
                cacheRecoveryMp = !isDebuff ? cacheSkill.buff.GetRecoveryMp(level) : cacheSkill.debuff.GetRecoveryMp(level);
                cacheIncreaseStats = !isDebuff ? cacheSkill.buff.GetIncreaseStats(level) : cacheSkill.debuff.GetIncreaseStats(level);
                cacheIncreaseAttributes = !isDebuff ? cacheSkill.buff.GetIncreaseAttributes(level) : cacheSkill.debuff.GetIncreaseAttributes(level);
                cacheIncreaseResistances = !isDebuff ? cacheSkill.buff.GetIncreaseResistances(level) : cacheSkill.debuff.GetIncreaseResistances(level);
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
        newBuff.skillId = skillId;
        newBuff.level = level;
        newBuff.isDebuff = isDebuff;
        newBuff.buffRemainsDuration = buffRemainsDuration;
        return newBuff;
    }

    public string GetBuffId()
    {
        return GetBuffId(characterId, skillId, isDebuff);
    }

    public static CharacterBuff Create(string characterId, string skillId, bool isDebuff, int level)
    {
        var newBuff = new CharacterBuff();
        newBuff.characterId = characterId;
        newBuff.skillId = skillId;
        newBuff.isDebuff = isDebuff;
        newBuff.level = level;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }

    public static string GetBuffId(string characterId, string skillId, bool isDebuff)
    {
        return string.Format("<{0}>_<{1}>_<{2}>", characterId, skillId, isDebuff.ToString());
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.characterId = reader.GetString();
        newValue.skillId = reader.GetString();
        newValue.isDebuff = reader.GetBool();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.characterId);
        writer.Put(Value.skillId);
        writer.Put(Value.isDebuff);
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
    public int IndexOf(string characterId, string skillId, bool isDebuff)
    {
        CharacterBuff tempBuff;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (tempBuff.characterId.Equals(characterId) && tempBuff.skillId.Equals(skillId) && tempBuff.isDebuff == isDebuff)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
