using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICashShopItem : UISelectionEntry<CashShopItem>
    {
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public string formatTitle = "{0}";
        /// <summary>
        /// Format => {0} = {Description}
        /// </summary>
        [Tooltip("Format => {0} = {Description}")]
        public string formatDescription = "{0}";
        /// <summary>
        /// Format => {0} = {Sell Price Label}, {1} = {Sell Price}
        /// </summary>
        [Tooltip("Format => {0} = {Sell Price Label}, {1} = {Sell Price}")]
        public string formatSellPrice = "{1}: {0}";

        [Header("UI Elements")]
        public UICashShop uiCashShop;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        public TextWrapper uiTextSellPrice;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(formatTitle, Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(formatDescription, Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
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
            {
                uiTextSellPrice.text = string.Format(
                    formatSellPrice,
                    LanguageManager.GetText(UILocaleKeys.UI_LABEL_SELL_PRICE.ToString()),
                    Data == null ? "0" : Data.sellPrice.ToString("N0"));
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
