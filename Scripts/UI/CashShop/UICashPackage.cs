using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICashPackage : UISelectionEntry<CashPackage>
    {
        [Header("Generic Info Format")]
        [Tooltip("Title Format => {0} = {Title}")]
        public string titleFormat = "{0}";
        [Tooltip("Description Format => {0} = {Description}")]
        public string descriptionFormat = "{0}";
        [Tooltip("Sell Price Format => {0} = {Sell price}, {1} = {Sell price Label}")]
        public string sellPriceFormat = "{1}: {0}";
        [Tooltip("Cash Amount Format => {0} = {Cash Amount}, {1} = {Cash Label}")]
        public string cashAmountFormat = "{1}: {0}";

        [Header("UI Elements")]
        public UICashPackages uiCashPackages;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        public TextWrapper uiTextSellPrice;
        public TextWrapper uiTextCashAmount;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(titleFormat, Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(descriptionFormat, Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);

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
                uiTextSellPrice.text = string.Format(sellPriceFormat, Data == null ? "0" : Data.GetSellPrice(), LanguageManager.GetText(UILocaleKeys.UI_LABEL_SELL_PRICE.ToString()));

            if (uiTextCashAmount != null)
                uiTextCashAmount.text = string.Format(cashAmountFormat, Data == null ? "0" : Data.cashAmount.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_CASH.ToString()));
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
            if (uiCashPackages != null)
                uiCashPackages.Buy(Data.Id);
        }
    }
}
