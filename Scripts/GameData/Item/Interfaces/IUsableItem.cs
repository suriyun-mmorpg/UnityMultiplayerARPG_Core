using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IUsableItem : IItem, ICustomAimController
    {
        /// <summary>
        /// Cooldown duration before it is able to use again
        /// </summary>
        float UseItemCooldown { get; }
        void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem);
        /// <summary>
        /// Requirement to equip the item
        /// </summary>
        EquipmentRequirement Requirement { get; }
        /// <summary>
        /// Cached required attribute amounts to equip the item
        /// </summary>
        Dictionary<Attribute, float> RequireAttributeAmounts { get; }
    }
}
