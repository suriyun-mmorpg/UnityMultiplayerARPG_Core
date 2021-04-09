using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class ServerGameMessageHandlersExtensions
    {
        public static void SendSetPartyData(this IServerGameMessageHandlers handlers, long connectionId, PartyData party)
        {
            if (party == null)
                return;
            handlers.SendSetPartyData(connectionId, party.id, party.shareExp, party.shareItem, party.leaderId);
        }

        public static void SendSetPartyLeaderToMembers(this IServerGameMessageHandlers handlers, PartyData party)
        {
            if (party == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetPartyLeader(connectionId, party.id, party.leaderId);
            }
        }

        public static void SendSetPartySettingToMembers(this IServerGameMessageHandlers handlers, PartyData party)
        {
            if (party == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetPartySetting(connectionId, party.id, party.shareExp, party.shareItem);
            }
        }

        public static void SendAddPartyMembersToOne(this IServerGameMessageHandlers handlers, long connectionId, PartyData party)
        {
            if (party == null)
                return;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                handlers.SendAddPartyMember(connectionId, party.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public static void SendAddPartyMemberToMembers(this IServerGameMessageHandlers handlers, PartyData party, string characterId, string characterName, int dataId, short level)
        {
            if (party == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendAddPartyMember(connectionId, party.id, characterId, characterName, dataId, level);
            }
        }

        public static void SendUpdatePartyMembersToOne(this IServerGameMessageHandlers handlers, long connectionId, PartyData party)
        {
            if (party == null)
                return;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                bool isOnline = true; // TODO: Implement this
                handlers.SendUpdatePartyMember(connectionId, party.id, isOnline, member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public static void SendRemovePartyMemberToMembers(this IServerGameMessageHandlers handlers, PartyData party, string characterId)
        {
            if (party == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendRemovePartyMember(connectionId, party.id, characterId);
            }
        }
        public static void SendSetGuildData(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            handlers.SendSetGuildData(connectionId, guild.id, guild.guildName, guild.leaderId);
        }

        public static void SendSetGuildLeaderToMembers(this IServerGameMessageHandlers handlers, GuildData guild)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildLeader(connectionId, guild.id, guild.leaderId);
            }
        }

        public static void SendSetGuildMessage(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            handlers.SendSetGuildMessage(connectionId, guild.id, guild.guildMessage);
        }

        public static void SendSetGuildMessageToMembers(this IServerGameMessageHandlers handlers, GuildData guild)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildMessage(connectionId, guild.id, guild.guildMessage);
            }
        }

        public static void SendSetGuildRoleToMembers(this IServerGameMessageHandlers handlers, GuildData guild, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildRole(connectionId, guild.id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            }
        }

        public static void SendSetGuildRolesToOne(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            List<GuildRoleData> roles = guild.GetRoles();
            GuildRoleData guildRoleData;
            for (byte role = 0; role < roles.Count; ++role)
            {
                guildRoleData = roles[role];
                handlers.SendSetGuildRole(connectionId, guild.id, role, guildRoleData.roleName, guildRoleData.canInvite, guildRoleData.canKick, guildRoleData.shareExpPercentage);
            }
        }

        public static void SendSetGuildMemberRoleToMembers(this IServerGameMessageHandlers handlers, GuildData guild, string characterId, byte guildRole)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildMemberRole(connectionId, guild.id, characterId, guildRole);
            }
        }

        public static void SendSetGuildMemberRolesToOne(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            byte role;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (guild.TryGetMemberRole(member.id, out role))
                    handlers.SendSetGuildMemberRole(connectionId, guild.id, member.id, role);
            }
        }

        public static void SendAddGuildMembersToOne(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                handlers.SendAddGuildMember(connectionId, guild.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public static void SendAddGuildMemberToMembers(this IServerGameMessageHandlers handlers, GuildData guild, string characterId, string characterName, int dataId, short level)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendAddGuildMember(connectionId, guild.id, characterId, characterName, dataId, level);
            }
        }

        public static void SendUpdateGuildMembersToOne(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                bool isOnline = true; // TODO: Implement this
                handlers.SendUpdateGuildMember(connectionId, guild.id, isOnline, member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public static void SendRemoveGuildMemberToMembers(this IServerGameMessageHandlers handlers, GuildData guild, string characterId)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendRemoveGuildMember(connectionId, guild.id, characterId);
            }
        }

        public static void SendSetGuildSkillLevelsToOne(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            foreach (KeyValuePair<int, short> guildSkillLevel in guild.GetSkillLevels())
            {
                handlers.SendSetGuildSkillLevel(connectionId, guild.id, guildSkillLevel.Key, guildSkillLevel.Value);
            }
        }

        public static void SendSetGuildSkillLevelToMembers(this IServerGameMessageHandlers handlers, GuildData guild, int dataId)
        {
            if (guild == null)
                return;
            short skillLevel = guild.GetSkillLevel(dataId);
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildSkillLevel(connectionId, guild.id, dataId, skillLevel);
            }
        }

        public static void SendSetGuildGold(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            handlers.SendSetGuildGold(connectionId, guild.id, guild.gold);
        }

        public static void SendSetGuildGoldToMembers(this IServerGameMessageHandlers handlers, GuildData guild)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildGold(connectionId, guild.id, guild.gold);
            }
        }

        public static void SendSetGuildLevelExpSkillPoint(this IServerGameMessageHandlers handlers, long connectionId, GuildData guild)
        {
            if (guild == null)
                return;
            handlers.SendSetGuildLevelExpSkillPoint(connectionId, guild.id, guild.level, guild.exp, guild.skillPoint);
        }

        public static void SendSetGuildLevelExpSkillPointToMembers(this IServerGameMessageHandlers handlers, GuildData guild)
        {
            if (guild == null)
                return;
            long connectionId;
            foreach (SocialCharacterData member in guild.GetMembers())
            {
                if (GameInstance.ServerUserHandlers.TryGetConnectionId(member.id, out connectionId))
                    handlers.SendSetGuildLevelExpSkillPoint(connectionId, guild.id, guild.level, guild.exp, guild.skillPoint);
            }
        }

        public static void NotifyStorageItemsToClients(this IServerGameMessageHandlers handlers, IEnumerable<long> connectionIds, List<CharacterItem> storageItems)
        {
            foreach (long connectionId in connectionIds)
            {
                handlers.NotifyStorageItems(connectionId, storageItems);
            }
        }
    }
}
