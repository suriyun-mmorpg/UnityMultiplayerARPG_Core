using Cysharp.Text;
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
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyReceiveGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REWARD_GOLD);
        [Tooltip("Format => {0} = {Item Amount}")]
        public UILocaleKeySetting formayKeySingleItemAmount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public UICashShop uiCashShop;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public RawImage rawImageExternalIcon;
        [FormerlySerializedAs("uiTextSellPrice")]
        public TextWrapper uiTextSellPriceCash;
        public TextWrapper uiTextSellPriceGold;
        [FormerlySerializedAs("textRecieveGold")]
        [FormerlySerializedAs("uiTextRecieveGold")]
        public TextWrapper uiTextReceiveGold;
        [Tooltip("This will be shown when there is only one kind of item in the package")]
        public TextWrapper uiTextSingleItemAmount;
        public UICurrencyAmounts uiReceiveCurrencies;
        public UIItemAmounts uiReceiveItems;
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
                inputAmount.onValueChanged.RemoveAllListeners();
                if (amount < 1)
                    inputAmount.text = "1";
                if (amount > 99)
                    inputAmount.text = "99";

                if (uiTextSellPriceCash != null)
                {
                    uiTextSellPriceCash.text = ZString.Format(
                        LanguageManager.GetText(formatKeySellPriceCash),
                        Data == null ? "0" : (Data.SellPriceCash * BuyAmount).ToString("N0"));
                    uiTextSellPriceCash.SetGameObjectActive(Data.SellPriceCash > 0);
                }

                if (uiTextSellPriceGold != null)
                {
                    uiTextSellPriceGold.text = ZString.Format(
                        LanguageManager.GetText(formatKeySellPriceGold),
                        Data == null ? "0" : (Data.SellPriceGold * BuyAmount).ToString("N0"));
                    uiTextSellPriceGold.SetGameObjectActive(Data.SellPriceGold > 0);
                }

                inputAmount.onValueChanged.AddListener(ValidateAmount);
            }
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Data == null || string.IsNullOrEmpty(Data.Title) ? BuildTitle() : Data.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Data == null || string.IsNullOrEmpty(Data.Description) ? BuildDescription() : Data.Description);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null || Data.Icon == null ? BuildIcon() : Data.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }

            if (rawImageExternalIcon != null)
            {
                rawImageExternalIcon.gameObject.SetActive(Data != null && !string.IsNullOrEmpty(Data.ExternalIconUrl));
                if (Data != null && !string.IsNullOrEmpty(Data.ExternalIconUrl))
                    StartCoroutine(LoadExternalIcon());
            }

            if (uiTextSellPriceCash != null)
            {
                uiTextSellPriceCash.text = ZString.Format(
                    LanguageManager.GetText(formatKeySellPriceCash),
                    Data == null ? "0" : (Data.SellPriceCash * BuyAmount).ToString("N0"));
                uiTextSellPriceCash.SetGameObjectActive(Data.SellPriceCash > 0);
            }

            if (uiTextSellPriceGold != null)
            {
                uiTextSellPriceGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeySellPriceGold),
                    Data == null ? "0" : (Data.SellPriceGold * BuyAmount).ToString("N0"));
                uiTextSellPriceGold.SetGameObjectActive(Data.SellPriceGold > 0);
            }

            if (uiTextReceiveGold != null)
            {
                uiTextReceiveGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeyReceiveGold),
                    Data == null ? "0" : Data.ReceiveGold.ToString("N0"));
            }

            if (uiTextSingleItemAmount != null)
            {
                if (Data == null || Data.ReceiveItems.Length != 1)
                {
                    uiTextSingleItemAmount.SetGameObjectActive(false);
                }
                else
                {
                    uiTextSingleItemAmount.SetGameObjectActive(true);
                    uiTextSingleItemAmount.text = ZString.Format(
                        LanguageManager.GetText(formayKeySingleItemAmount),
                        Data == null ? "0" : Data.ReceiveItems[0].amount.ToString("N0"));
                }
            }

            if (uiReceiveCurrencies != null)
            {
                uiReceiveCurrencies.Data = Data == null ? null : GameDataHelpers.CombineCurrencies(Data.ReceiveCurrencies, null);
            }

            if (uiReceiveItems != null)
            {
                uiReceiveItems.Data = Data == null ? null : GameDataHelpers.CombineItems(Data.ReceiveItems, null);
            }

            if (cashObjects != null && cashObjects.Length > 0)
            {
                foreach (GameObject cashObject in cashObjects)
                {
                    cashObject.SetActive(Data.SellPriceCash > 0);
                }
            }

            if (goldObjects != null && goldObjects.Length > 0)
            {
                foreach (GameObject goldObject in goldObjects)
                {
                    goldObject.SetActive(Data.SellPriceGold > 0);
                }
            }
        }

        public string BuildTitle()
        {
            if (Data != null)
            {
                if (Data.ReceiveItems.Length > 0)
                    return Data.ReceiveItems[0].item.Title;
                if (Data.ReceiveCurrencies.Length > 0)
                    return ZString.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT.ToString()), Data.ReceiveCurrencies[0].currency.Title, Data.ReceiveCurrencies[0].amount.ToString("N0"));
                if (Data.ReceiveGold > 0)
                    return ZString.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD.ToString()), Data.ReceiveGold.ToString("N0"));
            }
            return LanguageManager.GetUnknowTitle();
        }

        public string BuildDescription()
        {
            if (Data != null)
            {
                if (Data.ReceiveItems.Length > 0)
                    return Data.ReceiveItems[0].item.Description;
                if (Data.ReceiveCurrencies.Length > 0)
                    return ZString.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_CURRENCY_AMOUNT.ToString()), Data.ReceiveCurrencies[0].currency.Title, Data.ReceiveCurrencies[0].amount.ToString("N0"));
                if (Data.ReceiveGold > 0)
                    return ZString.Format(LanguageManager.GetText(UIFormatKeys.UI_FORMAT_GOLD.ToString()), Data.ReceiveGold.ToString("N0"));
            }
            return LanguageManager.GetUnknowTitle();
        }

        public Sprite BuildIcon()
        {
            if (Data != null)
            {
                if (Data.ReceiveItems.Length > 0)
                    return Data.ReceiveItems[0].item.Icon;
                if (Data.ReceiveCurrencies.Length > 0)
                    return Data.ReceiveCurrencies[0].currency.Icon;
            }
            return null;
        }

        IEnumerator LoadExternalIcon()
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(Data.ExternalIconUrl);
            yield return www.SendWebRequest();
            if (!www.IsError())
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
