using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        public void SendCreatePartyToClient(long connectionId, PartyData party)
        {
            this.SendCreateParty(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem, party.leaderId);
        }

        public void SendChangePartyLeaderToClient(long connectionId, PartyData party)
        {
            this.SendChangePartyLeader(connectionId, MsgTypes.UpdateParty, party.id, party.leaderId);
        }

        public void SendChangePartyLeaderToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangePartyLeaderToClient(playerCharacterEntity.ConnectionId, party);
            }
        }

        public void SendPartySettingToClient(long connectionId, PartyData party)
        {
            this.SendPartySetting(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem);
        }

        public void SendPartySettingToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendPartySettingToClient(playerCharacterEntity.ConnectionId, party);
            }
        }

        public void SendPartyTerminateToClient(long connectionId, int id)
        {
            this.SendPartyTerminate(connectionId, MsgTypes.UpdateParty, id);
        }

        public void SendAddPartyMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            this.SendAddSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddPartyMembersToClient(long connectionId, PartyData party)
        {
            if (party == null)
                return;

            foreach (SocialCharacterData member in party.GetMembers())
            {
                SendAddPartyMemberToClient(connectionId, party.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddPartyMemberToClients(PartyData party, string characterId, string characterName, int dataId, short level)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddPartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdatePartyMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            this.SendUpdateSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }

        public void SendUpdatePartyMembersToClient(long connectionId, PartyData party)
        {
            foreach (SocialCharacterData member in party.GetMembers())
            {
                SendUpdatePartyMemberToClient(connectionId, party.id, IsCharacterOnline(member.id), member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public void SendRemovePartyMemberToClient(long connectionId, int id, string characterId)
        {
            this.SendRemoveSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId);
        }

        public void SendRemovePartyMemberToClients(PartyData party, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemovePartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId);
            }
        }

        public void SendCreateGuildToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            this.SendCreateGuild(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildName, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            this.SendChangeGuildLeader(connectionId, MsgTypes.UpdateGuild, guild.id, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangeGuildLeaderToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendSetGuildMessageToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            this.SendSetGuildMessage(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildMessage);
        }

        public void SendSetGuildMessageToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildMessageToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendSetGuildRoleToClient(long connectionId, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            this.SendSetGuildRole(connectionId, MsgTypes.UpdateGuild, id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public void SendSetGuildRoleToClients(GuildData guild, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildRoleToClient(playerCharacterEntity.ConnectionId, guild.id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            }
        }

        public void SendSetGuildRolesToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            List<GuildRoleData> roles = guild.GetRoles();
            GuildRoleData guildRoleData;
            for (byte role = 0; role < roles.Count; ++role)
            {
                guildRoleData = roles[role];
                SendSetGuildRoleToClient(connectionId, guild.id, role, guildRoleData.roleName, guildRoleData.canInvite, guildRoleData.canKick, guildRoleData.shareExpPercentage);
            }
        }

        public void SendSetGuildMemberRoleToClient(long connectionId, int id, string characterId, byte guildRole)
        {
            this.SendSetGuildMemberRole(connectionId, MsgTypes.UpdateGuild, id, characterId, guildRole);
        }

        public void SendSetGuildMemberRoleToClients(GuildData guild, string characterId, byte guildRole)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildMemberRoleToClient(playerCharacterEntity.ConnectionId, guild.id, characterId, guildRole);
            }
        }

        public void SendSetGuildMemberRolesToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            byte role;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (guild.TryGetMemberRole(member.id, out role))
                    SendSetGuildMemberRoleToClient(connectionId, guild.id, member.id, role);
            }
        }

        public void SendGuildTerminateToClient(long connectionId, int id)
        {
            this.SendGuildTerminate(connectionId, MsgTypes.UpdateGuild, id);
        }

        public void SendAddGuildMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            this.SendAddSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddGuildMembersToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            foreach (SocialCharacterData member in guild.GetMembers())
            {
                SendAddGuildMemberToClient(connectionId, guild.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddGuildMemberToClients(GuildData guild, string characterId, string characterName, int dataId, short level)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdateGuildMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            this.SendUpdateSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }

        public void SendUpdateGuildMembersToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            foreach (SocialCharacterData member in guild.GetMembers())
            {
                SendUpdateGuildMemberToClient(connectionId, guild.id, IsCharacterOnline(member.id), member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public void SendRemoveGuildMemberToClient(long connectionId, int id, string characterId)
        {
            this.SendRemoveSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId);
        }

        public void SendRemoveGuildMemberToClients(GuildData guild, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemoveGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId);
            }
        }

        public void SendSetGuildSkillLevelToClient(long connectionId, int id, int dataId, short level)
        {
            this.SendSetGuildSkillLevel(connectionId, MsgTypes.UpdateGuild, id, dataId, level);
        }

        public void SendSetGuildSkillLevelsToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            foreach (KeyValuePair<int, short> guildSkillLevel in guild.GetSkillLevels())
            {
                SendSetGuildSkillLevelToClient(connectionId, guild.id, guildSkillLevel.Key, guildSkillLevel.Value);
            }
        }

        public void SendSetGuildSkillLevelToClients(GuildData guild, int dataId)
        {
            if (guild == null)
                return;

            short skillLevel = guild.GetSkillLevel(dataId);
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildSkillLevelToClient(playerCharacterEntity.ConnectionId, guild.id, dataId, skillLevel);
            }
        }

        public void SendSetGuildGoldToClient(long connectionId, int id, int gold)
        {
            this.SendSetGuildGold(connectionId, MsgTypes.UpdateGuild, id, gold);
        }

        public void SendSetGuildGoldToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            SendSetGuildGoldToClient(connectionId, guild.id, guild.gold);
        }

        public void SendSetGuildGoldToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildGoldToClient(playerCharacterEntity.ConnectionId, guild.id, guild.gold);
            }
        }

        public void SendGuildLevelExpSkillPointToClient(long connectionId, GuildData guild)
        {
            this.SendGuildLevelExpSkillPoint(connectionId, MsgTypes.UpdateGuild, guild.id, guild.level, guild.exp, guild.skillPoint);
        }

        public void SendGuildLevelExpSkillPointToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (ServerPlayerCharacterHandlers.TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendGuildLevelExpSkillPointToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendUpdateFriendsToClient(long connectionId, SocialCharacterData[] friends)
        {
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.UpdateFriends, (writer) =>
            {
                writer.PutArray(friends);
            });
        }

        public void SendServerGameMessage(long connectionId, GameMessage.Type type)
        {
            GameMessage message = new GameMessage();
            message.type = type;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.GameMessage, message);
        }

        public void SendNotifyRewardExp(long connectionId, int exp)
        {
            if (exp <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardExp, (writer) =>
            {
                writer.PutPackedInt(exp);
            });
        }

        public void SendNotifyRewardGold(long connectionId, int gold)
        {
            if (gold <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardGold, (writer) =>
            {
                writer.PutPackedInt(gold);
            });
        }

        public void SendNotifyRewardItem(long connectionId, int dataId, short amount)
        {
            if (amount <= 0)
                return;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyRewardItem, (writer) =>
            {
                writer.PutPackedInt(dataId);
                writer.PutPackedShort(amount);
            });
        }

        public void SendNotifyStorageOpenedToClient(long connectionId, StorageType storageType, string storageOwnerId, uint objectId, short weightLimit, short slotLimit)
        {
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyStorageOpened, (writer) =>
             {
                 writer.Put((byte)storageType);
                 writer.Put(storageOwnerId);
                 writer.PutPackedUInt(objectId);
                 writer.PutPackedShort(weightLimit);
                 writer.PutPackedShort(slotLimit);
             });
        }

        public void SendNotifyStorageItemsUpdatedToClient(long connectionId, List<CharacterItem> storageItems)
        {
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyStorageItemsUpdated, (writer) =>
            {
                writer.PutList(storageItems);
            });
        }

        public void SendNotifyStorageItemsUpdatedToClients(HashSet<long> connectionIds, List<CharacterItem> storageITems)
        {
            foreach (long connectionId in connectionIds)
            {
                if (Players.ContainsKey(connectionId))
                    SendNotifyStorageItemsUpdatedToClient(connectionId, storageITems);
            }
        }

        public void SendNotifyPartyInvitationToClient(long connectionId, PartyInvitationData invitation)
        {
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyPartyInvitation, invitation);
        }

        public void SendNotifyGuildInvitationToClient(long connectionId, GuildInvitationData invitation)
        {
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.NotifyGuildInvitation, invitation);
        }
    }
}
