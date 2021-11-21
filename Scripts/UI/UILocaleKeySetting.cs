namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UILocaleKeySetting
    {
        public UIFormatKeys localeKey;
        [StringShowConditional(nameof(localeKey), nameof(UIFormatKeys.UI_CUSTOM))]
        public string customKey;
        public UILocaleKeySetting(UIFormatKeys localeKey)
        {
            this.localeKey = localeKey;
            customKey = string.Empty;
        }

        public string ToFormat()
        {
            return LanguageManager.GetText(ToString());
        }

        public override string ToString()
        {
            if (localeKey == UIFormatKeys.UI_CUSTOM)
                return customKey;
            return localeKey.ToString();
        }

        public static implicit operator string(UILocaleKeySetting keySetting)
        {
            return keySetting.ToString();
        }
    }
}
