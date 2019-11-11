using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class UINpcDialog : UISelectionEntry<NpcDialog>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public UICharacterQuest uiCharacterQuest;
        public UICraftItem uiCraftItem;
        public UINpcDialogMenu uiMenuPrefab;
        public GameObject uiMenuRoot;
        public Transform uiMenuContainer;
        public UINpcSellItem uiSellItemPrefab;
        public GameObject uiSellItemRoot;
        public Transform uiSellItemContainer;

        [Header("Quest Accept Menu Title")]
        public string messageQuestAccept = "Accept";
        public LanguageData[] messageQuestAcceptTitles;
        public string MessageQuestAccept
        {
            get { return Language.GetText(messageQuestAcceptTitles, messageQuestAccept); }
        }

        [Header("Quest Decline Menu Title")]
        public string messageQuestDecline = "Decline";
        public LanguageData[] messageQuestDeclineTitles;
        public string MessageQuestDecline
        {
            get { return Language.GetText(messageQuestDeclineTitles, messageQuestDecline); }
        }

        [Header("Quest Abandon Menu Title")]
        public string messageQuestAbandon = "Abandon";
        public LanguageData[] messageQuestAbandonTitles;
        public string MessageQuestAbandon
        {
            get { return Language.GetText(messageQuestAbandonTitles, messageQuestAbandon); }
        }

        [Header("Quest Complete Menu Title")]
        public string messageQuestComplete = "Complete";
        public LanguageData[] messageQuestCompleteTitles;
        public string MessageQuestComplete
        {
            get { return Language.GetText(messageQuestCompleteTitles, messageQuestComplete); }
        }

        [Header("Craft Item Confirm Menu Title")]
        public string messageCraftItemConfirm = "Craft";
        public LanguageData[] messageCraftItemConfirmTitles;
        public string MessageCraftItemConfirm
        {
            get { return Language.GetText(messageCraftItemConfirmTitles, messageCraftItemConfirm); }
        }

        [Header("Craft Item Cancel Menu Title")]
        public string messageCraftItemCancel = "Cancel";
        public LanguageData[] messageCraftItemCancelTitles;
        public string MessageCraftItemCancel
        {
            get { return Language.GetText(messageCraftItemCancelTitles, messageCraftItemCancel); }
        }

        [Header("Save Respawn Point Confirm Menu Title")]
        public string messageSaveRespawnPointConfirm = "Confirm";
        public LanguageData[] messageSaveRespawnPointConfirmTitles;
        public string MessageSaveRespawnPointConfirm
        {
            get { return Language.GetText(messageSaveRespawnPointConfirmTitles, messageSaveRespawnPointConfirm); }
        }

        [Header("Save Respawn Point Cancel Menu Title")]
        public string messageSaveRespawnPointCancel = "Cancel";
        public LanguageData[] messageSaveRespawnPointCancelTitles;
        public string MessageSaveRespawnPointCancel
        {
            get { return Language.GetText(messageSaveRespawnPointCancelTitles, messageSaveRespawnPointCancel); }
        }

        [Header("Warp Confirm Menu Title")]
        public string messageWarpConfirm = "Confirm";
        public LanguageData[] messageWarpConfirmTitles;
        public string MessageWarpConfirm
        {
            get { return Language.GetText(messageWarpConfirmTitles, messageWarpConfirm); }
        }

        [Header("Warp Cancel Menu Title")]
        public string messageWarpCancel = "Cancel";
        public LanguageData[] messageWarpCancelTitles;
        public string MessageWarpCancel
        {
            get { return Language.GetText(messageWarpCancelTitles, messageWarpCancel); }
        }

        [Header("Refine Item Confirm Menu Title")]
        public string messageRefineItemConfirm = "Refine Item";
        public LanguageData[] messageRefineItemConfirmTitles;
        public string MessageRefineItemConfirm
        {
            get { return Language.GetText(messageRefineItemConfirmTitles, messageRefineItemConfirm); }
        }

        [Header("Refine Item Cancel Menu Title")]
        public string messageRefineItemCancel = "Cancel";
        public LanguageData[] messageRefineItemCancelTitles;
        public string MessageRefineItemCancel
        {
            get { return Language.GetText(messageRefineItemCancelTitles, messageRefineItemCancel); }
        }

        [Header("Open Player Storage Confirm Menu Title")]
        public string messagePlayerStorageConfirm = "Open Storage";
        public LanguageData[] messagePlayerStorageConfirmTitles;
        public string MessagePlayerStorageConfirm
        {
            get { return Language.GetText(messagePlayerStorageConfirmTitles, messagePlayerStorageConfirm); }
        }

        [Header("Open Player Storage Cancel Menu Title")]
        public string messagePlayerStorageCancel = "Cancel";
        public LanguageData[] messagePlayerStorageCancelTitles;
        public string MessagePlayerStorageCancel
        {
            get { return Language.GetText(messagePlayerStorageCancelTitles, messagePlayerStorageCancel); }
        }

        [Header("Open Guild Storage Confirm Menu Title")]
        public string messageGuildStorageConfirm = "Open Storage";
        public LanguageData[] messageGuildStorageConfirmTitles;
        public string MessageGuildStorageConfirm
        {
            get { return Language.GetText(messageGuildStorageConfirmTitles, messageGuildStorageConfirm); }
        }

        [Header("Open Guild Storage Cancel Menu Title")]
        public string messageGuildStorageCancel = "Cancel";
        public LanguageData[] messageGuildStorageCancelTitles;
        public string MessageGuildStorageCancel
        {
            get { return Language.GetText(messageGuildStorageCancelTitles, messageGuildStorageCancel); }
        }

        [Header("Event")]
        public UnityEvent onSwitchToNormalDialog;
        public UnityEvent onSwitchToQuestDialog;
        public UnityEvent onSwitchToSellItemDialog;
        public UnityEvent onSwitchToCraftItemDialog;
        public UnityEvent onSwitchToSaveRespawnPointDialog;
        public UnityEvent onSwitchToWarpDialog;
        public UnityEvent onSwitchToRefineItemDialog;
        public UnityEvent onSwitchToPlayerStorageDialog;
        public UnityEvent onSwitchToGuildStorageDialog;

        private UIList cacheMenuList;
        public UIList CacheMenuList
        {
            get
            {
                if (cacheMenuList == null)
                {
                    cacheMenuList = gameObject.AddComponent<UIList>();
                    cacheMenuList.uiPrefab = uiMenuPrefab.gameObject;
                    cacheMenuList.uiContainer = uiMenuContainer;
                }
                return cacheMenuList;
            }
        }

        private UIList cacheSellItemList;
        public UIList CacheSellItemList
        {
            get
            {
                if (cacheSellItemList == null)
                {
                    cacheSellItemList = gameObject.AddComponent<UIList>();
                    cacheSellItemList.uiPrefab = uiSellItemPrefab.gameObject;
                    cacheSellItemList.uiContainer = uiSellItemContainer;
                }
                return cacheSellItemList;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);
            }

            Quest quest = null;
            Item craftingItem = null;
            List<NpcSellItem> sellItems = new List<NpcSellItem>();
            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            switch (Data.type)
            {
                case NpcDialogType.Normal:
                    if (onSwitchToNormalDialog == null)
                        onSwitchToNormalDialog.Invoke();
                    NpcDialogMenu[] menus = Data.menus;
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
                    if (onSwitchToQuestDialog == null)
                        onSwitchToQuestDialog.Invoke();
                    if (uiCharacterQuest != null)
                    {
                        quest = Data.quest;
                        if (quest != null)
                        {
                            UINpcDialogMenuAction acceptMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction declineMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction abandonMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction completeMenuAction = new UINpcDialogMenuAction();
                            acceptMenuAction.title = MessageQuestAccept;
                            acceptMenuAction.menuIndex = NpcDialog.QUEST_ACCEPT_MENU_INDEX;
                            declineMenuAction.title = MessageQuestDecline;
                            declineMenuAction.menuIndex = NpcDialog.QUEST_DECLINE_MENU_INDEX;
                            abandonMenuAction.title = MessageQuestAbandon;
                            abandonMenuAction.menuIndex = NpcDialog.QUEST_ABANDON_MENU_INDEX;
                            completeMenuAction.title = MessageQuestComplete;
                            completeMenuAction.menuIndex = NpcDialog.QUEST_COMPLETE_MENU_INDEX;

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
                            uiCharacterQuest.Setup(characterQuest, owningCharacter, index);
                        }
                    }
                    break;
                case NpcDialogType.Shop:
                    if (onSwitchToSellItemDialog == null)
                        onSwitchToSellItemDialog.Invoke();
                    sellItems.AddRange(Data.sellItems);
                    break;
                case NpcDialogType.CraftItem:
                    if (onSwitchToCraftItemDialog == null)
                        onSwitchToCraftItemDialog.Invoke();
                    if (uiCraftItem != null)
                    {
                        craftingItem = Data.itemCraft.CraftingItem;
                        if (craftingItem != null)
                        {
                            UINpcDialogMenuAction startMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction cancelMenuAction = new UINpcDialogMenuAction();
                            startMenuAction.title = MessageCraftItemConfirm;
                            startMenuAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                            cancelMenuAction.title = MessageCraftItemCancel;
                            cancelMenuAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                            uiCraftItem.SetupForNpc(Data.itemCraft);
                            menuActions.Add(startMenuAction);
                            menuActions.Add(cancelMenuAction);
                        }
                    }
                    break;
                case NpcDialogType.SaveRespawnPoint:
                    if (onSwitchToSaveRespawnPointDialog != null)
                        onSwitchToSaveRespawnPointDialog.Invoke();
                    UINpcDialogMenuAction saveRespawnPointConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction saveRespawnPointCancelAction = new UINpcDialogMenuAction();
                    saveRespawnPointConfirmAction.title = MessageSaveRespawnPointConfirm;
                    saveRespawnPointConfirmAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                    saveRespawnPointCancelAction.title = MessageSaveRespawnPointCancel;
                    saveRespawnPointCancelAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                    menuActions.Add(saveRespawnPointConfirmAction);
                    menuActions.Add(saveRespawnPointCancelAction);
                    break;
                case NpcDialogType.Warp:
                    if (onSwitchToWarpDialog != null)
                        onSwitchToWarpDialog.Invoke();
                    UINpcDialogMenuAction warpConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction warpCancelAction = new UINpcDialogMenuAction();
                    warpConfirmAction.title = MessageWarpConfirm;
                    warpConfirmAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                    warpCancelAction.title = MessageWarpCancel;
                    warpCancelAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                    menuActions.Add(warpConfirmAction);
                    menuActions.Add(warpCancelAction);
                    break;
                case NpcDialogType.RefineItem:
                    if (onSwitchToRefineItemDialog != null)
                        onSwitchToRefineItemDialog.Invoke();
                    UINpcDialogMenuAction refineItemConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction refineItemCancelAction = new UINpcDialogMenuAction();
                    refineItemConfirmAction.title = MessageRefineItemConfirm;
                    refineItemConfirmAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                    refineItemCancelAction.title = MessageRefineItemCancel;
                    refineItemCancelAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                    menuActions.Add(refineItemConfirmAction);
                    menuActions.Add(refineItemCancelAction);
                    break;
                case NpcDialogType.PlayerStorage:
                    if (onSwitchToPlayerStorageDialog != null)
                        onSwitchToPlayerStorageDialog.Invoke();
                    UINpcDialogMenuAction playerStorageConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction playerStorageCancelAction = new UINpcDialogMenuAction();
                    playerStorageConfirmAction.title = MessagePlayerStorageConfirm;
                    playerStorageConfirmAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                    playerStorageCancelAction.title = MessagePlayerStorageCancel;
                    playerStorageCancelAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                    menuActions.Add(playerStorageConfirmAction);
                    menuActions.Add(playerStorageCancelAction);
                    break;
                case NpcDialogType.GuildStorage:
                    if (onSwitchToGuildStorageDialog != null)
                        onSwitchToGuildStorageDialog.Invoke();
                    UINpcDialogMenuAction guildStorageConfirmAction = new UINpcDialogMenuAction();
                    UINpcDialogMenuAction guildStorageCancelAction = new UINpcDialogMenuAction();
                    guildStorageConfirmAction.title = MessageGuildStorageConfirm;
                    guildStorageConfirmAction.menuIndex = NpcDialog.CONFIRM_MENU_INDEX;
                    guildStorageCancelAction.title = MessageGuildStorageCancel;
                    guildStorageCancelAction.menuIndex = NpcDialog.CANCEL_MENU_INDEX;
                    menuActions.Add(guildStorageConfirmAction);
                    menuActions.Add(guildStorageCancelAction);
                    break;
            }
            // Menu
            if (uiMenuRoot != null)
                uiMenuRoot.SetActive(menuActions.Count > 0);
            CacheMenuList.Generate(menuActions, (index, menuAction, ui) =>
            {
                UINpcDialogMenu uiNpcDialogMenu = ui.GetComponent<UINpcDialogMenu>();
                uiNpcDialogMenu.Data = menuAction;
                uiNpcDialogMenu.uiNpcDialog = this;
                uiNpcDialogMenu.Show();
            });
            // Quest
            if (uiCharacterQuest != null)
            {
                if (quest == null)
                    uiCharacterQuest.Hide();
                else
                    uiCharacterQuest.Show();
            }
            // Shop
            if (uiSellItemRoot != null)
                uiSellItemRoot.SetActive(sellItems.Count > 0);

            UINpcSellItem tempUiNpcSellItem;
            CacheSellItemList.Generate(sellItems, (index, sellItem, ui) =>
            {
                tempUiNpcSellItem = ui.GetComponent<UINpcSellItem>();
                tempUiNpcSellItem.Setup(sellItem, index);
                tempUiNpcSellItem.Show();
            });

            // Craft Item
            if (uiCraftItem != null)
            {
                if (craftingItem == null)
                    uiCraftItem.Hide();
                else
                    uiCraftItem.Show();
            }
        }
    }
}
