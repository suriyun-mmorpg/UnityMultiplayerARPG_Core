namespace MultiplayerARPG
{
    public static class ServerGuildHandlersExtensions
    {
        public static ValidateGuildRequestResult CanCreateGuild(this IPlayerCharacterData playerCharacter, string guildName)
        {
            GameMessage.Type gameMessageType;
            if (string.IsNullOrEmpty(guildName) || guildName.Length < GameInstance.Singleton.SocialSystemSetting.MinGuildNameLength)
            {
                gameMessageType = GameMessage.Type.TooShortGuildName;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildName.Length > GameInstance.Singleton.SocialSystemSetting.MaxGuildNameLength)
            {
                gameMessageType = GameMessage.Type.TooLongGuildName;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (playerCharacter.GuildId > 0)
            {
                gameMessageType = GameMessage.Type.JoinedAnotherGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!GameInstance.Singleton.SocialSystemSetting.CanCreateGuild(playerCharacter, out gameMessageType))
                return new ValidateGuildRequestResult(false, gameMessageType);
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType);
        }

        public static ValidateGuildRequestResult CanChangeGuildLeader(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, string memberId)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsLeader(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.ContainsMemberId(memberId))
            {
                gameMessageType = GameMessage.Type.CharacterNotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanChangeGuildMessage(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, string guildMessage)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsLeader(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildMessage.Length > GameInstance.Singleton.SocialSystemSetting.MaxGuildMessageLength)
            {
                gameMessageType = GameMessage.Type.TooLongGuildMessage;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanChangeGuildRole(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, byte guildRole, string roleName)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsLeader(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsRoleAvailable(guildRole))
            {
                gameMessageType = GameMessage.Type.GuildRoleNotAvailable;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (string.IsNullOrEmpty(roleName) || roleName.Length < GameInstance.Singleton.SocialSystemSetting.MinGuildRoleNameLength)
            {
                gameMessageType = GameMessage.Type.TooShortGuildRoleName;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (roleName.Length > GameInstance.Singleton.SocialSystemSetting.MaxGuildRoleNameLength)
            {
                gameMessageType = GameMessage.Type.TooLongGuildRoleName;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanSetGuildMemberRole(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsLeader(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanSendGuildInvitation(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData inviterCharacter, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            int guildId = inviterCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.CanInvite(inviterCharacter.Id))
            {
                gameMessageType = GameMessage.Type.CannotSendGuildInvitation;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (inviteeCharacter.GuildId > 0)
            {
                gameMessageType = GameMessage.Type.CharacterJoinedAnotherGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanAcceptGuildInvitation(this IServerGuildHandlers serverGuildHandlers, int guildId, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            GuildData guild;
            if (!serverGuildHandlers.HasGuildInvitation(guildId, inviteeCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotFoundGuildInvitation;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (inviteeCharacter.GuildId > 0)
            {
                gameMessageType = GameMessage.Type.JoinedAnotherGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotFoundGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guild.CountMember() >= guild.MaxMember())
            {
                gameMessageType = GameMessage.Type.GuildMemberReachedLimit;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanDeclineGuildInvitation(this IServerGuildHandlers serverGuildHandlers, int guildId, IPlayerCharacterData inviteeCharacter)
        {
            GameMessage.Type gameMessageType;
            GuildData guild;
            if (!serverGuildHandlers.HasGuildInvitation(guildId, inviteeCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotFoundGuildInvitation;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotFoundGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanKickMemberFromGuild(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, string memberId)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guild.IsLeader(memberId))
            {
                gameMessageType = GameMessage.Type.CannotKickGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.CanKick(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.CannotKickGuildMember;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (playerCharacter.Id.Equals(memberId))
            {
                gameMessageType = GameMessage.Type.CannotKickYourSelfFromGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            byte role;
            if (!guild.TryGetMemberRole(memberId, out role) && playerCharacter.GuildRole < role)
            {
                gameMessageType = GameMessage.Type.CannotKickHigherGuildMember;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.ContainsMemberId(memberId))
            {
                gameMessageType = GameMessage.Type.CharacterNotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanLeaveGuild(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanIncreaseGuildSkillLevel(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, int dataId)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (!GameInstance.GuildSkills.ContainsKey(dataId))
            {
                gameMessageType = GameMessage.Type.InvalidGuildSkillData;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (!guild.IsLeader(playerCharacter.Id))
            {
                gameMessageType = GameMessage.Type.NotGuildLeader;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guild.IsSkillReachedMaxLevel(dataId))
            {
                gameMessageType = GameMessage.Type.GuildSkillReachedMaxLevel;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guild.skillPoint <= 0)
            {
                gameMessageType = GameMessage.Type.NoGuildSkillPoint;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanIncreaseGuildExp(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, int exp)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }

        public static ValidateGuildRequestResult CanUseGuildSkill(this IServerGuildHandlers serverGuildHandlers, IPlayerCharacterData playerCharacter, int dataId)
        {
            GameMessage.Type gameMessageType;
            int guildId = playerCharacter.GuildId;
            GuildData guild;
            if (!GameInstance.GuildSkills.ContainsKey(dataId))
            {
                gameMessageType = GameMessage.Type.InvalidGuildSkillData;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guildId <= 0 || !serverGuildHandlers.TryGetGuild(guildId, out guild))
            {
                gameMessageType = GameMessage.Type.NotJoinedGuild;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (guild.GetSkillLevel(dataId) <= 0)
            {
                gameMessageType = GameMessage.Type.SkillLevelIsZero;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            if (playerCharacter.IndexOfSkillUsage(dataId, SkillUsageType.GuildSkill) >= 0)
            {
                gameMessageType = GameMessage.Type.SkillIsCoolingDown;
                return new ValidateGuildRequestResult(false, gameMessageType);
            }
            gameMessageType = GameMessage.Type.None;
            return new ValidateGuildRequestResult(true, gameMessageType, guildId, guild);
        }
    }
}
