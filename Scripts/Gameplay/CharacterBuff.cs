using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterBuff
{
    public string skillId;
    public int level;
    public float buffRemainsDuration;

    public Skill GetSkill()
    {
        return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
    }


    public CharacterStats GetStats()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStats();
        return skill.baseStats + skill.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStatsPercentage();
        return skill.statsPercentageIncreaseEachLevel * level;
    }

    public float GetBuffDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        var duration = skill.baseBuffDuration + skill.buffDurationIncreaseEachLevel * level;
        if (duration < 0)
            duration = 0;
        return duration;
    }

    public float GetRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseRecoveryHp + skill.recoveryHpIncreaseEachLevel * level;
    }

    public float GetRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseRecoveryMp + skill.recoveryMpIncreaseEachLevel * level;
    }

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0;
    }

    public void Added()
    {
        buffRemainsDuration = GetBuffDuration();
    }

    public void Update(float deltaTime)
    {
        buffRemainsDuration -= deltaTime;
    }
}

public class NetFieldCharacterBuff : LiteNetLibNetField<CharacterBuff>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterBuff();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterBuff();
        writer.Put(Value.skillId);
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
