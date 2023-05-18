using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterChargeComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterChargeComponent
    {
        public bool IsCharging { get; protected set; }

        protected struct ChargeState
        {
            public bool IsStopping;
            public bool IsLeftHand;
        }

        public bool WillDoActionWhenStopCharging
        {
            get
            {
                return IsCharging && (Time.unscaledTime - _chargeStartTime >= _chargeDuration);
            }
        }
        public float MoveSpeedRateWhileCharging { get; protected set; }
        public MovementRestriction MovementRestrictionWhileCharging { get; protected set; }

        protected CharacterActionComponentManager _manager;
        protected float _chargeStartTime;
        protected float _chargeDuration;
        // Network data sending
        protected ChargeState? _clientState;
        protected ChargeState? _serverState;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
        }

        public virtual void ClearChargeStates()
        {
            IsCharging = false;
        }

        protected void PlayChargeAnimation(bool isLeftHand)
        {
            // Get weapon type data
            IWeaponItem weaponItem = Entity.GetAvailableWeapon(ref isLeftHand).GetWeaponItem();
            int weaponTypeDataId = weaponItem.WeaponType.DataId;
            // Play animation
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
            {
                // TPS model
                Entity.CharacterModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                Entity.CharacterModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel vehicleModel)
            {
                // Vehicle model
                vehicleModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                vehicleModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
            {
                // FPS model
                Entity.FpsModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand);
                Entity.FpsModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            // Set weapon charging state
            MoveSpeedRateWhileCharging = Entity.GetMoveSpeedRateWhileCharging(weaponItem);
            MovementRestrictionWhileCharging = Entity.GetMovementRestrictionWhileCharging(weaponItem);
            IsCharging = true;
            _chargeStartTime = Time.unscaledTime;
            _chargeDuration = weaponItem.ChargeDuration;
        }

        protected void StopChargeAnimation()
        {
            // Play animation
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
            {
                // TPS model
                Entity.CharacterModel.StopWeaponChargeAnimation();
            }
            if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel vehicleModel)
            {
                // Vehicle model
                vehicleModel.StopWeaponChargeAnimation();
            }
            if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
            {
                // FPS model
                Entity.FpsModel.StopWeaponChargeAnimation();
            }
            // Set weapon charging state
            IsCharging = false;
        }

        public void StartCharge(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                // Prepare state data which will be sent to server
                _clientState = new ChargeState()
                {
                    IsLeftHand = isLeftHand,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Start charge immediately at server
                ProceedStartChargeStateAtServer(isLeftHand);
            }
        }

        public void StopCharge()
        {
            if (!IsServer && IsOwnerClient)
            {
                // Prepare state data which will be sent to server
                _clientState = new ChargeState()
                {
                    IsStopping = true,
                };
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Stop charge immediately at server
                ProceedStopChargeStateAtServer();
            }
        }

        protected void ProceedStartChargeStateAtServer(bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new ChargeState()
            {
                IsLeftHand = isLeftHand,
            };
#endif
        }

        protected void ProceedStopChargeStateAtServer()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!_manager.IsAcceptNewAction())
                return;
            _manager.ActionAccepted();
            // Prepare state data which will be sent to clients
            _serverState = new ChargeState()
            {
                IsStopping = true,
            };
#endif
        }

        public bool WriteClientStartChargeState(NetDataWriter writer)
        {
            if (_clientState.HasValue && !_clientState.Value.IsStopping)
            {
                // Simulate starting at client
                PlayChargeAnimation(_clientState.Value.IsLeftHand);
                // Send input to server
                writer.Put(_clientState.Value.IsLeftHand);
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerStartChargeState(NetDataWriter writer)
        {
            if (_serverState.HasValue && !_serverState.Value.IsStopping)
            {
                // Simulate starting at server
                PlayChargeAnimation(_serverState.Value.IsLeftHand);
                // Send input to client
                writer.Put(_serverState.Value.IsLeftHand);
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public bool WriteClientStopChargeState(NetDataWriter writer)
        {
            if (_clientState.HasValue && _clientState.Value.IsStopping)
            {
                // Simulate stopping at client
                StopChargeAnimation();
                // Clear Input
                _clientState = null;
                return true;
            }
            return false;
        }

        public bool WriteServerStopChargeState(NetDataWriter writer)
        {
            if (_serverState.HasValue && _serverState.Value.IsStopping)
            {
                // Simulate stopping at server
                StopChargeAnimation();
                // Clear Input
                _serverState = null;
                return true;
            }
            return false;
        }

        public void ReadClientStartChargeStateAtServer(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            ProceedStartChargeStateAtServer(isLeftHand);
        }

        public void ReadServerStartChargeStateAtClient(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't start charge again (it already done in `StartCharge` function)
                return;
            }
            PlayChargeAnimation(isLeftHand);
        }

        public void ReadClientStopChargeStateAtServer(NetDataReader reader)
        {
            ProceedStopChargeStateAtServer();
        }

        public void ReadServerStopChargeStateAtClient(NetDataReader reader)
        {
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't stop charge again (it already done in `StopCharge` function)
                return;
            }
            StopChargeAnimation();
        }
    }
}
