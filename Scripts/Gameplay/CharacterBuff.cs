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
    private string dirtySkillId;
    private int dirtyLevel;
    private Skill cacheSkill;

    private void MakeCache()
    {
        if (string.IsNullOrEmpty(skillId))
            return;
        if (string.IsNullOrEmpty(dirtySkillId) || dirtySkillId.Equals(skillId) || level != dirtyLevel)
        {
            dirtySkillId = skillId;
            dirtyLevel = level;
            if (cacheSkill == null)
                cacheSkill = GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
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

    public static CharacterBuff Create(Skill skill, int level, bool isDebuff)
    {
        var newBuff = new CharacterBuff();
        newBuff.skillId = skill.Id;
        newBuff.level = level;
        newBuff.isDebuff = isDebuff;
        newBuff.buffRemainsDuration = 0f;
        return newBuff;
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
public class SyncListCharacterBuff : LiteNetLibSyncList<NetFieldCharacterBuff, CharacterBuff> { }
