using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IRequirement
    {
        EquipmentRequirement Requirement { get; }
        /// <summary>
        /// Cached required attribute amounts to equip the item
        /// </summary>
        Dictionary<Attribute, float> RequireAttributeAmounts { get; }
    }
}
