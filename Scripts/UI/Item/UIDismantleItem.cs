using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDismantleItem : BaseUICharacterItemByIndex
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Return Gold Amount}")]
        public UILocaleKeySetting formatKeyReturnGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

        [Header("UI Elements for UI Dismantle Item")]
        public UIItemAmounts uiReturnItems;
        public TextWrapper uiTextReturnGold;

        protected bool activated;
        protected string activeItemId;

        public void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            if (activated && (CharacterItem.IsEmptySlot() || !CharacterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (CharacterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem, Level, InventoryType), base.OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            List<ItemAmount> returningItems = BaseItem.GetDismantleReturnItems(CharacterItem);
            // Multiplies with dismantle item amount
            ItemAmount tempReturningItem;
            for (int i = 0; i < returningItems.Count; ++i)
            {
                tempReturningItem = returningItems[i];
                tempReturningItem.amount *= Amount;
                returningItems[i] = tempReturningItem;
            }
            if (uiReturnItems != null)
            {
                if (CharacterItem.IsEmptySlot() || returningItems.Count == 0)
                {
                    uiReturnItems.Hide();
                }
                else
                {
                    uiReturnItems.showAsRequirement = false;
                    uiReturnItems.Show();
                    uiReturnItems.Data = GameDataHelpers.CombineItems(returningItems.ToArray(), new Dictionary<BaseItem, short>()); ;
                }
            }

            if (uiTextReturnGold != null)
            {
                if (CharacterItem.IsEmptySlot())
                {
                    uiTextReturnGold.text = string.Format(
                            LanguageManager.GetText(formatKeyReturnGold),
                            "0");
                }
                else
                {
                    uiTextReturnGold.text = string.Format(
                            LanguageManager.GetText(formatKeyReturnGold),
                            (CharacterItem.GetItem().DismantleReturnGold * Amount).ToString("N0"));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickDismantle()
        {
            if (InventoryType != InventoryType.NonEquipItems || CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.RequestDismantleItem((short)IndexOfData);
        }
    }
}
