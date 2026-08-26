using Cysharp.Text;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIRepairEquipItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Target Amount}")]
        public UILocaleKeySetting formatKeySimpleRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextRequireGold;
        public UIItemAmounts uiRequireItemAmounts;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        public TextWrapper uiTextSimpleRequireGold;

        protected Dictionary<Currency, int> _tempRequireCurrencies = new Dictionary<Currency, int>();
        protected Dictionary<BaseItem, int> _tempRequireItems = new Dictionary<BaseItem, int>();
        private readonly List<ItemAmount> _requireItems = new List<ItemAmount>();
        private readonly List<CurrencyAmount> _requireCurrencies = new List<CurrencyAmount>();
        private readonly Dictionary<BaseItem, int> _appliedRequireItems = new Dictionary<BaseItem, int>();
        private readonly Dictionary<Currency, int> _appliedRequireCurrencies = new Dictionary<Currency, int>();
        private bool _appliedHasItems;
        private bool _appliedHasCurrencies;
        private int _lastRequireGold = -1;
        private int _lastGold = -1;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextRequireGold = null;
            uiRequireItemAmounts = null;
            uiRequireCurrencyAmounts = null;
            uiTextSimpleRequireGold = null;
            _requireItems.Clear();
            _requireCurrencies.Clear();
            _tempRequireCurrencies.Clear();
            _tempRequireCurrencies = null;
            _tempRequireItems.Clear();
            _tempRequireItems = null;
            _appliedRequireItems.Clear();
            _appliedRequireCurrencies.Clear();
        }

        private void LateUpdate()
        {
            int requireGold = 0;
            List<ItemAmount> requireItems = _requireItems;
            List<CurrencyAmount> requireCurrencies = _requireCurrencies;
            requireItems.Clear();
            requireCurrencies.Clear();
            ItemRepairPrice tempRepairPrice;
            EquipWeapons equipWeapons = GameInstance.PlayingCharacterEntity.EquipWeapons;
            if (!equipWeapons.IsEmptyRightHandSlot() &&
                equipWeapons.GetRightHandItem().TryGetRepairPrice(equipWeapons.rightHand.durability, out _, out tempRepairPrice))
            {
                requireGold += tempRepairPrice.RequireGold;
                if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                    requireItems.AddRange(tempRepairPrice.RequireItems);
                if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                    requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
            }
            if (!equipWeapons.IsEmptyLeftHandSlot() &&
                equipWeapons.GetLeftHandItem().TryGetRepairPrice(equipWeapons.leftHand.durability, out _, out tempRepairPrice))
            {
                requireGold += tempRepairPrice.RequireGold;
                if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                    requireItems.AddRange(tempRepairPrice.RequireItems);
                if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                    requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
            }
            foreach (CharacterItem equipItem in GameInstance.PlayingCharacterEntity.EquipItems)
            {
                if (!equipItem.IsEmptySlot() &&
                    equipItem.GetItem().TryGetRepairPrice(equipItem.durability, out _, out tempRepairPrice))
                {
                    requireGold += tempRepairPrice.RequireGold;
                    if (tempRepairPrice.RequireItems != null && tempRepairPrice.RequireItems.Length > 0)
                        requireItems.AddRange(tempRepairPrice.RequireItems);
                    if (tempRepairPrice.RequireCurrencies != null && tempRepairPrice.RequireCurrencies.Length > 0)
                        requireCurrencies.AddRange(tempRepairPrice.RequireCurrencies);
                }
            }

            if (uiRequireItemAmounts != null)
            {
                if (requireItems.Count == 0)
                {
                    if (_appliedHasItems)
                    {
                        _appliedHasItems = false;
                        _appliedRequireItems.Clear();
                        uiRequireItemAmounts.Hide();
                    }
                }
                else
                {
                    _tempRequireItems.Clear();
                    GameDataHelpers.CombineItems(requireItems, _tempRequireItems);
                    if (!_appliedHasItems || !DictionaryEquals(_appliedRequireItems, _tempRequireItems))
                    {
                        _appliedHasItems = true;
                        CopyDictionary(_appliedRequireItems, _tempRequireItems);
                        uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                        uiRequireItemAmounts.Show();
                        uiRequireItemAmounts.Data = _tempRequireItems;
                    }
                }
            }

            if (uiRequireCurrencyAmounts != null)
            {
                if (requireCurrencies.Count == 0)
                {
                    if (_appliedHasCurrencies)
                    {
                        _appliedHasCurrencies = false;
                        _appliedRequireCurrencies.Clear();
                        uiRequireCurrencyAmounts.Hide();
                    }
                }
                else
                {
                    _tempRequireCurrencies.Clear();
                    GameDataHelpers.CombineCurrencies(requireCurrencies, _tempRequireCurrencies, 1f);
                    if (!_appliedHasCurrencies || !DictionaryEquals(_appliedRequireCurrencies, _tempRequireCurrencies))
                    {
                        _appliedHasCurrencies = true;
                        CopyDictionary(_appliedRequireCurrencies, _tempRequireCurrencies);
                        uiRequireCurrencyAmounts.displayType = UICurrencyAmounts.DisplayType.Requirement;
                        uiRequireCurrencyAmounts.Show();
                        uiRequireCurrencyAmounts.Data = _tempRequireCurrencies;
                    }
                }
            }

            int currentGold = GameInstance.PlayingCharacter.Gold;
            bool requireGoldChanged = requireGold != _lastRequireGold || currentGold != _lastGold;
            _lastRequireGold = requireGold;
            _lastGold = currentGold;

            if (uiTextRequireGold != null && requireGoldChanged)
            {
                uiTextRequireGold.text = ZString.Format(
                    currentGold >= requireGold ?
                        LanguageManager.GetText(formatKeyRequireGold) :
                        LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                    currentGold.ToString("N0"),
                    requireGold.ToString("N0"));
            }

            if (uiTextSimpleRequireGold != null && requireGoldChanged)
                uiTextSimpleRequireGold.text = ZString.Format(LanguageManager.GetText(formatKeySimpleRequireGold), requireGold.ToString("N0"));
        }

        public void OnClickRepairEquipItems()
        {
            GameInstance.ClientInventoryHandlers.RequestRepairEquipItems(ClientInventoryActions.ResponseRepairEquipItems);
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
