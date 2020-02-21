namespace MultiplayerARPG
{
    public partial interface IMountItem : IUsableItem
    {
        VehicleEntity MountEntity { get; }
    }
}
