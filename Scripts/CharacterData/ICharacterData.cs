using System.Collections.Generic;

public interface ICharacterData
{
    string Id { get; set; }
    int DataId { get; set; }
    string CharacterName { get; set; }
    short Level { get; set; }
    int Exp { get; set; }
    int CurrentHp { get; set; }
    int CurrentMp { get; set; }
    int CurrentStamina { get; set; }
    int CurrentFood { get; set; }
    int CurrentWater { get; set; }
    EquipWeapons EquipWeapons { get; set; }
    // Listing
    IList<CharacterAttribute> Attributes { get; set; }
    IList<CharacterSkill> Skills { get; set; }
    IList<CharacterBuff> Buffs { get; set; }
    IList<CharacterItem> EquipItems { get; set; }
    IList<CharacterItem> NonEquipItems { get; set; }
}
