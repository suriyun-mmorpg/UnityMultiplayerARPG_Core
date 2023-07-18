using UnityEngine;

namespace MultiplayerARPG
{
    public class UIVending : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UIVendingItem uiSelectedItem;
        public UIVendingItem uiItemPrefab;
        public Transform uiItemContainer;

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
            GameInstance.PlayingCharacterEntity.Vending.onUpdateItems += Store_onUpdateItems;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _itemListEventSetupManager.OnDisable();
            GameInstance.PlayingCharacterEntity.Vending.onUpdateItems -= Store_onUpdateItems;
            GameInstance.PlayingCharacterEntity.Vending.Unsubscribe();
        }

        private void Store_onUpdateItems(VendingItems items)
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
            GameInstance.PlayingCharacterEntity.Vending.Subscribe(Data.ObjectId);
        }

        public void OnClickStop()
        {
            GameInstance.PlayingCharacterEntity.Vending.StopVending();
        }
    }
}