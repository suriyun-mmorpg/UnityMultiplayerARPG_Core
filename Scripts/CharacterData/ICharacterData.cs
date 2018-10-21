using System.Collections.Generic;
using MultiplayerARPG;

public partial interface ICharacterData
{
    string Id { get; set; }
    int DataId { get; set; }
    int EntityId { get; set; }
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
    // Caching
    CharacterStats CacheStats { get; }
    Dictionary<Attribute, short> CacheAttributes { get; }
    Dictionary<Skill, short> CacheSkills { get; }
    Dictionary<DamageElement, float> CacheResistances { get; }
    Dictionary<DamageElement, MinMaxFloat> CacheIncreaseDamages { get; }
    int CacheMaxHp { get; }
    int CacheMaxMp { get; }
    int CacheMaxStamina { get; }
    int CacheMaxFood { get; }
    int CacheMaxWater { get; }
    float CacheTotalItemWeight { get; }
    float CacheAtkSpeed { get; }
    float CacheMoveSpeed { get; }
}
