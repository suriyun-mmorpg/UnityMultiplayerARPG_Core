using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPlayerStore : UISelectionEntry<BasePlayerCharacterEntity>
    {
        public UIPlayerStoreItem uiSelectedItem;
        public UIPlayerStoreItem uiItemPrefab;
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

        private UIPlayerStoreItemSelectionManager _itemSelectionManager;
        public UIPlayerStoreItemSelectionManager ItemSelectionManager
        {
            get
            {
                if (_itemSelectionManager == null)
                    _itemSelectionManager = gameObject.GetOrAddComponent<UIPlayerStoreItemSelectionManager>();
                _itemSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _itemSelectionManager;
            }
        }

        private UISelectionManagerShowOnSelectEventManager<PlayerStoreItem, UIPlayerStoreItem> _itemListEventSetupManager = new UISelectionManagerShowOnSelectEventManager<PlayerStoreItem, UIPlayerStoreItem>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _itemListEventSetupManager.OnEnable(ItemSelectionManager, uiSelectedItem);
            GameInstance.PlayingCharacterEntity.Store.onUpdateItems += Store_onUpdateItems;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _itemListEventSetupManager.OnDisable();
            GameInstance.PlayingCharacterEntity.Store.onUpdateItems -= Store_onUpdateItems;
            GameInstance.PlayingCharacterEntity.Store.Unsubscribe();
        }

        private void Store_onUpdateItems(PlayerStoreItems items)
        {
            UpdateItemList(items);
        }

        public void UpdateItemList(PlayerStoreItems items)
        {
            ItemSelectionManager.Clear();
            ItemList.HideAll();
            ItemList.Generate(items, (index, data, ui) =>
            {
                UIPlayerStoreItem uiComp = ui.GetComponent<UIPlayerStoreItem>();
                uiComp.Setup(data, Data, index);
                if (index == 0)
                    uiComp.OnClickSelect();
            });
        }

        protected override void UpdateData()
        {
            if (Data == null)
                return;
            GameInstance.PlayingCharacterEntity.Store.Subscribe(Data.ObjectId);
        }
    }
}