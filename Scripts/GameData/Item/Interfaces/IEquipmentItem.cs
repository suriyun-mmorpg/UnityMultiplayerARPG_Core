using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IEquipmentItem : IItem
    {
        /// <summary>
        /// Requirement to equip the item
        /// </summary>
        EquipmentRequirement Requirement { get; }
        /// <summary>
        /// Cached required attribute amounts to equip the item
        /// </summary>
        Dictionary<Attribute, float> RequireAttributeAmounts { get; }
        /// <summary>
        /// Equipment set, if character equipping the same set of items, it can increase extra stats to character
        /// </summary>
        EquipmentSet EquipmentSet { get; }
        /// <summary>
        /// Max durability
        /// </summary>
        float MaxDurability { get; }
        /// <summary>
        /// If this is `TRUE` this item will be destroyed if broken (current durability = 0)
        /// </summary>
        bool DestroyIfBroken { get; }
        /// <summary>
        /// Max enhancement socket
        /// </summary>
        byte MaxSocket { get; }
        /// <summary>
        /// Equipment models, these models will be instantiated when equipping this item, for weapons it will be instantiated when equipping to main-hand (right-hand)
        /// </summary>
        EquipmentModel[] EquipmentModels { get; }
        /// <summary>
        /// Increasing stats while equipping this item
        /// </summary>
        CharacterStatsIncremental IncreaseStats { get; }
        /// <summary>
        /// Increasing stats rate while equipping this item
        /// </summary>
        CharacterStatsIncremental IncreaseStatsRate { get; }
        /// <summary>
        /// Increasing attributes while equipping this item
        /// </summary>
        AttributeIncremental[] IncreaseAttributes { get; }
        /// <summary>
        /// Increasing attributes rate while equipping this item
        /// </summary>
        AttributeIncremental[] IncreaseAttributesRate { get; }
        /// <summary>
        /// Increasing resistances while equipping this item
        /// </summary>
        ResistanceIncremental[] IncreaseResistances { get; }
        /// <summary>
        /// Increasing armors stats while equipping this item
        /// </summary>
        ArmorIncremental[] IncreaseArmors { get; }
        /// <summary>
        /// Increasing damages stats while equipping this item
        /// </summary>
        DamageIncremental[] IncreaseDamages { get; }
        /// <summary>
        /// Increasing skills while equipping this item
        /// </summary>
        SkillIncremental[] IncreaseSkills { get; }
        /// <summary>
        /// Status effects that can be applied to the attacker when attacking
        /// </summary>
        StatusEffectApplying[] SelfStatusEffectsWhenAttacking { get; }
        /// <summary>
        /// Status effects that can be applied to the enemy when attacking
        /// </summary>
        StatusEffectApplying[] EnemyStatusEffectsWhenAttacking { get; }
        /// <summary>
        /// Status effects that can be applied to the attacker when attacked
        /// </summary>
        StatusEffectApplying[] SelfStatusEffectsWhenAttacked { get; }
        /// <summary>
        /// Status effects that can be applied to the enemy when attacked
        /// </summary>
        StatusEffectApplying[] EnemyStatusEffectsWhenAttacked { get; }
        /// <summary>
        /// Random bonus
        /// </summary>
        ItemRandomBonus RandomBonus { get; }
    }
}
