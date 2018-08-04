using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICashShopItem : UISelectionEntry<CashShopItem>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Sell Price Format => {0} = {Sell price}")]
        public string sellPriceFormat = "{0}";

        [Header("UI Elements")]
        public UICashShop uiCashShop;
        public Text textTitle;
        public TextWrapper uiTextTitle;
        public Text textDescription;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        public Text textSellPrice;
        public TextWrapper uiTextSellPrice;

        protected override void UpdateData()
        {
            UpdateUIComponents();

            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Data == null ? "Unknow" : Data.title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Data == null ? "N/A" : Data.description);
            
            if (imageIcon != null)
            {
                var iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (rawImageExternalIcon != null)
            {
                rawImageExternalIcon.gameObject.SetActive(Data != null && !string.IsNullOrEmpty(Data.externalIconUrl));
                if (Data != null && !string.IsNullOrEmpty(Data.externalIconUrl))
                    StartCoroutine(LoadExternalIcon());
            }

            if (uiTextSellPrice != null)
                uiTextSellPrice.text = string.Format(sellPriceFormat, Data == null ? "0" : Data.sellPrice.ToString("N0"));
        }

        IEnumerator LoadExternalIcon()
        {
            var www = new WWW(Data.externalIconUrl);
            yield return www;
            rawImageExternalIcon.texture = www.texture;
        }

        public void OnClickBuy()
        {
            if (uiCashShop != null)
                uiCashShop.Buy(Data.DataId);
        }

        [ContextMenu("Update UI Components")]
        public void UpdateUIComponents()
        {
            uiTextTitle = UIWrapperHelpers.SetWrapperToText(textTitle, uiTextTitle);
            uiTextDescription = UIWrapperHelpers.SetWrapperToText(textDescription, uiTextDescription);
            uiTextSellPrice = UIWrapperHelpers.SetWrapperToText(textSellPrice, uiTextSellPrice);
        }
    }
}
