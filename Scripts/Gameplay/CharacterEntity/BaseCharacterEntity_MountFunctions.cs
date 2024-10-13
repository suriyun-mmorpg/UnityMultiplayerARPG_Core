using Cysharp.Threading.Tasks;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {

        protected float _lastMountTime;

        public virtual async void SpawnMount(MountType mountType, int mountDataId, float duration, int level = 1, int currentHp = 0)
        {
            if (!IsServer)
                return;

            if (mountType == MountType.None)
                return;

            if (Time.unscaledTime - _lastMountTime < CurrentGameInstance.mountDelay)
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
            if (mountType.GetPrefab(mountDataId, out VehicleEntity prefab, out AssetReferenceVehicleEntity addressablePrefab))
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    addressablePrefab.HashAssetId, enterPosition,
                    Quaternion.Euler(0, EntityTransform.eulerAngles.y, 0));
            }
            else if (prefab != null)
            {
                spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    prefab.Identity.HashAssetId, enterPosition,
                    Quaternion.Euler(0, EntityTransform.eulerAngles.y, 0));
            }
            else
            {
                return;
            }

            VehicleEntity vehicle = spawnObj.GetComponent<VehicleEntity>();
            vehicle.InitStats();
            vehicle.Level = level;
            if (currentHp <= 0f)
                currentHp = vehicle.MaxHp;
            vehicle.CurrentHp = currentHp;
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj, 0, ConnectionId);

            // Seat index for mount entity always 0
            await EnterVehicle(vehicle, 0);

            // Update mount data
            Mount = new CharacterMount()
            {
                type = mountType,
                dataId = mountDataId,
                mountRemainsDuration = duration,
                level = level,
                currentHp = currentHp,
            };
        }

        public async override UniTask<bool> ExitVehicle()
        {
            if (await base.ExitVehicle())
            {
                Mount = new CharacterMount();
                return true;
            }
            return false;
        }

        public override void SetPassengingVehicle(byte seatIndex, IVehicleEntity vehicleEntity)
        {
            base.SetPassengingVehicle(seatIndex, vehicleEntity);
            _isRecaching = true;
        }

        public override bool CanEnterVehicle(IVehicleEntity vehicleEntity, byte seatIndex, out UITextKeys gameMessage)
        {
            if (!base.CanEnterVehicle(vehicleEntity, seatIndex, out gameMessage))
                return false;

            if (!IsGameEntityInDistance(vehicleEntity))
            {
                gameMessage = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                return false;
            }

            return true;
        }
    }
}