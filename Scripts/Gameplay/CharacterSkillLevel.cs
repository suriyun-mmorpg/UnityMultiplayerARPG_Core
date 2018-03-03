using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterSkillLevel
{
    public string skillId;
    public int level;
    public float coolDownRemainsDuration;

    public Skill GetSkill()
    {
        return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
    }

    public float GetConsumeMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseConsumeMp + skill.consumeMpIncreaseEachLevel * level;
    }

    public float GetCoolDownDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.baseCoolDownDuration + skill.coolDownDurationIncreaseEachLevel * level;
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

    public bool CanUse()
    {
        return level >= 1 && coolDownRemainsDuration <= 0;
    }

    public void Used()
    {
        coolDownRemainsDuration = GetCoolDownDuration();
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }
}

public class NetFieldCharacterSkillLevel : LiteNetLibNetField<CharacterSkillLevel>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkillLevel();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterSkillLevel();
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkillLevel newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkillLevel : LiteNetLibSyncList<NetFieldCharacterSkillLevel, CharacterSkillLevel> { }
