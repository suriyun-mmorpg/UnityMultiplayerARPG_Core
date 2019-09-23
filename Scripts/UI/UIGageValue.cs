using System.Collections;
using System.Collections.Generic;
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

        [Header("Min By Max Setting")]
        public UILocaleKeySetting formatCurrentByMax = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE_MIN_BY_MAX);
        public string formatCurrentAmount = "N0";
        public string formatMaxAmount = "N0";

        [Header("Percentage Setting")]
        public UILocaleKeySetting formatPercentage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE_PERCENTAGE);
        public string formatPercentageAmount = "N0";

        private float rate;

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
                    textValue.text = string.Format(
                        LanguageManager.GetText(formatCurrentByMax),
                        current.ToString(formatCurrentAmount),
                        max.ToString(formatMaxAmount));
                }
                else
                {
                    textValue.text = string.Format(
                        LanguageManager.GetText(formatPercentage),
                        (rate * 100f).ToString(formatPercentageAmount));
                }
            }

            if (imageGage != null)
                imageGage.fillAmount = rate;
        }

        // TODO: This is temporary use for migrate from old version
        public static bool Migrate(ref UIGageValue target, ref TextWrapper oldText, ref Image oldGage)
        {
            if (oldText == null && oldGage == null)
                return false;

            target = new UIGageValue()
            {
                textValue = oldText,
                imageGage = oldGage
            };

            oldText = null;
            oldGage = null;

            return true;
        }
    }
}
