using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct GameMessage : INetSerializable
    {
        public enum Type : byte
        {
            None,
            ServiceNotAvailable,
            InvalidItemData,
            NotFoundCharacter,
            NotAbleToLoot,
            NotEnoughGold,
            NotEnoughItems,
            CannotCarryAnymore,
            // Equip
            CannotEquip,
            LevelOrAttributeNotEnough,
            InvalidEquipPositionRightHand,
            InvalidEquipPositionLeftHand,
            InvalidEquipPositionRightHandOrLeftHand,
            InvalidEquipPositionArmor,
            // Refine
            CannotRefine,
            RefineItemReachedMaxLevel,
            RefineSuccess,
            RefineFail,
            // Enhance
            CannotEnhanceSocket,
            NotEnoughSocketEnchaner,
            // Repair
            CannotRepair,
            RepairSuccess,
            // Dealing
            CharacterIsInAnotherDeal,
            CharacterIsTooFar,
            CannotAcceptDealingRequest,
            DealingRequestDeclined,
            InvalidDealingState,
            DealingCanceled,
            AnotherCharacterCannotCarryAnymore,
            // Party
            PartyInvitationDeclined,
            CannotSendPartyInvitation,
            CannotKickPartyMember,
            CannotKickYourSelfFromParty,
            CannotKickPartyLeader,
            JoinedAnotherParty,
            NotJoinedParty,
            NotPartyLeader,
            CharacterJoinedAnotherParty,
            CharacterNotJoinedParty,
            PartyMemberReachedLimit,
            // Guild
            GuildInvitationDeclined,
            CannotSendGuildInvitation,
            CannotKickGuildMember,
            CannotKickYourSelfFromGuild,
            CannotKickGuildLeader,
            CannotKickHigherGuildMember,
            JoinedAnotherGuild,
            NotJoinedGuild,
            NotGuildLeader,
            CharacterJoinedAnotherGuild,
            CharacterNotJoinedGuild,
            GuildMemberReachedLimit,
            GuildRoleNotAvailable,
            GuildSkillReachedMaxLevel,
            NoGuildSkillPoint,
            // Game Data
            UnknowGameDataTitle,
            UnknowGameDataDescription,
            // Bank
            NotEnoughGoldToDeposit,
            NotEnoughGoldToWithdraw,
            CannotAccessStorage,
            // Combatant
            NoAmmo,
            NotEnoughMp,
            // Guild Name
            TooShortGuildName,
            TooLongGuildName,
            ExistedGuildName,
            // Guild Role Name
            TooShortGuildRoleName,
            TooLongGuildRoleName,
            // Guild Message
            TooLongGuildMessage,
            // Skills
            SkillLevelIsZero,
            CannotUseSkillByCurrentWeapon,
            SkillIsCoolingDown,
            SkillIsNotLearned,
            NoSkillTarget,
            // Requirements
            NotEnoughLevel,
            NotMatchCharacterClass,
            NotEnoughAttributeAmounts,
            NotEnoughSkillLevels,
            NotEnoughStatPoint,
            NotEnoughSkillPoint,
            AttributeReachedMaxAmount,
            SkillReachedMaxLevel,
        }
        public Type type;

        public void Deserialize(NetDataReader reader)
        {
            type = (Type)reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)type);
        }
    }
}
