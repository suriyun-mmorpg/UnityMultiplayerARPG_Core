using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum UITextKeys : ushort
    {
        UI_CUSTOM,
        // UI Generic Title
        UI_LABEL_DISCONNECTED,
        UI_LABEL_SUCCESS,
        UI_LABEL_ERROR,
        UI_LABEL_NONE,
        // Error - Generic Error
        UI_ERROR_KICKED_FROM_SERVER,
        UI_ERROR_CONNECTION_FAILED,
        UI_ERROR_CONNECTION_REJECTED,
        UI_ERROR_REMOTE_CONNECTION_CLOSE,
        UI_ERROR_INVALID_PROTOCOL,
        UI_ERROR_HOST_UNREACHABLE,
        UI_ERROR_CONNECTION_TIMEOUT,
        UI_ERROR_USER_NOT_FOUND,
        UI_ERROR_ITEM_NOT_FOUND,
        UI_ERROR_NOT_ENOUGH_GOLD,
        UI_ERROR_NOT_ENOUGH_CASH,
        UI_ERROR_NOT_LOGGED_IN,
        UI_ERROR_INVALID_DATA,
        UI_ERROR_INVALID_CHARACTER_DATA,
        UI_ERROR_USERNAME_IS_EMPTY,
        UI_ERROR_PASSWORD_IS_EMPTY,
        UI_ERROR_CANNOT_CARRY_ALL_REWARDS,
        // Error - UI Login
        UI_ERROR_INVALID_USERNAME_OR_PASSWORD,
        UI_ERROR_ALREADY_LOGGED_IN,
        // Error - UI Register
        UI_ERROR_INVALID_CONFIRM_PASSWORD,
        UI_ERROR_USERNAME_TOO_SHORT,
        UI_ERROR_USERNAME_TOO_LONG,
        UI_ERROR_PASSWORD_TOO_SHORT,
        UI_ERROR_USERNAME_EXISTED,
        // Error - UI Character List
        UI_ERROR_NO_CHOSEN_CHARACTER_TO_START,
        UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE,
        UI_ERROR_ALREADY_SELECT_CHARACTER,
        UI_ERROR_MAP_SERVER_NOT_READY,
        // Error - UI Character Create
        UI_ERROR_CHARACTER_NAME_TOO_SHORT,
        UI_ERROR_CHARACTER_NAME_TOO_LONG,
        UI_ERROR_CHARACTER_NAME_EXISTED,
        // Error - UI Cash Packages
        UI_ERROR_CANNOT_GET_CASH_PACKAGE_INFO,
        // Error - UI Cash Shop
        UI_ERROR_CANNOT_GET_CASH_SHOP_INFO,
        // Error - UI Guild Role Setting
        UI_ERROR_GUILD_ROLE_NAME_IS_EMPTY,
        UI_ERROR_GUILD_ROLE_SHARE_EXP_NOT_NUMBER,
        // Error - UI Guild Member Role Setting
        UI_ERROR_INVALID_GUILD_ROLE,
        // Success - UI Cash Shop
        UI_SUCCESS_CASH_SHOP_BUY,
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
        // UI Bank
        UI_BANK_DEPOSIT,
        UI_BANK_DEPOSIT_DESCRIPTION,
        UI_BANK_WITHDRAW,
        UI_BANK_WITHDRAW_DESCRIPTION,
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
        // Friend
        UI_FRIEND_ADD,
        UI_FRIEND_ADD_DESCRIPTION,
        UI_FRIEND_REMOVE,
        UI_FRIEND_REMOVE_DESCRIPTION,
        // Item Amount Title
        UI_LABEL_UNLIMIT_WEIGHT,
        UI_LABEL_UNLIMIT_SLOT,
        // Item Type Title
        UI_ITEM_TYPE_JUNK,
        UI_ITEM_TYPE_SHIELD,
        UI_ITEM_TYPE_POTION,
        UI_ITEM_TYPE_AMMO,
        UI_ITEM_TYPE_BUILDING,
        UI_ITEM_TYPE_PET,
        UI_ITEM_TYPE_SOCKET_ENHANCER,
        UI_ITEM_TYPE_MOUNT,
        UI_ITEM_TYPE_SKILL,
        // Skill Type Titles
        UI_SKILL_TYPE_ACTIVE,
        UI_SKILL_TYPE_PASSIVE,
        UI_SKILL_TYPE_CRAFT_ITEM,
    }

    public enum UIFormatKeys : ushort
    {
        UI_CUSTOM,
        // Format - Generic
        /// <summary>
        /// Format => {0} = {Value}
        /// </summary>
        UI_FORMAT_SIMPLE = 5,
        /// <summary>
        /// Format => {0} = {Value}
        /// </summary>
        UI_FORMAT_SIMPLE_PERCENTAGE,
        /// <summary>
        /// Format => {0} = {Min Value}, {1} = {Max Value}
        /// </summary>
        UI_FORMAT_SIMPLE_MIN_TO_MAX,
        /// <summary>
        /// Format => {0} = {Min Value}, {1} = {Max Value}
        /// </summary>
        UI_FORMAT_SIMPLE_MIN_BY_MAX,
        /// <summary>
        /// Format => {0} = {Level}
        /// </summary>
        UI_FORMAT_LEVEL,
        /// <summary>
        /// Format => {0} = {Current Exp}, {1} = {Exp To Level Up}
        /// </summary>
        UI_FORMAT_CURRENT_EXP,
        /// <summary>
        /// Format => {0} = {Stat Points}
        /// </summary>
        UI_FORMAT_STAT_POINTS,
        /// <summary>
        /// Format => {0} = {Skill Points}
        /// </summary>
        UI_FORMAT_SKILL_POINTS,
        /// <summary>
        /// Format => {0} = {Current Hp}, {1} = {Max Hp}
        /// </summary>
        UI_FORMAT_CURRENT_HP,
        /// <summary>
        /// Format => {0} = {Current Mp}, {1} = {Max Mp}
        /// </summary>
        UI_FORMAT_CURRENT_MP,
        /// <summary>
        /// Format => {0} = {Current Stamina}, {1} = {Max Stamina}
        /// </summary>
        UI_FORMAT_CURRENT_STAMINA,
        /// <summary>
        /// Format => {0} = {Current Food}, {1} = {Max Food}
        /// </summary>
        UI_FORMAT_CURRENT_FOOD,
        /// <summary>
        /// Format => {0} = {Current Water}, {1} = {Max Water}
        /// </summary>
        UI_FORMAT_CURRENT_WATER,
        /// <summary>
        /// Format => {0} = {Current Weight}, {1} = {Weight Limit}
        /// </summary>
        UI_FORMAT_CURRENT_WEIGHT,
        /// <summary>
        /// Format => {0} = {Current Slot}, {1} = {Slot Limit}
        /// </summary>
        UI_FORMAT_CURRENT_SLOT,
        // Format - Character Stats
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_HP,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_MP,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_STAMINA,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_FOOD,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_WATER,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_ACCURACY = 26,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_EVASION,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_CRITICAL_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_CRITICAL_DAMAGE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_BLOCK_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_BLOCK_DAMAGE_RATE,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_MOVE_SPEED,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_ATTACK_SPEED,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_WEIGHT,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_SLOT,
        /// <summary>
        /// Format => {0} = {Gold Amount}
        /// </summary>
        UI_FORMAT_GOLD = 38,
        /// <summary>
        /// Format => {0} = {Cash Amount}
        /// </summary>
        UI_FORMAT_CASH,
        /// <summary>
        /// Format => {0} = {Sell Price}
        /// </summary>
        UI_FORMAT_SELL_PRICE,
        /// <summary>
        /// Format => {0} = {Character Level}
        /// </summary>
        UI_FORMAT_REQUIRE_LEVEL,
        /// <summary>
        /// Format => {0} = {Character Class}
        /// </summary>
        UI_FORMAT_REQUIRE_CLASS,
        /// <summary>
        /// Format => {0} = {List Of Weapon Type}
        /// </summary>
        UI_FORMAT_AVAILABLE_WEAPONS,
        /// <summary>
        /// Format => {0} = {Consume Mp}
        /// </summary>
        UI_FORMAT_CONSUME_MP,
        // Format - Skill
        /// <summary>
        /// Format => {0} = {Skill Cooldown Duration}
        /// </summary>
        UI_FORMAT_SKILL_COOLDOWN_DURATION,
        /// <summary>
        /// Format => {0} = {Skill Type}
        /// </summary>
        UI_FORMAT_SKILL_TYPE,
        // Format - Buff
        /// <summary>
        /// Format => {0} = {Buff Duration}
        /// </summary>
        UI_FORMAT_BUFF_DURATION = 50,
        /// <summary>
        /// Format => {0} = {Buff Recovery Hp}
        /// </summary>
        UI_FORMAT_BUFF_RECOVERY_HP,
        /// <summary>
        /// Format => {0} = {Buff Recovery Mp}
        /// </summary>
        UI_FORMAT_BUFF_RECOVERY_MP,
        /// <summary>
        /// Format => {0} = {Buff Recovery Stamina}
        /// </summary>
        UI_FORMAT_BUFF_RECOVERY_STAMINA,
        /// <summary>
        /// Format => {0} = {Buff Recovery Food}
        /// </summary>
        UI_FORMAT_BUFF_RECOVERY_FOOD,
        /// <summary>
        /// Format => {0} = {Buff Recovery Water}
        /// </summary>
        UI_FORMAT_BUFF_RECOVERY_WATER,
        // Format -  Item
        /// <summary>
        /// Format => {0} = {Level - 1}
        /// </summary>
        UI_FORMAT_ITEM_REFINE_LEVEL,
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Level - 1}
        /// </summary>
        UI_FORMAT_ITEM_TITLE_WITH_REFINE_LEVEL,
        /// <summary>
        /// Format => {0} = {Item Type}
        /// </summary>
        UI_FORMAT_ITEM_TYPE,
        /// <summary>
        /// Format => {0} = {Item Rarity}
        /// </summary>
        UI_FORMAT_ITEM_RARITY = 66,
        /// <summary>
        /// Format => {0} = {Item Current Amount}, {1} = {Item Max Amount}
        /// </summary>
        UI_FORMAT_ITEM_STACK,
        /// <summary>
        /// Format => {0} = {Item Current Durability}, {1} = {Item Max Durability}
        /// </summary>
        UI_FORMAT_ITEM_DURABILITY,
        // Format -  Social
        /// <summary>
        /// Format => {0} = {Character Name}
        /// </summary>
        UI_FORMAT_SOCIAL_LEADER,
        /// <summary>
        /// Format => {0} = {Current Amount}, {1} = {Max Amount}
        /// </summary>
        UI_FORMAT_SOCIAL_MEMBER_AMOUNT,
        /// <summary>
        /// Format => {0} = {Current Amount}
        /// </summary>
        UI_FORMAT_SOCIAL_MEMBER_AMOUNT_NO_LIMIT,
        /// <summary>
        /// Format => {0} = {Share Exp}
        /// </summary>
        UI_FORMAT_SHARE_EXP_PERCENTAGE,
        /// <summary>
        /// Format => {0} = {Exp Amount}
        /// </summary>
        UI_FORMAT_REWARD_EXP,
        /// <summary>
        /// Format => {0} = {Gold Amount}
        /// </summary>
        UI_FORMAT_REWARD_GOLD,
        /// <summary>
        /// Format => {0} = {Cash Amount}
        /// </summary>
        UI_FORMAT_REWARD_CASH,
        // Format - Attribute Amount
        /// <summary>
        /// Format => {0} = {Attribute Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_ATTRIBUTE,
        /// <summary>
        /// Format => {0} = {Attribute Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_ATTRIBUTE_NOT_ENOUGH,
        /// <summary>
        /// Format => {0} = {Attribute Title}, {1} = {Amount * 100}
        /// </summary>
        UI_FORMAT_ATTRIBUTE_AMOUNT,
        // Format - Resistance Amount
        /// <summary>
        /// Format => {0} = {Resistance Title}, {1} = {Amount * 100}
        /// </summary>
        UI_FORMAT_RESISTANCE_AMOUNT,
        // Format - Skill Level
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}
        /// </summary>
        UI_FORMAT_CURRENT_SKILL,
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}
        /// </summary>
        UI_FORMAT_CURRENT_SKILL_NOT_ENOUGH,
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Target Level}
        /// </summary>
        UI_FORMAT_SKILL_LEVEL,
        // Format - Item Amount
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_ITEM,
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_ITEM_NOT_ENOUGH,
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Target Amount}
        /// </summary>
        UI_FORMAT_ITEM_AMOUNT,
        // Format - Damage
        /// <summary>
        /// Format => {0} = {Min Damage}, {1} = {Max Damage}
        /// </summary>
        UI_FORMAT_DAMAGE_AMOUNT,
        /// <summary>
        /// Format => {0} = {Damage Element Title}, {1} = {Min Damage}, {2} = {Max Damage}
        /// </summary>
        UI_FORMAT_DAMAGE_WITH_ELEMENTAL,
        /// <summary>
        /// Format => {0} = {Infliction * 100}
        /// </summary>
        UI_FORMAT_DAMAGE_INFLICTION,
        /// <summary>
        /// Format => {0} = {Damage Element Title}, {1} => {Infliction * 100}
        /// </summary>
        UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL,
        // Format - Gold Amount
        /// <summary>
        /// Format => {0} = {Current Gold Amount}, {1} = {Target Amount}
        /// </summary>
        UI_FORMAT_REQUIRE_GOLD,
        /// <summary>
        /// Format => {0} = {Current Gold Amount}, {1} = {Target Amount}
        /// </summary>
        UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH,
        // Format - UI Equipment Set
        /// <summary>
        /// Format => {0} = {Set Title}, {1} = {List Of Effect}
        /// </summary>
        UI_FORMAT_EQUIPMENT_SET,
        /// <summary>
        /// Format => {0} = {Equip Amount}, {1} = {List Of Bonus}
        /// </summary>
        UI_FORMAT_EQUIPMENT_SET_APPLIED_EFFECT,
        /// <summary>
        /// Format => {0} = {Equip Amount}, {1} = {List Of Bonus}
        /// </summary>
        UI_FORMAT_EQUIPMENT_SET_UNAPPLIED_EFFECT,
        // Format - UI Equipment Socket
        /// <summary>
        /// Format => {0} = {Socket Index}, {1} = {Item Title}, {2} = {List Of Bonus}
        /// </summary>
        UI_FORMAT_EQUIPMENT_SOCKET_FILLED,
        /// <summary>
        /// Format => {0} = {Socket Index}
        /// </summary>
        UI_FORMAT_EQUIPMENT_SOCKET_EMPTY,
        // Refine Item
        /// <summary>
        /// Format => {0} = {Rate * 100}
        /// </summary>
        UI_FORMAT_REFINE_SUCCESS_RATE,
        /// <summary>
        /// Format => {0} = {Refining Level}
        /// </summary>
        UI_FORMAT_REFINING_LEVEL,
        // Format - Guild Bonus
        UI_FORMAT_INCREASE_MAX_MEMBER,
        UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE,
        UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE,
        UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE,
        // Format - UI Character Quest
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_QUEST_TITLE_ON_GOING,
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_QUEST_TITLE_TASKS_COMPLETE,
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_QUEST_TITLE_COMPLETE,
        // Format - UI Quest Task
        /// <summary>
        /// Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}
        /// </summary>
        UI_FORMAT_QUEST_TASK_KILL_MONSTER,
        /// <summary>
        /// Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}
        /// </summary>
        UI_FORMAT_QUEST_TASK_COLLECT_ITEM,
        /// <summary>
        /// Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}
        /// </summary>
        UI_FORMAT_QUEST_TASK_KILL_MONSTER_COMPLETE,
        /// <summary>
        /// Format => {0} = {Title}, {1} = {Progress}, {2} = {Amount}
        /// </summary>
        UI_FORMAT_QUEST_TASK_COLLECT_ITEM_COMPLETE,
        // UI Chat Message
        /// <summary>
        /// Format => {0} = {Character Name}, {1} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_LOCAL,
        /// <summary>
        /// Format => {0} = {Character Name}, {1} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_GLOBAL,
        /// <summary>
        /// Format => {0} = {Character Name}, {1} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_WHISPER,
        /// <summary>
        /// Format => {0} = {Character Name}, {1} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_PARTY,
        /// <summary>
        /// Format => {0} = {Character Name}, {1} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_GUILD,
        // Format - Armor Amount
        /// <summary>
        /// Format => {0} = {Damage Element Title}, {1} = {Target Amount}
        /// </summary>
        UI_FORMAT_ARMOR_AMOUNT = 197,
        // Format - Character Stats Rate
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_HP_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_MP_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_STAMINA_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_FOOD_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_WATER_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_ACCURACY_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_EVASION_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 10000}
        /// </summary>
        UI_FORMAT_CRITICAL_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 10000}
        /// </summary>
        UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 10000}
        /// </summary>
        UI_FORMAT_BLOCK_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 10000}
        /// </summary>
        UI_FORMAT_BLOCK_DAMAGE_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_MOVE_SPEED_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_ATTACK_SPEED_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_WEIGHT_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_SLOT_RATE,
        // Format - Attribute Amount Rate
        /// <summary>
        /// Format => {0} = {Attribute Title}, {1} = {Amount * 100}
        /// </summary>
        UI_FORMAT_ATTRIBUTE_RATE,
        // Format - Item Building
        /// <summary>
        /// Format => {0} = {Building Title}
        /// </summary>
        UI_FORMAT_ITEM_BUILDING,
        // Format - Item Pet
        /// <summary>
        /// Format => {0} = {Pet Title}
        /// </summary>
        UI_FORMAT_ITEM_PET,
        // Format - Item Mount
        /// <summary>
        /// Format => {0} = {Mount Title}
        /// </summary>
        UI_FORMAT_ITEM_MOUNT,
        // Format - Item Skill
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Skill Level}
        /// </summary>
        UI_FORMAT_ITEM_SKILL,
        // Format - Skill Summon
        /// <summary>
        /// Format => {0} = {Monster Title}, {1} = {Monster Level}, {2} = {Amount}, {3} = {Max Stack}, {4} = {Duration}
        /// </summary>
        UI_FORMAT_SKILL_SUMMON,
        // Format - Skill Mount
        /// <summary>
        /// Format => {0} = {Mount Title}
        /// </summary>
        UI_FORMAT_SKILL_MOUNT
    }

    public static class DefaultLocale
    {
        public static readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        static DefaultLocale()
        {
            Texts.Add(GameMessage.Type.ServiceNotAvailable.ToString(), "Service not available");
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
            Texts.Add(GameMessage.Type.AnotherCharacterCannotCarryAnymore.ToString(), "Another character cannot carry anymore items");
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
            // Combatant
            Texts.Add(GameMessage.Type.NoAmmo.ToString(), "No Ammo");
            Texts.Add(GameMessage.Type.NotEnoughMp.ToString(), "Have not enough Mp");
            // Guild Name
            Texts.Add(GameMessage.Type.TooShortGuildName.ToString(), "Guild name is too short");
            Texts.Add(GameMessage.Type.TooLongGuildName.ToString(), "Guild name is too long");
            Texts.Add(GameMessage.Type.ExistedGuildName.ToString(), "Guild name is already existed");
            // Guild Role Name
            Texts.Add(GameMessage.Type.TooShortGuildRoleName.ToString(), "Guild role name is too short");
            Texts.Add(GameMessage.Type.TooLongGuildRoleName.ToString(), "Guild role name is too long");
            // Guild Message
            Texts.Add(GameMessage.Type.TooLongGuildMessage.ToString(), "Guild message is too long");
            // Skill
            Texts.Add(GameMessage.Type.SkillLevelIsZero.ToString(), "Skill not trained yet");
            Texts.Add(GameMessage.Type.CannotUseSkillByCurrentWeapon.ToString(), "Cannot use skill by current weapon");
            Texts.Add(GameMessage.Type.SkillIsCoolingDown.ToString(), "Skill is cooling down");
            Texts.Add(GameMessage.Type.SkillIsNotLearned.ToString(), "Skill is not learned");
            Texts.Add(GameMessage.Type.NoSkillTarget.ToString(), "No target");
            Texts.Add(GameMessage.Type.NotEnoughLevel.ToString(), "Not enough level");
            Texts.Add(GameMessage.Type.NotMatchCharacterClass.ToString(), "Not match character class");
            Texts.Add(GameMessage.Type.NotEnoughAttributeAmounts.ToString(), "Not enough attribute amounts");
            Texts.Add(GameMessage.Type.NotEnoughSkillLevels.ToString(), "Not enough skill levels");
            Texts.Add(GameMessage.Type.NotEnoughStatPoint.ToString(), "Not enough stat point");
            Texts.Add(GameMessage.Type.NotEnoughSkillPoint.ToString(), "Not enough skill point");
            Texts.Add(GameMessage.Type.AttributeReachedMaxAmount.ToString(), "Attribute reached max amount");
            Texts.Add(GameMessage.Type.SkillReachedMaxLevel.ToString(), "Skill reached max level");

            // UI Generic Title
            Texts.Add(UITextKeys.UI_LABEL_DISCONNECTED.ToString(), "Disconnected");
            Texts.Add(UITextKeys.UI_LABEL_SUCCESS.ToString(), "Success");
            Texts.Add(UITextKeys.UI_LABEL_ERROR.ToString(), "Error");
            Texts.Add(UITextKeys.UI_LABEL_NONE.ToString(), "None");
            // Format - Generic
            Texts.Add(UIFormatKeys.UI_FORMAT_SIMPLE.ToString(), "{0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SIMPLE_PERCENTAGE.ToString(), "{0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_SIMPLE_MIN_TO_MAX.ToString(), "{0}~{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SIMPLE_MIN_BY_MAX.ToString(), "{0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_LEVEL.ToString(), "Lv: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_EXP.ToString(), "Exp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAT_POINTS.ToString(), "Stat Points: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_POINTS.ToString(), "Skill Points: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_HP.ToString(), "Hp: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_MP.ToString(), "Mp: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_STAMINA.ToString(), "Stamina: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_FOOD.ToString(), "Food: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_WATER.ToString(), "Water: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_WEIGHT.ToString(), "Weight: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_SLOT.ToString(), "Slot: {0}/{1}");
            Texts.Add(UITextKeys.UI_LABEL_UNLIMIT_WEIGHT.ToString(), "Unlimit Weight");
            Texts.Add(UITextKeys.UI_LABEL_UNLIMIT_SLOT.ToString(), "Unlimit Slot");
            Texts.Add(UIFormatKeys.UI_FORMAT_HP.ToString(), "Hp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP.ToString(), "Mp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA.ToString(), "Stamina: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_FOOD.ToString(), "Food: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_WATER.ToString(), "Water: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ACCURACY.ToString(), "Accuracy: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_EVASION.ToString(), "Evasion: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CRITICAL_RATE.ToString(), "Cri. Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE.ToString(), "Cri. Damage: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_BLOCK_RATE.ToString(), "Block Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE.ToString(), "Block Damage: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MOVE_SPEED.ToString(), "Move Speed: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ATTACK_SPEED.ToString(), "Attack Speed: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_WEIGHT.ToString(), "Weight: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SLOT.ToString(), "Slot: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_GOLD.ToString(), "Gold: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CASH.ToString(), "Cash: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SELL_PRICE.ToString(), "Sell Price: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL.ToString(), "Require Level: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_CLASS.ToString(), "Require Class: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS.ToString(), "Available Weapons: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CONSUME_MP.ToString(), "Consume Mp: {0}");
            // Format - Skill
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_COOLDOWN_DURATION.ToString(), "Cooldown: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_TYPE.ToString(), "Skill Type: {0}");
            Texts.Add(UITextKeys.UI_SKILL_TYPE_ACTIVE.ToString(), "Active");
            Texts.Add(UITextKeys.UI_SKILL_TYPE_PASSIVE.ToString(), "Passive");
            Texts.Add(UITextKeys.UI_SKILL_TYPE_CRAFT_ITEM.ToString(), "Craft Item");
            // Format - Buff
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_DURATION.ToString(), "Duration: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_HP.ToString(), "Recovery Hp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_MP.ToString(), "Recovery Mp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_STAMINA.ToString(), "Recovery Stamina: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_FOOD.ToString(), "Recovery Food: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_BUFF_RECOVERY_WATER.ToString(), "Recovery Water: {0}");
            // Format - Item
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_REFINE_LEVEL.ToString(), "+{0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_TITLE_WITH_REFINE_LEVEL.ToString(), "{0} +{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_TYPE.ToString(), "Item Type: {0}");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_JUNK.ToString(), "Junk");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_SHIELD.ToString(), "Shield");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_POTION.ToString(), "Potion");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_AMMO.ToString(), "Ammo");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_BUILDING.ToString(), "Building");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_PET.ToString(), "Pet");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString(), "Socket Enhancer");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_MOUNT.ToString(), "Mount");
            Texts.Add(UITextKeys.UI_ITEM_TYPE_SKILL.ToString(), "Skill");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_RARITY.ToString(), "Rarity: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_STACK.ToString(), "{0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_DURABILITY.ToString(), "Durability: {0}");
            // Format - Social
            Texts.Add(UIFormatKeys.UI_FORMAT_SOCIAL_LEADER.ToString(), "Leader: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SOCIAL_MEMBER_AMOUNT.ToString(), "Member: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SOCIAL_MEMBER_AMOUNT_NO_LIMIT.ToString(), "Member: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SHARE_EXP_PERCENTAGE.ToString(), "Share Exp: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_REWARD_EXP.ToString(), "Reward Exp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REWARD_GOLD.ToString(), "Reward Gold: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REWARD_CASH.ToString(), "Reward Cash: {0}");
            // Format - Attribute Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE.ToString(), "{0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_ATTRIBUTE_NOT_ENOUGH.ToString(), "{0}: <color=red>{1}/{2}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_ATTRIBUTE_AMOUNT.ToString(), "{0}: {1}");
            // Format - Resistance Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_RESISTANCE_AMOUNT.ToString(), "{0} Resistance: {1}%");
            // Format - Armor Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_ARMOR_AMOUNT.ToString(), "{0} Armor: {1}");
            // Format - Skill Level
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_SKILL.ToString(), "{0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_SKILL_NOT_ENOUGH.ToString(), "{0}: <color=red>{1}/{2}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_LEVEL.ToString(), "{0}: {1}");
            // Format - Item Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_ITEM.ToString(), "{0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_ITEM_NOT_ENOUGH.ToString(), "{0}: <color=red>{1}/{2}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_AMOUNT.ToString(), "{0}: {1}");
            // Format - Damage
            Texts.Add(UIFormatKeys.UI_FORMAT_DAMAGE_AMOUNT.ToString(), "{0}~{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_DAMAGE_WITH_ELEMENTAL.ToString(), "{0} Damage: {1}~{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_DAMAGE_INFLICTION.ToString(), "Inflict {0}% damage");
            Texts.Add(UIFormatKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString(), "Inflict {1}% as {0} damage");
            // Format - Gold Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD.ToString(), "Gold: {0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH.ToString(), "Gold: <color=red>{0}/{1}</color>");
            // Format - UI Equipment Set
            Texts.Add(UIFormatKeys.UI_FORMAT_EQUIPMENT_SET.ToString(), "<color=#ffa500ff>{0}</color>\n{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_EQUIPMENT_SET_APPLIED_EFFECT.ToString(), "<color=#ffa500ff>({0}) {1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_EQUIPMENT_SET_UNAPPLIED_EFFECT.ToString(), "({0}) {1}");
            // Format - UI Equipment Socket
            Texts.Add(UIFormatKeys.UI_FORMAT_EQUIPMENT_SOCKET_FILLED.ToString(), "<color=#800080ff>({0}) - {1}\n{2}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_EQUIPMENT_SOCKET_EMPTY.ToString(), "<color=#800080ff>({0}) - Empty</color>");
            // Format - Refine Item
            Texts.Add(UIFormatKeys.UI_FORMAT_REFINE_SUCCESS_RATE.ToString(), "Success Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_REFINING_LEVEL.ToString(), "Refining Level: +{0}");
            // Format - Guild Bonus
            Texts.Add(UIFormatKeys.UI_FORMAT_INCREASE_MAX_MEMBER.ToString(), "Max Member +{0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_INCREASE_EXP_GAIN_PERCENTAGE.ToString(), "Exp Gain +{0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_INCREASE_GOLD_GAIN_PERCENTAGE.ToString(), "Gold Gain +{0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_INCREASE_SHARE_EXP_GAIN_PERCENTAGE.ToString(), "Party Share Exp +{0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_INCREASE_SHARE_GOLD_GAIN_PERCENTAGE.ToString(), "Party Share Gold +{0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_DECREASE_EXP_PENALTY_PERCENTAGE.ToString(), "Exp Penalty -{0}%");
            // Format - UI Character Quest
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TITLE_ON_GOING.ToString(), "{0} (Ongoing)");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TITLE_TASKS_COMPLETE.ToString(), "{0} (Task Completed)");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TITLE_COMPLETE.ToString(), "{0} (Completed)");
            // Format - UI Quest Task
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_KILL_MONSTER.ToString(), "Kills {0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_COLLECT_ITEM.ToString(), "Collects {0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_KILL_MONSTER_COMPLETE.ToString(), "Kills {0}: Complete");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_COLLECT_ITEM_COMPLETE.ToString(), "Collects {0}: Complete");
            // Format - UI Chat Message
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_LOCAL.ToString(), "<color=white>(LOCAL) {0}: {1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_GLOBAL.ToString(), "<color=white>(GLOBAL) {0}: {1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_WHISPER.ToString(), "<color=green>(WHISPER) {0}: {1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_PARTY.ToString(), "<color=cyan>(PARTY) {0}: {1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_GUILD.ToString(), "<color=blue>(GUILD) {0}: {1}</color>");
            // Error - Generic Error
            Texts.Add(UITextKeys.UI_ERROR_KICKED_FROM_SERVER.ToString(), "You have been kicked from server");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_FAILED.ToString(), "Cannot connect to the server");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_REJECTED.ToString(), "Connection rejected by server");
            Texts.Add(UITextKeys.UI_ERROR_REMOTE_CONNECTION_CLOSE.ToString(), "Server has been closed");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_PROTOCOL.ToString(), "Invalid protocol");
            Texts.Add(UITextKeys.UI_ERROR_HOST_UNREACHABLE.ToString(), "Host unreachable");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString(), "Connection timeout");
            Texts.Add(UITextKeys.UI_ERROR_USER_NOT_FOUND.ToString(), "User not found");
            Texts.Add(UITextKeys.UI_ERROR_ITEM_NOT_FOUND.ToString(), "Item not found");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD.ToString(), "Not enough gold");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_CASH.ToString(), "Not enough cash");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_DATA.ToString(), "Invalid data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA.ToString(), "Invalid character data");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_IS_EMPTY.ToString(), "Username is empty");
            Texts.Add(UITextKeys.UI_ERROR_PASSWORD_IS_EMPTY.ToString(), "Password is empty");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_CARRY_ALL_REWARDS.ToString(), "Cannot carry all rewards");
            // Error - UI Login
            Texts.Add(UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD.ToString(), "Invalid username or password");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_LOGGED_IN.ToString(), "User already logged in");
            // Error - UI Register
            Texts.Add(UITextKeys.UI_ERROR_INVALID_CONFIRM_PASSWORD.ToString(), "Invalid confirm password");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_TOO_SHORT.ToString(), "Username is too short");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_TOO_LONG.ToString(), "Username is too long");
            Texts.Add(UITextKeys.UI_ERROR_PASSWORD_TOO_SHORT.ToString(), "Password is too short");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_EXISTED.ToString(), "Username is already existed");
            // Error - UI Character List
            Texts.Add(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_START.ToString(), "Please choose character to start game");
            Texts.Add(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE.ToString(), "Please choose character to delete");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_SELECT_CHARACTER.ToString(), "Already select character");
            Texts.Add(UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY.ToString(), "Map server is not ready");
            // Error - UI Character Create
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT.ToString(), "Character name is too short");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG.ToString(), "Character name is too long");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NAME_EXISTED.ToString(), "Character name is already existed");
            // Error - UI Cash Packages
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_GET_CASH_PACKAGE_INFO.ToString(), "Cannot retrieve cash package info");
            // Error - UI Cash Shop
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_GET_CASH_SHOP_INFO.ToString(), "Cannot retrieve cash shop info");
            // Error - UI Guild Role Setting
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_NAME_IS_EMPTY.ToString(), "Role name must not empty");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_SHARE_EXP_NOT_NUMBER.ToString(), "Share exp percentage must be number");
            // Error - UI Guild Member Role Setting
            Texts.Add(UITextKeys.UI_ERROR_INVALID_GUILD_ROLE.ToString(), "Invalid role");
            // Success - UI Cash Shop
            Texts.Add(UITextKeys.UI_SUCCESS_CASH_SHOP_BUY.ToString(), "Success, let's check your inventory");
            // UI Character Item
            Texts.Add(UITextKeys.UI_DROP_ITEM.ToString(), "Drop Item");
            Texts.Add(UITextKeys.UI_DROP_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_SELL_ITEM.ToString(), "Sell Item");
            Texts.Add(UITextKeys.UI_SELL_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_OFFER_ITEM.ToString(), "Offer Item");
            Texts.Add(UITextKeys.UI_OFFER_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_TO_STORAGE.ToString(), "Move To Storage");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE.ToString(), "Move From Storage");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            // UI Bank
            Texts.Add(UITextKeys.UI_BANK_DEPOSIT.ToString(), "Deposit");
            Texts.Add(UITextKeys.UI_BANK_DEPOSIT_DESCRIPTION.ToString(), "Enter amount of gold");
            Texts.Add(UITextKeys.UI_BANK_WITHDRAW.ToString(), "Withdraw");
            Texts.Add(UITextKeys.UI_BANK_WITHDRAW_DESCRIPTION.ToString(), "Enter amount of gold");
            // UI Dealing
            Texts.Add(UITextKeys.UI_OFFER_GOLD.ToString(), "Offer Gold");
            Texts.Add(UITextKeys.UI_OFFER_GOLD_DESCRIPTION.ToString(), "Enter amount of gold");
            // UI Npc Sell Item
            Texts.Add(UITextKeys.UI_BUY_ITEM.ToString(), "Buy Item");
            Texts.Add(UITextKeys.UI_BUY_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            // UI Party
            Texts.Add(UITextKeys.UI_PARTY_CHANGE_LEADER.ToString(), "Change Leader");
            Texts.Add(UITextKeys.UI_PARTY_CHANGE_LEADER_DESCRIPTION.ToString(), "You sure you want to promote {0} to party leader?");
            Texts.Add(UITextKeys.UI_PARTY_KICK_MEMBER.ToString(), "Kick Member");
            Texts.Add(UITextKeys.UI_PARTY_KICK_MEMBER_DESCRIPTION.ToString(), "You sure you want to kick {0} from party?");
            Texts.Add(UITextKeys.UI_PARTY_LEAVE.ToString(), "Leave Party");
            Texts.Add(UITextKeys.UI_PARTY_LEAVE_DESCRIPTION.ToString(), "You sure you want to leave party?");
            // UI Guild
            Texts.Add(UITextKeys.UI_GUILD_CHANGE_LEADER.ToString(), "Change Leader");
            Texts.Add(UITextKeys.UI_GUILD_CHANGE_LEADER_DESCRIPTION.ToString(), "You sure you want to promote {0} to guild leader?");
            Texts.Add(UITextKeys.UI_GUILD_KICK_MEMBER.ToString(), "Kick Member");
            Texts.Add(UITextKeys.UI_GUILD_KICK_MEMBER_DESCRIPTION.ToString(), "You sure you want to kick {0} from guild?");
            Texts.Add(UITextKeys.UI_GUILD_LEAVE.ToString(), "Leave Guild");
            Texts.Add(UITextKeys.UI_GUILD_LEAVE_DESCRIPTION.ToString(), "You sure you want to leave guild?");
            // UI Guild Role
            Texts.Add(UITextKeys.UI_GUILD_ROLE_CAN_INVITE.ToString(), "Can invite");
            Texts.Add(UITextKeys.UI_GUILD_ROLE_CANNOT_INVITE.ToString(), "Cannot invite");
            Texts.Add(UITextKeys.UI_GUILD_ROLE_CAN_KICK.ToString(), "Can kick");
            Texts.Add(UITextKeys.UI_GUILD_ROLE_CANNOT_KICK.ToString(), "Cannot kick");
            // UI Friend
            Texts.Add(UITextKeys.UI_FRIEND_ADD.ToString(), "Add Friend");
            Texts.Add(UITextKeys.UI_FRIEND_ADD_DESCRIPTION.ToString(), "You want to add {0} to friend list?");
            Texts.Add(UITextKeys.UI_FRIEND_REMOVE.ToString(), "Remove Friend");
            Texts.Add(UITextKeys.UI_FRIEND_REMOVE_DESCRIPTION.ToString(), "You want to remove {0} from friend list?");
            // Format - Character Stats Rate
            Texts.Add(UIFormatKeys.UI_FORMAT_HP_RATE.ToString(), "Hp: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP_RATE.ToString(), "Mp: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA_RATE.ToString(), "Stamina: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_FOOD_RATE.ToString(), "Food: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_WATER_RATE.ToString(), "Water: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_ACCURACY_RATE.ToString(), "Accuracy: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_EVASION_RATE.ToString(), "Evasion: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_CRITICAL_RATE_RATE.ToString(), "% of Cri. Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE.ToString(), "% of Cri. Damage: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_BLOCK_RATE_RATE.ToString(), "% of Block Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_BLOCK_DAMAGE_RATE_RATE.ToString(), "% of Block Damage: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MOVE_SPEED_RATE.ToString(), "Move Speed: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_ATTACK_SPEED_RATE.ToString(), "Attack Speed: {0}%");
            // Format - Attribute Amount Rate
            Texts.Add(UIFormatKeys.UI_FORMAT_ATTRIBUTE_RATE.ToString(), "{0}: {1}%");
            // Format - Item Building
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_BUILDING.ToString(), "Build {0}");
            // Format - Item Pet
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_PET.ToString(), "Summon {0}");
            // Format - Item Mount
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_MOUNT.ToString(), "Mount {0}");
            // Format - Item Skill
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_SKILL.ToString(), "Use Skill {0} Lv. {1}");
            // Format - Skill Summon
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_SUMMON.ToString(), "Summon {0} Lv. {1} x {2} (Max: {3}), {4} Secs.");
            // Format - Skill Mount
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_MOUNT.ToString(), "Mount {0}");
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
                if (string.IsNullOrEmpty(entry.key))
                    continue;
                if (entry.key.Equals(key))
                    return true;
            }
            return false;
        }

        public static string GetText(IEnumerable<LanguageData> langs, string defaultValue)
        {
            if (langs != null)
            {
                foreach (LanguageData entry in langs)
                {
                    if (string.IsNullOrEmpty(entry.key))
                        continue;
                    if (entry.key.Equals(LanguageManager.CurrentLanguageKey))
                        return entry.value;
                }
            }
            return defaultValue;
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
