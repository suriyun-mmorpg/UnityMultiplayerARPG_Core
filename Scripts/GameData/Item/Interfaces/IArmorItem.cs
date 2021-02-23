namespace MultiplayerARPG
{
    public partial interface IArmorItem : IDefendEquipmentItem
    {
        /// <summary>
        /// Armor type data
        /// </summary>
        ArmorType ArmorType { get; }
        /// <summary>
        /// What kind of position (or slot) which can put this item into it.
        /// </summary>
        string EquipPosition { get; }
    }
}
