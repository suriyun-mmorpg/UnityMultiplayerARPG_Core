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
                    uiCharacterItem.Setup(new UICharacterItemData(CharacterItem.Create(Data.item), InventoryType.NonEquipItems), BasePlayerCharacterController.OwningCharacter, -1);
                    uiCharacterItem.Show();
                }
            }

            if (uiTextSellPrice != null)
            {
                uiTextSellPrice.text = string.Format(
                    LanguageManager.GetText(formatKeySellPrice),
                    Data.sellPrice.ToString("N0"));
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

            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (item.MaxStack == 1)
            {
                if (owningCharacter != null)
                    owningCharacter.CallServerBuyNpcItem((short)indexOfData, 1);
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
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.CallServerBuyNpcItem((short)indexOfData, (short)amount);
        }
    }
}
