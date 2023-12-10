using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIBuffRemovalAmount : UISelectionEntry<UIBuffRemovalAmountData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Level}, {1} = {Chance * 100}")]
        public UILocaleKeySetting formatKeyEntry = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_REMOVAL_ENTRY);
        [Tooltip("Format => {0} = {Entries}")]
        public UILocaleKeySetting formatKeyEntriesOnly = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Status Effect Title}, {1} = {Entries}")]
        public UILocaleKeySetting formatKeyTitleWithEntries = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BUFF_REMOVAL_ENTRIES);
        public string entriesSeparator = ",";

        [Header("UI Elements")]
        public UIGameDataElements uiGameDataElements;
        public TextWrapper uiTextEntriesOnly;
        public TextWrapper uiTextTitleWithEntries;

        protected override void UpdateData()
        {
            if (uiGameDataElements != null)
            {
                uiGameDataElements.Update(Data.removal.source.data);
            }

            if (uiTextEntriesOnly != null)
            {
                uiTextEntriesOnly.text = ZString.Format(
                    LanguageManager.GetText(formatKeyEntriesOnly),
                    Data.removal.GetChanceEntriesText(Data.amount, LanguageManager.GetText(formatKeyEntry), entriesSeparator));
            }

            if (uiTextTitleWithEntries != null)
            {
                uiTextTitleWithEntries.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitleWithEntries),
                    Data.removal.Title,
                    Data.removal.GetChanceEntriesText(Data.amount, LanguageManager.GetText(formatKeyEntry), entriesSeparator));
            }
        }
    }
}
