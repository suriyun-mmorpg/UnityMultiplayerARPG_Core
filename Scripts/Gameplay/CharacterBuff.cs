using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterBuff
{
    public string skillId;
    public bool isDebuff;
    public int level;
    public float buffRemainsDuration;

    public Skill GetSkill()
    {
        return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
    }

    #region Buff
    public CharacterStats GetBuffStats()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStats();
        return skill.buff.baseStats + skill.buff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetBuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStatsPercentage();
        return skill.buff.baseStatsPercentage + skill.buff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetBuffDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        var duration = skill.buff.baseDuration + skill.buff.durationIncreaseEachLevel * level;
        if (duration < 0)
            duration = 0;
        return duration;
    }

    public float GetBuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.buff.baseRecoveryHp + skill.buff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetBuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.buff.baseRecoveryMp + skill.buff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

    #region Debuff
    public CharacterStats GetDebuffStats()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStats();
        return skill.debuff.baseStats + skill.debuff.statsIncreaseEachLevel * level;
    }

    public CharacterStatsPercentage GetDebuffStatsPercentage()
    {
        var skill = GetSkill();
        if (skill == null)
            return new CharacterStatsPercentage();
        return skill.debuff.baseStatsPercentage + skill.debuff.statsPercentageIncreaseEachLevel * level;
    }

    public float GetDebuffDuration()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        var duration = skill.debuff.baseDuration + skill.debuff.durationIncreaseEachLevel * level;
        if (duration < 0)
            duration = 0;
        return duration;
    }

    public float GetDebuffRecoveryHp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.debuff.baseRecoveryHp + skill.debuff.recoveryHpIncreaseEachLevel * level;
    }

    public float GetDebuffRecoveryMp()
    {
        var skill = GetSkill();
        if (skill == null)
            return 0f;
        return skill.debuff.baseRecoveryMp + skill.debuff.recoveryMpIncreaseEachLevel * level;
    }
    #endregion

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
        newValue.isDebuff = reader.GetBool();
        newValue.level = reader.GetInt();
        newValue.buffRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterBuff();
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
