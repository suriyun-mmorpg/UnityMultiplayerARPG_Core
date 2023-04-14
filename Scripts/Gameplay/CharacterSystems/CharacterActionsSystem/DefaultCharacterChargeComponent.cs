using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultCharacterChargeComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterChargeComponent
    {
        public bool IsCharging { get; protected set; }
        public bool WillDoActionWhenStopCharging
        {
            get
            {
                return IsCharging && (Time.unscaledTime - _chargeStartTime >= _chargeDuration);
            }
        }
        public float MoveSpeedRateWhileCharging { get; protected set; }
        public MovementRestriction MovementRestrictionWhileCharging { get; protected set; }

        protected float _chargeStartTime;
        protected float _chargeDuration;
        protected bool _sendingClientStartCharge;
        protected bool _sendingClientStopCharge;
        protected bool _sendingServerStartCharge;
        protected bool _sendingServerStopCharge;
        protected bool _sendingIsLeftHand;

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
                // Simulate start charge at client immediately
                PlayChargeAnimation(isLeftHand);
                // Tell the server to start charge
                _sendingClientStartCharge = true;
                _sendingIsLeftHand = isLeftHand;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                ProceedStartChargeStateAtServer(isLeftHand);
            }
        }

        public void StopCharge()
        {
            if (!IsServer && IsOwnerClient)
            {
                // Simulate stop charge at client immediately
                StopChargeAnimation();
                // Tell the server to stop charge
                _sendingClientStopCharge = true;
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                ProceedStopChargeStateAtServer();
            }
        }

        public bool WriteClientStartChargeState(NetDataWriter writer)
        {
            if (_sendingClientStartCharge)
            {
                writer.Put(_sendingIsLeftHand);
                _sendingClientStartCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteServerStartChargeState(NetDataWriter writer)
        {
            if (_sendingServerStartCharge)
            {
                writer.Put(_sendingIsLeftHand);
                _sendingServerStartCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteClientStopChargeState(NetDataWriter writer)
        {
            if (_sendingClientStopCharge)
            {
                _sendingClientStopCharge = false;
                return true;
            }
            return false;
        }

        public bool WriteServerStopChargeState(NetDataWriter writer)
        {
            if (_sendingServerStopCharge)
            {
                _sendingServerStopCharge = false;
                return true;
            }
            return false;
        }

        public void ReadClientStartChargeStateAtServer(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            ProceedStartChargeStateAtServer(isLeftHand);
        }

        protected void ProceedStartChargeStateAtServer(bool isLeftHand)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Start charge at server immediately
            PlayChargeAnimation(isLeftHand);
            // Tell clients to start charge later
            _sendingServerStartCharge = true;
            _sendingIsLeftHand = isLeftHand;
#endif
        }

        public void ReadServerStartChargeStateAtClient(NetDataReader reader)
        {
            bool isLeftHand = reader.GetBool();
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't start charge again (it already played in `StartCharge` function)
                return;
            }
            PlayChargeAnimation(isLeftHand);
        }

        public void ReadClientStopChargeStateAtServer(NetDataReader reader)
        {
            ProceedStopChargeStateAtServer();
        }

        protected void ProceedStopChargeStateAtServer()
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Stop charge at server immediately
            StopChargeAnimation();
            // Tell clients to stop charge later
            _sendingServerStopCharge = true;
#endif
        }

        public void ReadServerStopChargeStateAtClient(NetDataReader reader)
        {
            if (IsOwnerClientOrOwnedByServer)
            {
                // Don't stop charge again (it already played in `StopCharge` function)
                return;
            }
            StopChargeAnimation();
        }
    }
}
