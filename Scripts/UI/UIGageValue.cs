using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class UIGageValue
    {
        public enum DisplayType
        {
            CurrentByMax,
            Percentage
        }
        [Header("General Setting")]
        public DisplayType displayType = DisplayType.CurrentByMax;
        public TextWrapper textValue;
        public Image imageGage;
        public Slider sliderGage;

        [Header("Min By Max Setting")]
        public UILocaleKeySetting formatCurrentByMax = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE_MIN_BY_MAX);
        public string formatCurrentAmount = "N0";
        public string formatMaxAmount = "N0";

        [Header("Percentage Setting")]
        public UILocaleKeySetting formatPercentage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE_PERCENTAGE);
        public string formatPercentageAmount = "N0";

        private float rate;

        public void SetVisible(bool isVisible)
        {
            if (textValue != null)
                textValue.SetGameObjectActive(isVisible);

            if (imageGage != null)
                imageGage.gameObject.SetActive(isVisible);

            if (sliderGage != null)
                sliderGage.gameObject.SetActive(isVisible);
        }

        public void Update(int current, int max)
        {
            Update((float)current, (float)max);
        }

        public void Update(float current, float max)
        {
            rate = max == 0 ? 1 : current / max;

            if (textValue != null)
            {
                if (displayType == DisplayType.CurrentByMax)
                {
                    textValue.text = ZString.Format(
                        LanguageManager.GetText(formatCurrentByMax),
                        current.ToString(formatCurrentAmount),
                        max.ToString(formatMaxAmount));
                }
                else
                {
                    textValue.text = ZString.Format(
                        LanguageManager.GetText(formatPercentage),
                        (rate * 100f).ToString(formatPercentageAmount));
                }
            }

            if (imageGage != null)
                imageGage.fillAmount = rate;

            if (sliderGage != null)
            {
                sliderGage.maxValue = max;
                sliderGage.value = current;
            }
        }
    }
}
