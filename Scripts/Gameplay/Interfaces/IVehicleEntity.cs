using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IGameEntity, IEntityMovement
    {
        VehicleType VehicleType { get; }
        bool IsDrivable { get; }
        List<VehicleSeat> Seats { get; }
    }
}
