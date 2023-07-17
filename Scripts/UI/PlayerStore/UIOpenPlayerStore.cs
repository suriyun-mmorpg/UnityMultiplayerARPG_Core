using UnityEngine;

namespace MultiplayerARPG
{
    public class UIOpenPlayerStore : UIBase
    {
        public InputFieldWrapper inputTitle;
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
        private PlayerStoreOpenItems _items = new PlayerStoreOpenItems();

        private void OnEnable()
        {
            _itemListEventSetupManager.OnEnable(ItemSelectionManager, uiSelectedItem);
            inputTitle.text = string.Empty;
            _items.Clear();
        }

        private void OnDisable()
        {
            _itemListEventSetupManager.OnDisable();
        }

        public void PutItem(string id, int amount, int price)
        {
            _items.Add(new PlayerStoreOpenItem()
            {
                id = id,
                amount = amount,
                price = price,
            });
            UpdateItemList();
        }

        public void RemoveItem(int index)
        {
            _items.RemoveAt(index);
            UpdateItemList();
        }

        public void UpdateItemList()
        {
            ItemSelectionManager.Clear();
            ItemList.HideAll();
            ItemList.Generate(_items, (index, data, ui) =>
            {
                int indexOfItem = GameInstance.PlayingCharacterEntity.NonEquipItems.IndexOf(data.id);
                if (indexOfItem < 0)
                    return;
                CharacterItem item = GameInstance.PlayingCharacterEntity.NonEquipItems[indexOfItem].Clone(false);
                item.amount = data.amount;
                UIPlayerStoreItem uiComp = ui.GetComponent<UIPlayerStoreItem>();
                uiComp.Setup(new PlayerStoreItem()
                {
                    item = item,
                    price = data.price,
                }, GameInstance.PlayingCharacterEntity, index);
                if (index == 0)
                    uiComp.OnClickSelect();
            });
        }

        public void OnClickOpen()
        {
            GameInstance.PlayingCharacterEntity.Store.OpenStore(inputTitle.text, _items);
            Hide();
        }
    }
}