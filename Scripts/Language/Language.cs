using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum UILocaleKeys : ushort
    {
        // UI Generic Title
        UI_LABEL_DISCONNECTED,
        UI_LABEL_SUCCESS,
        UI_LABEL_WARNING,
        UI_LABEL_ERROR,
        UI_LABEL_NONE,
        UI_LABEL_LEVEL,
        UI_LABEL_EXP,
        UI_LABEL_STAT_POINTS,
        UI_LABEL_SKILL_POINTS,
        UI_LABEL_HP,
        UI_LABEL_MP,
        UI_LABEL_STAMINA,
        UI_LABEL_FOOD,
        UI_LABEL_WATER,
        UI_LABEL_ARMOR,
        UI_LABEL_ACCURACY,
        UI_LABEL_EVASION,
        UI_LABEL_CRITICAL_RATE,
        UI_LABEL_CRITICAL_DAMAGE_RATE,
        UI_LABEL_BLOCK_RATE,
        UI_LABEL_BLOCK_DAMAGE_RATE,
        UI_LABEL_MOVE_SPEED,
        UI_LABEL_ATTACK_SPEED,
        UI_LABEL_WEIGHT,
        UI_LABEL_SLOT,
        UI_LABEL_UNLIMIT_WEIGHT,
        UI_LABEL_UNLIMIT_SLOT,
        UI_LABEL_GOLD,
        UI_LABEL_CASH,
        UI_LABEL_SELL_PRICE,
        UI_LABEL_REQUIRE_LEVEL,
        UI_LABEL_REQUIRE_CLASS,
        UI_LABEL_REQUIRE_GOLD,
        UI_LABEL_AVAILABLE_WEAPONS,
        UI_LABEL_CONSUME_MP,
        UI_LABEL_SKILL_COOLDOWN,
        UI_LABEL_SKILL_TYPE,
        UI_SKILL_TYPE_ACTIVE,
        UI_SKILL_TYPE_PASSIVE,
        UI_SKILL_TYPE_CRAFT_ITEM,
        UI_LABEL_BUFF_DURATION,
        UI_LABEL_BUFF_RECOVERY_HP,
        UI_LABEL_BUFF_RECOVERY_MP,
        UI_LABEL_BUFF_RECOVERY_STAMINA,
        UI_LABEL_BUFF_RECOVERY_FOOD,
        UI_LABEL_BUFF_RECOVERY_WATER,
        UI_LABEL_ITEM_TYPE,
        UI_ITEM_TYPE_JUNK,
        UI_ITEM_TYPE_SHIELD,
        UI_ITEM_TYPE_POTION,
        UI_ITEM_TYPE_AMMO,
        UI_ITEM_TYPE_BUILDING,
        UI_ITEM_TYPE_PET,
        UI_ITEM_TYPE_SOCKET_ENHANCER,
        UI_LABEL_ITEM_RARITY,
        UI_LABEL_ITEM_AMOUNT,
        UI_LABEL_ITEM_DURABILITY,
        UI_LABEL_SOCIAL_LEADER,
        UI_LABEL_SOCIAL_MEMBER,
        // Generic Format
        UI_FORMAT_INCREASE_MAX_MEMBER,
        UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE,
        UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE,
        // Damage Format
        UI_FORMAT_DAMAGE_INFLICTION,
        UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL,
        // Generic Error
        UI_USER_NOT_FOUND,
        UI_ITEM_NOT_FOUND,
        UI_NOT_ENOUGH_GOLD,
        UI_NOT_ENOUGH_CASH,
        UI_NOT_LOGGED_IN,
        UI_INVALID_DATA,
        UI_INVALID_CHARACTER_DATA,
        UI_USERNAME_IS_EMPTY,
        UI_PASSWORD_IS_EMPTY,
        // UI Login
        UI_INVALID_USERNAME_OR_PASSWORD,
        UI_ALREADY_LOGGED_IN,
        // UI Register
        UI_INVALID_CONFIRM_PASSWORD,
        UI_USERNAME_TOO_SHORT,
        UI_USERNAME_TOO_LONG,
        UI_PASSWORD_TOO_SHORT,
        UI_USERNAME_EXISTED,
        // UI Character List
        UI_NO_CHOSEN_CHARACTER_TO_START,
        UI_NO_CHOSEN_CHARACTER_TO_DELETE,
        UI_ALREADY_SELECT_CHARACTER,
        UI_MAP_SERVER_NOT_READY,
        // UI Character Create
        UI_CHARACTER_NAME_TOO_SHORT,
        UI_CHARACTER_NAME_TOO_LONG,
        UI_CHARACTER_NAME_EXISTED,
        // UI Character Item
        UI_DROP_ITEM,
        UI_DROP_ITEM_DESCRIPTION,
        UI_SELL_ITEM,
        UI_SELL_ITEM_DESCRIPTION,
        UI_OFFER_ITEM,
        UI_OFFER_ITEM_DESCRIPTION,
        UI_MOVE_ITEM_TO_STORAGE,
        UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION,
        UI_MOVE_ITEM_FROM_STORAGE,
        UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION,
        // UI Refine Item
        UI_LABEL_REFINE_SUCCESS_RATE,
        UI_LABEL_REFINING_LEVEL,
        // UI Bank
        UI_BANK_DEPOSIT,
        UI_BANK_DEPOSIT_DESCRIPTION,
        UI_BANK_WITHDRAW,
        UI_BANK_WITHDRAW_DESCRIPTION,
        // UI Cash Packages
        UI_CANNOT_GET_CASH_PACKAGE_INFO,
        // UI Cash Shop
        UI_CANNOT_GET_CASH_SHOP_INFO,
        UI_CASH_SHOP_BUY_SUCCESS,
        // UI Dealing
        UI_OFFER_GOLD,
        UI_OFFER_GOLD_DESCRIPTION,
        // UI Npc Sell Item
        UI_BUY_ITEM,
        UI_BUY_ITEM_DESCRIPTION,
        // UI Party
        UI_PARTY_CHANGE_LEADER,
        UI_PARTY_CHANGE_LEADER_DESCRIPTION,
        UI_PARTY_KICK_MEMBER,
        UI_PARTY_KICK_MEMBER_DESCRIPTION,
        UI_PARTY_LEAVE,
        UI_PARTY_LEAVE_DESCRIPTION,
        // UI Guild
        UI_GUILD_CHANGE_LEADER,
        UI_GUILD_CHANGE_LEADER_DESCRIPTION,
        UI_GUILD_KICK_MEMBER,
        UI_GUILD_KICK_MEMBER_DESCRIPTION,
        UI_GUILD_LEAVE,
        UI_GUILD_LEAVE_DESCRIPTION,
        // UI Guild Role
        UI_GUILD_ROLE_CAN_INVITE,
        UI_GUILD_ROLE_CANNOT_INVITE,
        UI_GUILD_ROLE_CAN_KICK,
        UI_GUILD_ROLE_CANNOT_KICK,
        UI_GUILD_ROLE_SHARE_EXP,
        // UI Guild Role Setting
        UI_GUILD_ROLE_NAME_IS_EMPTY,
        UI_GUILD_ROLE_SHARE_EXP_NOT_NUMBER,
        // UI Guild Member Role Setting
        UI_INVALID_GUILD_ROLE,
        // UI Guild Skill

        // UI Scene Global
        UI_KICKED_FROM_SERVER,
        UI_CONNECTION_FAILED,
        UI_CONNECTION_REJECTED,
        UI_REMOTE_CONNECTION_CLOSE,
        UI_INVALID_PROTOCOL,
        UI_HOST_UNREACHABLE,
        UI_CONNECTION_TIMEOUT,
    }

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
            Texts.Add(GameMessage.Type.UnknowGameDataTitle.ToString(), "Unknow");
            Texts.Add(GameMessage.Type.UnknowGameDataDescription.ToString(), "N/A");
            Texts.Add(GameMessage.Type.NotEnoughGoldToDeposit.ToString(), "Not enough gold to deposit");
            Texts.Add(GameMessage.Type.NotEnoughGoldToWithdraw.ToString(), "Not enough gold to withdraw");
            Texts.Add(GameMessage.Type.CannotAccessStorage.ToString(), "Cannot access storage");
            // Battle
            Texts.Add(GameMessage.Type.NoAmmo.ToString(), "No Ammo");
            // UI Generic Title
            Texts.Add(UILocaleKeys.UI_LABEL_DISCONNECTED.ToString(), "Disconnected");
            Texts.Add(UILocaleKeys.UI_LABEL_SUCCESS.ToString(), "Success");
            Texts.Add(UILocaleKeys.UI_LABEL_WARNING.ToString(), "Warning");
            Texts.Add(UILocaleKeys.UI_LABEL_ERROR.ToString(), "Error");
            Texts.Add(UILocaleKeys.UI_LABEL_NONE.ToString(), "None");
            Texts.Add(UILocaleKeys.UI_LABEL_LEVEL.ToString(), "Lv.");
            Texts.Add(UILocaleKeys.UI_LABEL_EXP.ToString(), "Exp");
            Texts.Add(UILocaleKeys.UI_LABEL_STAT_POINTS.ToString(), "Stat Points");
            Texts.Add(UILocaleKeys.UI_LABEL_SKILL_POINTS.ToString(), "Skill Points");
            Texts.Add(UILocaleKeys.UI_LABEL_HP.ToString(), "Hp");
            Texts.Add(UILocaleKeys.UI_LABEL_MP.ToString(), "Mp");
            Texts.Add(UILocaleKeys.UI_LABEL_STAMINA.ToString(), "Stamina");
            Texts.Add(UILocaleKeys.UI_LABEL_FOOD.ToString(), "Food");
            Texts.Add(UILocaleKeys.UI_LABEL_WATER.ToString(), "Water");
            Texts.Add(UILocaleKeys.UI_LABEL_ARMOR.ToString(), "Armor");
            Texts.Add(UILocaleKeys.UI_LABEL_ACCURACY.ToString(), "Accuracy");
            Texts.Add(UILocaleKeys.UI_LABEL_EVASION.ToString(), "Evasion");
            Texts.Add(UILocaleKeys.UI_LABEL_CRITICAL_RATE.ToString(), "Cri. Rate");
            Texts.Add(UILocaleKeys.UI_LABEL_CRITICAL_DAMAGE_RATE.ToString(), "Cri. Damage");
            Texts.Add(UILocaleKeys.UI_LABEL_BLOCK_RATE.ToString(), "Block Rate");
            Texts.Add(UILocaleKeys.UI_LABEL_BLOCK_DAMAGE_RATE.ToString(), "Block Damage");
            Texts.Add(UILocaleKeys.UI_LABEL_MOVE_SPEED.ToString(), "Move Speed");
            Texts.Add(UILocaleKeys.UI_LABEL_ATTACK_SPEED.ToString(), "Attack Speed");
            Texts.Add(UILocaleKeys.UI_LABEL_WEIGHT.ToString(), "Weight");
            Texts.Add(UILocaleKeys.UI_LABEL_SLOT.ToString(), "Slot");
            Texts.Add(UILocaleKeys.UI_LABEL_UNLIMIT_WEIGHT.ToString(), "Unlimit Weight");
            Texts.Add(UILocaleKeys.UI_LABEL_UNLIMIT_SLOT.ToString(), "Unlimit Slot");
            Texts.Add(UILocaleKeys.UI_LABEL_GOLD.ToString(), "Gold");
            Texts.Add(UILocaleKeys.UI_LABEL_CASH.ToString(), "Cash");
            Texts.Add(UILocaleKeys.UI_LABEL_SELL_PRICE.ToString(), "Sell Price");
            Texts.Add(UILocaleKeys.UI_LABEL_REQUIRE_LEVEL.ToString(), "Require Level");
            Texts.Add(UILocaleKeys.UI_LABEL_REQUIRE_CLASS.ToString(), "Require Class");
            Texts.Add(UILocaleKeys.UI_LABEL_REQUIRE_GOLD.ToString(), "Require Gold");
            Texts.Add(UILocaleKeys.UI_LABEL_AVAILABLE_WEAPONS.ToString(), "Available Weapons");
            Texts.Add(UILocaleKeys.UI_LABEL_CONSUME_MP.ToString(), "Consume Mp");
            Texts.Add(UILocaleKeys.UI_LABEL_SKILL_COOLDOWN.ToString(), "Cooldown");
            Texts.Add(UILocaleKeys.UI_LABEL_SKILL_TYPE.ToString(), "Skill Type");
            Texts.Add(UILocaleKeys.UI_SKILL_TYPE_ACTIVE.ToString(), "Active");
            Texts.Add(UILocaleKeys.UI_SKILL_TYPE_PASSIVE.ToString(), "Passive");
            Texts.Add(UILocaleKeys.UI_SKILL_TYPE_CRAFT_ITEM.ToString(), "Craft Item");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_DURATION.ToString(), "Duration");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_HP.ToString(), "Recovery Hp");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_MP.ToString(), "Recovery Mp");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_STAMINA.ToString(), "Recovery Stamina");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_FOOD.ToString(), "Recovery Food");
            Texts.Add(UILocaleKeys.UI_LABEL_BUFF_RECOVERY_WATER.ToString(), "Recovery Water");
            Texts.Add(UILocaleKeys.UI_LABEL_ITEM_TYPE.ToString(), "Item Type");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_JUNK.ToString(), "Junk");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_SHIELD.ToString(), "Shield");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_POTION.ToString(), "Potion");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_AMMO.ToString(), "Ammo");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_BUILDING.ToString(), "Building");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_PET.ToString(), "Pet");
            Texts.Add(UILocaleKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString(), "Socket Enhancer");
            Texts.Add(UILocaleKeys.UI_LABEL_ITEM_RARITY.ToString(), "Rarity");
            Texts.Add(UILocaleKeys.UI_LABEL_ITEM_AMOUNT.ToString(), "Amount");
            Texts.Add(UILocaleKeys.UI_LABEL_ITEM_DURABILITY.ToString(), "Durability");
            Texts.Add(UILocaleKeys.UI_LABEL_SOCIAL_LEADER.ToString(), "Leader");
            Texts.Add(UILocaleKeys.UI_LABEL_SOCIAL_MEMBER.ToString(), "Member");
            // Generic Format
            Texts.Add(UILocaleKeys.UI_FORMAT_INCREASE_MAX_MEMBER.ToString(), "Max Member +{0}");
            Texts.Add(UILocaleKeys.UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE.ToString(), "Exp Gain +{0}%");
            Texts.Add(UILocaleKeys.UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE.ToString(), "Gold Gain +{0}%");
            Texts.Add(UILocaleKeys.UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE.ToString(), "Party Share Exp +{0}%");
            Texts.Add(UILocaleKeys.UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE.ToString(), "Party Share Gold +{0}%");
            Texts.Add(UILocaleKeys.UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE.ToString(), "Exp Penalty -{0}%");
            // Damage Format
            Texts.Add(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION.ToString(), "Inflict {0}% damage");
            Texts.Add(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString(), "Inflict {1}% as {0} damage");
            // Generic Error
            Texts.Add(UILocaleKeys.UI_USER_NOT_FOUND.ToString(), "User not found");
            Texts.Add(UILocaleKeys.UI_ITEM_NOT_FOUND.ToString(), "Item not found");
            Texts.Add(UILocaleKeys.UI_NOT_ENOUGH_GOLD.ToString(), "Not enough gold");
            Texts.Add(UILocaleKeys.UI_NOT_ENOUGH_CASH.ToString(), "Not enough cash");
            Texts.Add(UILocaleKeys.UI_INVALID_DATA.ToString(), "Invalid data");
            Texts.Add(UILocaleKeys.UI_INVALID_CHARACTER_DATA.ToString(), "Invalid character data");
            Texts.Add(UILocaleKeys.UI_USERNAME_IS_EMPTY.ToString(), "Username is empty");
            Texts.Add(UILocaleKeys.UI_PASSWORD_IS_EMPTY.ToString(), "Password is empty");
            // UI Login
            Texts.Add(UILocaleKeys.UI_INVALID_USERNAME_OR_PASSWORD.ToString(), "Invalid username or password");
            Texts.Add(UILocaleKeys.UI_ALREADY_LOGGED_IN.ToString(), "User already logged in");
            // UI Register
            Texts.Add(UILocaleKeys.UI_INVALID_CONFIRM_PASSWORD.ToString(), "Invalid confirm password");
            Texts.Add(UILocaleKeys.UI_USERNAME_TOO_SHORT.ToString(), "Username is too short");
            Texts.Add(UILocaleKeys.UI_USERNAME_TOO_LONG.ToString(), "Username is too long");
            Texts.Add(UILocaleKeys.UI_PASSWORD_TOO_SHORT.ToString(), "Password is too short");
            Texts.Add(UILocaleKeys.UI_USERNAME_EXISTED.ToString(), "Username is already existed");
            // UI Character List
            Texts.Add(UILocaleKeys.UI_NO_CHOSEN_CHARACTER_TO_START.ToString(), "Please choose character to start game");
            Texts.Add(UILocaleKeys.UI_NO_CHOSEN_CHARACTER_TO_DELETE.ToString(), "Please choose character to delete");
            Texts.Add(UILocaleKeys.UI_ALREADY_SELECT_CHARACTER.ToString(), "Already select character");
            Texts.Add(UILocaleKeys.UI_MAP_SERVER_NOT_READY.ToString(), "Map server is not ready");
            // UI Character Create
            Texts.Add(UILocaleKeys.UI_CHARACTER_NAME_TOO_SHORT.ToString(), "Character name is too short");
            Texts.Add(UILocaleKeys.UI_CHARACTER_NAME_TOO_LONG.ToString(), "Character name is too long");
            Texts.Add(UILocaleKeys.UI_CHARACTER_NAME_EXISTED.ToString(), "Character name is already existed");
            // UI Character Item
            Texts.Add(UILocaleKeys.UI_DROP_ITEM.ToString(), "Drop Item");
            Texts.Add(UILocaleKeys.UI_DROP_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UILocaleKeys.UI_SELL_ITEM.ToString(), "Sell Item");
            Texts.Add(UILocaleKeys.UI_SELL_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UILocaleKeys.UI_OFFER_ITEM.ToString(), "Offer Item");
            Texts.Add(UILocaleKeys.UI_OFFER_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UILocaleKeys.UI_MOVE_ITEM_TO_STORAGE.ToString(), "Move To Storage");
            Texts.Add(UILocaleKeys.UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UILocaleKeys.UI_MOVE_ITEM_FROM_STORAGE.ToString(), "Move From Storage");
            Texts.Add(UILocaleKeys.UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            // UI Refine Item
            Texts.Add(UILocaleKeys.UI_LABEL_REFINE_SUCCESS_RATE.ToString(), "Success Rate");
            Texts.Add(UILocaleKeys.UI_LABEL_REFINING_LEVEL.ToString(), "Refining Level");
            // UI Bank
            Texts.Add(UILocaleKeys.UI_BANK_DEPOSIT.ToString(), "Deposit");
            Texts.Add(UILocaleKeys.UI_BANK_DEPOSIT_DESCRIPTION.ToString(), "Enter amount of gold");
            Texts.Add(UILocaleKeys.UI_BANK_WITHDRAW.ToString(), "Withdraw");
            Texts.Add(UILocaleKeys.UI_BANK_WITHDRAW_DESCRIPTION.ToString(), "Enter amount of gold");
            // UI Cash Packages
            Texts.Add(UILocaleKeys.UI_CANNOT_GET_CASH_PACKAGE_INFO.ToString(), "Cannot retrieve cash package info");
            // UI Cash Shop
            Texts.Add(UILocaleKeys.UI_CANNOT_GET_CASH_SHOP_INFO.ToString(), "Cannot retrieve cash shop info");
            Texts.Add(UILocaleKeys.UI_CASH_SHOP_BUY_SUCCESS.ToString(), "Success, let's check your inventory");
            // UI Dealing
            Texts.Add(UILocaleKeys.UI_OFFER_GOLD.ToString(), "Offer Gold");
            Texts.Add(UILocaleKeys.UI_OFFER_GOLD_DESCRIPTION.ToString(), "Enter amount of gold");
            // UI Npc Sell Item
            Texts.Add(UILocaleKeys.UI_BUY_ITEM.ToString(), "Buy Item");
            Texts.Add(UILocaleKeys.UI_BUY_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            // UI Party
            Texts.Add(UILocaleKeys.UI_PARTY_CHANGE_LEADER.ToString(), "Change Leader");
            Texts.Add(UILocaleKeys.UI_PARTY_CHANGE_LEADER_DESCRIPTION.ToString(), "You sure you want to promote {0} to party leader?");
            Texts.Add(UILocaleKeys.UI_PARTY_KICK_MEMBER.ToString(), "Kick Member");
            Texts.Add(UILocaleKeys.UI_PARTY_KICK_MEMBER_DESCRIPTION.ToString(), "You sure you want to kick {0} from party?");
            Texts.Add(UILocaleKeys.UI_PARTY_LEAVE.ToString(), "Leave Party");
            Texts.Add(UILocaleKeys.UI_PARTY_LEAVE_DESCRIPTION.ToString(), "You sure you want to leave party?");
            // UI Guild
            Texts.Add(UILocaleKeys.UI_GUILD_CHANGE_LEADER.ToString(), "Change Leader");
            Texts.Add(UILocaleKeys.UI_GUILD_CHANGE_LEADER_DESCRIPTION.ToString(), "You sure you want to promote {0} to guild leader?");
            Texts.Add(UILocaleKeys.UI_GUILD_KICK_MEMBER.ToString(), "Kick Member");
            Texts.Add(UILocaleKeys.UI_GUILD_KICK_MEMBER_DESCRIPTION.ToString(), "You sure you want to kick {0} from guild?");
            Texts.Add(UILocaleKeys.UI_GUILD_LEAVE.ToString(), "Leave Guild");
            Texts.Add(UILocaleKeys.UI_GUILD_LEAVE_DESCRIPTION.ToString(), "You sure you want to leave guild?");
            // UI Guild Role
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_CAN_INVITE.ToString(), "Can invite");
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_CANNOT_INVITE.ToString(), "Cannot invite");
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_CAN_KICK.ToString(), "Can kick");
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_CANNOT_KICK.ToString(), "Cannot kick");
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_SHARE_EXP.ToString(), "Share Exp");
            // UI Guild Role Setting
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_NAME_IS_EMPTY.ToString(), "Role name must not empty");
            Texts.Add(UILocaleKeys.UI_GUILD_ROLE_SHARE_EXP_NOT_NUMBER.ToString(), "Share exp percentage must be number");
            // UI Guild Member Role Setting
            Texts.Add(UILocaleKeys.UI_INVALID_GUILD_ROLE.ToString(), "Invalid role");
            // UI Scene Global
            Texts.Add(UILocaleKeys.UI_KICKED_FROM_SERVER.ToString(), "You have been kicked from server");
            Texts.Add(UILocaleKeys.UI_CONNECTION_FAILED.ToString(), "Cannot connect to the server");
            Texts.Add(UILocaleKeys.UI_CONNECTION_REJECTED.ToString(), "Connection rejected by server");
            Texts.Add(UILocaleKeys.UI_REMOTE_CONNECTION_CLOSE.ToString(), "Server has been closed");
            Texts.Add(UILocaleKeys.UI_INVALID_PROTOCOL.ToString(), "Invalid protocol");
            Texts.Add(UILocaleKeys.UI_HOST_UNREACHABLE.ToString(), "Host unreachable");
            Texts.Add(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString(), "Connection timeout");
        }
    }

    [System.Serializable]
    public class Language
    {
        public string languageKey;
        public List<LanguageData> dataList = new List<LanguageData>();

        public bool ContainKey(string key)
        {
            foreach (LanguageData entry in dataList)
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
        [TextArea]
        public string value;
    }
}
