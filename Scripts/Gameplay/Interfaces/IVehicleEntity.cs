using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IEntityMovement
    {
        VehicleType VehicleType { get; }
        List<VehicleSeat> Seats { get; }
        bool HasDriver { get; }
        bool IsAttackable(byte seatIndex);
        BaseGameEntity GetPassenger(byte seatIndex);
        List<BaseGameEntity> GetAllPassengers();
        void SetPassenger(byte seatIndex, BaseGameEntity gameEntity);
        bool RemovePassenger(byte seatIndex);
        void RemoveAllPassengers();
        bool IsSeatAvailable(byte seatIndex);
        bool GetAvailableSeat(out byte seatIndex);
        CalculatedBuff GetBuff();
    }
}
