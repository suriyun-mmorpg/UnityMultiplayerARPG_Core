using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameEntity
    {
        public byte PassengingVehicleSeatIndex { get; private set; }

        private IVehicleEntity _passengingVehicleEntity;
        public IVehicleEntity PassengingVehicleEntity
        {
            get
            {
                if (_passengingVehicleEntity.IsNull())
                    _passengingVehicleEntity = null;
                return _passengingVehicleEntity;
            }
            private set
            {
                _passengingVehicleEntity = value;
            }
        }

        public VehicleType PassengingVehicleType
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.VehicleType;
                return null;
            }
        }

        public VehicleSeat PassengingVehicleSeat
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.Seats[PassengingVehicleSeatIndex];
                return VehicleSeat.Empty;
            }
        }

        public GameEntityModel PassengingVehicleModel
        {
            get
            {
                if (!PassengingVehicleEntity.IsNull())
                    return PassengingVehicleEntity.Entity.Model;
                return null;
            }
        }

        protected float _lastMountTime;

        public virtual async void Mount(VehicleEntity prefab, AssetReferenceLiteNetLibBehaviour<VehicleEntity> addressablePrefab)
        {
            if (!IsServer || (prefab == null && !addressablePrefab.IsDataValid()) || Time.unscaledTime - _lastMountTime < CurrentGameInstance.mountDelay)
                return;

            _lastMountTime = Time.unscaledTime;

            Vector3 enterPosition = EntityTransform.position;
            if (PassengingVehicleEntity != null)
            {
                enterPosition = PassengingVehicleEntity.Entity.EntityTransform.position;
                await ExitVehicle();
            }

            // Instantiate new mount entity
            LiteNetLibIdentity spawnObj;
            if (prefab != null)
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId, enterPosition,
                    Quaternion.Euler(0, EntityTransform.eulerAngles.y, 0));
            }
            else if (addressablePrefab.IsDataValid())
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId, enterPosition,
                    Quaternion.Euler(0, EntityTransform.eulerAngles.y, 0));
            }
            else
            {
                return;
            }

            if (spawnObj == null)
            {
                return;
            }

            VehicleEntity vehicle = spawnObj.GetComponent<VehicleEntity>();
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj, 0, ConnectionId);

            // Seat index for mount entity always 0
            await EnterVehicle(vehicle, 0);
        }

        protected virtual async UniTask<bool> EnterVehicle(IVehicleEntity vehicle, byte seatIndex)
        {
            if (!IsServer || vehicle.IsNull())
                return false;

            if (!vehicle.IsSeatAvailable(seatIndex))
            {
                // TODO: Send error message
                return false;
            }

            if (!vehicle.CanBePassenger(seatIndex, this))
            {
                // TODO: Send error message
                return false;
            }

            // Change object owner to driver
            if (vehicle.IsDriver(seatIndex))
                Manager.Assets.SetObjectOwner(vehicle.Entity.ObjectId, ConnectionId);

            // Set passenger to vehicle
            vehicle.SetPassenger(seatIndex, this);

            // Play enter vehicle animation
            float enterDuration = 0f;
            if (Model is IVehicleEnterExitModel vehicleEnterExitModel)
            {
                enterDuration = vehicleEnterExitModel.GetEnterVehicleAnimationDuration(PassengingVehicleEntity);
            }

            if (enterDuration > 0f)
            {
                CallRpcPlayEnterVehicleAnimation();
                await UniTask.Delay(Mathf.CeilToInt(enterDuration * 1000));
            }

            return true;
        }

        protected virtual async void EnterVehicleAndForget(IVehicleEntity vehicle, byte seatIndex)
        {
            await EnterVehicle(vehicle, seatIndex);
        }

        protected virtual async UniTask<bool> ExitVehicle()
        {
            if (!IsServer || PassengingVehicleEntity.IsNull())
                return false;

            bool isDriver = PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex);
            bool isDestroying = PassengingVehicleEntity.IsDestroyWhenExit(PassengingVehicleSeatIndex);

            // Play exit vehicle animation
            float exitDuration = 0f;
            if (Model is IVehicleEnterExitModel vehicleEnterExitModel)
            {
                exitDuration = vehicleEnterExitModel.GetExitVehicleAnimationDuration(PassengingVehicleEntity);
            }

            if (exitDuration > 0f)
            {
                CallRpcPlayExitVehicleAnimation();
                await UniTask.Delay(Mathf.CeilToInt(exitDuration * 1000));
            }

            // Clear object owner from driver
            if (PassengingVehicleEntity.IsDriver(PassengingVehicleSeatIndex))
                Manager.Assets.SetObjectOwner(PassengingVehicleEntity.Entity.ObjectId, -1);

            BaseGameEntity vehicleEntity = PassengingVehicleEntity.Entity;
            if (isDestroying)
            {
                // Remove all entity from vehicle
                PassengingVehicleEntity.RemoveAllPassengers();
                // Destroy vehicle entity
                vehicleEntity.NetworkDestroy();
            }
            else
            {
                // Remove this from vehicle
                PassengingVehicleEntity.RemovePassenger(PassengingVehicleSeatIndex);
                // Stop move if driver exit (if not driver continue move by driver controls)
                if (isDriver)
                    vehicleEntity.StopMove();
            }

            return true;
        }

        protected virtual async void ExitVehicleAndForget()
        {
            await ExitVehicle();
        }

        /// <summary>
        /// This function will be called by Vehicle Entity to inform that this entity exited vehicle
        /// </summary>
        public void ExitedVehicle(Vector3 exitPosition, Quaternion exitRotation)
        {
            CallRpcOnExitVehicle();
            Teleport(exitPosition, exitRotation, true);
        }

        public virtual void ClearPassengingVehicle()
        {
            SetPassengingVehicle(0, null);
        }

        public virtual void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            PassengingVehicleSeatIndex = seatIndex;
            PassengingVehicleEntity = vehicleEntity;
        }
        public void CallCmdEnterVehicle(uint objectId, byte seatIndex)
        {
            RPC(CmdEnterVehicle, objectId, seatIndex);
        }

        public virtual bool CanEnterVehicle(IVehicleEntity vehicleEntity, byte seatIndex, out UITextKeys gameMessage)
        {
            gameMessage = UITextKeys.NONE;
            if (vehicleEntity.IsNull())
            {
                gameMessage = UITextKeys.UI_ERROR_INVALID_VEHICLE_ENTITY;
                return false;
            }
            if (!vehicleEntity.IsSeatAvailable(seatIndex))
            {
                gameMessage = UITextKeys.UI_ERROR_SEAT_NOT_AVAILABLE;
                return false;
            }
            return true;
        }

        [ServerRpc]
        protected void CmdEnterVehicle(uint objectId, byte seatIndex)
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            if (!Manager.Assets.TryGetSpawnedObject(objectId, out LiteNetLibIdentity identity))
                return;
            IVehicleEntity vehicleEntity = identity.GetComponent<IVehicleEntity>();
            if (!CanEnterVehicle(vehicleEntity, seatIndex, out UITextKeys error))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, error);
                return;
            }
            EnterVehicleAndForget(vehicleEntity, seatIndex);
#endif
        }

        public void CallCmdExitVehicle()
        {
            RPC(CmdExitVehicle);
        }

        [ServerRpc]
        protected void CmdExitVehicle()
        {
#if UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES
            ExitVehicleAndForget();
#endif
        }

        public void CallRpcOnExitVehicle()
        {
            RPC(RpcOnExitVehicle);
        }

        [AllRpc]
        protected void RpcOnExitVehicle()
        {
            ClearPassengingVehicle();
        }

        public void CallRpcPlayEnterVehicleAnimation()
        {
            RPC(RpcPlayEnterVehicleAnimation);
        }

        [AllRpc]
        protected void RpcPlayEnterVehicleAnimation()
        {
            PlayEnterVehicleAnimation();
        }

        public void CallRpcPlayExitVehicleAnimation()
        {
            RPC(RpcPlayExitVehicleAnimation);
        }

        [AllRpc]
        protected void RpcPlayExitVehicleAnimation()
        {
            PlayExitVehicleAnimation();
        }

        public virtual void PlayEnterVehicleAnimation()
        {
            if (Model is IVehicleEnterExitModel vehicleEnterExitModel)
                vehicleEnterExitModel.PlayEnterVehicleAnimation(PassengingVehicleEntity);
        }

        public virtual void PlayExitVehicleAnimation()
        {
            if (Model is IVehicleEnterExitModel vehicleEnterExitModel)
                vehicleEnterExitModel.PlayExitVehicleAnimation(PassengingVehicleEntity);
        }
    }
}
