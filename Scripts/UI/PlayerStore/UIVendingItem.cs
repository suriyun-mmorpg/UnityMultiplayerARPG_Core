using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIVendingItem : UIDataForCharacter<VendingItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Open Price}")]
        public UILocaleKeySetting formatKeySellPrice = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SELL_PRICE);

        [Header("UI Elements")]
        public UICharacterItem uiCharacterItem;
        public TextWrapper textPrice;

        protected override void UpdateData()
        {
            if (uiCharacterItem != null)
                uiCharacterItem.Setup(new UICharacterItemData(Data.item, InventoryType.Vending), Character, IndexOfData);

            if (textPrice != null)
            {
                textPrice.text = ZString.Format(
                    LanguageManager.GetText(formatKeySellPrice),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.price.ToString("N0"));
            }
        }
    }
}