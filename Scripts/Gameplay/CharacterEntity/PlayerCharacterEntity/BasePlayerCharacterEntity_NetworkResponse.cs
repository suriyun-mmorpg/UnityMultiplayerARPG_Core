using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerUseGuildSkill(int dataId)
        {
#if !CLIENT_BUILD
            if (this.IsDead())
                return;

            GuildSkill guildSkill;
            if (!GameInstance.GuildSkills.TryGetValue(dataId, out guildSkill) || guildSkill.GetSkillType() != GuildSkillType.Active)
                return;

            GuildData guild;
            if (GuildId <= 0 || !GameInstance.ServerGuildHandlers.TryGetGuild(GuildId, out guild))
                return;

            short level = guild.GetSkillLevel(dataId);
            if (level <= 0)
                return;

            if (this.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
                return;

            // Apply guild skill
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.GuildSkill, dataId);
            newSkillUsage.Use(this, level);
            skillUsages.Add(newSkillUsage);
            ApplyBuff(dataId, BuffType.GuildSkillBuff, level, GetInfo());
#endif
        }

        [ServerRpc]
        protected void ServerAssignHotkey(string hotkeyId, HotkeyType type, string relateId)
        {
#if !CLIENT_BUILD
            CharacterHotkey characterHotkey = new CharacterHotkey();
            characterHotkey.hotkeyId = hotkeyId;
            characterHotkey.type = type;
            characterHotkey.relateId = relateId;
            int hotkeyIndex = this.IndexOfHotkey(hotkeyId);
            if (hotkeyIndex >= 0)
                hotkeys[hotkeyIndex] = characterHotkey;
            else
                hotkeys.Add(characterHotkey);
#endif
        }

        [ServerRpc]
        protected void ServerEnterWarp(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            WarpPortalEntity warpPortalEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out warpPortalEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(warpPortalEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            warpPortalEntity.EnterWarp(this);
#endif
        }

        [ServerRpc]
        protected void ServerConstructBuilding(short itemIndex, Vector3 position, Quaternion rotation, uint parentObjectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions() ||
                itemIndex >= NonEquipItems.Count)
                return;

            BuildingEntity buildingEntity;
            CharacterItem nonEquipItem = NonEquipItems[itemIndex];
            if (nonEquipItem.IsEmptySlot() ||
                nonEquipItem.GetBuildingItem() == null ||
                nonEquipItem.GetBuildingItem().BuildingEntity == null ||
                !GameInstance.BuildingEntities.TryGetValue(nonEquipItem.GetBuildingItem().BuildingEntity.EntityId, out buildingEntity) ||
                !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            this.FillEmptySlots();
            BuildingSaveData buildingSaveData = new BuildingSaveData();
            buildingSaveData.Id = GenericUtils.GetUniqueId();
            buildingSaveData.ParentId = string.Empty;
            BuildingEntity parentBuildingEntity;
            if (Manager.TryGetEntityByObjectId(parentObjectId, out parentBuildingEntity))
                buildingSaveData.ParentId = parentBuildingEntity.Id;
            buildingSaveData.EntityId = buildingEntity.EntityId;
            buildingSaveData.CurrentHp = buildingEntity.MaxHp;
            buildingSaveData.RemainsLifeTime = buildingEntity.LifeTime;
            buildingSaveData.Position = position;
            buildingSaveData.Rotation = rotation;
            buildingSaveData.CreatorId = Id;
            buildingSaveData.CreatorName = CharacterName;
            CurrentGameManager.CreateBuildingEntity(buildingSaveData, false);
#endif
        }

        [ServerRpc]
        protected void ServerDestroyBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(this))
            {
                // Character is not the creator
                return;
            }

            buildingEntity.Destroy();
#endif
        }

        #region Dealing
        [ServerRpc]
        protected void ServerSendDealingRequest(uint objectId)
        {
#if !CLIENT_BUILD
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND);
                return;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_DEALING);
                return;
            }
            if (!IsGameEntityInDistance(targetCharacterEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.DealingCharacter = this;
            // Send receive dealing request to player
            DealingCharacter.CallOwnerReceiveDealingRequest(ObjectId);
#endif
        }

        [TargetRpc]
        protected void TargetReceiveDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        [ServerRpc]
        protected void ServerAcceptDealingRequest()
        {
#if !CLIENT_BUILD
            if (DealingCharacter == null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCEPT_DEALING_REQUEST);
                StopDealing();
                return;
            }
            if (!IsGameEntityInDistance(DealingCharacter, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingState = DealingState.Dealing;
            DealingCharacter.CallOwnerAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            CallOwnerAcceptedDealingRequest(DealingCharacter.ObjectId);
#endif
        }

        [ServerRpc]
        protected void ServerDeclineDealingRequest()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_DEALING_REQUEST_DECLINED);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DEALING_REQUEST_DECLINED);
            StopDealing();
#endif
        }

        [TargetRpc]
        protected void TargetAcceptedDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        [ServerRpc]
        protected void ServerSetDealingItem(short itemIndex, short amount)
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }

            if (itemIndex >= nonEquipItems.Count)
                return;

            DealingCharacterItems dealingItems = DealingItems;
            for (int i = dealingItems.Count - 1; i >= 0; --i)
            {
                if (itemIndex == dealingItems[i].nonEquipIndex)
                {
                    dealingItems.RemoveAt(i);
                    break;
                }
            }
            CharacterItem characterItem = nonEquipItems[itemIndex].Clone();
            characterItem.amount = amount;
            DealingCharacterItem dealingItem = new DealingCharacterItem();
            dealingItem.nonEquipIndex = itemIndex;
            dealingItem.characterItem = characterItem;
            dealingItems.Add(dealingItem);
            // Update to clients
            DealingItems = dealingItems;
#endif
        }

        [ServerRpc]
        protected void ServerSetDealingGold(int gold)
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            if (gold > Gold)
                gold = Gold;
            if (gold < 0)
                gold = 0;
            DealingGold = gold;
#endif
        }

        [ServerRpc]
        protected void ServerLockDealing()
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            DealingState = DealingState.LockDealing;
#endif
        }

        [ServerRpc]
        protected void ServerConfirmDealing()
        {
#if !CLIENT_BUILD
            if (DealingState != DealingState.LockDealing || !(DealingCharacter.DealingState == DealingState.LockDealing || DealingCharacter.DealingState == DealingState.ConfirmDealing))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && DealingCharacter.DealingState == DealingState.ConfirmDealing)
            {
                if (ExchangingDealingItemsWillOverwhelming())
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING);
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                }
                else if (DealingCharacter.ExchangingDealingItemsWillOverwhelming())
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING);
                }
                else
                {
                    ExchangeDealingItemsAndGold();
                    DealingCharacter.ExchangeDealingItemsAndGold();
                }
                StopDealing();
            }
#endif
        }

        [ServerRpc]
        protected void ServerCancelDealing()
        {
#if !CLIENT_BUILD
            if (DealingCharacter != null)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_DEALING_CANCELED);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DEALING_CANCELED);
            StopDealing();
#endif
        }

        [TargetRpc]
        protected void TargetUpdateDealingState(DealingState dealingState)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke(dealingState);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingState(DealingState dealingState)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke(dealingState);
        }

        [TargetRpc]
        protected void TargetUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        [TargetRpc]
        protected void TargetUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
        #endregion

        #region Banking
        [ServerRpc]
        protected void ServerOpenStorage(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            StorageEntity storageEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out storageEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(storageEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (storageEntity.Lockable && storageEntity.IsLocked && !storageEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            StorageId storageId;
            if (!this.GetStorageId(StorageType.Building, objectId, out storageId))
            {
                // Wrong storage type or relative data
                return;
            }

            GameInstance.ServerStorageHandlers.OpenStorage(ConnectionId, this, storageId);
#endif
        }
        #endregion

        #region Building Entities
        [ServerRpc]
        protected void ServerOpenDoor(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(doorEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (doorEntity.Lockable && doorEntity.IsLocked && !doorEntity.LockPassword.Equals(password))
            {
                // Wrong password
                return;
            }

            doorEntity.IsOpen = true;
#endif
        }

        [ServerRpc]
        protected void ServerCloseDoor(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            DoorEntity doorEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out doorEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(doorEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            doorEntity.IsOpen = false;
#endif
        }

        [ServerRpc]
        protected void ServerTurnOnCampFire(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(campfireEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            campfireEntity.TurnOn();
#endif
        }

        [ServerRpc]
        protected void ServerTurnOffCampFire(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            CampFireEntity campfireEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out campfireEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(campfireEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            campfireEntity.TurnOff();
#endif
        }

        [ServerRpc]
        protected void ServerCraftItemByWorkbench(uint objectId, int dataId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            WorkbenchEntity workbenchEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out workbenchEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(workbenchEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            workbenchEntity.CraftItem(this, dataId);
#endif
        }
        #endregion

        #region Building Locking
        [ServerRpc]
        protected void ServerSetBuildingPassword(uint objectId, string password)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(this))
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

        [ServerRpc]
        protected void ServerLockBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(this))
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

        [ServerRpc]
        protected void ServerUnlockBuilding(uint objectId)
        {
#if !CLIENT_BUILD
            if (!CanDoActions())
                return;

            BuildingEntity buildingEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out buildingEntity))
            {
                // Can't find the entity
                return;
            }

            if (!IsGameEntityInDistance(buildingEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }

            if (!buildingEntity.IsCreator(this))
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
        #endregion

        public void StopDealing()
        {
            if (DealingCharacter == null)
            {
                ClearDealingData();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.ClearDealingData();
            DealingCharacter.DealingCharacter = null;
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingCharacter = null;
        }
    }
}
