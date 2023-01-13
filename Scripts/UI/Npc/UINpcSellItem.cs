using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINpcSellItem : UISelectionEntry<NpcSellItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Sell Price}")]
        public UILocaleKeySetting formatKeySellPrice = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SELL_PRICE);

        [Header("UI Elements")]
        public UICharacterItem uiCharacterItem;
        public TextWrapper uiTextSellPrice;
        public UICurrencyAmounts uiSellPrices;

        [Header("Options")]
        [Tooltip("If this is `TRUE`, `uiTextSellPrice` will be inactivated when item's sell price is 0")]
        public bool inactiveSellPriceIfZero;

        public int indexOfData { get; protected set; }

        public void Setup(NpcSellItem data, int indexOfData)
        {
            this.indexOfData = indexOfData;
            Data = data;
        }

        protected override void UpdateData()
        {
            if (uiCharacterItem != null)
            {
                if (Data.item == null)
                    uiCharacterItem.Hide();
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem.Create(Data.item), InventoryType.NonEquipItems), GameInstance.PlayingCharacter, -1);
                    uiCharacterItem.Show();
                }
            }

            if (uiTextSellPrice != null)
            {
                uiTextSellPrice.text = ZString.Format(
                    LanguageManager.GetText(formatKeySellPrice),
                    Data.sellPrice.ToString("N0"));
                uiTextSellPrice.SetGameObjectActive(!inactiveSellPriceIfZero || Data.sellPrice != 0);
            }

            if (uiSellPrices != null)
            {
                uiSellPrices.displayType = UICurrencyAmounts.DisplayType.Simple;
                uiSellPrices.isBonus = false;
                uiSellPrices.Data = GameDataHelpers.CombineCurrencies(Data.sellPrices, null);
            }
        }

        public void OnClickBuy()
        {
            BaseItem item = Data.item;
            if (item == null)
            {
                Debug.LogWarning("Cannot buy item, the item data is empty");
                return;
            }

            if (item.MaxStack == 1)
            {
                GameInstance.PlayingCharacterEntity.NpcAction.CallServerBuyNpcItem(indexOfData, 1);
            }
            else
            {
                UISceneGlobal.Singleton.ShowInputDialog(
                    LanguageManager.GetText(UITextKeys.UI_BUY_ITEM.ToString()),
                    LanguageManager.GetText(UITextKeys.UI_BUY_ITEM_DESCRIPTION.ToString()),
                    OnBuyAmountConfirmed,
                    1,  /* Min Amount */
                    item.MaxStack,
                    1   /* Start Amount*/);
            }
        }

        private void OnBuyAmountConfirmed(int amount)
        {
            GameInstance.PlayingCharacterEntity.NpcAction.CallServerBuyNpcItem(indexOfData, amount);
        }
    }
}
