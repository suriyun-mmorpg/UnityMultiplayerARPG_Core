using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterSkillLevel
{
    public string skillId;
    public int level;

    public Skill Skill
    {
        get { return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null; }
    }

    public float ConsumeMp
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return 0f;
            return skill.baseConsumeMp + skill.consumeMpIncreaseEachLevel * level;
        }
    }

    public float CoolDown
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return 0f;
            return skill.baseCoolDown + skill.coolDownIncreaseEachLevel * level;
        }
    }

    public CharacterStats Stats
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return new CharacterStats();
            return skill.baseStats + skill.statsIncreaseEachLevel * level;
        }
    }

    public CharacterStatsPercentage StatsPercentage
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return new CharacterStatsPercentage();
            return skill.statsPercentageIncreaseEachLevel * level;
        }
    }

    public float BuffDuration
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return 0f;
            var duration = skill.baseBuffDuration + skill.buffDurationIncreaseEachLevel * level;
            if (duration < 0)
                duration = 0;
            return duration;
        }
    }

    public float RecoveryHp
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return 0f;
            return skill.baseRecoveryHp + skill.recoveryHpIncreaseEachLevel * level;
        }
    }

    public float RecoveryMp
    {
        get
        {
            var skill = Skill;
            if (skill == null)
                return 0f;
            return skill.baseRecoveryMp + skill.recoveryMpIncreaseEachLevel * level;
        }
    }
}

public class NetFieldCharacterSkillLevel : LiteNetLibNetField<CharacterSkillLevel>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkillLevel();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(Value.skillId);
        writer.Put(Value.level);
    }

    public override bool IsValueChanged(CharacterSkillLevel newValue)
    {
        return !newValue.Equals(Value);
    }
}

[System.Serializable]
public class SyncListCharacterSkillLevel : LiteNetLibSyncList<NetFieldCharacterSkillLevel, CharacterSkillLevel> { }
