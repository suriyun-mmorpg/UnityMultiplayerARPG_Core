using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDismantleItem : UIBaseOwningCharacterItem
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Return Gold Amount}")]
        public UILocaleKeySetting formatKeyReturnGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);

        [Header("UI Elements for UI Dismantle Item")]
        public InputFieldWrapper inputFieldDismantleAmount;
        public UIItemAmounts uiReturnItems;
        public TextWrapper uiTextReturnGold;

        protected bool activated;
        protected string activeItemId;

        public short DismantleAmount
        {
            get
            {
                if (inputFieldDismantleAmount != null)
                {
                    short amount;
                    if (short.TryParse(inputFieldDismantleAmount.text, out amount))
                        return amount;
                }
                return Amount;
            }
        }

        public override void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            // Store data to variable so it won't lookup for data from property again
            CharacterItem characterItem = CharacterItem;

            if (activated && (characterItem.IsEmptySlot() || !characterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType), base.OwningCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            List<ItemAmount> returningItems = BaseItem.GetDismantleReturnItems(characterItem);
            // Multiplies with dismantle item amount
            ItemAmount tempReturningItem;
            for (int i = 0; i < returningItems.Count; ++i)
            {
                tempReturningItem = returningItems[i];
                tempReturningItem.amount *= DismantleAmount;
                returningItems[i] = tempReturningItem;
            }
            if (uiReturnItems != null)
            {
                if (characterItem.IsEmptySlot() || returningItems.Count == 0)
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
                if (characterItem.IsEmptySlot())
                {
                    uiTextReturnGold.text = string.Format(
                            LanguageManager.GetText(formatKeyReturnGold),
                            "0");
                }
                else
                {
                    uiTextReturnGold.text = string.Format(
                            LanguageManager.GetText(formatKeyReturnGold),
                            (characterItem.GetItem().DismantleReturnGold * DismantleAmount).ToString("N0"));
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
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickDismantle()
        {
            if (InventoryType != InventoryType.NonEquipItems || CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            OwningCharacter.CallServerDismantleItem((short)IndexOfData, DismantleAmount);
        }
    }
}
