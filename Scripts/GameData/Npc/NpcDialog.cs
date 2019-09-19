using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public enum NpcDialogType : byte
    {
        Normal,
        Quest,
        Shop,
        CraftItem,
        SaveRespawnPoint,
        Warp,
        RefineItem,
        PlayerStorage,
        GuildStorage,
    }

    [CreateAssetMenu(fileName = "Npc Dialog", menuName = "Create GameData/Npc Dialog", order = -4798)]
    public partial class NpcDialog : BaseGameData
    {
        public const int QUEST_ACCEPT_MENU_INDEX = 0;
        public const int QUEST_DECLINE_MENU_INDEX = 1;
        public const int QUEST_ABANDON_MENU_INDEX = 2;
        public const int QUEST_COMPLETE_MENU_INDEX = 3;
        public const int CRAFT_ITEM_START_MENU_INDEX = 0;
        public const int CRAFT_ITEM_CANCEL_MENU_INDEX = 1;
        public const int REFINE_ITEM_START_MENU_INDEX = 0;
        public const int REFINE_ITEM_CANCEL_MENU_INDEX = 1;
        public const int SAVE_SPAWN_POINT_CONFIRM_MENU_INDEX = 0;
        public const int SAVE_SPAWN_POINT_CANCEL_MENU_INDEX = 1;
        public const int WARP_CONFIRM_MENU_INDEX = 0;
        public const int WARP_CANCEL_MENU_INDEX = 1;
        public const int STORAGE_CONFIRM_MENU_INDEX = 0;
        public const int STORAGE_CANCEL_MENU_INDEX = 1;

        [Header("NPC Dialog Configs")]
        public NpcDialogType type;
        public NpcDialogMenu[] menus;
        // Quest
        public Quest quest;
        public NpcDialog questAcceptedDialog;
        public NpcDialog questDeclinedDialog;
        public NpcDialog questAbandonedDialog;
        public NpcDialog questCompletedDialog;
        // Shop
        public NpcSellItem[] sellItems;
        // Craft Item
        public ItemCraft itemCraft;
        public NpcDialog craftDoneDialog;
        public NpcDialog craftNotMeetRequirementsDialog;
        public NpcDialog craftCancelDialog;
        // Save Spawn Point
        public MapInfo saveRespawnMap;
        public Vector3 saveRespawnPosition;
        public NpcDialog saveRespawnConfirmDialog;
        public NpcDialog saveRespawnCancelDialog;
        // Teleport
        public WarpPortalType warpPortalType;
        public MapInfo warpMap;
        public Vector3 warpPosition;
        public NpcDialog warpCancelDialog;
        // Teleport
        public NpcDialog storageCancelDialog;
    }

    public enum NpcDialogConditionType : byte
    {
        LevelMoreThanOrEqual,
        LevelLessThanOrEqual,
        QuestNotStarted,
        QuestOngoing,
        QuestTasksCompleted,
        QuestCompleted,
    }

    [System.Serializable]
    public struct NpcDialogCondition
    {
        public NpcDialogConditionType conditionType;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "QuestNotStarted", "QuestOngoing", "QuestTasksCompleted", "QuestCompleted" })]
        public Quest quest;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "LevelMoreThanOrEqual", "LevelLessThanOrEqual" })]
        public int conditionalLevel;
        public bool IsPass(IPlayerCharacterData character)
        {
            int indexOfQuest = -1;
            bool questTasksCompleted = false;
            bool questCompleted = false;
            if (quest != null)
            {
                indexOfQuest = character.IndexOfQuest(quest.DataId);
                if (indexOfQuest >= 0)
                {
                    CharacterQuest characterQuest = character.Quests[indexOfQuest];
                    questTasksCompleted = characterQuest.IsAllTasksDone(character);
                    questCompleted = characterQuest.isComplete;
                }
            }
            switch (conditionType)
            {
                case NpcDialogConditionType.LevelMoreThanOrEqual:
                    return character.Level >= conditionalLevel;
                case NpcDialogConditionType.LevelLessThanOrEqual:
                    return character.Level <= conditionalLevel;
                case NpcDialogConditionType.QuestNotStarted:
                    return indexOfQuest < 0;
                case NpcDialogConditionType.QuestOngoing:
                    return !questTasksCompleted;
                case NpcDialogConditionType.QuestTasksCompleted:
                    return questTasksCompleted;
                case NpcDialogConditionType.QuestCompleted:
                    return questCompleted;
            }
            return true;
        }
    }

    [System.Serializable]
    public struct NpcDialogMenu
    {
        public string title;
        public NpcDialogCondition[] showConditions;
        public bool isCloseMenu;
        [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
        public NpcDialog dialog;

        public bool IsPassConditions(IPlayerCharacterData character)
        {
            if (dialog != null && dialog.type == NpcDialogType.Quest)
            {
                if (dialog.quest == null)
                    return false;
                int indexOfQuest = character.IndexOfQuest(dialog.quest.DataId);
                if (indexOfQuest >= 0 && character.Quests[indexOfQuest].isComplete)
                    return false;
            }
            foreach (NpcDialogCondition showCondition in showConditions)
            {
                if (!showCondition.IsPass(character))
                    return false;
            }
            return true;
        }
    }

    [System.Serializable]
    public partial struct NpcSellItem
    {
        /// <summary>
        /// Selling item
        /// </summary>
        public Item item;
        /// <summary>
        /// Require gold to buy item
        /// </summary>
        public int sellPrice;
    }
}
