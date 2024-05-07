namespace MultiplayerARPG
{
    public interface IItemWithBuildingEntity : IItem
    {
        /// <summary>
        /// Building entity for this item
        /// </summary>
        BuildingEntity BuildingEntity { get; }
        AssetReferenceBuildingEntity AddressableBuildingEntity { get; }
    }
}