using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IUsableItem : IItem, ICustomAimController, IRequirement
    {
        /// <summary>
        /// Cooldown duration before it is able to use again
        /// </summary>
        float UseItemCooldown { get; }
        void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem);

    }
}
