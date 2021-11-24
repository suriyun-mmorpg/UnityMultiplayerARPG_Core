using LiteNetLibManager;

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
