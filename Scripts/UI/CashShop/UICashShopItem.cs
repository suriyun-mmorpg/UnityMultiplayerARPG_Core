using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICashShopItem : UISelectionEntry<CashShopItem>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [FormerlySerializedAs("formatKeySellPrice")]
        [Tooltip("Format => {0} = {Sell Price}")]
        public UILocaleKeySetting formatKeySellPriceCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SELL_PRICE);
        [Tooltip("Format => {0} = {Sell Price}")]
        public UILocaleKeySetting formatKeySellPriceGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SELL_PRICE);

        [Header("UI Elements")]
        public UICashShop uiCashShop;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        [FormerlySerializedAs("uiTextSellPrice")]
        public TextWrapper uiTextSellPriceCash;
        public TextWrapper uiTextSellPriceGold;
        public TextWrapper textRecieveGold;
        public UICharacterCurrencies uiReceiveCurrencies;
        public UICharacterItems uiReceiveItems;

        protected override void UpdateData()
        {
            if (uiTextTitle != null) {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (rawImageExternalIcon != null)
            {
                rawImageExternalIcon.gameObject.SetActive(Data != null && !string.IsNullOrEmpty(Data.externalIconUrl));
                if (Data != null && !string.IsNullOrEmpty(Data.externalIconUrl))
                    StartCoroutine(LoadExternalIcon());
            }

            if (uiTextSellPriceCash != null)
            {
                uiTextSellPriceCash.text = string.Format(
                    LanguageManager.GetText(formatKeySellPriceCash),
                    Data == null ? 0.ToString("N0") : Data.sellPriceCash.ToString("N0"));
            }

            if (uiTextSellPriceGold != null)
            {
                uiTextSellPriceGold.text = string.Format(
                    LanguageManager.GetText(formatKeySellPriceGold),
                    Data == null ? 0.ToString("N0") : Data.sellPriceGold.ToString("N0"));
            }
        }

        IEnumerator LoadExternalIcon()
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(Data.externalIconUrl);
            yield return www.SendWebRequest();
            if (!www.isNetworkError && !www.isHttpError)
                rawImageExternalIcon.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }

        public void OnClickBuy()
        {
            if (uiCashShop != null)
                uiCashShop.Buy(Data.DataId);
        }
    }
}
