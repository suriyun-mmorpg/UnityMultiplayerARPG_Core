using LiteNetLibManager;
using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public class GameMessage : ILiteNetLibMessage
    {
        public enum Type : byte
        {
            None,
            InvalidItemData,
            NotFoundCharacter,
            NotAbleToLoot,
            NotEnoughGold,
            NotEnoughItems,
            CannotCarryAnymore,
            // Refine
            RefineItemReachedMaxLevel,
            RefineSuccess,
            RefineFail,
            // Dealing
            CharacterIsInAnotherDeal,
            CharacterIsTooFar,
            CannotAcceptDealingRequest,
            DealingRequestDeclined,
            InvalidDealingState,
            DealingCanceled,
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
