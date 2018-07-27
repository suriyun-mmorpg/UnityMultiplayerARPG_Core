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
        public Text textDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        public Text textSellPrice;

        protected override void UpdateData()
        {
            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, Data == null ? "Unknow" : Data.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, Data == null ? "N/A" : Data.description);
            
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

            if (textSellPrice != null)
                textSellPrice.text = string.Format(sellPriceFormat, Data == null ? "0" : Data.sellPrice.ToString("N0"));
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
    }
}
