using LiteNetLibManager;
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
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_GUILD_SKILL_DATA);
                return;
            }

            GuildData guild;
            if (GuildId <= 0 || !GameInstance.ServerGuildHandlers.TryGetGuild(GuildId, out guild))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_NOT_JOINED_GUILD);
                return;
            }

            short level = guild.GetSkillLevel(dataId);
            if (level <= 0)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_SKILL_LEVEL_IS_ZERO);
                return;
            }

            if (this.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_SKILL_IS_COOLING_DOWN);
                return;
            }

            // Apply guild skill to guild members in the same map
            CharacterSkillUsage newSkillUsage = CharacterSkillUsage.Create(SkillUsageType.GuildSkill, dataId);
            newSkillUsage.Use(this, level);
            skillUsages.Add(newSkillUsage);
            SocialCharacterData[] members = guild.GetMembers();
            BasePlayerCharacterEntity memberEntity;
            foreach (SocialCharacterData member in members)
            {
                if (GameInstance.ServerUserHandlers.TryGetPlayerCharacterById(member.id, out memberEntity))
                {
                    memberEntity.ApplyBuff(dataId, BuffType.GuildSkillBuff, level, GetInfo());
                }
            }
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

        [ServerRpc]
        protected void ServerAppendCraftingQueueItem(uint sourceObjectId, int dataId, short amount)
        {
            if (sourceObjectId == ObjectId)
            {
                Crafting.AppendCraftingQueueItem(ObjectId, dataId, amount);
            }
            else if (CurrentGameManager.TryGetEntityByObjectId(sourceObjectId, out ICraftingQueueSource source))
            {
                source.AppendCraftingQueueItem(ObjectId, dataId, amount);
            }
        }

        [ServerRpc]
        protected void ServerChangeCraftingQueueItem(uint sourceObjectId, int indexOfData, short amount)
        {
            if (sourceObjectId == ObjectId)
            {
                Crafting.ChangeCraftingQueueItem(ObjectId, indexOfData, amount);
            }
            else if (CurrentGameManager.TryGetEntityByObjectId(sourceObjectId, out ICraftingQueueSource source))
            {
                source.ChangeCraftingQueueItem(ObjectId, indexOfData, amount);
            }
        }

        [ServerRpc]
        protected void ServerCancelCraftingQueueItem(uint sourceObjectId, int indexOfData)
        {
            if (sourceObjectId == ObjectId)
            {
                Crafting.CancelCraftingQueueItem(ObjectId, indexOfData);
            }
            else if (CurrentGameManager.TryGetEntityByObjectId(sourceObjectId, out ICraftingQueueSource source))
            {
                source.CancelCraftingQueueItem(ObjectId, indexOfData);
            }
        }
    }
}
