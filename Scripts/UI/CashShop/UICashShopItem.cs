using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICashShopItem : UIDataForCharacter<CashShopItem>
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
        public RawImage rawImageIcon;
        public Text textSellPrice;

        protected override void UpdateData()
        {
            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, Data == null ? "Unknow" : Data.title);

            if (textDescription != null)
                textDescription.text = string.Format(descriptionFormat, Data == null ? "N/A" : Data.description);

            if (rawImageIcon != null)
            {
                var iconSprite = Data == null ? null : Data.icon;
                rawImageIcon.gameObject.SetActive(iconSprite != null || !string.IsNullOrEmpty(Data.info.externalIconUrl));
                rawImageIcon.texture = iconSprite.texture;
                if (!string.IsNullOrEmpty(Data.info.externalIconUrl))
                    StartCoroutine(LoadExternalIcon());
            }

            if (textSellPrice != null)
                textSellPrice.text = string.Format(sellPriceFormat, Data == null ? "0" : Data.info.sellPrice.ToString("N0"));
        }

        IEnumerator LoadExternalIcon()
        {
            var www = new WWW(Data.info.externalIconUrl);
            yield return www;
            rawImageIcon.texture = www.texture;
        }

        public void OnClickBuy()
        {
            if (uiCashShop != null)
                uiCashShop.Buy(Data.DataId);
        }
    }
}
