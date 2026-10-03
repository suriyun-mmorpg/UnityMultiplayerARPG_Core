using System.Collections.Generic;

namespace MultiplayerARPG
{
    public interface IVehicleEntity : IActivatableEntity, IEntityMovement
    {
        int Level { get; set; }
        int CurrentHp { get; set; }
        VehicleType VehicleType { get; }
        List<VehicleSeat> Seats { get; }
        event System.Action onPassengersChanged;
        bool TryChangePassengerSeat(BaseGameEntity passenger, byte seatIndex);
        bool HasDriver { get; }
        bool CanBePassenger(byte seatIndex, BaseGameEntity gameEntity);
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
