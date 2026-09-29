using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IItemWithRequirement
    {
        ItemRequirement Requirement { get; }
        /// <summary>
        /// Cached required attribute amounts to equip the item
        /// </summary>
        AttributeAmounts RequireAttributeAmounts { get; }
    }
}
