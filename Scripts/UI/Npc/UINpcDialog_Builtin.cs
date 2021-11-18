using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class UINpcDialog
    {
        [Header("Built-in UIs")]
        [Header("Menu")]
        public UINpcDialogMenu uiMenuPrefab;
        public Transform uiMenuContainer;
        public GameObject uiMenuRoot;

        [Header("Shop Dialog")]
        public UINpcSellItem uiSellItemDialog;
        public UINpcSellItem uiSellItemPrefab;
        public Transform uiSellItemContainer;
        public GameObject uiSellItemRoot;

        [Header("Quest Dialog")]
        public UICharacterQuest uiCharacterQuest;

        [Header("Craft Item Dialog")]
        public UIItemCraft uiCraftItem;

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

        [Header("Dismantle Item Confirm Menu Title")]
        public string messageDismantleItemConfirm = "Dismantle Item";
        public LanguageData[] messageDismantleItemConfirmTitles;
        public string MessageDismantleItemConfirm
        {
            get { return Language.GetText(messageDismantleItemConfirmTitles, messageDismantleItemConfirm); }
        }

        [Header("Dismantle Item Cancel Menu Title")]
        public string messageDismantleItemCancel = "Cancel";
        public LanguageData[] messageDismantleItemCancelTitles;
        public string MessageDismantleItemCancel
        {
            get { return Language.GetText(messageDismantleItemCancelTitles, messageDismantleItemCancel); }
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

        [Header("Repair Item Confirm Menu Title")]
        public string messageRepairItemConfirm = "Repair Item";
        public LanguageData[] messageRepairItemConfirmTitles;
        public string MessageRepairItemConfirm
        {
            get { return Language.GetText(messageRepairItemConfirmTitles, messageRepairItemConfirm); }
        }

        [Header("Repair Item Cancel Menu Title")]
        public string messageRepairItemCancel = "Cancel";
        public LanguageData[] messageRepairItemCancelTitles;
        public string MessageRepairItemCancel
        {
            get { return Language.GetText(messageRepairItemCancelTitles, messageRepairItemCancel); }
        }

        [Header("Event")]
        public UnityEvent onSwitchToNormalDialog;
        public UnityEvent onSwitchToQuestDialog;
        public UnityEvent onSwitchToSellItemDialog;
        public UnityEvent onSwitchToCraftItemDialog;
        public UnityEvent onSwitchToSaveRespawnPointDialog;
        public UnityEvent onSwitchToWarpDialog;
        public UnityEvent onSwitchToRefineItemDialog;
        public UnityEvent onSwitchToDismantleItemDialog;
        public UnityEvent onSwitchToPlayerStorageDialog;
        public UnityEvent onSwitchToGuildStorageDialog;
        public UnityEvent onSwitchToRepairItemDialog;

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

        private UINpcSellItemSelectionManager cacheSellItemSelectionManager;
        public UINpcSellItemSelectionManager CacheSellItemSelectionManager
        {
            get
            {
                if (cacheSellItemSelectionManager == null)
                    cacheSellItemSelectionManager = gameObject.GetOrAddComponent<UINpcSellItemSelectionManager>();
                cacheSellItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSellItemSelectionManager;
            }
        }

        [DevExtMethods("Show")]
        protected void Show_Builtin()
        {
            CacheSellItemSelectionManager.eventOnSelected.RemoveListener(OnSelectSellItem);
            CacheSellItemSelectionManager.eventOnSelected.AddListener(OnSelectSellItem);
            CacheSellItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectSellItem);
            CacheSellItemSelectionManager.eventOnDeselected.AddListener(OnDeselectSellItem);
            if (uiSellItemDialog != null)
                uiSellItemDialog.onHide.AddListener(OnSellItemDialogHide);
        }

        [DevExtMethods("Hide")]
        protected void Hide_Builtin()
        {
            if (uiSellItemDialog != null)
                uiSellItemDialog.onHide.RemoveListener(OnSellItemDialogHide);
            CacheSellItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSellItemDialogHide()
        {
            CacheSellItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectSellItem(UINpcSellItem ui)
        {
            if (uiSellItemDialog != null)
            {
                uiSellItemDialog.selectionManager = CacheSellItemSelectionManager;
                uiSellItemDialog.Setup(ui.Data, ui.indexOfData);
                uiSellItemDialog.Show();
            }
        }

        protected void OnDeselectSellItem(UINpcSellItem ui)
        {
            if (uiSellItemDialog != null)
            {
                uiSellItemDialog.onHide.RemoveListener(OnSellItemDialogHide);
                uiSellItemDialog.Hide();
                uiSellItemDialog.onHide.AddListener(OnSellItemDialogHide);
            }
        }
    }
}
