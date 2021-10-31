using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum UITextKeys : ushort
    {
        NONE,
        // UI Generic Title
        UI_LABEL_DISCONNECTED,
        UI_LABEL_SUCCESS,
        UI_LABEL_ERROR,
        UI_LABEL_NONE,
        // Error - Generic Error
        UI_ERROR_UNKNOW,
        UI_ERROR_SERVICE_NOT_AVAILABLE,
        UI_ERROR_CONTENT_NOT_AVAILABLE,
        UI_ERROR_REQUEST_TIMEOUT,
        UI_ERROR_KICKED_FROM_SERVER,
        UI_ERROR_CONNECTION_FAILED,
        UI_ERROR_CONNECTION_REJECTED,
        UI_ERROR_REMOTE_CONNECTION_CLOSE,
        UI_ERROR_INVALID_PROTOCOL,
        UI_ERROR_HOST_UNREACHABLE,
        UI_ERROR_CONNECTION_TIMEOUT,
        UI_ERROR_INTERNAL_SERVER_ERROR,
        UI_ERROR_SERVER_NOT_FOUND,
        UI_ERROR_USER_NOT_FOUND,
        UI_ERROR_CHARACTER_NOT_FOUND,
        UI_ERROR_ITEM_NOT_FOUND,
        UI_ERROR_CASH_PACKAGE_NOT_FOUND,
        UI_ERROR_NOT_ENOUGH_GOLD,
        UI_ERROR_NOT_ENOUGH_CASH,
        UI_ERROR_NOT_ENOUGH_ITEMS,
        UI_ERROR_NOT_ENOUGH_STAT_POINT,
        UI_ERROR_NOT_ENOUGH_SKILL_POINT,
        UI_ERROR_NOT_LOGGED_IN,
        UI_ERROR_USERNAME_IS_EMPTY,
        UI_ERROR_PASSWORD_IS_EMPTY,
        UI_ERROR_WILL_OVERWHELMING,
        UI_ERROR_NOT_ABLE_TO_LOOT,
        // Error - Game Data
        UI_ERROR_INVALID_DATA,
        UI_ERROR_INVALID_CHARACTER_DATA,
        UI_ERROR_INVALID_ITEM_DATA,
        UI_ERROR_INVALID_ITEM_INDEX,
        UI_ERROR_INVALID_ENHANCER_ITEM_INDEX,
        UI_ERROR_ITEM_NOT_EQUIPMENT,
        UI_ERROR_INVALID_ATTRIBUTE_DATA,
        UI_ERROR_INVALID_SKILL_DATA,
        UI_ERROR_INVALID_GUILD_SKILL_DATA,
        // Error - UI Login
        UI_ERROR_INVALID_USERNAME_OR_PASSWORD,
        UI_ERROR_INVALID_USER_TOKEN,
        UI_ERROR_ALREADY_LOGGED_IN,
        UI_ERROR_ACCOUNT_LOGGED_IN_BY_OTHER,
        UI_ERROR_USER_BANNED,
        // Error - UI Register
        UI_ERROR_INVALID_CONFIRM_PASSWORD,
        UI_ERROR_USERNAME_TOO_SHORT,
        UI_ERROR_USERNAME_TOO_LONG,
        UI_ERROR_PASSWORD_TOO_SHORT,
        UI_ERROR_USERNAME_EXISTED,
        // Error - UI Lobby
        UI_ERROR_ALREADY_CONNECTED_TO_LOBBY,
        UI_ERROR_ALREADY_CONNECTED_TO_GAME,
        UI_ERROR_NO_SELECTED_REALM,
        UI_ERROR_NO_AVAILABLE_REALM,
        UI_ERROR_NO_AVAILABLE_LOBBY,
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
        // Error - UI Guild Name
        UI_ERROR_GUILD_NAME_TOO_SHORT,
        UI_ERROR_GUILD_NAME_TOO_LONG,
        UI_ERROR_GUILD_NAME_EXISTED,
        // Error - UI Guild Role Setting
        UI_ERROR_GUILD_ROLE_NAME_TOO_SHORT,
        UI_ERROR_GUILD_ROLE_NAME_TOO_LONG,
        UI_ERROR_GUILD_ROLE_SHARE_EXP_NOT_NUMBER,
        // Error - UI Guild Member Role Setting
        UI_ERROR_INVALID_GUILD_ROLE,
        // Error - UI Guild Message Setting
        UI_ERROR_GUILD_MESSAGE_TOO_LONG,
        // Error - Equip
        UI_ERROR_CANNOT_EQUIP,
        UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND,
        UI_ERROR_INVALID_EQUIP_POSITION_LEFT_HAND,
        UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND_OR_LEFT_HAND,
        UI_ERROR_INVALID_EQUIP_POSITION_ARMOR,
        // Error - Refine
        UI_ERROR_CANNOT_REFINE,
        UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL,
        UI_REFINE_SUCCESS,
        UI_REFINE_FAIL,
        // Enhance
        UI_ERROR_CANNOT_ENHANCE_SOCKET,
        UI_ERROR_NOT_ENOUGH_SOCKET_ENCHANER,
        UI_ERROR_NO_EMPTY_SOCKET,
        UI_ERROR_SOCKET_NOT_EMPTY,
        UI_ERROR_CANNOT_REMOVE_ENHANCER,
        UI_ERROR_NO_ENHANCER,
        // Repair
        UI_ERROR_CANNOT_REPAIR,
        UI_REPAIR_SUCCESS,
        // Dealing
        UI_ERROR_CHARACTER_IS_DEALING,
        UI_ERROR_CHARACTER_IS_TOO_FAR,
        UI_ERROR_CANNOT_ACCEPT_DEALING_REQUEST,
        UI_ERROR_DEALING_REQUEST_DECLINED,
        UI_ERROR_INVALID_DEALING_STATE,
        UI_ERROR_DEALING_CANCELED,
        UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING,
        // Party
        UI_ERROR_PARTY_NOT_FOUND,
        UI_ERROR_PARTY_INVITATION_NOT_FOUND,
        UI_PARTY_INVITATION_ACCEPTED,
        UI_PARTY_INVITATION_DECLINED,
        UI_ERROR_CANNOT_SEND_PARTY_INVITATION,
        UI_ERROR_CANNOT_KICK_PARTY_MEMBER,
        UI_ERROR_CANNOT_KICK_YOURSELF_FROM_PARTY,
        UI_ERROR_CANNOT_KICK_PARTY_LEADER,
        UI_ERROR_JOINED_ANOTHER_PARTY,
        UI_ERROR_NOT_JOINED_PARTY,
        UI_ERROR_NOT_PARTY_LEADER,
        UI_ERROR_ALREADY_IS_PARTY_LEADER,
        UI_ERROR_CHARACTER_JOINED_ANOTHER_PARTY,
        UI_ERROR_CHARACTER_NOT_JOINED_PARTY,
        UI_ERROR_PARTY_MEMBER_REACHED_LIMIT,
        UI_ERROR_PARTY_MEMBER_CANNOT_ENTER_INSTANCE,
        // Guild
        UI_ERROR_GUILD_NOT_FOUND,
        UI_ERROR_GUILD_INVITATION_NOT_FOUND,
        UI_GUILD_INVITATION_ACCEPTED,
        UI_GUILD_INVITATION_DECLINED,
        UI_ERROR_CANNOT_SEND_GUILD_INVITATION,
        UI_ERROR_CANNOT_KICK_GUILD_MEMBER,
        UI_ERROR_CANNOT_KICK_YOURSELF_FROM_GUILD,
        UI_ERROR_CANNOT_KICK_GUILD_LEADER,
        UI_ERROR_CANNOT_KICK_HIGHER_GUILD_MEMBER,
        UI_ERROR_JOINED_ANOTHER_GUILD,
        UI_ERROR_NOT_JOINED_GUILD,
        UI_ERROR_NOT_GUILD_LEADER,
        UI_ERROR_ALREADY_IS_GUILD_LEADER,
        UI_ERROR_CANNOT_CHANGE_GUILD_LEADER_ROLE,
        UI_ERROR_CHARACTER_JOINED_ANOTHER_GUILD,
        UI_ERROR_CHARACTER_NOT_JOINED_GUILD,
        UI_ERROR_GUILD_MEMBER_REACHED_LIMIT,
        UI_ERROR_GUILD_ROLE_NOT_AVAILABLE,
        UI_ERROR_GUILD_SKILL_REACHED_MAX_LEVEL,
        UI_ERROR_NOT_ENOUGH_GUILD_SKILL_POINT,
        UI_ERROR_CANNOT_ACCEPT_GUILD_REQUEST,
        UI_ERROR_CANNOT_DECLINE_GUILD_REQUEST,
        // Game Data
        UI_UNKNOW_GAME_DATA_TITLE,
        UI_UNKNOW_GAME_DATA_DESCRIPTION,
        // Bank
        UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
        UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
        UI_ERROR_CANNOT_ACCESS_STORAGE,
        UI_ERROR_STORAGE_NOT_FOUND,
        // Combatant
        UI_ERROR_NO_AMMO,
        UI_ERROR_NOT_ENOUGH_HP,
        UI_ERROR_NOT_ENOUGH_MP,
        UI_ERROR_NOT_ENOUGH_STAMINA,
        UI_ERROR_NOT_DEAD,
        // Skills
        UI_ERROR_SKILL_LEVEL_IS_ZERO,
        UI_ERROR_CANNOT_USE_SKILL_WITHOUT_SHIELD,
        UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_WEAPON,
        UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_ARMOR,
        UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_VEHICLE,
        UI_ERROR_SKILL_IS_COOLING_DOWN,
        UI_ERROR_SKILL_IS_NOT_LEARNED,
        UI_ERROR_NO_SKILL_TARGET,
        // Requirement
        UI_ERROR_NOT_ENOUGH_LEVEL,
        UI_ERROR_NOT_MATCH_CHARACTER_CLASS,
        UI_ERROR_NOT_ENOUGH_ATTRIBUTE_AMOUNTS,
        UI_ERROR_NOT_ENOUGH_SKILL_LEVELS,
        UI_ERROR_ATTRIBUTE_REACHED_MAX_AMOUNT,
        UI_ERROR_SKILL_REACHED_MAX_LEVEL,
        // Success - UI Cash Shop
        UI_CASH_SHOP_ITEM_BOUGHT,
        // UI Character Item
        UI_DROP_ITEM,
        UI_DROP_ITEM_DESCRIPTION,
        UI_SELL_ITEM,
        UI_SELL_ITEM_DESCRIPTION,
        UI_DISMANTLE_ITEM,
        UI_DISMANTLE_ITEM_DESCRIPTION,
        UI_OFFER_ITEM,
        UI_OFFER_ITEM_DESCRIPTION,
        UI_MOVE_ITEM_TO_STORAGE,
        UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION,
        UI_MOVE_ITEM_FROM_STORAGE,
        UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION,
        UI_ERROR_STORAGE_WILL_OVERWHELMING,
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
        UI_GUILD_REQUEST,
        UI_GUILD_REQUEST_DESCRIPTION,
        UI_GUILD_REQUESTED,
        UI_GUILD_REQUEST_ACCEPTED,
        UI_GUILD_REQUEST_DECLINED,
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
        UI_FRIEND_REQUEST,
        UI_FRIEND_REQUEST_DESCRIPTION,
        UI_FRIEND_ADDED,
        UI_FRIEND_REMOVED,
        UI_FRIEND_REQUESTED,
        UI_FRIEND_REQUEST_ACCEPTED,
        UI_FRIEND_REQUEST_DECLINED,
        // Item Amount Title
        UI_LABEL_UNLIMIT_WEIGHT,
        UI_LABEL_UNLIMIT_SLOT,
        // Enter Building Password
        UI_ENTER_BUILDING_PASSWORD,
        UI_ENTER_BUILDING_PASSWORD_DESCRIPTION,
        // Enter Building Password
        UI_SET_BUILDING_PASSWORD,
        UI_SET_BUILDING_PASSWORD_DESCRIPTION,
        // IAP Error
        UI_ERROR_IAP_NOT_INITIALIZED,
        UI_ERROR_IAP_PURCHASING_UNAVAILABLE,
        UI_ERROR_IAP_EXISTING_PURCHASE_PENDING,
        UI_ERROR_IAP_PRODUCT_UNAVAILABLE,
        UI_ERROR_IAP_SIGNATURE_INVALID,
        UI_ERROR_IAP_USER_CANCELLED,
        UI_ERROR_IAP_PAYMENT_DECLINED,
        UI_ERROR_IAP_DUPLICATE_TRANSACTION,
        UI_ERROR_IAP_UNKNOW,
        // Mail
        UI_ERROR_MAIL_SEND_NOT_ALLOWED,
        UI_ERROR_MAIL_SEND_NO_RECEIVER,
        UI_MAIL_SENT,
        UI_ERROR_MAIL_READ_NOT_ALLOWED,
        UI_ERROR_MAIL_CLAIM_NOT_ALLOWED,
        UI_ERROR_MAIL_CLAIM_ALREADY_CLAIMED,
        UI_ERROR_MAIL_CLAIM_WILL_OVERWHELMING,
        UI_MAIL_CLAIMED,
        UI_ERROR_MAIL_DELETE_NOT_ALLOWED,
        UI_MAIL_DELETED,
        // Error - App Server
        UI_ERROR_APP_NOT_READY,
        UI_ERROR_MAP_EXISTED,
        UI_ERROR_EVENT_EXISTED,
        UI_ERROR_INVALID_SERVER_HASH,
        // Error - Map Spawn Server
        UI_ERROR_EMPTY_SCENE_NAME,
        UI_ERROR_CANNOT_EXCUTE_MAP_SERVER,
    }

    public enum UIItemTypeKeys : byte
    {
        UI_ITEM_TYPE_JUNK,
        UI_ITEM_TYPE_SHIELD,
        UI_ITEM_TYPE_CONSUMABLE,
        UI_ITEM_TYPE_POTION,
        UI_ITEM_TYPE_AMMO,
        UI_ITEM_TYPE_BUILDING,
        UI_ITEM_TYPE_PET,
        UI_ITEM_TYPE_SOCKET_ENHANCER,
        UI_ITEM_TYPE_MOUNT,
        UI_ITEM_TYPE_SKILL,
    }

    public enum UISkillTypeKeys : byte
    {
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
        /// Format => {0} = {Character Classes}
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
        /// Format => {0} = {Attribute Title}, {1} = {Amount}
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
        /// <summary>
        /// Format => {0} = {Message}
        /// </summary>
        UI_FORMAT_CHAT_SYSTEM,

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
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_CRITICAL_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_CRITICAL_DAMAGE_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_BLOCK_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
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
        UI_FORMAT_SKILL_MOUNT,

        // Format - Skip Title
        /// <summary>
        /// Format => {1} = {Value}
        /// </summary>
        UI_FORMAT_SKIP_TITLE,
        /// <summary>
        /// Format => {1} = {Value}
        /// </summary>
        UI_FORMAT_SKIP_TITLE_PERCENTAGE,

        // Format - Notify Rewards
        /// <summary>
        /// Format => {0} = {Exp Amount}
        /// </summary>
        UI_FORMAT_NOTIFY_REWARD_EXP,
        /// <summary>
        /// Format => {0} = {Gold Amount}
        /// </summary>
        UI_FORMAT_NOTIFY_REWARD_GOLD,
        /// <summary>
        /// Format => {0} = {Item Title}, {1} = {Amount}
        /// </summary>
        UI_FORMAT_NOTIFY_REWARD_ITEM,

        // 1.61 Talk to NPC quest task
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_QUEST_TASK_TALK_TO_NPC,
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_QUEST_TASK_TALK_TO_NPC_COMPLETE,
        // Format - Currency Amount
        /// <summary>
        /// Format => {0} = {Currency Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_CURRENCY,
        /// <summary>
        /// Format => {0} = {Currency Title}, {1} = {Current Amount}, {2} = {Target Amount}
        /// </summary>
        UI_FORMAT_CURRENT_CURRENCY_NOT_ENOUGH,
        /// <summary>
        /// Format => {0} = {Currency Title}, {1} = {Amount}
        /// </summary>
        UI_FORMAT_CURRENCY_AMOUNT,

        // 1.61b New Formats
        /// <summary>
        /// Format => {0} = {Consume Hp}
        /// </summary>
        UI_FORMAT_CONSUME_HP,
        /// <summary>
        /// Format => {0} = {Consume Stamina}
        /// </summary>
        UI_FORMAT_CONSUME_STAMINA,
        /// <summary>
        /// Format => {0} = {Sender Name}
        /// </summary>
        UI_FORMAT_MAIL_SENDER_NAME,
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        UI_FORMAT_MAIL_TITLE,
        /// <summary>
        /// Format => {0} = {Content}
        /// </summary>
        UI_FORMAT_MAIL_CONTENT,
        /// <summary>
        /// Format => {0} = {Sent Date}
        /// </summary>
        UI_FORMAT_MAIL_SENT_DATE,

        // 1.62 New Formats
        /// <summary>
        /// Format => {0} = {List Of Armor Type}
        /// </summary>
        UI_FORMAT_AVAILABLE_ARMORS,
        /// <summary>
        /// Format => {0} = {List Of Vehicle Type}
        /// </summary>
        UI_FORMAT_AVAILABLE_VEHICLES,
        /// <summary>
        /// Format => {0} = {Craft Duration}
        /// </summary>
        UI_FORMAT_CRAFT_DURATION,

        // 1.63 New Formats
        /// <summary>
        /// Format => {0} = {Currency Title}, {1} = {Amount}
        /// </summary>
        UI_FORMAT_REWARD_CURRENCY,
        /// <summary>
        /// Format => {0} = {Currency Title}, {1} = {Amount}
        /// </summary>
        UI_FORMAT_NOTIFY_REWARD_CURRENCY,

        // 1.65f New Formats
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_HP_RECOVERY,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_HP_LEECH_RATE,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_MP_RECOVERY,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_MP_LEECH_RATE,
        /// <summary>
        /// Format => {0} = {Amount}
        /// </summary>
        UI_FORMAT_STAMINA_RECOVERY,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_STAMINA_LEECH_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_HP_RECOVERY_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_HP_LEECH_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_MP_RECOVERY_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_MP_LEECH_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_STAMINA_RECOVERY_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_STAMINA_LEECH_RATE_RATE,

        // 1.66c New Formats
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_GOLD_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_EXP_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_GOLD_RATE_RATE,
        /// <summary>
        /// Format => {0} = {Amount * 100}
        /// </summary>
        UI_FORMAT_EXP_RATE_RATE,

        // 1.67b New Formats
        /// <summary>
        /// Format => {0} = {Item's Title}, {1} = {Amount}
        /// </summary>
        UI_FORMAT_GENERATE_CAST_SHOP_ITEM_TITLE,
        /// <summary>
        /// Format => {0} = {Item's Title}, {1} = {Amount}, {2} = {Item's Description}
        /// </summary>
        UI_FORMAT_GENERATE_CAST_SHOP_ITEM_DESCRIPTION,

        // 1.68b New Formats
        /// <summary>
        /// Format => {0} = {Loading Asset Bundle File Name}
        /// </summary>
        UI_FORMAT_LOADING_ASSET_BUNDLE_FILE_NAME,
        /// <summary>
        /// Format => {0} = {Current Loaded Asset Bundles Count}, {1} = {Total Loading Asset Bundles Count}
        /// </summary>
        UI_FORMAT_LOADED_ASSET_BUNDLES_COUNT,

        // 1.71 New Formats
        /// <summary>
        /// Format => {0} = {Character Level}, {1} = {Require Level}
        /// </summary>
        UI_FORMAT_REQUIRE_LEVEL_NOT_ENOUGH,
        /// <summary>
        /// Format => {0} = {Character Classes}
        /// </summary>
        UI_FORMAT_INVALID_REQUIRE_CLASS,
    }

    public static class DefaultLocale
    {
        public static readonly Dictionary<string, string> Texts = new Dictionary<string, string>();
        static DefaultLocale()
        {
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
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_LEVEL_NOT_ENOUGH.ToString(), "Require Level: <color=red>{0}/{1}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_REQUIRE_CLASS.ToString(), "Require Class: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_INVALID_REQUIRE_CLASS.ToString(), "Require Class: <color=red>{0}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS.ToString(), "Have to equip: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_AVAILABLE_ARMORS.ToString(), "Have to equip: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_AVAILABLE_VEHICLES.ToString(), "Have to drive: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CONSUME_HP.ToString(), "Consume Hp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CONSUME_MP.ToString(), "Consume Mp: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CONSUME_STAMINA.ToString(), "Consume Stamina: {0}");
            // Format - Skill
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_COOLDOWN_DURATION.ToString(), "Cooldown: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SKILL_TYPE.ToString(), "Skill Type: {0}");
            Texts.Add(UISkillTypeKeys.UI_SKILL_TYPE_ACTIVE.ToString(), "Active");
            Texts.Add(UISkillTypeKeys.UI_SKILL_TYPE_PASSIVE.ToString(), "Passive");
            Texts.Add(UISkillTypeKeys.UI_SKILL_TYPE_CRAFT_ITEM.ToString(), "Craft Item");
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
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_JUNK.ToString(), "Junk");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_SHIELD.ToString(), "Shield");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_CONSUMABLE.ToString(), "Consumable");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_POTION.ToString(), "Potion");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_AMMO.ToString(), "Ammo");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_BUILDING.ToString(), "Building");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_PET.ToString(), "Pet");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_SOCKET_ENHANCER.ToString(), "Socket Enhancer");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_MOUNT.ToString(), "Mount");
            Texts.Add(UIItemTypeKeys.UI_ITEM_TYPE_SKILL.ToString(), "Skill");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_RARITY.ToString(), "Rarity: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_STACK.ToString(), "{0}/{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_ITEM_DURABILITY.ToString(), "Durability: {0}/{1}");
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
            Texts.Add(UIFormatKeys.UI_FORMAT_CHAT_SYSTEM.ToString(), "<color=orange>{0}</color>");
            // Format - UI Mail
            Texts.Add(UIFormatKeys.UI_FORMAT_MAIL_SENDER_NAME.ToString(), "From: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_MAIL_TITLE.ToString(), "Title: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_MAIL_CONTENT.ToString(), "{0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_MAIL_SENT_DATE.ToString(), "Date: {0}");
            // Format - UI Crafting
            Texts.Add(UIFormatKeys.UI_FORMAT_CRAFT_DURATION.ToString(), "Duration: {0}");
            // Error - Generic Error
            Texts.Add(UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE.ToString(), "Service is not available");
            Texts.Add(UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE.ToString(), "Content is not available");
            Texts.Add(UITextKeys.UI_ERROR_REQUEST_TIMEOUT.ToString(), "Request timeout");
            Texts.Add(UITextKeys.UI_ERROR_KICKED_FROM_SERVER.ToString(), "You have been kicked from server");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_FAILED.ToString(), "Cannot connect to the server");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_REJECTED.ToString(), "Connection rejected by server");
            Texts.Add(UITextKeys.UI_ERROR_REMOTE_CONNECTION_CLOSE.ToString(), "Server has been closed");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_PROTOCOL.ToString(), "Invalid protocol");
            Texts.Add(UITextKeys.UI_ERROR_HOST_UNREACHABLE.ToString(), "Host unreachable");
            Texts.Add(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString(), "Connection timeout");
            Texts.Add(UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR.ToString(), "Internal server error");
            Texts.Add(UITextKeys.UI_ERROR_SERVER_NOT_FOUND.ToString(), "Server not found");
            Texts.Add(UITextKeys.UI_ERROR_USER_NOT_FOUND.ToString(), "User not found");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND.ToString(), "Character not found");
            Texts.Add(UITextKeys.UI_ERROR_ITEM_NOT_FOUND.ToString(), "Item not found");
            Texts.Add(UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND.ToString(), "Cash package not found");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD.ToString(), "Not enough gold");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_CASH.ToString(), "Not enough cash");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS.ToString(), "Not enough items");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_STAT_POINT.ToString(), "Not enough stat points");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_POINT.ToString(), "Not enough skill points");
            Texts.Add(UITextKeys.UI_ERROR_NOT_LOGGED_IN.ToString(), "Not logged in");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_IS_EMPTY.ToString(), "Username is empty");
            Texts.Add(UITextKeys.UI_ERROR_PASSWORD_IS_EMPTY.ToString(), "Password is empty");
            Texts.Add(UITextKeys.UI_ERROR_WILL_OVERWHELMING.ToString(), "Cannot carry all items");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ABLE_TO_LOOT.ToString(), "Not allowed to loot");
            // Error - Game Data
            Texts.Add(UITextKeys.UI_ERROR_INVALID_DATA.ToString(), "Invalid data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA.ToString(), "Invalid character data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_ITEM_DATA.ToString(), "Invalid item data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_ITEM_INDEX.ToString(), "Invalid item index");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_ENHANCER_ITEM_INDEX.ToString(), "Invalid enhancer item index");
            Texts.Add(UITextKeys.UI_ERROR_ITEM_NOT_EQUIPMENT.ToString(), "Item is not a equipment item");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_ATTRIBUTE_DATA.ToString(), "Invalid attribute data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_SKILL_DATA.ToString(), "Invalid skill data");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_GUILD_SKILL_DATA.ToString(), "Invalid guild skill data");
            // Error - UI Login
            Texts.Add(UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD.ToString(), "Invalid username or password");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_USER_TOKEN.ToString(), "Invalid user token");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_LOGGED_IN.ToString(), "User already logged in");
            Texts.Add(UITextKeys.UI_ERROR_ACCOUNT_LOGGED_IN_BY_OTHER.ToString(), "Your account was logged in by other");
            Texts.Add(UITextKeys.UI_ERROR_USER_BANNED.ToString(), "Your account was banned");
            // Error - UI Register
            Texts.Add(UITextKeys.UI_ERROR_INVALID_CONFIRM_PASSWORD.ToString(), "Invalid confirm password");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_TOO_SHORT.ToString(), "Username is too short");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_TOO_LONG.ToString(), "Username is too long");
            Texts.Add(UITextKeys.UI_ERROR_PASSWORD_TOO_SHORT.ToString(), "Password is too short");
            Texts.Add(UITextKeys.UI_ERROR_USERNAME_EXISTED.ToString(), "Username is already existed");
            // Error - UI Lobby
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_CONNECTED_TO_LOBBY.ToString(), "Already connected to lobby server");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_CONNECTED_TO_GAME.ToString(), "Already connected to game server");
            Texts.Add(UITextKeys.UI_ERROR_NO_SELECTED_REALM.ToString(), "Please select realm");
            Texts.Add(UITextKeys.UI_ERROR_NO_AVAILABLE_REALM.ToString(), "No available realm");
            Texts.Add(UITextKeys.UI_ERROR_NO_AVAILABLE_LOBBY.ToString(), "No available lobby");
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
            // Error - UI Guild Name
            Texts.Add(UITextKeys.UI_ERROR_GUILD_NAME_TOO_SHORT.ToString(), "Guild name is too short");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_NAME_TOO_LONG.ToString(), "Guild name is too long");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_NAME_EXISTED.ToString(), "Guild name is already existed");
            // Error - UI Guild Role Setting
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_NAME_TOO_SHORT.ToString(), "Guild role name is too short");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_NAME_TOO_LONG.ToString(), "Guild role name is too long");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_SHARE_EXP_NOT_NUMBER.ToString(), "Share exp percentage must be number");
            // Error - UI Guild Member Role Setting
            Texts.Add(UITextKeys.UI_ERROR_INVALID_GUILD_ROLE.ToString(), "Invalid role");
            // Error - UI Guild Message Setting
            Texts.Add(UITextKeys.UI_ERROR_GUILD_MESSAGE_TOO_LONG.ToString(), "Guild message is too long");
            // Error - Equip
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_EQUIP.ToString(), "Cannot equip the item");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND.ToString(), "Invalid equip position for right hand equipment");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_LEFT_HAND.ToString(), "Invalid equip position for left hand equipment");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_RIGHT_HAND_OR_LEFT_HAND.ToString(), "Invalid equip position for right hand or left hand equipment");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_EQUIP_POSITION_ARMOR.ToString(), "Invalid equip position for armor equipment");
            // Error - Refine
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_REFINE.ToString(), "Cannot refine the item");
            Texts.Add(UITextKeys.UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL.ToString(), "Item reached max level");
            Texts.Add(UITextKeys.UI_REFINE_SUCCESS.ToString(), "Refine success");
            Texts.Add(UITextKeys.UI_REFINE_FAIL.ToString(), "Refine fail");
            // Error - Enhance
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_ENHANCE_SOCKET.ToString(), "Cannot enhance the item");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_SOCKET_ENCHANER.ToString(), "Have not enough items");
            Texts.Add(UITextKeys.UI_ERROR_NO_EMPTY_SOCKET.ToString(), "No empty socket");
            Texts.Add(UITextKeys.UI_ERROR_SOCKET_NOT_EMPTY.ToString(), "Socket is not empty");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_REMOVE_ENHANCER.ToString(), "Cannot remove enhancer item from socket");
            Texts.Add(UITextKeys.UI_ERROR_NO_ENHANCER.ToString(), "No enhancer item");
            // Error - Repair
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_REPAIR.ToString(), "Cannot repair the item");
            Texts.Add(UITextKeys.UI_REPAIR_SUCCESS.ToString(), "Repair success");
            // Error - Dealing
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_IS_DEALING.ToString(), "Character is in another deal");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR.ToString(), "Character is too far");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_ACCEPT_DEALING_REQUEST.ToString(), "Cannot accept dealing request");
            Texts.Add(UITextKeys.UI_ERROR_DEALING_REQUEST_DECLINED.ToString(), "Dealing request declined");
            Texts.Add(UITextKeys.UI_ERROR_INVALID_DEALING_STATE.ToString(), "Invalid dealing state");
            Texts.Add(UITextKeys.UI_ERROR_DEALING_CANCELED.ToString(), "Dealing canceled");
            Texts.Add(UITextKeys.UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING.ToString(), "Another character cannot carry all items");
            // Error - Party
            Texts.Add(UITextKeys.UI_ERROR_PARTY_NOT_FOUND.ToString(), "Party not found");
            Texts.Add(UITextKeys.UI_ERROR_PARTY_INVITATION_NOT_FOUND.ToString(), "Party invitation not found");
            Texts.Add(UITextKeys.UI_PARTY_INVITATION_ACCEPTED.ToString(), "Party invitation accepted");
            Texts.Add(UITextKeys.UI_PARTY_INVITATION_DECLINED.ToString(), "Party invitation declined");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_SEND_PARTY_INVITATION.ToString(), "Cannot send party invitation");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_PARTY_MEMBER.ToString(), "Cannot kick party member");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_YOURSELF_FROM_PARTY.ToString(), "Cannot kick yourself from party");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_PARTY_LEADER.ToString(), "Cannot kick party leader");
            Texts.Add(UITextKeys.UI_ERROR_JOINED_ANOTHER_PARTY.ToString(), "Already joined another party");
            Texts.Add(UITextKeys.UI_ERROR_NOT_JOINED_PARTY.ToString(), "Not joined the party");
            Texts.Add(UITextKeys.UI_ERROR_NOT_PARTY_LEADER.ToString(), "Not a party leader");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_IS_PARTY_LEADER.ToString(), "You are already a leader");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_JOINED_ANOTHER_PARTY.ToString(), "Character already joined another party");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NOT_JOINED_PARTY.ToString(), "Character not joined the party");
            Texts.Add(UITextKeys.UI_ERROR_PARTY_MEMBER_REACHED_LIMIT.ToString(), "Party member reached limit");
            Texts.Add(UITextKeys.UI_ERROR_PARTY_MEMBER_CANNOT_ENTER_INSTANCE.ToString(), "Only party leader can enter instance");
            // Error - Guild
            Texts.Add(UITextKeys.UI_ERROR_GUILD_NOT_FOUND.ToString(), "Guild not found");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_INVITATION_NOT_FOUND.ToString(), "Guild invitation not found");
            Texts.Add(UITextKeys.UI_GUILD_INVITATION_ACCEPTED.ToString(), "Guild invitation accepted");
            Texts.Add(UITextKeys.UI_GUILD_INVITATION_DECLINED.ToString(), "Guild invitation declined");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_SEND_GUILD_INVITATION.ToString(), "Cannot send guild invitation");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_GUILD_MEMBER.ToString(), "Cannot kick guild member");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_YOURSELF_FROM_GUILD.ToString(), "Cannot kick yourself from guild");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_GUILD_LEADER.ToString(), "Cannot kick guild leader");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_KICK_HIGHER_GUILD_MEMBER.ToString(), "Cannot kick higher guild member");
            Texts.Add(UITextKeys.UI_ERROR_JOINED_ANOTHER_GUILD.ToString(), "Already joined another guild");
            Texts.Add(UITextKeys.UI_ERROR_NOT_JOINED_GUILD.ToString(), "Not joined the guild");
            Texts.Add(UITextKeys.UI_ERROR_NOT_GUILD_LEADER.ToString(), "Not a guild leader");
            Texts.Add(UITextKeys.UI_ERROR_ALREADY_IS_GUILD_LEADER.ToString(), "You are already a leader");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_CHANGE_GUILD_LEADER_ROLE.ToString(), "Cannot change guild leader's role");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_JOINED_ANOTHER_GUILD.ToString(), "Character already joined another guild");
            Texts.Add(UITextKeys.UI_ERROR_CHARACTER_NOT_JOINED_GUILD.ToString(), "Character not joined the guild");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_MEMBER_REACHED_LIMIT.ToString(), "Guild member reached limit");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_ROLE_NOT_AVAILABLE.ToString(), "Guild role is not available");
            Texts.Add(UITextKeys.UI_ERROR_GUILD_SKILL_REACHED_MAX_LEVEL.ToString(), "Guild skill is reached max level");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_GUILD_SKILL_POINT.ToString(), "Not enough guild skill point");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_ACCEPT_GUILD_REQUEST.ToString(), "You're not allowed to accept guild request");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_DECLINE_GUILD_REQUEST.ToString(), "You're not allowed to decline guild request");
            // Error - Game Data
            Texts.Add(UITextKeys.UI_UNKNOW_GAME_DATA_TITLE.ToString(), "Unknow");
            Texts.Add(UITextKeys.UI_UNKNOW_GAME_DATA_DESCRIPTION.ToString(), "N/A");
            // Error - Bank
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT.ToString(), "Not enough gold to deposit");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW.ToString(), "Not enough gold to withdraw");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE.ToString(), "Cannot access storage");
            Texts.Add(UITextKeys.UI_ERROR_STORAGE_NOT_FOUND.ToString(), "Storage not found");
            // Error - Combatant
            Texts.Add(UITextKeys.UI_ERROR_NO_AMMO.ToString(), "No Ammo");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_HP.ToString(), "Not enough Hp");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_MP.ToString(), "Not enough Mp");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_STAMINA.ToString(), "Not enough Stamina");
            Texts.Add(UITextKeys.UI_ERROR_NOT_DEAD.ToString(), "Cannot respawn");
            // Error - Skill
            Texts.Add(UITextKeys.UI_ERROR_SKILL_LEVEL_IS_ZERO.ToString(), "Skill not trained yet");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_USE_SKILL_WITHOUT_SHIELD.ToString(), "Cannot use skill without shield");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_WEAPON.ToString(), "Cannot use skill by current weapons");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_ARMOR.ToString(), "Cannot use skill by current armors");
            Texts.Add(UITextKeys.UI_ERROR_CANNOT_USE_SKILL_BY_CURRENT_VEHICLE.ToString(), "Cannot use skill by current vehicle");
            Texts.Add(UITextKeys.UI_ERROR_SKILL_IS_COOLING_DOWN.ToString(), "Skill is cooling down");
            Texts.Add(UITextKeys.UI_ERROR_SKILL_IS_NOT_LEARNED.ToString(), "Skill is not learned");
            Texts.Add(UITextKeys.UI_ERROR_NO_SKILL_TARGET.ToString(), "No target");
            // Error - Requirement
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_LEVEL.ToString(), "Not enough level");
            Texts.Add(UITextKeys.UI_ERROR_NOT_MATCH_CHARACTER_CLASS.ToString(), "Not match character class");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_ATTRIBUTE_AMOUNTS.ToString(), "Not enough attribute amounts");
            Texts.Add(UITextKeys.UI_ERROR_NOT_ENOUGH_SKILL_LEVELS.ToString(), "Not enough skill levels");
            Texts.Add(UITextKeys.UI_ERROR_ATTRIBUTE_REACHED_MAX_AMOUNT.ToString(), "Attribute reached max amount");
            Texts.Add(UITextKeys.UI_ERROR_SKILL_REACHED_MAX_LEVEL.ToString(), "Skill reached max level");
            // Success - UI Cash Shop
            Texts.Add(UITextKeys.UI_CASH_SHOP_ITEM_BOUGHT.ToString(), "Cash shop item purchased");
            // UI Character Item
            Texts.Add(UITextKeys.UI_DROP_ITEM.ToString(), "Drop Item");
            Texts.Add(UITextKeys.UI_DROP_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_SELL_ITEM.ToString(), "Sell Item");
            Texts.Add(UITextKeys.UI_SELL_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_DISMANTLE_ITEM.ToString(), "Dismantle Item");
            Texts.Add(UITextKeys.UI_DISMANTLE_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_OFFER_ITEM.ToString(), "Offer Item");
            Texts.Add(UITextKeys.UI_OFFER_ITEM_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_TO_STORAGE.ToString(), "Move To Storage");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_TO_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE.ToString(), "Move From Storage");
            Texts.Add(UITextKeys.UI_MOVE_ITEM_FROM_STORAGE_DESCRIPTION.ToString(), "Enter amount of item");
            Texts.Add(UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING.ToString(), "Storage will overwhelming");
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
            Texts.Add(UITextKeys.UI_GUILD_REQUEST.ToString(), "Guild Application");
            Texts.Add(UITextKeys.UI_GUILD_REQUEST_DESCRIPTION.ToString(), "You want to request to join guild {0}?");
            Texts.Add(UITextKeys.UI_GUILD_REQUESTED.ToString(), "Guild request was sent to the guild");
            Texts.Add(UITextKeys.UI_GUILD_REQUEST_ACCEPTED.ToString(), "Guild request accepted");
            Texts.Add(UITextKeys.UI_GUILD_REQUEST_DECLINED.ToString(), "Guild request accepted");
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
            Texts.Add(UITextKeys.UI_FRIEND_REQUEST.ToString(), "Friend Request");
            Texts.Add(UITextKeys.UI_FRIEND_REQUEST_DESCRIPTION.ToString(), "You want to request {0} to be friend?");
            Texts.Add(UITextKeys.UI_FRIEND_ADDED.ToString(), "The character was added to the friend list");
            Texts.Add(UITextKeys.UI_FRIEND_REMOVED.ToString(), "The character was removed from the friend list");
            Texts.Add(UITextKeys.UI_FRIEND_REQUESTED.ToString(), "Friend request was sent to the character");
            Texts.Add(UITextKeys.UI_FRIEND_REQUEST_ACCEPTED.ToString(), "Friend request accepted");
            Texts.Add(UITextKeys.UI_FRIEND_REQUEST_DECLINED.ToString(), "Friend request declined");
            // UI Password Dialogs
            Texts.Add(UITextKeys.UI_ENTER_BUILDING_PASSWORD.ToString(), "Enter password");
            Texts.Add(UITextKeys.UI_ENTER_BUILDING_PASSWORD_DESCRIPTION.ToString(), "Enter 6 digits number");
            Texts.Add(UITextKeys.UI_SET_BUILDING_PASSWORD.ToString(), "Set password");
            Texts.Add(UITextKeys.UI_SET_BUILDING_PASSWORD_DESCRIPTION.ToString(), "Enter 6 digits number");
            // UI Mail
            Texts.Add(UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED.ToString(), "You're not allowed to send mail");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER.ToString(), "No receiver, you may entered wrong name");
            Texts.Add(UITextKeys.UI_MAIL_SENT.ToString(), "Mail sent");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED.ToString(), "You're not allowed to read the mail");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_CLAIM_NOT_ALLOWED.ToString(), "You're not allowed to claim attached items");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_CLAIM_ALREADY_CLAIMED.ToString(), "Cannot claim items, it was already claimed");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_CLAIM_WILL_OVERWHELMING.ToString(), "Cannot carry all items");
            Texts.Add(UITextKeys.UI_MAIL_CLAIMED.ToString(), "Claimed an items");
            Texts.Add(UITextKeys.UI_ERROR_MAIL_DELETE_NOT_ALLOWED.ToString(), "You're not allowed to delete the mail");
            Texts.Add(UITextKeys.UI_MAIL_DELETED.ToString(), "Mail deleted");
            // Error - IAP
            Texts.Add(UITextKeys.UI_ERROR_IAP_NOT_INITIALIZED.ToString(), "In-App Purchasing system not initialized yet");
            Texts.Add(UITextKeys.UI_ERROR_IAP_PURCHASING_UNAVAILABLE.ToString(), "Purchasing is unavailable");
            Texts.Add(UITextKeys.UI_ERROR_IAP_EXISTING_PURCHASE_PENDING.ToString(), "Existing purchase pending");
            Texts.Add(UITextKeys.UI_ERROR_IAP_PRODUCT_UNAVAILABLE.ToString(), "Product is unavailable");
            Texts.Add(UITextKeys.UI_ERROR_IAP_SIGNATURE_INVALID.ToString(), "Invalid signature");
            Texts.Add(UITextKeys.UI_ERROR_IAP_USER_CANCELLED.ToString(), "Purchase was cancelled");
            Texts.Add(UITextKeys.UI_ERROR_IAP_PAYMENT_DECLINED.ToString(), "Payment was declined");
            Texts.Add(UITextKeys.UI_ERROR_IAP_DUPLICATE_TRANSACTION.ToString(), "Duplicate transaction");
            Texts.Add(UITextKeys.UI_ERROR_IAP_UNKNOW.ToString(), "Unknow");
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
            // Format - Skip Title
            Texts.Add(UIFormatKeys.UI_FORMAT_SKIP_TITLE.ToString(), "{1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_SKIP_TITLE_PERCENTAGE.ToString(), "{1}%");
            // Format - Notify Rewards
            Texts.Add(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_EXP.ToString(), "Obtain {0} Exp");
            Texts.Add(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_GOLD.ToString(), "Obtain {0} Gold");
            Texts.Add(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_ITEM.ToString(), "Obtain {0} x {1} ea");
            Texts.Add(UIFormatKeys.UI_FORMAT_NOTIFY_REWARD_CURRENCY.ToString(), "Obtain {1} {0}");
            // Format - 1.61 - Talk to NPC quest task
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_TALK_TO_NPC.ToString(), "Talk to {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_QUEST_TASK_TALK_TO_NPC_COMPLETE.ToString(), "Talk to {0}: Complete");
            // Format - Currency Amount
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_CURRENCY.ToString(), "{0}: {1}/{2}");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENT_CURRENCY_NOT_ENOUGH.ToString(), "{0}: <color=red>{1}/{2}</color>");
            Texts.Add(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT.ToString(), "{0}: {1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_REWARD_CURRENCY.ToString(), "Reward {0}: {1}");
            // Format - 1.65f - New stats
            Texts.Add(UIFormatKeys.UI_FORMAT_HP_RECOVERY.ToString(), "Hp Recovery: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE.ToString(), "Hp Leech Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP_RECOVERY.ToString(), "Mp Recovery: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE.ToString(), "Mp Leech Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY.ToString(), "Stamina Recovery: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE.ToString(), "Stamina Leech Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_HP_RECOVERY_RATE.ToString(), "Hp Recovery Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_HP_LEECH_RATE_RATE.ToString(), "% of Hp Leech Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP_RECOVERY_RATE.ToString(), "Mp Recovery Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_MP_LEECH_RATE_RATE.ToString(), "% of Mp Leech Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA_RECOVERY_RATE.ToString(), "Stamina Recovery Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_STAMINA_LEECH_RATE_RATE.ToString(), "% of Stamina Leech Rate: {0}%");
            // Format - 1.66c - New stats
            Texts.Add(UIFormatKeys.UI_FORMAT_GOLD_RATE.ToString(), "Gold Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_EXP_RATE.ToString(), "Exp Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_GOLD_RATE_RATE.ToString(), "% of Gold Rate: {0}%");
            Texts.Add(UIFormatKeys.UI_FORMAT_EXP_RATE_RATE.ToString(), "% of Exp Rate: {0}%");
            // Format - 1.67b - Cash shop item generator
            Texts.Add(UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_TITLE.ToString(), "{0} x {1}");
            Texts.Add(UIFormatKeys.UI_FORMAT_GENERATE_CAST_SHOP_ITEM_DESCRIPTION.ToString(), "Buy {0} x {1}\n\n{2}");
            // Format - 1.68b - Asset bundle
            Texts.Add(UIFormatKeys.UI_FORMAT_LOADING_ASSET_BUNDLE_FILE_NAME.ToString(), "Loading File: {0}");
            Texts.Add(UIFormatKeys.UI_FORMAT_LOADED_ASSET_BUNDLES_COUNT.ToString(), "Loaded Files: {0}/{1}");
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

        public static string GetTextByLanguageKey(IEnumerable<LanguageData> langs, string languageKey, string defaultValue)
        {
            if (langs != null)
            {
                foreach (LanguageData entry in langs)
                {
                    if (string.IsNullOrEmpty(entry.key))
                        continue;
                    if (entry.key.Equals(languageKey))
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
