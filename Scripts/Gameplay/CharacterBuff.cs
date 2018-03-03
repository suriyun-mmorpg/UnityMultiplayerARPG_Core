using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public struct CharacterBuff
{
    public string skillId;
    public int level;
    public float buffRemainsDuration;

    public Skill Skill
    {
        get { return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null; }
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

    public bool ShouldRemove()
    {
        return buffRemainsDuration <= 0;
    }

    public void Added()
    {
        buffRemainsDuration = BuffDuration;
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
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.buffRemainsDuration);
    }

    public override bool IsValueChanged(CharacterBuff newValue)
    {
        return !newValue.Equals(Value);
    }
}

[System.Serializable]
public class SyncListCharacterBuff : LiteNetLibSyncList<NetFieldCharacterBuff, CharacterBuff> { }
