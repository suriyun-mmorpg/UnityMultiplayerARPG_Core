using System.Collections.Generic;
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
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheSelectionManager.GetSelectedUIs();
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                returningItems.AddRange(BaseItem.GetDismantleReturnItems(tempCharacterItem, tempCharacterItem.amount));
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
                    uiReturnItems.Data = GameDataHelpers.CombineItems(returningItems.ToArray(), new Dictionary<BaseItem, short>()); ;
                }
            }

            if (uiTextReturnGold != null)
            {
                uiTextReturnGold.text = string.Format(
                        LanguageManager.GetText(formatKeyReturnGold),
                        returnGold.ToString("N0"));
            }
        }

        public void OnClickDismantleItems()
        {
            List<short> indexes = new List<short>();
            CharacterItem tempCharacterItem;
            List<UICharacterItem> selectedUIs = uiNonEquipItems.CacheSelectionManager.GetSelectedUIs();
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                tempCharacterItem = selectedUI.Data.characterItem;
                if (tempCharacterItem.IsEmptySlot() || selectedUI.InventoryType != InventoryType.NonEquipItems)
                    continue;
                indexes.Add((short)selectedUI.IndexOfData);
            }
            GameInstance.ClientInventoryHandlers.RequestDismantleItems(new RequestDismantleItemsMessage()
            {
                selectedIndexes = indexes.ToArray(),
            }, ClientInventoryActions.ResponseDismantleItems);
        }
    }
}
