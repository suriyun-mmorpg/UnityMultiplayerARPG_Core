using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        [ServerRpc]
        protected void ServerUseGuildSkill(int dataId)
        {
#if UNITY_EDITOR || UNITY_SERVER
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

            int level = guild.GetSkillLevel(dataId);
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
                    memberEntity.ApplyBuff(dataId, BuffType.GuildSkillBuff, level, GetInfo(), null);
                }
            }
#endif
        }

        [ServerRpc]
        protected void ServerAssignHotkey(string hotkeyId, HotkeyType type, string relateId)
        {
#if UNITY_EDITOR || UNITY_SERVER
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
#if UNITY_EDITOR || UNITY_SERVER
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
        protected void ServerAppendCraftingQueueItem(uint sourceObjectId, int dataId, int amount)
        {
            UITextKeys errorMessage;
            if (sourceObjectId == ObjectId)
            {
                if (!Crafting.AppendCraftingQueueItem(this, ObjectId, dataId, amount, out errorMessage))
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, errorMessage);
            }
            else if (CurrentGameManager.TryGetEntityByObjectId(sourceObjectId, out ICraftingQueueSource source))
            {
                if (!source.AppendCraftingQueueItem(this, ObjectId, dataId, amount, out errorMessage))
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, errorMessage);
            }
        }

        [ServerRpc]
        protected void ServerChangeCraftingQueueItem(uint sourceObjectId, int indexOfData, int amount)
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
