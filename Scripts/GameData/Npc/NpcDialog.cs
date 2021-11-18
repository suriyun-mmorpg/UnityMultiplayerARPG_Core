using System.Collections.Generic;
using LiteNetLibManager;
using UnityEngine;
using XNode;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Npc Dialog", menuName = "Create GameData/Npc Dialog", order = -4798)]
    public partial class NpcDialog : BaseNpcDialog
    {
        public const int QUEST_ACCEPT_MENU_INDEX = 0;
        public const int QUEST_DECLINE_MENU_INDEX = 1;
        public const int QUEST_ABANDON_MENU_INDEX = 2;
        public const int QUEST_COMPLETE_MENU_INDEX = 3;
        public const int CONFIRM_MENU_INDEX = 0;
        public const int CANCEL_MENU_INDEX = 1;

        public NpcDialogType type;
        [Output(dynamicPortList = true, connectionType = ConnectionType.Override)]
        public NpcDialogMenu[] menus;
        // Quest
        public Quest quest;
        [Output(backingValue = ShowBackingValue.Always, connectionType = ConnectionType.Override)]
        public BaseNpcDialog questAcceptedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog questDeclinedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog questAbandonedDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog questCompletedDialog;
        // Shop
        public NpcSellItem[] sellItems;
        // Craft Item
        public ItemCraft itemCraft;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog craftDoneDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog craftItemWillOverwhelmingDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog craftNotMeetRequirementsDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog craftCancelDialog;
        // Save Spawn Point
        public BaseMapInfo saveRespawnMap;
        public Vector3 saveRespawnPosition;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog saveRespawnConfirmDialog;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog saveRespawnCancelDialog;
        // Warp
        public WarpPortalType warpPortalType;
        [Tooltip("Map which character will warp to when use the warp portal, leave this empty to warp character to other position in the same map")]
        public BaseMapInfo warpMap;
        [Tooltip("Position which character will warp to when use the warp portal")]
        public Vector3 warpPosition;
        [Tooltip("If this is `TRUE` it will change character's rotation when warp")]
        public bool warpOverrideRotation;
        [Tooltip("This will be used if `warpOverrideRotation` is `TRUE` to change character's rotation when warp")]
        public Vector3 warpRotation;
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog warpCancelDialog;
        // Refine Item
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog refineItemCancelDialog;
        // Dismantle Item
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog dismantleItemCancelDialog;
        // Storage
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog storageCancelDialog;
        // Repair Item
        [Output(connectionType = ConnectionType.Override)]
        public BaseNpcDialog repairItemCancelDialog;

        public override void PrepareRelatesData()
        {
            if (menus != null && menus.Length > 0)
            {
                foreach (NpcDialogMenu menu in menus)
                {
                    GameInstance.AddNpcDialogs(menu.dialog);
                }
            }
            if (sellItems != null && sellItems.Length > 0)
            {
                foreach (NpcSellItem sellItem in sellItems)
                {
                    GameInstance.AddItems(sellItem.item);
                    if (sellItem.sellPrices != null && sellItem.sellPrices.Length > 0)
                    {
                        foreach (CurrencyAmount rewardCurrency in sellItem.sellPrices)
                        {
                            GameInstance.AddCurrencies(rewardCurrency.currency);
                        }
                    }
                }
            }
            GameInstance.AddItems(itemCraft.CraftingItem);
            GameInstance.AddItems(itemCraft.CacheCraftRequirements.Keys);
            GameInstance.AddQuests(quest);
        }

        public override bool IsPassMenuCondition(IPlayerCharacterData character)
        {
            if (type == NpcDialogType.Quest)
            {
                if (quest == null || !quest.CanReceiveQuest(character))
                    return false;
            }
            return true;
        }

        public override void RenderUI(UINpcDialog uiNpcDialog)
        {
            BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;
            BaseItem craftingItem = null;

            if (uiNpcDialog.uiSellItemRoot != null)
                uiNpcDialog.uiSellItemRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemDialog != null)
                uiNpcDialog.uiSellItemDialog.Hide();

            if (uiNpcDialog.uiCharacterQuest != null)
                uiNpcDialog.uiCharacterQuest.Hide();

            if (uiNpcDialog.uiCraftItem != null)
                uiNpcDialog.uiCraftItem.Hide();

            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            UINpcDialogMenuAction confirmMenuAction;
            UINpcDialogMenuAction cancelMenuAction;
            switch (type)
            {
                case NpcDialogType.Normal:
                    if (uiNpcDialog.onSwitchToNormalDialog == null)
                        uiNpcDialog.onSwitchToNormalDialog.Invoke();
                    for (int i = 0; i < menus.Length; ++i)
                    {
                        NpcDialogMenu menu = menus[i];
                        if (menu.IsPassConditions(owningCharacter))
                        {
                            UINpcDialogMenuAction menuAction = new UINpcDialogMenuAction();
                            menuAction.title = menu.Title;
                            menuAction.menuIndex = i;
                            menuActions.Add(menuAction);
                        }
                    }
                    break;
                case NpcDialogType.Quest:
                    if (uiNpcDialog.onSwitchToQuestDialog == null)
                        uiNpcDialog.onSwitchToQuestDialog.Invoke();
                    if (uiNpcDialog.uiCharacterQuest != null)
                    {
                        if (quest != null)
                        {
                            UINpcDialogMenuAction acceptMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction declineMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction abandonMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction completeMenuAction = new UINpcDialogMenuAction();
                            acceptMenuAction.title = uiNpcDialog.MessageQuestAccept;
                            acceptMenuAction.menuIndex = QUEST_ACCEPT_MENU_INDEX;
                            declineMenuAction.title = uiNpcDialog.MessageQuestDecline;
                            declineMenuAction.menuIndex = QUEST_DECLINE_MENU_INDEX;
                            abandonMenuAction.title = uiNpcDialog.MessageQuestAbandon;
                            abandonMenuAction.menuIndex = QUEST_ABANDON_MENU_INDEX;
                            completeMenuAction.title = uiNpcDialog.MessageQuestComplete;
                            completeMenuAction.menuIndex = QUEST_COMPLETE_MENU_INDEX;

                            CharacterQuest characterQuest;
                            int index = owningCharacter.IndexOfQuest(quest.DataId);
                            if (index >= 0)
                            {
                                characterQuest = owningCharacter.Quests[index];
                                if (!characterQuest.IsAllTasksDone(owningCharacter))
                                    menuActions.Add(abandonMenuAction);
                                else
                                    menuActions.Add(completeMenuAction);
                            }
                            else
                            {
                                characterQuest = CharacterQuest.Create(quest);
                                menuActions.Add(acceptMenuAction);
                                menuActions.Add(declineMenuAction);
                            }
                            uiNpcDialog.uiCharacterQuest.Setup(characterQuest, owningCharacter, index);
                        }
                        uiNpcDialog.uiCharacterQuest.Show();
                    }
                    break;
                case NpcDialogType.Shop:
                    if (uiNpcDialog.onSwitchToSellItemDialog == null)
                        uiNpcDialog.onSwitchToSellItemDialog.Invoke();
                    if (uiNpcDialog.uiSellItemRoot != null)
                        uiNpcDialog.uiSellItemRoot.SetActive(true);
                    UINpcSellItem tempUiNpcSellItem;
                    uiNpcDialog.CacheSellItemList.Generate(sellItems, (index, sellItem, ui) =>
                    {
                        tempUiNpcSellItem = ui.GetComponent<UINpcSellItem>();
                        tempUiNpcSellItem.Setup(sellItem, index);
                        tempUiNpcSellItem.Show();
                        uiNpcDialog.CacheSellItemSelectionManager.Add(tempUiNpcSellItem);
                    });
                    break;
                case NpcDialogType.CraftItem:
                    if (uiNpcDialog.onSwitchToCraftItemDialog == null)
                        uiNpcDialog.onSwitchToCraftItemDialog.Invoke();
                    if (uiNpcDialog.uiCraftItem != null)
                    {
                        craftingItem = itemCraft.CraftingItem;
                        if (craftingItem != null)
                        {
                            confirmMenuAction = new UINpcDialogMenuAction();
                            cancelMenuAction = new UINpcDialogMenuAction();
                            confirmMenuAction.title = uiNpcDialog.MessageCraftItemConfirm;
                            confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                            cancelMenuAction.title = uiNpcDialog.MessageCraftItemCancel;
                            cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                            uiNpcDialog.uiCraftItem.Setup(CrafterType.Npc, null, itemCraft);
                            menuActions.Add(confirmMenuAction);
                            menuActions.Add(cancelMenuAction);
                        }
                        uiNpcDialog.uiCraftItem.Show();
                    }
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    if (uiNpcDialog.onSwitchToSaveRespawnPointDialog != null)
                        uiNpcDialog.onSwitchToSaveRespawnPointDialog.Invoke();
                    confirmMenuAction = new UINpcDialogMenuAction();
                    cancelMenuAction = new UINpcDialogMenuAction();
                    confirmMenuAction.title = uiNpcDialog.MessageSaveRespawnPointConfirm;
                    confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                    cancelMenuAction.title = uiNpcDialog.MessageSaveRespawnPointCancel;
                    cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(confirmMenuAction);
                    menuActions.Add(cancelMenuAction);
                    break;
                case NpcDialogType.Warp:
                    if (uiNpcDialog.onSwitchToWarpDialog != null)
                        uiNpcDialog.onSwitchToWarpDialog.Invoke();
                    confirmMenuAction = new UINpcDialogMenuAction();
                    cancelMenuAction = new UINpcDialogMenuAction();
                    confirmMenuAction.title = uiNpcDialog.MessageWarpConfirm;
                    confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                    cancelMenuAction.title = uiNpcDialog.MessageWarpCancel;
                    cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(confirmMenuAction);
                    menuActions.Add(cancelMenuAction);
                    break;
                case NpcDialogType.RefineItem:
                    if (uiNpcDialog.onSwitchToRefineItemDialog != null)
                        uiNpcDialog.onSwitchToRefineItemDialog.Invoke();
                    confirmMenuAction = new UINpcDialogMenuAction();
                    cancelMenuAction = new UINpcDialogMenuAction();
                    confirmMenuAction.title = uiNpcDialog.MessageRefineItemConfirm;
                    confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                    cancelMenuAction.title = uiNpcDialog.MessageRefineItemCancel;
                    cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(confirmMenuAction);
                    menuActions.Add(cancelMenuAction);
                    break;
                case NpcDialogType.DismantleItem:
                    if (uiNpcDialog.onSwitchToDismantleItemDialog != null)
                        uiNpcDialog.onSwitchToDismantleItemDialog.Invoke();
                    confirmMenuAction = new UINpcDialogMenuAction();
                    cancelMenuAction = new UINpcDialogMenuAction();
                    confirmMenuAction.title = uiNpcDialog.MessageDismantleItemConfirm;
                    confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                    cancelMenuAction.title = uiNpcDialog.MessageDismantleItemCancel;
                    cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(confirmMenuAction);
                    menuActions.Add(cancelMenuAction);
                    break;
                case NpcDialogType.PlayerStorage:
                    if (uiNpcDialog.onSwitchToPlayerStorageDialog != null)
                        uiNpcDialog.onSwitchToPlayerStorageDialog.Invoke();
                    UINpcDialogMenuAction playerStorageConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction playerStorageCancelAction = new UINpcDialogMenuAction();
                    playerStorageConfirmAction.title = uiNpcDialog.MessagePlayerStorageConfirm;
                    playerStorageConfirmAction.menuIndex = CONFIRM_MENU_INDEX;
                    playerStorageCancelAction.title = uiNpcDialog.MessagePlayerStorageCancel;
                    playerStorageCancelAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(playerStorageConfirmAction);
                    menuActions.Add(playerStorageCancelAction);
                    break;
                case NpcDialogType.GuildStorage:
                    if (uiNpcDialog.onSwitchToGuildStorageDialog != null)
                        uiNpcDialog.onSwitchToGuildStorageDialog.Invoke();
                    UINpcDialogMenuAction guildStorageConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction guildStorageCancelAction = new UINpcDialogMenuAction();
                    guildStorageConfirmAction.title = uiNpcDialog.MessageGuildStorageConfirm;
                    guildStorageConfirmAction.menuIndex = CONFIRM_MENU_INDEX;
                    guildStorageCancelAction.title = uiNpcDialog.MessageGuildStorageCancel;
                    guildStorageCancelAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(guildStorageConfirmAction);
                    menuActions.Add(guildStorageCancelAction);
                    break;
                case NpcDialogType.RepairItem:
                    if (uiNpcDialog.onSwitchToRepairItemDialog != null)
                        uiNpcDialog.onSwitchToRepairItemDialog.Invoke();
                    confirmMenuAction = new UINpcDialogMenuAction();
                    cancelMenuAction = new UINpcDialogMenuAction();
                    confirmMenuAction.title = uiNpcDialog.MessageRepairItemConfirm;
                    confirmMenuAction.menuIndex = CONFIRM_MENU_INDEX;
                    cancelMenuAction.title = uiNpcDialog.MessageRepairItemCancel;
                    cancelMenuAction.menuIndex = CANCEL_MENU_INDEX;
                    menuActions.Add(confirmMenuAction);
                    menuActions.Add(cancelMenuAction);
                    break;
            }

            // Menu
            if (uiNpcDialog.uiMenuRoot != null)
                uiNpcDialog.uiMenuRoot.SetActive(menuActions.Count > 0);
            UINpcDialogMenu tempUiNpcDialogMenu;
            uiNpcDialog.CacheMenuList.Generate(menuActions, (index, menuAction, ui) =>
            {
                tempUiNpcDialogMenu = ui.GetComponent<UINpcDialogMenu>();
                tempUiNpcDialogMenu.Data = menuAction;
                tempUiNpcDialogMenu.uiNpcDialog = uiNpcDialog;
                tempUiNpcDialogMenu.Show();
            });
        }

        public override void UnrenderUI(UINpcDialog uiNpcDialog)
        {
            if (uiNpcDialog.uiMenuRoot != null)
                uiNpcDialog.uiMenuRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemRoot != null)
                uiNpcDialog.uiSellItemRoot.SetActive(false);

            if (uiNpcDialog.uiSellItemDialog != null)
                uiNpcDialog.uiSellItemDialog.Hide();

            if (uiNpcDialog.uiCharacterQuest != null)
                uiNpcDialog.uiCharacterQuest.Hide();

            if (uiNpcDialog.uiCraftItem != null)
                uiNpcDialog.uiCraftItem.Hide();
        }

        public override bool ValidateDialog(BasePlayerCharacterEntity characterEntity)
        {
            switch (type)
            {
                case NpcDialogType.Quest:
                    if (quest == null)
                    {
                        // Validate quest data
                        Logging.LogWarning(ToString(), "Quest dialog's quest is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.CraftItem:
                    if (itemCraft.CraftingItem == null)
                    {
                        // Validate crafting item
                        Logging.LogWarning(ToString(), "Item craft dialog's crafting item is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    if (saveRespawnMap == null)
                    {
                        // Validate quest data
                        Logging.LogWarning(ToString(), "Save respawn point dialog's save respawn map is empty");
                        return false;
                    }
                    break;
                case NpcDialogType.Warp:
                    if (warpMap == null)
                    {
                        // Validate quest data
                        Logging.LogWarning(ToString(), "Warp dialog's warp map is empty");
                        return false;
                    }
                    break;
            }
            return true;
        }

        public override void GoToNextDialog(BasePlayerCharacterEntity characterEntity, byte menuIndex)
        {
            characterEntity.CurrentNpcDialog = null;
            characterEntity.CompletingQuest = null;
            characterEntity.NpcDialogAfterSelectRewardItem = null;
            // This dialog is current NPC dialog
            switch (type)
            {
                case NpcDialogType.Normal:
                    if (menuIndex >= menus.Length)
                    {
                        // Invalid menu, so no next dialog, so return itself
                        characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(this, characterEntity);
                        return;
                    }
                    // Changing current npc dialog
                    NpcDialogMenu selectedMenu = menus[menuIndex];
                    if (!selectedMenu.IsPassConditions(characterEntity) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                    {
                        // Close dialog, so return null
                        return;
                    }
                    characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(selectedMenu.dialog, characterEntity);
                    return;
                case NpcDialogType.Quest:
                    switch (menuIndex)
                    {
                        case QUEST_ACCEPT_MENU_INDEX:
                            characterEntity.AcceptQuest(quest.DataId);
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(questAcceptedDialog, characterEntity);
                            return;
                        case QUEST_DECLINE_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(questDeclinedDialog, characterEntity);
                            return;
                        case QUEST_ABANDON_MENU_INDEX:
                            characterEntity.AbandonQuest(quest.DataId);
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(questAbandonedDialog, characterEntity);
                            return;
                        case QUEST_COMPLETE_MENU_INDEX:
                            if (quest.selectableRewardItems != null &&
                                quest.selectableRewardItems.Length > 0)
                            {
                                // Show quest reward dialog at client
                                characterEntity.CallOwnerShowQuestRewardItemSelection(quest.DataId);
                                characterEntity.CompletingQuest = quest;
                                characterEntity.NpcDialogAfterSelectRewardItem = GetValidatedDialogOrNull(questCompletedDialog, characterEntity);
                            }
                            else
                            {
                                // No selectable reward items, complete the quest immediately
                                characterEntity.CompleteQuest(quest.DataId, 0);
                                characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(questCompletedDialog, characterEntity);
                            }
                            return;
                    }
                    return;
                case NpcDialogType.CraftItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            UITextKeys gameMessage;
                            if (itemCraft.CanCraft(characterEntity, out gameMessage))
                            {
                                itemCraft.CraftItem(characterEntity);
                                characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(craftDoneDialog, characterEntity);
                                return;
                            }
                            // Cannot craft item
                            switch (gameMessage)
                            {
                                case UITextKeys.UI_ERROR_WILL_OVERWHELMING:
                                    characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(craftItemWillOverwhelmingDialog, characterEntity);
                                    return;
                                default:
                                    characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(craftNotMeetRequirementsDialog, characterEntity);
                                    return;
                            }
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(craftCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.SaveRespawnPoint:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.RespawnMapName = saveRespawnMap.Id;
                            characterEntity.RespawnPosition = saveRespawnPosition;
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(saveRespawnConfirmDialog, characterEntity);
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(saveRespawnCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.Warp:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            BaseGameNetworkManager.Singleton.WarpCharacter(warpPortalType, characterEntity, warpMap.Id, warpPosition, warpOverrideRotation, warpRotation);
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(warpCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.RefineItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.CallOwnerShowNpcRefineItem();
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(refineItemCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.DismantleItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.CallOwnerShowNpcDismantleItem();
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(dismantleItemCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.PlayerStorage:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            StorageId storageId;
                            if (characterEntity.GetStorageId(StorageType.Player, 0, out storageId))
                                GameInstance.ServerStorageHandlers.OpenStorage(characterEntity.ConnectionId, characterEntity, storageId);
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(storageCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.GuildStorage:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            StorageId storageId;
                            if (characterEntity.GetStorageId(StorageType.Guild, 0, out storageId))
                                GameInstance.ServerStorageHandlers.OpenStorage(characterEntity.ConnectionId, characterEntity, storageId);
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(storageCancelDialog, characterEntity);
                            return;
                    }
                    return;
                case NpcDialogType.RepairItem:
                    switch (menuIndex)
                    {
                        case CONFIRM_MENU_INDEX:
                            characterEntity.CallOwnerShowNpcRepairItem();
                            return;
                        case CANCEL_MENU_INDEX:
                            characterEntity.CurrentNpcDialog = GetValidatedDialogOrNull(repairItemCancelDialog, characterEntity);
                            return;
                    }
                    return;
            }
        }

        protected override void SetDialogByPort(NodePort from, NodePort to)
        {
            if (from.node != this)
                return;

            BaseNpcDialog dialog = null;
            if (to != null && to.node != null)
                dialog = to.node as BaseNpcDialog;

            int arrayIndex;
            if (from.fieldName.Contains("menus ") && int.TryParse(from.fieldName.Split(' ')[1], out arrayIndex) && arrayIndex < menus.Length)
                menus[arrayIndex].dialog = dialog;

            if (from.fieldName.Equals(nameof(questAcceptedDialog)))
                questAcceptedDialog = dialog;

            if (from.fieldName.Equals(nameof(questDeclinedDialog)))
                questDeclinedDialog = dialog;

            if (from.fieldName.Equals(nameof(questAbandonedDialog)))
                questAbandonedDialog = dialog;

            if (from.fieldName.Equals(nameof(questCompletedDialog)))
                questCompletedDialog = dialog;

            if (from.fieldName.Equals(nameof(craftDoneDialog)))
                craftDoneDialog = dialog;

            if (from.fieldName.Equals(nameof(craftItemWillOverwhelmingDialog)))
                craftItemWillOverwhelmingDialog = dialog;

            if (from.fieldName.Equals(nameof(craftNotMeetRequirementsDialog)))
                craftNotMeetRequirementsDialog = dialog;

            if (from.fieldName.Equals(nameof(craftCancelDialog)))
                craftCancelDialog = dialog;

            if (from.fieldName.Equals(nameof(saveRespawnConfirmDialog)))
                saveRespawnConfirmDialog = dialog;

            if (from.fieldName.Equals(nameof(saveRespawnCancelDialog)))
                saveRespawnCancelDialog = dialog;

            if (from.fieldName.Equals(nameof(warpCancelDialog)))
                warpCancelDialog = dialog;

            if (from.fieldName.Equals(nameof(refineItemCancelDialog)))
                refineItemCancelDialog = dialog;

            if (from.fieldName.Equals(nameof(storageCancelDialog)))
                storageCancelDialog = dialog;

            if (from.fieldName.Equals(nameof(repairItemCancelDialog)))
                repairItemCancelDialog = dialog;
        }

        public override bool IsShop
        {
            get { return type == NpcDialogType.Shop; }
        }
    }
}
