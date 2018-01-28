using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

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

public class SyncListCharacterSkillLevel : SyncListStruct<CharacterSkillLevel> { }
