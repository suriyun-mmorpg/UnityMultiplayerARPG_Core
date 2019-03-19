using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameNetworkManager
    {
        #region Activity validation functions
        public virtual bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer || playerCharacterEntity.IsWarping)
                return false;
            return true;
        }

        public virtual bool CanCreateParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return false;
            if (playerCharacterEntity.PartyId > 0)
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.JoinedAnotherParty);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedParty);
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotPartyLeader);
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedParty);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedParty);
                return false;
            }
            if (!party.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotPartyLeader);
                return false;
            }
            return true;
        }

        public virtual bool CanSendPartyInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            int partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedParty);
                return false;
            }
            if (!party.CanInvite(inviteCharacterEntity.Id))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CannotSendPartyInvitation);
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.NotFoundCharacter);
                return false;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CharacterIsInAnotherDeal);
                return false;
            }
            if (targetCharacterEntity.PartyId > 0)
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CharacterJoinedAnotherParty);
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
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.JoinedAnotherParty);
                return false;
            }
            partyId = inviteCharacterEntity.PartyId;
            if (partyId <= 0 || !parties.TryGetValue(partyId, out party))
            {
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedParty);
                return false;
            }
            if (party.CountMember() >= party.MaxMember())
            {
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.PartyMemberReachedLimit);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedParty);
                return false;
            }
            if (party.IsLeader(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickPartyLeader);
                return false;
            }
            if (!party.CanKick(playerCharacterEntity.Id))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickPartyMember);
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickYourSelfFromParty);
                return false;
            }
            if (!party.ContainsMemberId(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedParty);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedParty);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.JoinedAnotherGuild);
                return false;
            }
            GameMessage.Type gameMessageType;
            if (!gameInstance.SocialSystemSetting.CanCreateGuild(playerCharacterEntity, out gameMessageType))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, gameMessageType);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotGuildLeader);
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedGuild);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotGuildLeader);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotGuildLeader);
                return false;
            }
            if (!guild.IsRoleAvailable(guildRole))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.GuildRoleNotAvailable);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (!guild.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.GuildRoleNotAvailable);
                return false;
            }
            return true;
        }

        public virtual bool CanSendGuildInvitation(BasePlayerCharacterEntity inviteCharacterEntity, uint objectId, out BasePlayerCharacterEntity targetCharacterEntity)
        {
            targetCharacterEntity = null;
            int guildId = inviteCharacterEntity.GuildId;
            GuildData guild;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (!guild.CanInvite(inviteCharacterEntity.Id))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CannotSendGuildInvitation);
                return false;
            }
            if (!inviteCharacterEntity.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.NotFoundCharacter);
                return false;
            }
            if (targetCharacterEntity.DealingCharacter != null)
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CharacterIsInAnotherDeal);
                return false;
            }
            if (targetCharacterEntity.GuildId > 0)
            {
                SendServerGameMessage(inviteCharacterEntity.ConnectionId, GameMessage.Type.CharacterJoinedAnotherGuild);
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
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.JoinedAnotherGuild);
                return false;
            }
            guildId = inviteCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
            {
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedGuild);
                return false;
            }
            if (guild.CountMember() >= guild.MaxMember())
            {
                SendServerGameMessage(acceptCharacterEntity.ConnectionId, GameMessage.Type.GuildMemberReachedLimit);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            if (guild.IsLeader(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickGuildLeader);
                return false;
            }
            if (!guild.CanKick(playerCharacterEntity.Id))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickGuildMember);
                return false;
            }
            if (playerCharacterEntity.Id.Equals(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickYourSelfFromGuild);
                return false;
            }
            byte role;
            if (!guild.TryGetMemberRole(characterId, out role) && playerCharacterEntity.GuildRole < role)
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotKickHigherGuildMember);
                return false;
            }
            if (!guild.ContainsMemberId(characterId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CharacterNotJoinedGuild);
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
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
                return false;
            }
            return true;
        }

        public virtual bool CanIncreaseGuildExp(BasePlayerCharacterEntity playerCharacterEntity, int exp, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (exp <= 0 || playerCharacterEntity == null || !IsServer)
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
                return false;
            return true;
        }

        public virtual bool CanAddGuildSkill(BasePlayerCharacterEntity playerCharacterEntity, int dataId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer || !GameInstance.GuildSkills.ContainsKey(dataId))
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
                return false;
            if (!guild.IsLeader(playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotGuildLeader);
                return false;
            }
            if (guild.IsSkillReachedMaxLevel(dataId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.GuildSkillReachedMaxLevel);
                return false;
            }
            if (guild.skillPoint <= 0)
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NoGuildSkillPoint);
                return false;
            }
            return true;
        }

        public virtual bool CanUseGuildSkill(BasePlayerCharacterEntity playerCharacterEntity, int dataId, out int guildId, out GuildData guild)
        {
            guildId = 0;
            guild = null;
            if (playerCharacterEntity == null || !IsServer || !GameInstance.GuildSkills.ContainsKey(dataId))
                return false;
            guildId = playerCharacterEntity.GuildId;
            if (guildId <= 0 || !guilds.TryGetValue(guildId, out guild))
                return false;
            if (guild.GetSkillLevel(dataId) <= 0)
                return false;
            if (playerCharacterEntity.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
                return false;
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

        public virtual void RespawnCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            string respawnMapName = playerCharacterEntity.RespawnMapName;
            Vector3 respawnPosition = playerCharacterEntity.RespawnPosition;
            if (CurrentMapInfo != null && CurrentMapInfo.overrideRespawnPoint)
            {
                if (CurrentMapInfo.overrideRespawnPointMap != null)
                    respawnMapName = CurrentMapInfo.overrideRespawnPointMap.Id;
                respawnPosition = CurrentMapInfo.overrideRespawnPointPosition;
            }
            WarpCharacter(playerCharacterEntity, respawnMapName, respawnPosition);
        }

        protected virtual void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
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

            PartyData party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
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
                foreach (string memberId in party.GetMemberIds())
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
            GuildData guild = new GuildData(guildId, guildName, playerCharacterEntity);
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
            playerCharacterEntity.GuildName = guildName;
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
            foreach (string memberId in guild.GetMemberIds())
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
            acceptCharacterEntity.GuildName = guild.guildName;
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
                foreach (string memberId in guild.GetMemberIds())
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

        public virtual void IncreaseGuildExp(BasePlayerCharacterEntity playerCharacterEntity, int exp)
        {
            int guildId;
            GuildData guild;
            if (!CanIncreaseGuildExp(playerCharacterEntity, exp, out guildId, out guild))
                return;

            guild = gameInstance.SocialSystemSetting.IncreaseGuildExp(guild, exp);
            guilds[guildId] = guild;
            SendGuildLevelExpSkillPointToClients(guild);
        }

        public virtual void AddGuildSkill(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildSkill(playerCharacterEntity, dataId, out guildId, out guild))
                return;
            
            guild.AddSkillLevel(dataId);
            guilds[guildId] = guild;
            SendSetGuildSkillLevelToClients(guild, dataId);
            SendGuildLevelExpSkillPointToClients(guild);
        }
        #endregion

        public virtual string GetCurrentMapId(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return CurrentMapInfo.Id;
        }

        public virtual Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return playerCharacterEntity.CacheTransform.position;
        }

        public virtual void SetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity, Vector3 position)
        {
            playerCharacterEntity.CacheNetTransform.Teleport(position, playerCharacterEntity.CacheTransform.rotation);
            playerCharacterEntity.CacheTransform.position = position;
        }

        public void WarpCharacter(WarpPortalType warpPortalType, BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            switch (warpPortalType)
            {
                case WarpPortalType.Default:
                    WarpCharacter(playerCharacterEntity, mapName, position);
                    break;
                case WarpPortalType.EnterInstance:
                    WarpCharacterToInstance(playerCharacterEntity, mapName, position);
                    break;
            }
        }

        public Storage GetStorage(StorageId storageId)
        {
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = gameInstance.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = gameInstance.guildStorage;
                    break;
                case StorageType.Building:
                    BuildingEntity buildingEntity;
                    if (TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity) &&
                        buildingEntity is StorageEntity)
                        storage = (buildingEntity as StorageEntity).storage;
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacterEntity.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!guilds.ContainsKey(playerCharacterEntity.GuildId) ||
                        !playerCharacterEntity.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    BuildingEntity buildingEntity;
                    StorageEntity storageEntity;
                    if (TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity))
                    {
                        storageEntity = buildingEntity as StorageEntity;
                        if (storageEntity == null)
                            return false;
                        if (!playerCharacterEntity.Id.Equals(storageEntity.CreatorId))
                            return false;
                    }
                    break;
            }
            return true;
        }

        public abstract void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem);
        public abstract void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName);
        public abstract void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity);
        public abstract void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity);
        public abstract void MoveItemToStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short nonEquipIndex, short amount, short storageItemIndex);
        public abstract void MoveItemFromStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short storageItemIndex, short amount, short nonEquipIndex);
        public abstract bool IsStorageEntityOpen(StorageEntity storageEntity);
        public abstract void DepositGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);
        public abstract void WithdrawGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);
        public abstract void DepositGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);
        public abstract void WithdrawGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount);
        protected abstract void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position);
        protected abstract bool IsInstanceMap();
    }
}
