using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameEntity
    {
        private float _lastVehicleSeatChangeTime = float.NegativeInfinity;

        public virtual bool CanChangeVehicleSeat(byte seatIndex, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.UI_ERROR_INVALID_VEHICLE_ENTITY;
            IVehicleEntity vehicle = PassengingVehicleEntity;
            if (vehicle.IsNull() || vehicle.Seats == null || PassengingVehicleSeatIndex >= vehicle.Seats.Count)
                return false;
            gameMessage = UITextKeys.UI_ERROR_SEAT_NOT_AVAILABLE;
            if (seatIndex == PassengingVehicleSeatIndex || seatIndex >= vehicle.Seats.Count ||
                vehicle.Seats[seatIndex] == null || !vehicle.IsSeatAvailable(seatIndex) ||
                !vehicle.CanBePassenger(seatIndex, this))
                return false;
            gameMessage = UITextKeys.NONE;
            return true;
        }

        public virtual bool ChangeVehicleSeat(byte seatIndex)
        {
            return IsServer && CanChangeVehicleSeat(seatIndex, out _) &&
                PassengingVehicleEntity.TryChangePassengerSeat(this, seatIndex);
        }

        public void CallCmdChangeVehicleSeat(uint vehicleObjectId, byte seatIndex)
        {
            if (!IsServer && !ConsumeVehicleSeatChangeDelay())
                return;
            RPC(CmdChangeVehicleSeat, vehicleObjectId, seatIndex);
        }

        [ServerRpc]
        protected void CmdChangeVehicleSeat(uint vehicleObjectId, byte seatIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            if (!ConsumeVehicleSeatChangeDelay())
                return;
            UITextKeys gameMessage = UITextKeys.UI_ERROR_INVALID_VEHICLE_ENTITY;
            IVehicleEntity vehicle = PassengingVehicleEntity;
            // A delayed request must never change seats in a different vehicle.
            if (!vehicle.IsNull() && vehicle.Entity.ObjectId == vehicleObjectId &&
                CanChangeVehicleSeat(seatIndex, out gameMessage))
            {
                if (ChangeVehicleSeat(seatIndex))
                    return;
                gameMessage = UITextKeys.UI_ERROR_SEAT_NOT_AVAILABLE;
            }
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, gameMessage);
#endif
        }

        private bool ConsumeVehicleSeatChangeDelay()
        {
            float time = Time.unscaledTime;
            if (time - _lastVehicleSeatChangeTime < CurrentGameInstance.mountDelay)
                return false;
            _lastVehicleSeatChangeTime = time;
            return true;
        }
    }
}
