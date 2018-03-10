using System.Collections;
using System.Collections.Generic;
using LiteNetLib.Utils;
using LiteNetLibHighLevel;

[System.Serializable]
public class CharacterSkill
{
    public string skillId;
    public int level;
    public float coolDownRemainsDuration;
    
    public Skill GetSkill()
    {
        return GameInstance.Skills.ContainsKey(skillId) ? GameInstance.Skills[skillId] : null;
    }

    public int GetConsumeMp()
    {
        return GetSkill().GetConsumeMp(level);
    }

    public float GetCoolDownDuration()
    {
        return GetSkill().GetCoolDownDuration(level);
    }

    public bool CanLevelUp(ICharacterData character)
    {
        return GetSkill().CanLevelUp(character, level);
    }

    public bool CanUse(int currentMp)
    {
        return level >= 1 && coolDownRemainsDuration <= 0f && currentMp >= GetConsumeMp();
    }

    public void Used()
    {
        coolDownRemainsDuration = GetCoolDownDuration();
    }

    public bool ShouldUpdate()
    {
        return coolDownRemainsDuration > 0f;
    }

    public void Update(float deltaTime)
    {
        coolDownRemainsDuration -= deltaTime;
    }
}

public class NetFieldCharacterSkill : LiteNetLibNetField<CharacterSkill>
{
    public override void Deserialize(NetDataReader reader)
    {
        var newValue = new CharacterSkill();
        newValue.skillId = reader.GetString();
        newValue.level = reader.GetInt();
        newValue.coolDownRemainsDuration = reader.GetFloat();
        Value = newValue;
    }

    public override void Serialize(NetDataWriter writer)
    {
        if (Value == null)
            Value = new CharacterSkill();
        writer.Put(Value.skillId);
        writer.Put(Value.level);
        writer.Put(Value.coolDownRemainsDuration);
    }

    public override bool IsValueChanged(CharacterSkill newValue)
    {
        return true;
    }
}

[System.Serializable]
public class SyncListCharacterSkill : LiteNetLibSyncList<NetFieldCharacterSkill, CharacterSkill> { }
