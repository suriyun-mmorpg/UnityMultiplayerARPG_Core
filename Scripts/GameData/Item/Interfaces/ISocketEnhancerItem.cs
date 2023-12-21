namespace MultiplayerARPG
{
    public partial interface ISocketEnhancerItem : IItem, IItemWithStatusEffectApplyings
    {
        /// <summary>
        /// Stats whichc will be increased to item which put this item into it
        /// </summary>
        EquipmentBonus SocketEnhanceEffect { get; }
    }
}
