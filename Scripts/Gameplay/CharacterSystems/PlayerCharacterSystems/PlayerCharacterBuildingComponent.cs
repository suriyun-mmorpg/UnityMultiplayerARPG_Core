using LiteNetLibManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public partial class PlayerCharacterBuildingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        public bool CallCmdConstructBuilding(int itemIndex, Vector3 position, Vector3 rotation, uint parentObjectId)
        {
            if (!Entity.CanDoActions())
                return false;
            RPC(CmdConstructBuilding, itemIndex, position, rotation, parentObjectId);
            return true;
        }

        [ServerRpc]
        protected async void CmdConstructBuilding(int itemIndex, Vector3 position, Vector3 rotation, uint parentObjectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
            {
                // Not allow to do it
                return;
            }

            if (itemIndex >= Entity.NonEquipItems.Count)
            {
                // Invalid data index
                return;
            }

            CharacterItem nonEquipItem = Entity.NonEquipItems[itemIndex];
            if (nonEquipItem.IsEmptySlot())
            {
                // Invalid data
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_BUILDING_DATA);
                return;
            }

            IBuildingItem buildingItem = nonEquipItem.GetBuildingItem();
            if (buildingItem == null)
            {
                // Invalid data
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_BUILDING_DATA);
                return;
            }

            AsyncOperationHandle<BuildingEntity>? asyncOpHandler = null;
            BuildingEntity buildingEntity;
            if (buildingItem.AddressableBuildingEntity.IsDataValid())
            {
                asyncOpHandler = buildingItem.AddressableBuildingEntity.LoadAssetAsync();
                buildingEntity = await asyncOpHandler.Value.Task;
            }
            else if (buildingItem.BuildingEntity != null)
            {
                buildingEntity = buildingItem.BuildingEntity;
            }
            else
            {
                // Invalid data
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_BUILDING_ENTITY);
                return;
            }

            if (buildingEntity.BuildLimit > 0 && GameInstance.ServerBuildingHandlers.CountPlayerBuildings(Entity.Id, buildingEntity.EntityId) >= buildingEntity.BuildLimit)
            {
                // Reached limit amount
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_REACHED_BUILD_LIMIT);
                return;
            }

            if (!Entity.DecreaseItemsByIndex(itemIndex, 1, false))
            {
                // Not enough items?
                return;
            }

            Entity.FillEmptySlots();

            // Create the building
            BuildingSaveData buildingSaveData = new BuildingSaveData();
            buildingSaveData.Id = GenericUtils.GetUniqueId();
            buildingSaveData.ParentId = string.Empty;
            if (Manager.TryGetEntityByObjectId(parentObjectId, out BuildingEntity parentBuildingEntity))
                buildingSaveData.ParentId = parentBuildingEntity.Id;
            buildingSaveData.EntityId = buildingEntity.EntityId;
            buildingSaveData.CurrentHp = buildingEntity.MaxHp;
            buildingSaveData.RemainsLifeTime = buildingEntity.LifeTime;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Entity.Id;
            buildingSaveData.CreatorName = Entity.CharacterName;
            CurrentGameManager.CreateBuildingEntity(buildingSaveData, false);

            if (asyncOpHandler.HasValue)
                Addressables.Release(asyncOpHandler.Value);
#endif
        }

        public bool CallCmdRepairBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdRepairBuilding, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdRepairBuilding(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out BuildingEntity buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(buildingEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.Repair(Entity, out UITextKeys errorMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, errorMessage);
                return;
            }
#endif
        }
        
        public bool CallCmdDestroyBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdDestroyBuilding, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdDestroyBuilding(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out BuildingEntity buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(buildingEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(Entity))
            {
                // Character is not the creator
                return;
            }

            buildingEntity.Destroy();
#endif
        }

        public bool CallCmdOpenStorage(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdOpenStorage, objectId, password);
            return true;
        }

        [ServerRpc]
        protected void CmdOpenStorage(uint objectId, string password)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out StorageEntity storageEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(storageEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (storageEntity.Lockable && storageEntity.IsLocked && !storageEntity.LockPassword.Equals(password))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WRONG_BUILDING_PASSWORD);
                return;
            }

            StorageId storageId;
            if (!Entity.GetStorageId(StorageType.Building, objectId, out storageId))
            {
                // Wrong storage type or relative data
                return;
            }

            GameInstance.ServerStorageHandlers.OpenStorage(ConnectionId, Entity, storageId);
#endif
        }

        public bool CallCmdOpenDoor(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdOpenDoor, objectId, password);
            return true;
        }

        [ServerRpc]
        protected void CmdOpenDoor(uint objectId, string password)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out DoorEntity doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(doorEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (doorEntity.Lockable && doorEntity.IsLocked && !doorEntity.LockPassword.Equals(password))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WRONG_BUILDING_PASSWORD);
                return;
            }

            doorEntity.IsOpen = true;
#endif
        }

        public bool CallCmdCloseDoor(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdCloseDoor, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdCloseDoor(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out DoorEntity doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(doorEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            doorEntity.IsOpen = false;
#endif
        }

        public bool CallCmdTurnOnCampFire(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdTurnOnCampFire, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdTurnOnCampFire(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out CampFireEntity campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(campfireEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            campfireEntity.TurnOn();
#endif
        }

        public bool CallCmdTurnOffCampFire(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdTurnOffCampFire, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdTurnOffCampFire(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out CampFireEntity campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(campfireEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            campfireEntity.TurnOff();
#endif
        }

        public bool CallCmdCraftItemByWorkbench(uint objectId, int dataId)
        {
            RPC(CmdCraftItemByWorkbench, objectId, dataId);
            return true;
        }

        [ServerRpc]
        protected void CmdCraftItemByWorkbench(uint objectId, int dataId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out WorkbenchEntity workbenchEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(workbenchEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            workbenchEntity.CraftItem(Entity, dataId);
#endif
        }

        public bool CallCmdSetBuildingPassword(uint objectId, string password)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdSetBuildingPassword, objectId, password);
            return true;
        }

        [ServerRpc]
        protected void CmdSetBuildingPassword(uint objectId, string password)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out BuildingEntity buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(buildingEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(Entity))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.LockPassword = password;
            buildingEntity.IsLocked = true;
#endif
        }

        public bool CallCmdLockBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdLockBuilding, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdLockBuilding(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out BuildingEntity buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(buildingEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(Entity))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.IsLocked = true;
#endif
        }

        public bool CallCmdUnlockBuilding(uint objectId)
        {
            if (!CurrentGameplayRule.CanInteractEntity(Entity, objectId))
            {
                ClientGenericActions.ClientReceiveGameMessage(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return false;
            }
            RPC(CmdUnlockBuilding, objectId);
            return true;
        }

        [ServerRpc]
        protected void CmdUnlockBuilding(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (!Entity.CanDoActions())
                return;

            if (!Manager.TryGetEntityByObjectId(objectId, out BuildingEntity buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!Entity.IsGameEntityInDistance(buildingEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(Entity))
            {
                // Character is not the creator
                return;
            }

            if (!buildingEntity.Lockable)
            {
                // It's not lockable building
                return;
            }

            buildingEntity.IsLocked = false;
#endif
        }
    }
}
