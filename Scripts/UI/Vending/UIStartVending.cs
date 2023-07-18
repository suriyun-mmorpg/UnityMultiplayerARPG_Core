using UnityEngine;

namespace MultiplayerARPG
{
    public class UIStartVending : UIBase
    {
        public InputFieldWrapper inputTitle;
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
        private StartVendingItems _items = new StartVendingItems();

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

        public void AddItem(string id, int amount, int price)
        {
            int indexOfData = GameInstance.PlayingCharacterEntity.NonEquipItems.IndexOf(id);
            if (indexOfData < 0)
            {
                // Invalid index
                return;
            }
            int countItem = 0;
            foreach (StartVendingItem item in _items)
            {
                if (id == item.id)
                    countItem += item.amount;
            }
            countItem += amount;
            if (GameInstance.PlayingCharacterEntity.NonEquipItems[indexOfData].amount < countItem)
            {
                // Invalid amount
                return;
            }
            _items.Add(new StartVendingItem()
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
                {
                    // Invalid index
                    return;
                }
                CharacterItem item = GameInstance.PlayingCharacterEntity.NonEquipItems[indexOfItem].Clone(false);
                item.amount = data.amount;
                UIVendingItem uiComp = ui.GetComponent<UIVendingItem>();
                uiComp.uiStartVending = this;
                uiComp.Setup(new VendingItem()
                {
                    item = item,
                    price = data.price,
                }, GameInstance.PlayingCharacterEntity, index);
                if (index == 0)
                    uiComp.OnClickSelect();
            });
        }

        public void SetPrice(int index, int price)
        {
            StartVendingItem item = _items[index];
            item.price = price;
            _items[index] = item;
        }

        public void Cancel(int index)
        {
            _items.RemoveAt(index);
            UpdateItemList();
        }

        public void OnClickStart()
        {
            GameInstance.PlayingCharacterEntity.Vending.StartVending(inputTitle.text, _items);
            Hide();
        }
    }
}