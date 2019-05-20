namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UILocaleKeySetting
    {
        public UILocaleKeys localeKey;
        [StringShowConditional("localeKey", "UI_CUSTOM")]
        public string customKey;
        public UILocaleKeySetting(UILocaleKeys localeKey)
        {
            this.localeKey = localeKey;
            customKey = string.Empty;
        }

        public override string ToString()
        {
            if (localeKey == UILocaleKeys.UI_CUSTOM)
                return customKey;
            return localeKey.ToString();
        }

        public static implicit operator string(UILocaleKeySetting keySetting)
        {
            return keySetting.ToString();
        }
    }
}
