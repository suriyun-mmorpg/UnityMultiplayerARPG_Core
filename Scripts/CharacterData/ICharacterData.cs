using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface ICharacterData
    {
        string Id { get; set; }
        int DataId { get; set; }
        int EntityId { get; set; }
        string CharacterName { get; set; }
        string Title { get; set; }
        int Level { get; set; }
        int Exp { get; set; }
        int CurrentHp { get; set; }
        int CurrentMp { get; set; }
        int CurrentStamina { get; set; }
        int CurrentFood { get; set; }
        int CurrentWater { get; set; }
        byte EquipWeaponSet { get; set; }
        IList<EquipWeapons> SelectableWeaponSets { get; set; }
        EquipWeapons EquipWeapons { get; set; }
        // Listing
        IList<CharacterAttribute> Attributes { get; set; }
        IList<CharacterSkill> Skills { get; set; }
        IList<CharacterSkillUsage> SkillUsages { get; set; }
        IList<CharacterBuff> Buffs { get; set; }
        IList<CharacterItem> EquipItems { get; set; }
        IList<CharacterItem> NonEquipItems { get; set; }
        IList<CharacterSummon> Summons { get; set; }
        IVehicleEntity PassengingVehicleEntity { get; }
    }
}
