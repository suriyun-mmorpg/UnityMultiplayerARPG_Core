using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIBulkDismantleItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Return Gold Amount}")]
        public UILocaleKeySetting formatKeyReturnGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

        [Header("UI Elements")]
        [Tooltip("UI which showing items in inventory, will use it to select items to dismantle")]
        public UINonEquipItems uiNonEquipItems;
        public UIItemAmounts uiReturnItems;
        public UICurrencyAmounts uiReturnCurrencies;
        public TextWrapper uiTextReturnGold;

        protected Dictionary<Currency, int> _tempReturningCurrencies = new Dictionary<Currency, int>();
        protected Dictionary<BaseItem, int> _tempReturningItems = new Dictionary<BaseItem, int>();
        private readonly List<ItemAmount> _returningItems = new List<ItemAmount>();
        private readonly List<CurrencyAmount> _returningCurrencies = new List<CurrencyAmount>();
        private readonly Dictionary<BaseItem, int> _appliedReturningItems = new Dictionary<BaseItem, int>();
        private readonly Dictionary<Currency, int> _appliedReturningCurrencies = new Dictionary<Currency, int>();
        private bool _appliedHasItems;
        private bool _appliedHasCurrencies;
        private int _lastReturnGold = -1;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiNonEquipItems = null;
            uiReturnItems = null;
            uiReturnCurrencies = null;
            uiTextReturnGold = null;
            _returningItems.Clear();
            _returningCurrencies.Clear();
            _tempReturningCurrencies.Clear();
            _tempReturningCurrencies = null;
            _tempReturningItems.Clear();
            _tempReturningItems = null;
            _appliedReturningItems.Clear();
            _appliedReturningCurrencies.Clear();
        }

        private void OnEnable()
        {
            if (uiNonEquipItems == null)
                uiNonEquipItems = FindFirstObjectByType<UINonEquipItems>();
            if (uiNonEquipItems != null)
                uiNonEquipItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectMultiple;
            _lastReturnGold = -1;
        }

        private void OnDisable()
        {
            uiNonEquipItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
        }

        private void LateUpdate()
        {
            int returnGold = 0;
            List<ItemAmount> returningItems = _returningItems;
            List<CurrencyAmount> returningCurrencies = _returningCurrencies;
            returningItems.Clear();
            returningCurrencies.Clear();
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheSelectionManager.GetSelectedUIs();
            List<ItemAmount> tempReturningItems;
            List<CurrencyAmount> tempReturningCurrencies;
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                tempCharacterItem.GetDismantleReturnItems(tempCharacterItem.amount, out tempReturningItems, out tempReturningCurrencies);
                returningItems.AddRange(tempReturningItems);
                returningCurrencies.AddRange(tempReturningCurrencies);
                returnGold += tempCharacterItem.GetItem().DismantleReturnGold * tempCharacterItem.amount;
            }

            if (uiReturnItems != null)
            {
                if (returningItems.Count == 0)
                {
                    if (_appliedHasItems)
                    {
                        _appliedHasItems = false;
                        _appliedReturningItems.Clear();
                        uiReturnItems.Hide();
                    }
                }
                else
                {
                    _tempReturningItems.Clear();
                    GameDataHelpers.CombineItems(returningItems, _tempReturningItems);
                    if (!_appliedHasItems || !DictionaryEquals(_appliedReturningItems, _tempReturningItems))
                    {
                        _appliedHasItems = true;
                        CopyDictionary(_appliedReturningItems, _tempReturningItems);
                        uiReturnItems.displayType = UIItemAmounts.DisplayType.Simple;
                        uiReturnItems.Show();
                        uiReturnItems.Data = _tempReturningItems;
                    }
                }
            }

            if (uiReturnCurrencies != null)
            {
                if (returningCurrencies.Count == 0)
                {
                    if (_appliedHasCurrencies)
                    {
                        _appliedHasCurrencies = false;
                        _appliedReturningCurrencies.Clear();
                        uiReturnCurrencies.Hide();
                    }
                }
                else
                {
                    _tempReturningCurrencies.Clear();
                    GameDataHelpers.CombineCurrencies(returningCurrencies, _tempReturningCurrencies, 1f);
                    if (!_appliedHasCurrencies || !DictionaryEquals(_appliedReturningCurrencies, _tempReturningCurrencies))
                    {
                        _appliedHasCurrencies = true;
                        CopyDictionary(_appliedReturningCurrencies, _tempReturningCurrencies);
                        uiReturnCurrencies.displayType = UICurrencyAmounts.DisplayType.Simple;
                        uiReturnCurrencies.Show();
                        uiReturnCurrencies.Data = _tempReturningCurrencies;
                    }
                }
            }

            if (uiTextReturnGold != null && returnGold != _lastReturnGold)
            {
                _lastReturnGold = returnGold;
                uiTextReturnGold.text = ZString.Format(
                        LanguageManager.GetText(formatKeyReturnGold),
                        returnGold.ToString("N0"));
            }
        }

        public void OnClickDismantleItems()
        {
            List<int> indexes = new List<int>();
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheSelectionManager.GetSelectedUIs();
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                indexes.Add(selectedUI.IndexOfData);
            }
            GameInstance.ClientInventoryHandlers.RequestDismantleItems(new RequestDismantleItemsMessage()
            {
                selectedIndexes = indexes.ToArray(),
            }, ClientInventoryActions.ResponseDismantleItems);
        }

        private static bool DictionaryEquals<TKey, TValue>(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
        {
            if (a.Count != b.Count)
                return false;
            foreach (KeyValuePair<TKey, TValue> pair in a)
            {
                if (!b.TryGetValue(pair.Key, out TValue value) || !EqualityComparer<TValue>.Default.Equals(value, pair.Value))
                    return false;
            }
            return true;
        }

        private static void CopyDictionary<TKey, TValue>(Dictionary<TKey, TValue> destination, Dictionary<TKey, TValue> source)
        {
            destination.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in source)
            {
                destination.Add(pair.Key, pair.Value);
            }
        }
    }
}
