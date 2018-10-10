using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        #region Activity validation functions
        public virtual bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            return true;
        }

        public virtual bool CanCreateParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            if (playerCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            return true;
        }

        public virtual bool CanChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanPartySetting(BasePlayerCharacterEntity playerCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            return true;
        }

        public virtual bool CanSendPartyInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            var partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.CanInvite(inviteCharacterEntity.Id))
            {
                // TODO: May send warn message that player can not invite
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                // TODO: May send warn message that character is not found
                return false;
            }
            if (targetCharacterEntity.CoCharacter != null)
            {
                // TODO: May send warn message that character is not available
                return false;
            }
            if (targetCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            return true;
        }

        public virtual bool CanAddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return false;
            if (acceptCharacterEntity.PartyId > 0)
            {
                // TODO: May send warn message that player already in party
                return false;
            }
            partyId = inviteCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player not in party
                return false;
            }
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (party.CountMember() == gameInstance.SocialSystemSetting.MaxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return false;
            }
            return true;
        }

        public virtual bool CanKickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player is not in party
                return false;
            }
            if (!party.CanKick(playerCharacterEntity.Id))
            {
                // TODO: May send warn message that player can not kick
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                // TODO: May warn that it's owning character so it's not able to kick
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanLeaveParty(BasePlayerCharacterEntity playerCharacterEntity, out int partyId, out PartyData party)
        {
            partyId = 0;
            party = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            partyId = playerCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                // TODO: May send warn message that player is not in party
                return false;
            }
            return true;
        }

        public virtual bool CanCreateGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            if (playerCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            if (!gameInstance.SocialSystemSetting.CanCreateGuild(playerCharacterEntity))
            {
                // TODO: May send warn message that player have not enough gold or items
                return false;
            }
            return true;
        }

        public virtual bool CanChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in guild
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.IsRoleAvailable(guildRole))
            {
                // TODO: May warn that guild role is not available
                return false;
            }
            return true;
        }

        public virtual bool CanSetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            return true;
        }

        public virtual bool CanSendGuildInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            var guildId = inviteCharacterEntity.GuildId;
            GuildData guild;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.CanInvite(inviteCharacterEntity.Id))
            {
                // TODO: May send warn message that player can not invite
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                // TODO: May send warn message that character is not found
                return false;
            }
            if (targetCharacterEntity.CoCharacter != null)
            {
                // TODO: May send warn message that character is not available
                return false;
            }
            if (targetCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            return true;
        }

        public virtual bool CanAddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return false;
            if (acceptCharacterEntity.GuildId > 0)
            {
                // TODO: May send warn message that player already in guild
                return false;
            }
            guildId = inviteCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (guild.CountMember() == gameInstance.SocialSystemSetting.MaxGuildMember)
            {
                // TODO: May warn that it's exceeds limit max guild member
                return false;
            }
            return true;
        }

        public virtual bool CanKickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            if (!guild.CanKick(playerCharacterEntity.Id))
            {
                // TODO: May send warn message that player can not kick
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                // TODO: May warn that it's owning character so it's not able to kick
                return false;
            }
            byte role;
            if (!guild.TryGetMemberRole(characterId, out role) && playerCharacterEntity.GuildRole < role)
            {
                // TODO: May warn that character rank is lower
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                // TODO: May warn that target character is not in guild
                return false;
            }
            return true;
        }

        public virtual bool CanLeaveGuild(BasePlayerCharacterEntity playerCharacterEntity, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                // TODO: May send warn message that player not in guild
                return false;
            }
            return true;
        }
        #endregion

        #region Activity functions
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
            foreach (var member in party.GetMembers())
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
            foreach (var member in party.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendPartySettingToClient(playerCharacterEntity.ConnectionId, party);
            }
        }

        public void SendPartyTerminateToClient(long connectionId, int id)
        {
            Server.SendPartyTerminate(connectionId, MsgTypes.UpdateParty, id);
        }

        public void SendAddPartyMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, int level)
        {
            Server.SendAddSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddPartyMembersToClient(long connectionId, PartyData party)
        {
            if (party == null)
                return;

            foreach (var member in party.GetMembers())
            {
                SendAddPartyMemberToClient(connectionId, party.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddPartyMemberToClients(PartyData party, string characterId, string characterName, int dataId, int level)
        {
            if (party == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddPartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdatePartyMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, int level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            Server.SendUpdateSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }
        
        public void SendUpdatePartyMembersToClient(long connectionId, PartyData party)
        {
            foreach (var member in party.GetMembers())
            {
                SendUpdatePartyMemberToClient(connectionId, party.id, party.IsOnline(member.id), member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public void SendRemovePartyMemberToClient(long connectionId, int id, string characterId)
        {
            Server.SendRemoveSocialMember(connectionId, MsgTypes.UpdatePartyMember, id, characterId);
        }

        public void SendRemovePartyMemberToClients(PartyData party, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in party.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemovePartyMemberToClient(playerCharacterEntity.ConnectionId, party.id, characterId);
            }
        }

        public void SendCreateGuildToClient(long connectionId, GuildData guild)
        {
            Server.SendCreateGuild(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildName, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClient(long connectionId, GuildData guild)
        {
            Server.SendChangeGuildLeader(connectionId, MsgTypes.UpdateGuild, guild.id, guild.leaderId);
        }

        public void SendChangeGuildLeaderToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendChangeGuildLeaderToClient(playerCharacterEntity.ConnectionId, guild);
            }
        }

        public void SendSetGuildMessageToClient(long connectionId, GuildData guild)
        {
            Server.SendSetGuildMessage(connectionId, MsgTypes.UpdateGuild, guild.id, guild.guildMessage);
        }

        public void SendSetGuildMessageToClients(GuildData guild)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
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
            foreach (var member in guild.GetMembers())
            {
                if (TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendSetGuildRoleToClient(playerCharacterEntity.ConnectionId, guild.id, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            }
        }

        public void SendSetGuildRolesToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            var roles = guild.GetRoles();
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
            foreach (var member in guild.GetMembers())
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
            foreach (var member in guild.GetMembers())
            {
                if (guild.TryGetMemberRole(member.id, out role))
                    SendSetGuildMemberRoleToClient(connectionId, guild.id, member.id, role);
            }
        }

        public void SendGuildTerminateToClient(long connectionId, int id)
        {
            Server.SendGuildTerminate(connectionId, MsgTypes.UpdateGuild, id);
        }

        public void SendAddGuildMemberToClient(long connectionId, int id, string characterId, string characterName, int dataId, int level)
        {
            Server.SendAddSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId, characterName, dataId, level);
        }

        public void SendAddGuildMembersToClient(long connectionId, GuildData guild)
        {
            if (guild == null)
                return;

            foreach (var member in guild.GetMembers())
            {
                SendAddGuildMemberToClient(connectionId, guild.id, member.id, member.characterName, member.dataId, member.level);
            }
        }

        public void SendAddGuildMemberToClients(GuildData guild, string characterId, string characterName, int dataId, int level)
        {
            if (guild == null)
                return;

            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendAddGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId, characterName, dataId, level);
            }
        }

        public void SendUpdateGuildMemberToClient(long connectionId, int id, bool isOnline, string characterId, string characterName, int dataId, int level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
                Server.SendUpdateSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, isOnline, characterId, characterName, dataId, level, currentHp, maxHp, currentMp, maxMp);
        }

        public void SendUpdateGuildMembersToClient(long connectionId, GuildData guild)
        {
            foreach (var member in guild.GetMembers())
            {
                SendUpdateGuildMemberToClient(connectionId, guild.id, guild.IsOnline(member.id), member.id, member.characterName, member.dataId, member.level, member.currentHp, member.maxHp, member.currentMp, member.maxMp);
            }
        }

        public void SendRemoveGuildMemberToClient(long connectionId, int id, string characterId)
        {
            Server.SendRemoveSocialMember(connectionId, MsgTypes.UpdateGuildMember, id, characterId);
        }

        public void SendRemoveGuildMemberToClients(GuildData guild, string characterId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            foreach (var member in guild.GetMembers())
            {
                if (!member.id.Equals(characterId) && TryGetPlayerCharacterById(member.id, out playerCharacterEntity))
                    SendRemoveGuildMemberToClient(playerCharacterEntity.ConnectionId, guild.id, characterId);
            }
        }

        public virtual void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
        }

        public virtual void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem, int partyId)
        {
            if (!CanCreateParty(playerCharacterEntity))
                return;

            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
            SendCreatePartyToClient(playerCharacterEntity.ConnectionId, party);
            SendAddPartyMembersToClient(playerCharacterEntity.ConnectionId, party);
        }

        public virtual void ChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanChangePartyLeader(playerCharacterEntity, characterId, out partyId, out party))
                return;

            party.SetLeader(characterId);
            parties[partyId] = party;
            SendChangePartyLeaderToClients(party);
        }

        public virtual void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            int partyId;
            PartyData party;
            if (!CanPartySetting(playerCharacterEntity, out partyId, out party))
                return;

            party.Setting(shareExp, shareItem);
            parties[partyId] = party;
            SendPartySettingToClients(party);
        }

        public virtual void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int partyId;
            PartyData party;
            if (!CanAddPartyMember(inviteCharacterEntity, acceptCharacterEntity, out partyId, out party))
                return;

            party.AddMember(acceptCharacterEntity);
            parties[partyId] = party;
            acceptCharacterEntity.PartyId = partyId;
            SendCreatePartyToClient(acceptCharacterEntity.ConnectionId, party);
            SendAddPartyMembersToClient(acceptCharacterEntity.ConnectionId, party);
            SendAddPartyMemberToClients(party, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public virtual void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanKickFromParty(playerCharacterEntity, characterId, out partyId, out party))
                return;

            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
            {
                memberCharacterEntity.ClearParty();
                SendPartyTerminateToClient(memberCharacterEntity.ConnectionId, partyId);
            }
            party.RemoveMember(characterId);
            parties[partyId] = party;
            SendRemovePartyMemberToClients(party, characterId);
        }

        public virtual void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            int partyId;
            PartyData party;
            if (!CanLeaveParty(playerCharacterEntity, out partyId, out party))
                return;

            if (party.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in party.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearParty();
                        SendPartyTerminateToClient(memberCharacterEntity.ConnectionId, partyId);
                    }
                }
                parties.Remove(partyId);
            }
            else
            {
                playerCharacterEntity.ClearParty();
                SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, partyId);
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
                SendRemovePartyMemberToClients(party, playerCharacterEntity.Id);
            }
        }

        public virtual void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName, int guildId)
        {
            if (!CanCreateGuild(playerCharacterEntity))
                return;

            gameInstance.SocialSystemSetting.ReduceCreateGuildResource(playerCharacterEntity);
            var guild = new GuildData(guildId, guildName, playerCharacterEntity);
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
            playerCharacterEntity.SharedGuildExp = 0;
            SendCreateGuildToClient(playerCharacterEntity.ConnectionId, guild);
            SendAddGuildMembersToClient(playerCharacterEntity.ConnectionId, guild);
        }

        public virtual void ChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanChangeGuildLeader(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            guild.SetLeader(characterId);
            guilds[guildId] = guild;
            BasePlayerCharacterEntity targetCharacterEntity;
            if (TryGetPlayerCharacterById(characterId, out targetCharacterEntity))
                targetCharacterEntity.GuildRole = guild.GetMemberRole(targetCharacterEntity.Id);
            playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
            SendChangeGuildLeaderToClients(guild);
        }

        public virtual void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMessage(playerCharacterEntity, out guildId, out guild))
                return;

            guild.guildMessage = guildMessage;
            guilds[guildId] = guild;
            SendSetGuildMessageToClients(guild);
        }

        public virtual void SetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildRole(playerCharacterEntity, guildRole, out guildId, out guild))
                return;

            guild.SetRole(guildRole, roleName, canInvite, canKick, shareExpPercentage);
            guilds[guildId] = guild;
            // Change characters guild role
            foreach (var memberId in guild.GetMemberIds())
            {
                BasePlayerCharacterEntity memberCharacterEntity;
                if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    memberCharacterEntity.GuildRole = guild.GetMemberRole(memberCharacterEntity.Id);
            }
            SendSetGuildRoleToClients(guild, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public virtual void SetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, string characterId, byte guildRole)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMemberRole(playerCharacterEntity, out guildId, out guild))
                return;

            guild.SetMemberRole(characterId, guildRole);
            guilds[guildId] = guild;
            BasePlayerCharacterEntity memberCharacterEntity;
            if (TryGetPlayerCharacterById(characterId, out memberCharacterEntity))
                memberCharacterEntity.GuildRole = guild.GetMemberRole(memberCharacterEntity.Id);
            SendSetGuildMemberRoleToClients(guild, characterId, guildRole);
        }

        public virtual void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildMember(inviteCharacterEntity, acceptCharacterEntity, out guildId, out guild))
                return;

            guild.AddMember(acceptCharacterEntity);
            guilds[guildId] = guild;
            acceptCharacterEntity.GuildId = guildId;
            acceptCharacterEntity.GuildRole = guild.GetMemberRole(acceptCharacterEntity.Id);
            acceptCharacterEntity.SharedGuildExp = 0;
            SendCreateGuildToClient(acceptCharacterEntity.ConnectionId, guild);
            SendAddGuildMembersToClient(acceptCharacterEntity.ConnectionId, guild);
            SendSetGuildMessageToClient(acceptCharacterEntity.ConnectionId, guild);
            SendSetGuildRolesToClient(acceptCharacterEntity.ConnectionId, guild);
            SendSetGuildMemberRolesToClient(acceptCharacterEntity.ConnectionId, guild);
            SendAddGuildMemberToClients(guild, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public virtual void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanKickFromGuild(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
            {
                memberCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(memberCharacterEntity.ConnectionId, guildId);
            }
            guild.RemoveMember(characterId);
            guilds[guildId] = guild;
            SendRemoveGuildMemberToClients(guild, characterId);
        }

        public virtual void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanLeaveGuild(playerCharacterEntity, out guildId, out guild))
                return;

            if (guild.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in guild.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearGuild();
                        SendGuildTerminateToClient(memberCharacterEntity.ConnectionId, guildId);
                    }
                }
                guilds.Remove(guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, guildId);
                guild.RemoveMember(playerCharacterEntity.Id);
                guilds[guildId] = guild;
                SendRemoveGuildMemberToClients(guild, playerCharacterEntity.Id);
            }
        }
        #endregion

        public abstract void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem);
        public abstract void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName);
    }
}
