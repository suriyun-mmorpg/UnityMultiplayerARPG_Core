namespace MultiplayerARPG
{
    public partial interface IArmorItem : IDefendEquipmentItem
    {
        ArmorType ArmorType { get; }
        string EquipPosition { get; }
    }
}
