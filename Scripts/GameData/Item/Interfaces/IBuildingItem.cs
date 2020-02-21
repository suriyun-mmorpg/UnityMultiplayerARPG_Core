namespace MultiplayerARPG
{
    public partial interface IBuildingItem : IUsableItem
    {
        BuildingEntity BuildingEntity { get; }
    }
}
