namespace MultiplayerARPG
{
    public partial interface IBuildingItem : IUsableItem
    {
        /// <summary>
        /// Which building entity which will be constructed when use this item
        /// </summary>
        BuildingEntity BuildingEntity { get; }
    }
}
