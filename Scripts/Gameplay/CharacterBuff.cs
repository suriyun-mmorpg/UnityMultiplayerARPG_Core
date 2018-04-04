using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterBuff
{
    public static readonly CharacterBuff Empty = new CharacterBuff();
    public string skillId;
    public bool isDebuff;
    public int level;
    public float buffRemainsDuration;
    [System.NonSerialized]
    private string dirtySkillId;
    [System.NonSerialized]
    private Skill cacheSkill;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(skillId))
        {
            cacheSkill = null;
            return;
        }
        if (string.IsNullOrEmpty(dirtySkillId) || !dirtySkillId.Equals(skillId))
        {
            dirtySkillId = skillId;
            cacheSkill = GameInstance.Skills.TryGetValue(skillId, out cacheSkill) ? cacheSkill : null;
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

    public Dictionary<Attribute, int> GetAttributes()
    {
        return !isDebuff ? GetSkill().buff.GetIncreaseAttributes(level) : GetSkill().debuff.GetIncreaseAttributes(level);
    }

    public Dictionary<Resistance, float> GetResistances()
    {
        return !isDebuff ? GetSkill().buff.GetIncreaseResistances(level) : GetSkill().debuff.GetIncreaseResistances(level);
    }

    public float GetDuration()
    {
        return !isDebuff ? GetSkill().GetBuffDuration(level) : GetSkill().GetDebuffDuration(level);
    }

    public CharacterStats GetStats()
    {
        return !isDebuff ? GetSkill().GetBuffStats(level) : GetSkill().GetDebuffStats(level);
    }

    public int GetBuffRecoveryHp()
    {
        return !isDebuff ? GetSkill().GetBuffRecoveryHp(level) : GetSkill().GetDebuffRecoveryHp(level);
    }

    public int GetBuffRecoveryMp()
    {
        return !isDebuff ? GetSkill().GetBuffRecoveryMp(level) : GetSkill().GetDebuffRecoveryMp(level);
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
        return GetBuffId(skillId, isDebuff);
    }

    public static CharacterBuff Create(string skillId, bool isDebuff, int level)
    {
        var newBuff = new CharacterBuff();
        newBuff.skillId = skillId;
        newBuff.isDebuff = isDebuff;
        newBuff.level = level;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
    }

    public static string GetBuffId(string skillId, bool isDebuff)
    {
        var keyPrefix = isDebuff ? GameDataConst.CHARACTER_DEBUFF_PREFIX : GameDataConst.CHARACTER_BUFF_PREFIX;
        return keyPrefix + skillId;
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.skillId = reader.GetString();
        newValue.isDebuff = reader.GetBool();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
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
    public int IndexOf(string skillId, bool isDebuff)
    {
        CharacterBuff tempBuff;
        var index = -1;
        for (var i = 0; i < list.Count; ++i)
        {
            tempBuff = list[i];
            if (!string.IsNullOrEmpty(tempBuff.skillId) &&
                tempBuff.skillId.Equals(skillId) && tempBuff.isDebuff == isDebuff)
            {
                index = i;
                break;
            }
        }
        return index;
    }
}
