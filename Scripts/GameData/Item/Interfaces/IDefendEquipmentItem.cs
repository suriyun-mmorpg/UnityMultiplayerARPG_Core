namespace MultiplayerARPG
{
    public partial interface IDefendEquipmentItem : IEquipmentItem
    {
        ArmorIncremental ArmorAmount { get; }
    }
}
