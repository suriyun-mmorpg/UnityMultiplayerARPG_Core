using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class DefaultLocale
    {
        public static readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        static DefaultLocale()
        {
            Texts.Add(GameMessage.Type.InvalidItemData.ToString(), "Invalid item data");
            Texts.Add(GameMessage.Type.NotFoundCharacter.ToString(), "Character not found");
            Texts.Add(GameMessage.Type.NotAbleToLoot.ToString(), "Cannot get this item");
            Texts.Add(GameMessage.Type.NotEnoughGold.ToString(), "Have not enough gold");
            Texts.Add(GameMessage.Type.NotEnoughItems.ToString(), "Have not enough items");
            Texts.Add(GameMessage.Type.CannotCarryAnymore.ToString(), "Cannot carry anymore items");
            // Refine
            Texts.Add(GameMessage.Type.RefineItemReachedMaxLevel.ToString(), "Item reached max level");
            Texts.Add(GameMessage.Type.RefineSuccess.ToString(), "Item level up success");
            Texts.Add(GameMessage.Type.RefineFail.ToString(), "Item level up fail");
            // Dealing
            Texts.Add(GameMessage.Type.CharacterIsInAnotherDeal.ToString(), "Character is in another deal");
            Texts.Add(GameMessage.Type.CharacterIsTooFar.ToString(), "Character is too far");
            Texts.Add(GameMessage.Type.CannotAcceptDealingRequest.ToString(), "Cannot accept dealing request");
            Texts.Add(GameMessage.Type.DealingRequestDeclined.ToString(), "Dealing request declined");
            Texts.Add(GameMessage.Type.InvalidDealingState.ToString(), "Invalid dealing state");
            Texts.Add(GameMessage.Type.DealingCanceled.ToString(), "Dealing canceled");
            // Party
            Texts.Add(GameMessage.Type.PartyInvitationDeclined.ToString(), "Party invitation declined");
            Texts.Add(GameMessage.Type.CannotSendPartyInvitation.ToString(), "Cannot send party invitation");
            Texts.Add(GameMessage.Type.CannotKickPartyMember.ToString(), "Cannot kick party member");
            Texts.Add(GameMessage.Type.CannotKickYourSelfFromParty.ToString(), "Cannot kick yourself from party");
            Texts.Add(GameMessage.Type.CannotKickPartyLeader.ToString(), "Cannot kick party leader");
            Texts.Add(GameMessage.Type.JoinedAnotherParty.ToString(), "Already joined another party");
            Texts.Add(GameMessage.Type.NotJoinedParty.ToString(), "Not joined the party");
            Texts.Add(GameMessage.Type.NotPartyLeader.ToString(), "Not a party member");
            Texts.Add(GameMessage.Type.CharacterJoinedAnotherParty.ToString(), "Character already joined another party");
            Texts.Add(GameMessage.Type.CharacterNotJoinedParty.ToString(), "Character not joined the party");
            Texts.Add(GameMessage.Type.PartyMemberReachedLimit.ToString(), "Party member reached limit");
            // Guild
            Texts.Add(GameMessage.Type.GuildInvitationDeclined.ToString(), "Guild invitation declined");
            Texts.Add(GameMessage.Type.CannotSendGuildInvitation.ToString(), "Cannot send guild invitation");
            Texts.Add(GameMessage.Type.CannotKickGuildMember.ToString(), "Cannot kick guild member");
            Texts.Add(GameMessage.Type.CannotKickYourSelfFromGuild.ToString(), "Cannot kick yourself from guild");
            Texts.Add(GameMessage.Type.CannotKickGuildLeader.ToString(), "Cannot kick guild leader");
            Texts.Add(GameMessage.Type.CannotKickHigherGuildMember.ToString(), "Cannot kick higher guild member");
            Texts.Add(GameMessage.Type.JoinedAnotherGuild.ToString(), "Already joined another guild");
            Texts.Add(GameMessage.Type.NotJoinedGuild.ToString(), "Not joined the guild");
            Texts.Add(GameMessage.Type.NotGuildLeader.ToString(), "Not a guild member");
            Texts.Add(GameMessage.Type.CharacterJoinedAnotherGuild.ToString(), "Character already joined another guild");
            Texts.Add(GameMessage.Type.CharacterNotJoinedGuild.ToString(), "Character not joined the guild");
            Texts.Add(GameMessage.Type.GuildMemberReachedLimit.ToString(), "Guild member reached limit");
            Texts.Add(GameMessage.Type.GuildRoleNotAvailable.ToString(), "Guild role is not available");
            Texts.Add(GameMessage.Type.GuildSkillReachedMaxLevel.ToString(), "Guild skill is reached max level");
            Texts.Add(GameMessage.Type.NoGuildSkillPoint.ToString(), "No guild skill point");
        }
    }

    [System.Serializable]
    public class Language
    {
        public string languageKey;
        public List<LanguageData> dataList = new List<LanguageData>();

        public bool ContainKey(string key)
        {
            foreach (var entry in dataList)
            {
                if (entry.key == key)
                    return true;
            }
            return false;
        }
    }

    [System.Serializable]
    public struct LanguageData
    {
        public string key;
        [TextArea(1, 30)]
        public string value;
    }
}
