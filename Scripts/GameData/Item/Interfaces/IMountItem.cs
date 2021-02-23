namespace MultiplayerARPG
{
    public partial interface IMountItem : IUsableItem
    {
        /// <summary>
        /// Which vehicle entity which will be rided when use this item
        /// </summary>
        VehicleEntity MountEntity { get; }
    }
}
