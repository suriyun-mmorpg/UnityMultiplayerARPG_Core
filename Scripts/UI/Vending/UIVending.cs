using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIVending : UISelectionEntry<BasePlayerCharacterEntity>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper textTitle;
        public UIVendingItem uiSelectedItem;
        public UIVendingItem uiItemPrefab;
        public Transform uiItemContainer;
        public GameObject[] ownerObjects;
        public GameObject[] nonOwnerObjects;

        private UIList _itemList;
        public UIList ItemList
        {
            get
            {
                if (_itemList == null)
                {
                    _itemList = gameObject.AddComponent<UIList>();
                    _itemList.uiPrefab = uiItemPrefab.gameObject;
                    _itemList.uiContainer = uiItemContainer;
                }
                return _itemList;
            }
        }

        private UIVendingItemSelectionManager _itemSelectionManager;
        public UIVendingItemSelectionManager ItemSelectionManager
        {
            get
            {
                if (_itemSelectionManager == null)
                    _itemSelectionManager = gameObject.GetOrAddComponent<UIVendingItemSelectionManager>();
                _itemSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _itemSelectionManager;
            }
        }

        private UISelectionManagerShowOnSelectEventManager<VendingItem, UIVendingItem> _itemListEventSetupManager = new UISelectionManagerShowOnSelectEventManager<VendingItem, UIVendingItem>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _itemListEventSetupManager.OnEnable(ItemSelectionManager, uiSelectedItem);
            GameInstance.PlayingCharacterEntity.Vending.onVendingDataChange += Vending_onVendingDataChange;
            GameInstance.PlayingCharacterEntity.Vending.onUpdateItems += Vending_onUpdateItems;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _itemListEventSetupManager.OnDisable();
            GameInstance.PlayingCharacterEntity.Vending.onVendingDataChange -= Vending_onVendingDataChange;
            GameInstance.PlayingCharacterEntity.Vending.onUpdateItems -= Vending_onUpdateItems;
            GameInstance.PlayingCharacterEntity.Vending.Unsubscribe();
        }

        private void Vending_onVendingDataChange(VendingData data)
        {
            if (!data.isStarted)
                Hide();
        }

        private void Vending_onUpdateItems(VendingItems items)
        {
            UpdateItemList(items);
        }

        public void UpdateItemList(VendingItems items)
        {
            ItemSelectionManager.Clear();
            ItemList.HideAll();
            ItemList.Generate(items, (index, data, ui) =>
            {
                UIVendingItem uiComp = ui.GetComponent<UIVendingItem>();
                uiComp.Setup(data, Data, index);
                if (index == 0)
                    uiComp.OnClickSelect();
            });
        }

        protected override void UpdateData()
        {
            if (Data == null)
                return;

            if (textTitle != null)
            {
                textTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Vending.Data.title);
            }
            ItemSelectionManager.Clear();
            ItemList.HideAll();
            GameInstance.PlayingCharacterEntity.Vending.Subscribe(Data.ObjectId);
        }

        public void OnClickStop()
        {
            GameInstance.PlayingCharacterEntity.Vending.StopVending();
            Hide();
        }
    }
}