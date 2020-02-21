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

        public void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

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
                            CharacterItem.GetItem().DismantleReturnGold.ToString("N0"));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UICharacterItemByIndexData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickDismantle()
        {
            if (InventoryType != InventoryType.NonEquipItems || IndexOfData < 0)
                return;
            OwningCharacter.RequestDismantleItem((short)IndexOfData);
        }
    }
}
