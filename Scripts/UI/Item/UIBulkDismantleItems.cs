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

        private void OnEnable()
        {
            if (uiNonEquipItems == null)
                uiNonEquipItems = FindObjectOfType<UINonEquipItems>();
        }

        private void OnDisable()
        {
            uiNonEquipItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
        }

        private void Update()
        {
            uiNonEquipItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectMultiple;
        }

        private void LateUpdate()
        {
            int returnGold = 0;
            List<ItemAmount> returningItems = new List<ItemAmount>();
            List<CurrencyAmount> returningCurrencies = new List<CurrencyAmount>();
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheSelectionManager.GetSelectedUIs();
            List<ItemAmount> tempReturningItems;
            List<CurrencyAmount> tempReturningCurrencies;
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                BaseItem.GetDismantleReturnItems(tempCharacterItem, tempCharacterItem.amount, out tempReturningItems, out tempReturningCurrencies);
                returningItems.AddRange(tempReturningItems);
                returningCurrencies.AddRange(tempReturningCurrencies);
                returnGold += tempCharacterItem.GetItem().DismantleReturnGold * tempCharacterItem.amount;
            }

            if (uiReturnItems != null)
            {
                if (returningItems.Count == 0)
                {
                    uiReturnItems.Hide();
                }
                else
                {
                    uiReturnItems.displayType = UIItemAmounts.DisplayType.Simple;
                    uiReturnItems.Show();
                    uiReturnItems.Data = GameDataHelpers.CombineItems(returningItems, null);
                }
            }

            if (uiReturnCurrencies != null)
            {
                if (returningCurrencies.Count == 0)
                {
                    uiReturnCurrencies.Hide();
                }
                else
                {
                    uiReturnCurrencies.displayType = UICurrencyAmounts.DisplayType.Simple;
                    uiReturnCurrencies.Show();
                    uiReturnCurrencies.Data = GameDataHelpers.CombineCurrencies(returningCurrencies, null);
                }
            }

            if (uiTextReturnGold != null)
            {
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
    }
}
