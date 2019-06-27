using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IGameEntity, IEntityMovement
    {
        VehicleType VehicleType { get; }
        List<VehicleSeat> Seats { get; }
        bool IsDriveable(byte seatIndex);
        bool IsAttackable(byte seatIndex);
        bool IsDestroyWhenExit(byte seatIndex);
    }
}
