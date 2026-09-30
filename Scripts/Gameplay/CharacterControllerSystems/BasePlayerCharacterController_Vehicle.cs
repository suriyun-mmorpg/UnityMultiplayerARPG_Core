using Insthync.CameraAndInput;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct VehiclePlayerController
    {
        public VehicleType vehicleType;
        [Tooltip("Scene components on this player controller prefab, indexed by vehicle seat. Empty seats use normal character controls.")]
        public BaseVehiclePlayerController[] controllersForEachSeats;
    }

    public abstract partial class BasePlayerCharacterController
    {
        [Header("Vehicle Controls")]
        [SerializeField]
        protected VehiclePlayerController[] vehicleControllers = new VehiclePlayerController[0];

        public BaseVehiclePlayerController ActiveVehicleController { get; private set; }
        public virtual IGameplayCameraController GameplayCameraController => null;

        private BasePlayerCharacterEntity _updatedCharacter;
        private IVehicleEntity _updatedVehicle;
        private byte _updatedSeat;
        private BaseVehiclePlayerController _updatedVehicleController;
        private bool _usedVehicleController;
        private int _controllerUpdateFrame = -1;

        public virtual bool IsVehicleInputBlocked()
        {
            return !Application.isFocused || PlayingCharacterEntity == null ||
                PlayingCharacterEntity.IsDead() || PlayingCharacterEntity.IsDealing ||
                PlayingCharacterEntity.IsVendingStarted ||
                (UISceneGameplay != null && UISceneGameplay.IsBlockController()) ||
                (InputManager.IsUseNonMobileInput() && GenericUtils.IsFocusInputField());
        }

        public BaseVehiclePlayerController GetVehicleController(VehicleType vehicleType, byte seatIndex)
        {
            if (vehicleType == null || vehicleControllers == null)
                return null;
            for (int i = 0; i < vehicleControllers.Length; ++i)
            {
                VehiclePlayerController entry = vehicleControllers[i];
                if (entry.vehicleType == null || entry.vehicleType.DataId != vehicleType.DataId)
                    continue;
                if (entry.controllersForEachSeats == null || seatIndex >= entry.controllersForEachSeats.Length)
                    return null;
                BaseVehiclePlayerController controller = entry.controllersForEachSeats[seatIndex];
                // References are local components, never assets or another player's controller.
                if (controller == null || !controller.isActiveAndEnabled ||
                    !controller.transform.IsChildOf(transform))
                    return null;
                return controller;
            }
            return null;
        }

        protected bool UpdateVehicleController()
        {
            _updatedVehicleController = null;
            _usedVehicleController = false;
            IVehicleEntity vehicle = PlayingCharacterEntity.PassengingVehicleEntity;
            byte seatIndex = PlayingCharacterEntity.PassengingVehicleSeatIndex;
            BaseVehiclePlayerController next = vehicle.IsNull() || seatIndex >= vehicle.Seats.Count
                ? null : GetVehicleController(vehicle.VehicleType, seatIndex);

            if (next != ActiveVehicleController || (next != null && !IsVehicleControllerContextCurrent()))
            {
                ReleaseVehicleController();
                if (next != null)
                {
                    ResetCharacterControlState();
                    ActiveVehicleController = next;
                    next.Activate(this, PlayingCharacterEntity, vehicle, seatIndex);
                }
            }
            if (ActiveVehicleController == null)
                return false;
            _usedVehicleController = true;
            _updatedVehicleController = ActiveVehicleController;
            ActiveVehicleController.TickUpdate(Time.deltaTime);
            return true;
        }

        private bool IsVehicleControllerContextCurrent()
        {
            return ActiveVehicleController != null && ActiveVehicleController.isActiveAndEnabled &&
                ActiveVehicleController.Character == PlayingCharacterEntity &&
                !ActiveVehicleController.Vehicle.IsNull() &&
                ReferenceEquals(ActiveVehicleController.Vehicle, PlayingCharacterEntity.PassengingVehicleEntity) &&
                ActiveVehicleController.SeatIndex == PlayingCharacterEntity.PassengingVehicleSeatIndex;
        }

        internal void ReleaseVehicleController()
        {
            BaseVehiclePlayerController previous = ActiveVehicleController;
            ActiveVehicleController = null;
            if (previous != null)
            {
                previous.Deactivate();
                ResetCharacterControlState();
            }
        }

        /// <summary>Release queued actions and held input when changing control modes.</summary>
        protected virtual void ResetCharacterControlState()
        {
            // Clear the queued approach before stopping a channel: character-specific overrides
            // must not send StopMove to a vehicle now occupied as a passenger.
            ClearQueueUsingSkill();
            StopChanneledSkill();
            CancelBuild();
            SelectedEntity = null;
            TargetEntity = null;
            if (UISceneGameplay != null)
                UISceneGameplay.SetTargetEntity(null);
        }
    }
}
