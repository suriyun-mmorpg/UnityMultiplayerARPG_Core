using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterData
{
    string ModelId { get; set; }
    string ClassId { get; set; }
    string CharacterName { get; set; }
    int Level { get; set; }
    int Exp { get; set; }
    int CurrentHp { get; set; }
    int CurrentMp { get; set; }
    EquipWeapons EquipWeapons { get; set; }
    // Listing
    IList<CharacterAttribute> Attributes { get; set; }
    IList<CharacterSkill> Skills { get; set; }
    IList<CharacterBuff> Buffs { get; set; }
    IList<CharacterItem> EquipItems { get; set; }
    IList<CharacterItem> NonEquipItems { get; set; }
}
