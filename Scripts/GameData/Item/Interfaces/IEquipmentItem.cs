using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IEquipmentItem : IItem
    {
        EquipmentRequirement Requirement { get; }
        Dictionary<Attribute, float> RequireAttributeAmounts { get; }
        EquipmentSet EquipmentSet { get; }
        float MaxDurability { get; }
        bool DestroyIfBroken { get; }
        byte MaxSocket { get; }
        EquipmentModel[] EquipmentModels { get; }
        CharacterStatsIncremental IncreaseStats { get; }
        CharacterStatsIncremental IncreaseStatsRate { get; }
        AttributeIncremental[] IncreaseAttributes { get; }
        AttributeIncremental[] IncreaseAttributesRate { get; }
        ResistanceIncremental[] IncreaseResistances { get; }
        ArmorIncremental[] IncreaseArmors { get; }
        DamageIncremental[] IncreaseDamages { get; }
        SkillLevel[] IncreaseSkillLevels { get; }
    }
}
