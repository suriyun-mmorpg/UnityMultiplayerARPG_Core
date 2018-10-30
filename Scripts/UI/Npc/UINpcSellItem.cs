using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UINpcSellItem : UISelectionEntry<NpcSellItem>
    {
        [Tooltip("Sell Price Format => {0} = {Sell price}")]
        public string sellPriceFormat = "{0}";

        [Header("Input Dialog Settings")]
        public string buyInputTitle = "Buy Item";
        public string buyInputDescription = "";

        [Header("UI Elements")]
        public UICharacterItem uiCharacterItem;
        public Text textSellPrice;
        public TextWrapper uiTextSellPrice;

        public int indexOfData { get; protected set; }

        public void Setup(NpcSellItem data, int indexOfData)
        {
            this.indexOfData = indexOfData;
            Data = data;
        }

        protected override void UpdateData()
        {
            MigrateUIComponents();

            if (uiCharacterItem != null)
            {
                if (Data.item == null)
                    uiCharacterItem.Hide();
                else
                {
                    uiCharacterItem.Setup(new CharacterItemTuple(CharacterItem.Create(Data.item), 1, string.Empty), null, -1);
                    uiCharacterItem.Show();
                }
            }

            if (uiTextSellPrice != null)
                uiTextSellPrice.text = string.Format(sellPriceFormat, Data.sellPrice.ToString("N0"));
        }

        public void OnClickBuy()
        {
            var item = Data.item;
            if (item == null)
            {
                Debug.LogWarning("Cannot buy item, the item data is empty");
                return;
            }

            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (item.maxStack == 1)
            {
                if (owningCharacter != null)
                    owningCharacter.RequestBuyNpcItem((ushort)indexOfData, 1);
            }
            else
                UISceneGlobal.Singleton.ShowInputDialog(buyInputTitle, buyInputDescription, OnBuyAmountConfirmed, 1, item.maxStack, 1);
        }

        private void OnBuyAmountConfirmed(int amount)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestBuyNpcItem((ushort)indexOfData, (short)amount);
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextSellPrice = MigrateUIHelpers.SetWrapperToText(textSellPrice, uiTextSellPrice);
        }
    }
}
