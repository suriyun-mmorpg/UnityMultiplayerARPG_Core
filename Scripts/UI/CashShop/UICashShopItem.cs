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
        public InputFieldWrapper inputAmount;
        [Tooltip("These objects will be activated while sell price cash currency is not 0.")]
        public GameObject[] cashObjects;
        [Tooltip("These objects will be activated while sell price gold currency is not 0.")]
        public GameObject[] goldObjects;

        public int BuyAmount
        {
            get
            {
                int amount;
                if (inputAmount != null && int.TryParse(inputAmount.text, out amount))
                    return amount;
                return 1;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (inputAmount != null)
            {
                inputAmount.contentType = InputField.ContentType.IntegerNumber;
                inputAmount.text = "1";
                inputAmount.onValueChanged.RemoveAllListeners();
                inputAmount.onValueChanged.AddListener(ValidateAmount);
            }
        }

        private void ValidateAmount(string result)
        {
            int amount;
            if (int.TryParse(result, out amount))
            {

                if (uiTextSellPriceCash != null)
                {
                    uiTextSellPriceCash.text = string.Format(
                        LanguageManager.GetText(formatKeySellPriceCash),
                        Data == null ? 0.ToString("N0") : (Data.sellPriceCash * BuyAmount).ToString("N0"));
                }

                if (uiTextSellPriceGold != null)
                {
                    uiTextSellPriceGold.text = string.Format(
                        LanguageManager.GetText(formatKeySellPriceGold),
                        Data == null ? 0.ToString("N0") : (Data.sellPriceGold * BuyAmount).ToString("N0"));
                }

                inputAmount.onValueChanged.RemoveAllListeners();
                if (amount < 1)
                    inputAmount.text = "1";
                if (amount > 99)
                    inputAmount.text = "99";
                inputAmount.onValueChanged.AddListener(ValidateAmount);
            }
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null || string.IsNullOrEmpty(Data.Title) ? BuildTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = string.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null || string.IsNullOrEmpty(Data.Description) ? BuildDescription() : Data.Description);
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
                    Data == null ? 0.ToString("N0") : (Data.sellPriceCash * BuyAmount).ToString("N0"));
                uiTextSellPriceCash.SetGameObjectActive(Data.sellPriceCash > 0);
            }

            if (uiTextSellPriceGold != null)
            {
                uiTextSellPriceGold.text = string.Format(
                    LanguageManager.GetText(formatKeySellPriceGold),
                    Data == null ? 0.ToString("N0") : (Data.sellPriceGold * BuyAmount).ToString("N0"));
                uiTextSellPriceGold.SetGameObjectActive(Data.sellPriceGold > 0);
            }

            if (cashObjects != null && cashObjects.Length > 0)
            {
                foreach (GameObject cashObject in cashObjects)
                {
                    cashObject.SetActive(Data.sellPriceCash > 0);
                }
            }

            if (goldObjects != null && goldObjects.Length > 0)
            {
                foreach (GameObject goldObject in goldObjects)
                {
                    goldObject.SetActive(Data.sellPriceGold > 0);
                }
            }
        }

        public string BuildTitle()
        {
            if (Data.receiveItems.Length > 0)
                return Data.receiveItems[0].item.Title;
            if (Data.receiveCurrencies.Length > 0)
                return string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT.ToString()), Data.receiveCurrencies[0].currency.Title, Data.receiveCurrencies[0].amount);
            if (Data.receiveGold > 0)
                return string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD.ToString()), Data.receiveGold.ToString("N0"));
            return LanguageManager.GetUnknowTitle();
        }

        public string BuildDescription()
        {
            if (Data.receiveItems.Length > 0)
                return Data.receiveItems[0].item.Description;
            if (Data.receiveCurrencies.Length > 0)
                return string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT.ToString()), Data.receiveCurrencies[0].currency.Title, Data.receiveCurrencies[0].amount);
            if (Data.receiveGold > 0)
                return string.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD.ToString()), Data.receiveGold.ToString("N0"));
            return LanguageManager.GetUnknowTitle();
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
                uiCashShop.Buy(Data.DataId, CashShopItemCurrencyType.CASH, BuyAmount);
        }

        public void OnClickBuyWithGold()
        {
            if (uiCashShop != null)
                uiCashShop.Buy(Data.DataId, CashShopItemCurrencyType.GOLD, BuyAmount);
        }
    }
}
