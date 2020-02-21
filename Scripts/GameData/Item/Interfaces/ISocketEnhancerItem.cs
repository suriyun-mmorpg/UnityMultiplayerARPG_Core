namespace MultiplayerARPG
{
    public partial interface ISocketEnhancerItem : IItem
    {
        EquipmentBonus SocketEnhanceEffect { get; }
    }
}
