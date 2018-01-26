using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSave
{
    public string characterName;
    public string characterClassId;
    public int level;
    public int exp;
    public int statPoint;
    public int skillPoint;
    public int gold;
    public readonly Dictionary<string, int> attributeLevels = new Dictionary<string, int>();
    public readonly Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    public CharacterClass Class
    {
        get { return GameInstance.CharacterClasses[characterClassId]; }
    }
}
