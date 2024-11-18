using LiteNetLib;
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
                return IsCharging && (Time.unscaledTime - _chargeStartTime >= _chargeDuration) && !Entity.IsDead();
            }
        }
        public float MoveSpeedRateWhileCharging { get; protected set; }
        public MovementRestriction MovementRestrictionWhileCharging { get; protected set; }

        protected CharacterActionComponentManager _manager;
        protected float _chargeStartTime;
        protected float _chargeDuration;
        // Logging data
        bool _entityIsPlayer = false;
        BasePlayerCharacterEntity _playerCharacterEntity = null;

        public override void EntityStart()
        {
            _manager = GetComponent<CharacterActionComponentManager>();
            if (Entity is BasePlayerCharacterEntity)
            {
                _entityIsPlayer = true;
                _playerCharacterEntity = Entity as BasePlayerCharacterEntity;
            }
        }

        public override void EntityOnDestroy()
        {
            ClearChargeStates();
            _manager = null;
            _entityIsPlayer = false;
            _playerCharacterEntity = null;
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
            // Vehicle model
            BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
            bool vehicleModelAvailable = vehicleModel != null;
            bool overridePassengerActionAnimations = Entity.PassengingVehicleSeat != null && Entity.PassengingVehicleSeat.overridePassengerActionAnimations;
            // Play animation
            if (vehicleModelAvailable)
            {
                // Vehicle model
                vehicleModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _skipMovementValidation, out _shouldUseRootMotion);
                vehicleModel.PlayEquippedWeaponCharge(isLeftHand);
            }
            if (!overridePassengerActionAnimations)
            {
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _skipMovementValidation, out _shouldUseRootMotion);
                    Entity.CharacterModel.PlayEquippedWeaponCharge(isLeftHand);
                }
                if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                {
                    // FPS model
                    Entity.FpsModel.PlayWeaponChargeClip(weaponTypeDataId, isLeftHand, out _, out _);
                    Entity.FpsModel.PlayEquippedWeaponCharge(isLeftHand);
                }
            }
            // Set weapon charging state
            MoveSpeedRateWhileCharging = Entity.GetMoveSpeedRateWhileCharging(weaponItem);
            MovementRestrictionWhileCharging = Entity.GetMovementRestrictionWhileCharging(weaponItem);
            IsCharging = true;
            _chargeStartTime = Time.unscaledTime;
            _chargeDuration = weaponItem.ChargeDuration;
            if (_entityIsPlayer && IsServer)
                GameInstance.ServerLogHandlers.LogChargeStart(_playerCharacterEntity);
        }

        protected virtual void StopChargeAnimation()
        {
            bool doActionWhenStopCharging = WillDoActionWhenStopCharging;
            // Vehicle model
            BaseCharacterModel vehicleModel = Entity.PassengingVehicleModel as BaseCharacterModel;
            bool vehicleModelAvailable = vehicleModel != null;
            bool overridePassengerActionAnimations = Entity.PassengingVehicleSeat != null && Entity.PassengingVehicleSeat.overridePassengerActionAnimations;
            // Play animation
            if (vehicleModelAvailable)
            {
                // Vehicle model
                vehicleModel.StopWeaponChargeAnimation();
            }
            if (!overridePassengerActionAnimations)
            {
                if (Entity.CharacterModel && Entity.CharacterModel.gameObject.activeSelf)
                {
                    // TPS model
                    Entity.CharacterModel.StopWeaponChargeAnimation();
                }
                if (IsClient && Entity.FpsModel && Entity.FpsModel.gameObject.activeSelf)
                {
                    // FPS model
                    Entity.FpsModel.StopWeaponChargeAnimation();
                }
            }
            // Set weapon charging state
            IsCharging = false;
            if (_entityIsPlayer && IsServer)
                GameInstance.ServerLogHandlers.LogChargeEnd(_playerCharacterEntity, doActionWhenStopCharging);
        }

        public virtual void StartCharge(bool isLeftHand)
        {
            if (!IsServer && IsOwnerClient)
            {
                PlayChargeAnimation(isLeftHand);
                RPC(CmdStartCharge, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
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
            RPC(RpcStartCharge, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered, isLeftHand);
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
                RPC(CmdStopCharge, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
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
            RPC(RpcStopCharge, BaseGameEntity.STATE_DATA_CHANNEL, DeliveryMethod.ReliableOrdered);
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
