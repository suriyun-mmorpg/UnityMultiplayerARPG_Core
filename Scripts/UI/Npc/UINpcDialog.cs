using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class UINpcDialog : UISelectionEntry<NpcDialog>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";

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
        public string messageQuestAccept = "Accept";
        public string messageQuestDecline = "Decline";
        public string messageQuestAbandon = "Abandon";
        public string messageQuestComplete = "Complete";
        public string messageCraftItemStart = "Craft";
        public string messageCraftItemCancel = "Cancel";

        [Header("Event")]
        public UnityEvent onSwitchToNormalDialog;
        public UnityEvent onSwitchToQuestDialog;
        public UnityEvent onSwitchToSellItemDialog;
        public UnityEvent onSwitchToCraftItemDialog;

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
                uiTextTitle.text = string.Format(titleFormat, Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);

            Quest quest = null;
            ItemCraft itemCraft = null;
            List<NpcSellItem> sellItems = new List<NpcSellItem>();
            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            switch (Data.type)
            {
                case NpcDialogType.Normal:
                case NpcDialogType.SaveRespawnPoint:
                    if (onSwitchToNormalDialog == null)
                        onSwitchToNormalDialog.Invoke();
                    NpcDialogMenu[] menus = Data.menus;
                    for (int i = 0; i < menus.Length; ++i)
                    {
                        NpcDialogMenu menu = menus[i];
                        if (menu.IsPassConditions(owningCharacter))
                        {
                            UINpcDialogMenuAction menuAction = new UINpcDialogMenuAction();
                            menuAction.title = menu.title;
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
                            acceptMenuAction.title = messageQuestAccept;
                            acceptMenuAction.menuIndex = NpcDialog.QUEST_ACCEPT_MENU_INDEX;
                            declineMenuAction.title = messageQuestDecline;
                            declineMenuAction.menuIndex = NpcDialog.QUEST_DECLINE_MENU_INDEX;
                            abandonMenuAction.title = messageQuestAbandon;
                            abandonMenuAction.menuIndex = NpcDialog.QUEST_ABANDON_MENU_INDEX;
                            completeMenuAction.title = messageQuestComplete;
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
                        itemCraft = Data.itemCraft;
                        if (itemCraft != null)
                        {
                            UINpcDialogMenuAction startMenuAction = new UINpcDialogMenuAction();
                            UINpcDialogMenuAction cancelMenuAction = new UINpcDialogMenuAction();
                            startMenuAction.title = messageCraftItemStart;
                            startMenuAction.menuIndex = NpcDialog.CRAFT_ITEM_START_MENU_INDEX;
                            cancelMenuAction.title = messageCraftItemCancel;
                            cancelMenuAction.menuIndex = NpcDialog.CRAFT_ITEM_CANCEL_MENU_INDEX;
                            uiCraftItem.Data = Data.itemCraft;
                            menuActions.Add(startMenuAction);
                            menuActions.Add(cancelMenuAction);
                        }
                    }
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
            CacheSellItemList.Generate(sellItems, (index, sellItem, ui) =>
            {
                UINpcSellItem uiNpcSellItem = ui.GetComponent<UINpcSellItem>();
                uiNpcSellItem.Setup(sellItem, index);
            });
            // Craft Item
            if (uiCraftItem != null)
            {
                if (itemCraft == null)
                    uiCraftItem.Hide();
                else
                    uiCraftItem.Show();
            }
        }
    }
}
