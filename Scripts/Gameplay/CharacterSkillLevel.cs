using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterSkillLevel
{
    public string skillId;
    public int level;
    public float coolDownRemainsDuration;

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

    public bool CanUse()
    {
        return level >= 1 && coolDownRemainsDuration <= 0;
    }

    public void Used()
    {
        coolDownRemainsDuration = CoolDown;
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
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkillLevel newValue)
    {
        return !newValue.Equals(Value);
    }
}

[System.Serializable]
public class SyncListCharacterSkillLevel : LiteNetLibSyncList<NetFieldCharacterSkillLevel, CharacterSkillLevel> { }
