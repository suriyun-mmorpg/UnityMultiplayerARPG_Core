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
