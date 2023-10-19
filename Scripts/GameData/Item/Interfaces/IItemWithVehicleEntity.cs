namespace MultiplayerARPG
{
    public interface IItemWithVehicleEntity : IItem
    {
        /// <summary>
        /// Vehicle entity for this item
        /// </summary>
        VehicleEntity VehicleEntity { get; }
    }
}