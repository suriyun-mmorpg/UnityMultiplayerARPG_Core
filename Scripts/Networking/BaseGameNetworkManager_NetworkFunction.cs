using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        public void SendCreatePartyToClient(long connectionId, PartyData party)
        {
            Server.SendCreateParty(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem, party.leaderId);
        }

        public void SendChangePartyLeaderToClient(long connectionId, PartyData party)
        {
            Server.SendChangePartyLeader(connectionId, MsgTypes.UpdateParty, party.id, party.leaderId);
        }

        public void SendChangePartyLeaderToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangePartyLeaderToClient(playerCharacterEntity.ConnectionId, party);
            }
        }

        public void SendPartySettingToClient(long connectionId, PartyData party)
        {
            Server.SendPartySetting(connectionId, MsgTypes.UpdateParty, party.id, party.shareExp, party.shareItem);
        }

        public void SendPartySettingToClients(PartyData party)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendPartySettingToClient(playerCharacterEntity.ConnectionId, party);
            }
        }

        public void SendPartyTerminateToClient(long connectionId, int id)
        {
            Server.SendPartyTerminate(connectionId, MsgTypes.UpdateParty, id);
        }

        public void SendAddPartyMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            Server.SendAddSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId, characterName, dataId, level);
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
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddPartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdatePartyMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            Server.SendUpdateSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
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
            Server.SendRemoveSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId);
        }

        public void SendRemovePartyMemberToClients(PartyData party, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemovePartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId);
            }
        }

        public void SendCreateGuildToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            Server.SendCreateGuild(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildName, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            Server.SendChangeGuildLeader(connectionId, MsgTypes.UpdateGuild, guild.id, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangeGuildLeaderToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendSetGuildMessageToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            Server.SendSetGuildMessage(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildMessage);
        }

        public void SendSetGuildMessageToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildMessageToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendSetGuildRoleToClient(long connectionId, int id, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            Server.SendSetGuildRole(connectionId, MsgTypes.UpdateGuild, id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public void SendSetGuildRoleToClients(GuildData guild, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
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
            Server.SendSetGuildMemberRole(connectionId, MsgTypes.UpdateGuild, id, characterId, guildRole);
        }

        public void SendSetGuildMemberRoleToClients(GuildData guild, string characterId, byte guildRole)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
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
            Server.SendGuildTerminate(connectionId, MsgTypes.UpdateGuild, id);
        }

        public void SendAddGuildMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, short level)
        {
            Server.SendAddSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId, characterName, dataId, level);
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
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdateGuildMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, short level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            Server.SendUpdateSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
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
            Server.SendRemoveSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId);
        }

        public void SendRemoveGuildMemberToClients(GuildData guild, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemoveGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId);
            }
        }

        public void SendSetGuildSkillLevelToClient(long connectionId, int id, int dataId, short level)
        {
            Server.SendSetGuildSkillLevel(connectionId, MsgTypes.UpdateGuild, id, dataId, level);
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
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildSkillLevelToClient(playerCharacterEntity.ConnectionId, guild.id, dataId, skillLevel);
            }
        }

        public void SendSetGuildGoldToClient(long connectionId, int id, int gold)
        {
            Server.SendSetGuildGold(connectionId, MsgTypes.UpdateGuild, id, gold);
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
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildGoldToClient(playerCharacterEntity.ConnectionId, guild.id, guild.gold);
            }
        }

        public void SendGuildLevelExpSkillPointToClient(long connectionId, GuildData guild)
        {
            Server.SendGuildLevelExpSkillPoint(connectionId, MsgTypes.UpdateGuild, guild.id, guild.level, guild.exp, guild.skillPoint);
        }

        public void SendGuildLevelExpSkillPointToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendGuildLevelExpSkillPointToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendUpdateFoundCharactersToClient(long connectionId, SocialCharacterData[] members)
        {
            Server.SendSocialMembers(connectionId, MsgTypes.UpdateFoundCharacters, members);
        }

        public void SendUpdateFriendsToClient(long connectionId, SocialCharacterData[] members)
        {
            Server.SendSocialMembers(connectionId, MsgTypes.UpdateFriends, members);
        }
    }
}
