using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(CharacterActionComponentManager))]
    public class DefaultCharacterChargeComponent : BaseNetworkedGameEntityComponent<BaseCharacterEntity>, ICharacterChargeComponent
    {
        public bool IsCharging { get; protected set; }
        protected bool _skipMovementValidation;
        public bool IsSkipMovementValidationWhileCharging { get { return _skipMovementValidation; } set { _skipMovementValidation = value; } }
        protected bool _shouldUseRootMotion;
        public bool IsUseRootMotionWhileCharging { get { return _shouldUseRootMotion; } protected set { _shouldUseRootMotion = value; } }

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

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
        }

        public virtual void ClearChargeStates()
        {
            IsCharging = false;
        }

        protected virtual void PlayChargeAnimation(bool isLeftHand)
        {
            // Get weapon type data
            IWeaponItem weaponItem = Entity.GetAvailableWeapon(ref isLeftHand).GetWeaponItem();
            int weaponTypeDataId = weaponItem.WeaponType.DataId;
            // Play animation
            if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
            {
                // TPS model
                Entity.CharacterModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _skipMovementValidation, out _shouldUseRootMotion);
                Entity.CharacterModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (Entity.PassengingVehicleModel && Entity.PassengingVehicleModel is BaseCharacterModel vehicleModel)
            {
                // Vehicle model
                vehicleModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _skipMovementValidation, out _shouldUseRootMotion);
                vehicleModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
            {
                // FPS model
                Entity.FpsModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _, out _);
                Entity.FpsModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            // Set weapon charging state
            MoveSpeedRateWhileCharging = Entity.GetMoveSpeedRateWhileCharging(weaponItem);
            MovementRestrictionWhileCharging = Entity.GetMovementRestrictionWhileCharging(weaponItem);
            IsCharging = true;
            _chargeStartTime = Time.unscaledTime;
            _chargeDuration = weaponItem.ChargeDuration;
        }

        protected virtual void StopChargeAnimation()
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

        public virtual void StartCharge(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                PlayChargeAnimation(isLeftHand);
                RPC(CmdStartCharge, isLeftHand);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Start charge immediately at server
                ProceedCmdStartCharge(isLeftHand);
            }
        }

        [ServerRpc]
        protected void CmdStartCharge(bool isLeftHand)
        {
            ProceedCmdStartCharge(isLeftHand);
        }

        protected void ProceedCmdStartCharge(bool isLeftHand)
        {
            if (!_manager.IsAcceptNewAction())
                return;
            _manager.ActionAccepted();
            PlayChargeAnimation(isLeftHand);
            RPC(RpcStartCharge, isLeftHand);
        }

        [AllRpc]
        protected void RpcStartCharge(bool isLeftHand)
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't stop charge again
                return;
            }
            PlayChargeAnimation(isLeftHand);
        }

        public virtual void StopCharge()
        {
            if (!IsServer && IsOwnerClient)
            {
                StopChargeAnimation();
                RPC(CmdStopCharge);
            }
            else if (IsOwnerClientOrOwnedByServer)
            {
                // Stop charge immediately at server
                ProceedCmdStopCharge();
            }
        }

        [ServerRpc]
        protected void CmdStopCharge()
        {
            ProceedCmdStopCharge();
        }

        protected void ProceedCmdStopCharge()
        {
            StopChargeAnimation();
            RPC(RpcStopCharge);
        }

        [AllRpc]
        protected void RpcStopCharge()
        {
            if (IsServer || IsOwnerClient)
            {
                // Don't stop charge again
                return;
            }
            StopChargeAnimation();
        }
    }
}
