using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UINpcDialog : UISelectionEntry<NpcDialog>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";

        [Header("UI Elements")]
        public Text textTitle;
        public Text textDescription;
        public UICharacterQuest uiCharacterQuest;
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

        [Header("Event")]
        public UnityEvent onSwitchToNormalDialog;
        public UnityEvent onSwitchToQuestDialog;
        public UnityEvent onSwitchToSellItemDialog;

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
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;

            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, Data == null ? "Unknow" : Data.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, Data == null ? "N/A" : Data.description);

            Quest quest = null;
            List<NpcSellItem> sellItems = new List<NpcSellItem>();
            List<UINpcDialogMenuAction> menuActions = new List<UINpcDialogMenuAction>();
            switch (Data.type)
            {
                case NpcDialogType.Normal:
                    if (onSwitchToNormalDialog == null)
                        onSwitchToNormalDialog.Invoke();
                    var menus = Data.menus;
                    for (var i = 0; i < menus.Length; ++i)
                    {
                        var menu = menus[i];
                        if (menu.IsPassConditions(owningCharacter))
                        {
                            var menuAction = new UINpcDialogMenuAction();
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
                            var acceptMenuAction = new UINpcDialogMenuAction();
                            var declineMenuAction = new UINpcDialogMenuAction();
                            var abandonMenuAction = new UINpcDialogMenuAction();
                            var completeMenuAction = new UINpcDialogMenuAction();
                            acceptMenuAction.title = messageQuestAccept;
                            acceptMenuAction.menuIndex = NpcDialog.QUEST_ACCEPT_MENU_INDEX;
                            declineMenuAction.title = messageQuestDecline;
                            declineMenuAction.menuIndex = NpcDialog.QUEST_DECLINE_MENU_INDEX;
                            abandonMenuAction.title = messageQuestAbandon;
                            abandonMenuAction.menuIndex = NpcDialog.QUEST_ABANDON_MENU_INDEX;
                            completeMenuAction.title = messageQuestComplete;
                            completeMenuAction.menuIndex = NpcDialog.QUEST_COMPLETE_MENU_INDEX;

                            CharacterQuest characterQuest;
                            var index = owningCharacter.IndexOfQuest(quest.DataId);
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
                            uiCharacterQuest.Show();
                        }
                    }
                    break;
                case NpcDialogType.Shop:
                    if (onSwitchToSellItemDialog == null)
                        onSwitchToSellItemDialog.Invoke();
                    sellItems.AddRange(Data.sellItems);
                    break;
            }
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
                var uiNpcSellItem = ui.GetComponent<UINpcSellItem>();
                uiNpcSellItem.Data = sellItem;
            });
            // Menu
            if (uiMenuRoot != null)
                uiMenuRoot.SetActive(menuActions.Count > 0);
            CacheMenuList.Generate(menuActions, (index, menuAction, ui) =>
            {
                var uiNpcDialogMenu = ui.GetComponent<UINpcDialogMenu>();
                uiNpcDialogMenu.Data = menuAction;
                uiNpcDialogMenu.uiNpcDialog = this;
                uiNpcDialogMenu.Show();
            });
        }
    }
}
