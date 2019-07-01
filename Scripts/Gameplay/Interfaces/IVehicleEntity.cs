using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IGameEntity, IEntityMovement
    {
        VehicleType VehicleType { get; }
        List<VehicleSeat> Seats { get; }
        bool IsDestroyWhenDriverExit { get; }
        bool IsAttackable(byte seatIndex);
        void SetPassenger(byte seatIndex, BaseGameEntity gameEntity);
        bool RemovePassenger(byte seatIndex);
        bool IsSeatAvailable(byte seatIndex);
        bool GetAvailableSeat(out byte seatIndex);
    }
}
