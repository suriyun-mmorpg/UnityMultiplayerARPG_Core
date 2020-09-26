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
        public UINonEquipItems uiNonEquipItems;
        public TextWrapper uiTextReturnGold;

        private void OnEnable()
        {
            uiNonEquipItems.CacheItemSelectionManager.selectionMode = UISelectionMode.Toggle;
        }

        private void OnDisable()
        {
            uiNonEquipItems.CacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
        }

        private void LateUpdate()
        {
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
            BasePlayerCharacterController.OwningCharacter.CallServerSellItems(indexes);
        }
    }
}
