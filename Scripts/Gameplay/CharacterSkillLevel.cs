using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public struct CharacterSkillLevel
{
    public string skillId;
    public int level;
}

public class SyncListCharacterSkillLevel : SyncListStruct<CharacterSkillLevel> { }
