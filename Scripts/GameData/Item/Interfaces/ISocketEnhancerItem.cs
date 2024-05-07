namespace MultiplayerARPG
{
    public partial interface ISocketEnhancerItem : IItem, IItemWithStatusEffectApplyings
    {
        /// <summary>
        /// Can put gem into the specific socket :P
        /// </summary>
        SocketEnhancerType SocketEnhancerType { get; }
        /// <summary>
        /// Stats which will be increased to item which put this item into it
        /// </summary>
        EquipmentBonus SocketEnhanceEffect { get; }
        /// <summary>
        /// These abilities will replaces weapon's abilities by key, or add abilities that are not existed in the weapon
        /// </summary>
        BaseWeaponAbility[] WeaponAbilities { get; }
    }
}
