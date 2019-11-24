using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
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
    public partial class NpcDialog : Node
    {
        public const int QUEST_ACCEPT_MENU_INDEX = 0;
        public const int QUEST_DECLINE_MENU_INDEX = 1;
        public const int QUEST_ABANDON_MENU_INDEX = 2;
        public const int QUEST_COMPLETE_MENU_INDEX = 3;
        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        [Input]
        public NpcDialog input;

        [Header("NPC Dialog Configs")]
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        [Tooltip("Default description")]
        [TextArea]
        public string description;
        [Tooltip("Descriptions by language keys")]
        public LanguageData[] descriptions;
        public Sprite icon;
        public NpcDialogType type;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        public NpcDialogMenu[] menus;
        // Quest
        public Quest quest;
        [Output(backingValue = ShowBackingValue.Always, connectionType = ConnectionType.Override)]
        public NpcDialog questAcceptedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog questDeclinedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog questAbandonedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog questCompletedDialog;
        // Shop
        public NpcSellItem[] sellItems;
        // Craft Item
        public ItemCraft itemCraft;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog craftDoneDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog craftItemWillOverwhelmingDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog craftNotMeetRequirementsDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog craftCancelDialog;
        // Save Spawn Point
        public MapInfo saveRespawnMap;
        public Vector3 saveRespawnPosition;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog saveRespawnConfirmDialog;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog saveRespawnCancelDialog;
        // Warp
        public WarpPortalType warpPortalType;
        public MapInfo warpMap;
        public Vector3 warpPosition;
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog warpCancelDialog;
        // Refine Item
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog refineItemCancelDialog;
        // Storage
        [Output(connectionType = ConnectionType.Override)]
        public NpcDialog storageCancelDialog;

        #region Generic Data
        public string Id { get { return name; } }
        public string Title
        {
            get { return Language.GetText(titles, title); }
        }
        public string Description
        {
            get { return Language.GetText(descriptions, description); }
        }
        public int DataId { get { return MakeDataId(Id); } }

        public static int MakeDataId(string id)
        {
            return id.GenerateHashId();
        }
        #endregion
        
#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (Validate())
                EditorUtility.SetDirty(this);
        }
#endif

        public bool Validate()
        {
            return false;
        }

        public void PrepareRelatesData()
        {
            // Add dialogs from menus
            List<NpcDialog> menuDialogs = new List<NpcDialog>();
            if (menus != null && menus.Length > 0)
            {
                foreach (NpcDialogMenu menu in menus)
                {
                    if (menu.dialog != null)
                        menuDialogs.Add(menu.dialog);
                }
            }
            GameInstance.AddNpcDialogs(menuDialogs);
            // Add items
            List<Item> items = new List<Item>();
            if (sellItems != null && sellItems.Length > 0)
            {
                foreach (NpcSellItem sellItem in sellItems)
                {
                    if (sellItem.item != null)
                        items.Add(sellItem.item);
                }
            }
            if (itemCraft.CraftingItem != null)
                items.Add(itemCraft.CraftingItem);
            GameInstance.AddItems(items);
            // Add quest
            GameInstance.AddQuests(new Quest[] { quest });
        }

        public bool ValidateDialog(BasePlayerCharacterEntity characterEntity)
        {
            switch (type)
            {
                case NpcDialogType.Quest:
                    if (quest == null)
                    {
                        // Validate quest data
                        Debug.LogWarning("[NpcDialog] Quest dialog's quest is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.CraftItem:
                    if (itemCraft.CraftingItem == null)
                    {
                        // Validate crafting item
                        Debug.LogWarning("[NpcDialog] Item craft dialog's crafting item is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    if (saveRespawnMap == null)
                    {
                        // Validate quest data
                        Debug.LogWarning("[NpcDialog] Save respawn point dialog's save respawn map is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.Warp:
                    if (warpMap == null)
                    {
                        // Validate quest data
                        Debug.LogWarning("[NpcDialog] Warp dialog's warp map is empty");
                        return false;
                    }
                    break;
            }
            return true;
        }

        public NpcDialog GetNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex)
        {
            // This dialog is current NPC dialog
            NpcDialog nextDialog = null;
            switch (type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex >= menus.Length)
                    {
                        // Invalid menu, so no next dialog, so return itself
                        return this;
                    }
                    // Changing current npc dialog
                    NpcDialogMenu selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(characterEntity) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        // Close dialog, so return null
                        return null;
                    }
                    nextDialog = selectedMenu.dialog;
                    break;
                case NpcDialogType.Quest:
                    switch (menuIndex)
                    {
                        case QUEST_ACCEPT_MENU_INDEX:
                            characterEntity.AcceptQuest(quest.DataId);
                            nextDialog = questAcceptedDialog;
                            break;
                        case QUEST_DECLINE_MENU_INDEX:
                            nextDialog = questDeclinedDialog;
                            break;
                        case QUEST_ABANDON_MENU_INDEX:
                            characterEntity.AbandonQuest(quest.DataId);
                            nextDialog = questAbandonedDialog;
                            break;
                        case QUEST_COMPLETE_MENU_INDEX:
                            characterEntity.CompleteQuest(quest.DataId);
                            nextDialog = questCompletedDialog;
                            break;
                    }
                    break;
                case NpcDialogType.CraftItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            GameMessage.Type gameMessageType;
                            if (itemCraft.CanCraft(characterEntity, out gameMessageType))
                            {
                                itemCraft.CraftItem(characterEntity);
                                nextDialog = craftDoneDialog;
                            }
                            else
                            {
                                // Cannot craft item
                                switch (gameMessageType)
                                {
                                    case GameMessage.Type.CannotCarryAnymore:
                                        nextDialog = craftItemWillOverwhelmingDialog;
                                        break;
                                    default:
                                        nextDialog = craftNotMeetRequirementsDialog;
                                        break;
                                }
                            }
                            break;
                        case CANCEL_MENU_INDEX:
                            nextDialog = craftCancelDialog;
                            break;
                    }
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.RespawnMapName = saveRespawnMap.Id;
                            characterEntity.RespawnPosition = saveRespawnPosition;
                            nextDialog = saveRespawnConfirmDialog;
                            break;
                        case CANCEL_MENU_INDEX:
                            nextDialog = saveRespawnCancelDialog;
                            break;
                    }
                    break;
                case NpcDialogType.Warp:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, characterEntity, warpMap.Id, warpPosition);
                            return null;
                        case CANCEL_MENU_INDEX:
                            nextDialog = warpCancelDialog;
                            break;
                    }
                    break;
                case NpcDialogType.RefineItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.RequestShowNpcRefine();
                            return null;
                        case CANCEL_MENU_INDEX:
                            nextDialog = refineItemCancelDialog;
                            break;
                    }
                    break;
                case NpcDialogType.PlayerStorage:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.OpenStorage(StorageType.Player, characterEntity.UserId);
                            return null;
                        case CANCEL_MENU_INDEX:
                            nextDialog = storageCancelDialog;
                            break;
                    }
                    break;
                case NpcDialogType.GuildStorage:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.OpenStorage(StorageType.Guild, characterEntity.GuildId.ToString());
                            return null;
                        case CANCEL_MENU_INDEX:
                            nextDialog = storageCancelDialog;
                            break;
                    }
                    break;
            }

            if (nextDialog == null || !nextDialog.ValidateDialog(characterEntity))
                return null;

            return nextDialog;
        }

        public override object GetValue(NodePort port)
        {
            return port.node;
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            SetDialogByPort(from, to);
        }

        public override void OnRemoveConnection(NodePort port)
        {
            SetDialogByPort(port, null);
        }

        private void SetDialogByPort(NodePort from, NodePort to)
        {
            NpcDialog dialog = null;
            if (to != null && to.node != null)
                dialog = to.node as NpcDialog;

            int arrayIndex;
            if (from.fieldName.Contains("menus ") && int.TryParse(from.fieldName.Split(' ')[1], out arrayIndex) && arrayIndex < menus.Length)
                menus[arrayIndex].dialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.questAcceptedDialog)))
                questAcceptedDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.questDeclinedDialog)))
                questDeclinedDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.questAbandonedDialog)))
                questAbandonedDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.questCompletedDialog)))
                questCompletedDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.craftDoneDialog)))
                craftDoneDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.craftItemWillOverwhelmingDialog)))
                craftItemWillOverwhelmingDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.craftNotMeetRequirementsDialog)))
                craftNotMeetRequirementsDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.craftCancelDialog)))
                craftCancelDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.saveRespawnConfirmDialog)))
                saveRespawnConfirmDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.saveRespawnCancelDialog)))
                saveRespawnCancelDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.warpCancelDialog)))
                warpCancelDialog = dialog;
            
            if (from.fieldName.Equals(this.GetMemberName(a => a.refineItemCancelDialog)))
                refineItemCancelDialog = dialog;

            if (from.fieldName.Equals(this.GetMemberName(a => a.storageCancelDialog)))
                storageCancelDialog = dialog;
        }
    }

    public enum NpcDialogConditionType : byte
    {
        LevelMoreThanOrEqual,
        LevelLessThanOrEqual,
        QuestNotStarted,
        QuestOngoing,
        QuestTasksCompleted,
        QuestCompleted,
        FactionIs,
    }

    [System.Serializable]
    public struct NpcDialogCondition
    {
        public NpcDialogConditionType conditionType;
        [StringShowConditional(conditionFieldName: "conditionType", conditionValues: new string[] { "FactionIs" })]
        public Faction faction;
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
                case NpcDialogConditionType.FactionIs:
                    return character.FactionId == faction.DataId;
            }
            return true;
        }
    }

    [System.Serializable]
    public struct NpcDialogMenu
    {
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        public NpcDialogCondition[] showConditions;
        public bool isCloseMenu;
        [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
        public NpcDialog dialog;

        public string Title
        {
            get { return Language.GetText(titles, title); }
        }

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
