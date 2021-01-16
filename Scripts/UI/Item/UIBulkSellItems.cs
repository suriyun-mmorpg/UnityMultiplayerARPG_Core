using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIBulkSellItems : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Return Gold Amount}")]
        public UILocaleKeySetting formatKeyReturnGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

        [Header("UI Elements")]
        [Tooltip("UI which showing items in inventory, will use it to select items to sell")]
        public UINonEquipItems uiNonEquipItems;
        public TextWrapper uiTextReturnGold;

        private void OnDisable()
        {
            uiNonEquipItems.CacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
        }

        private void Update()
        {
            uiNonEquipItems.CacheItemSelectionManager.selectionMode = UISelectionMode.SelectMultiple;
        }

        private void LateUpdate()
        {
            if (uiNonEquipItems == null)
                uiNonEquipItems = FindObjectOfType<UINonEquipItems>();
            int returnGold = 0;
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheItemSelectionManager.GetSelectedUIs();
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                returnGold += tempCharacterItem.GetItem().SellPrice * tempCharacterItem.amount;
            }

            if (uiTextReturnGold != null)
            {
                uiTextReturnGold.text = string.Format(
                        LanguageManager.GetText(formatKeyReturnGold),
                        returnGold.ToString("N0"));
            }
        }

        public void OnClickSellItems()
        {
            List<short> indexes = new List<short>();
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheItemSelectionManager.GetSelectedUIs();
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                indexes.Add((short)selectedUI.IndexOfData);
            }
            GameInstance.PlayingCharacterEntity.CallServerSellItems(indexes.ToArray());
        }
    }
}
