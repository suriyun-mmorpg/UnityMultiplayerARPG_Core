using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData : ICharacterData
{
    public string id;
    public int dataId;
    public string characterName;
    public short level;
    public int exp;
    public int currentHp;
    public int currentMp;
    public int currentStamina;
    public int currentFood;
    public int currentWater;
    public EquipWeapons equipWeapons;

    public List<CharacterAttribute> attributes = new List<CharacterAttribute>();
    public List<CharacterSkill> skills = new List<CharacterSkill>();
    public List<CharacterBuff> buffs = new List<CharacterBuff>();
    public List<CharacterItem> equipItems = new List<CharacterItem>();
    public List<CharacterItem> nonEquipItems = new List<CharacterItem>();

    public string Id { get { return id; } set { id = value; } }
    public int DataId { get { return dataId; } set { dataId = value; } }
    public string CharacterName { get { return characterName; } set { characterName = value; } }
    public short Level { get { return level; } set { level = value; } }
    public int Exp { get { return exp; } set { exp = value; } }
    public int CurrentHp { get { return currentHp; } set { currentHp = value; } }
    public int CurrentMp { get { return currentMp; } set { currentMp = value; } }
    public int CurrentStamina { get { return currentStamina; } set { currentStamina = value; } }
    public int CurrentFood { get { return currentFood; } set { currentFood = value; } }
    public int CurrentWater { get { return currentWater; } set { currentWater = value; } }
    public EquipWeapons EquipWeapons { get { return equipWeapons; } set { equipWeapons = value; } }

    public IList<CharacterAttribute> Attributes
    {
        get { return attributes; }
        set
        {
            attributes = new List<CharacterAttribute>();
            attributes.AddRange(value);
        }
    }

    public IList<CharacterSkill> Skills
    {
        get { return skills; }
        set
        {
            skills = new List<CharacterSkill>();
            skills.AddRange(value);
        }
    }

    public IList<CharacterBuff> Buffs
    {
        get { return buffs; }
        set
        {
            buffs = new List<CharacterBuff>();
            buffs.AddRange(value);
        }
    }

    public IList<CharacterItem> EquipItems
    {
        get { return equipItems; }
        set
        {
            equipItems = new List<CharacterItem>();
            equipItems.AddRange(value);
        }
    }

    public IList<CharacterItem> NonEquipItems
    {
        get { return nonEquipItems; }
        set
        {
            nonEquipItems = new List<CharacterItem>();
            nonEquipItems.AddRange(value);
        }
    }
}
