namespace MultiplayerARPG
{
    public partial class VehicleEntity
    {
        public event System.Action onPassengersChanged;

        protected void NotifyPassengersChanged()
        {
            onPassengersChanged?.Invoke();
        }

        public virtual bool TryChangePassengerSeat(BaseGameEntity passenger, byte seatIndex)
        {
            if (!IsServer || passenger == null || passenger.ObjectId == 0 ||
                passenger.PassengingVehicleEntity != this ||
                !passenger.CanChangeVehicleSeat(seatIndex, out _))
                return false;
            byte previousSeat = passenger.PassengingVehicleSeatIndex;
            if (previousSeat >= passengerIds.Count || passengerIds[previousSeat] != passenger.ObjectId)
                return false;

            if (this.IsDriver(previousSeat))
            {
                Manager.Assets.SetObjectOwner(ObjectId, -1);
                StopMove();
            }

            // Preserve the passenger throughout the transfer. RemovePassenger would
            // play the exit flow, teleport them, and potentially destroy a mount.
            passengerIds[seatIndex] = passenger.ObjectId;
            passengerIds[previousSeat] = 0;
            if (passenger.PassengingVehicleSeatIndex != seatIndex)
                passenger.SetPassengingVehicle(seatIndex, this);
            if (this.IsDriver(seatIndex))
                Manager.Assets.SetObjectOwner(ObjectId, passenger.ConnectionId);
            return true;
        }
    }
}
