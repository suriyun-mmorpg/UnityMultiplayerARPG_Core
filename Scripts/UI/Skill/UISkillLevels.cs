using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillLevels : UISelectionEntry<Dictionary<BaseSkill, short>>
    {
        public enum DisplayType
        {
            Simple,
            Requirement
        }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_SKILL);
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public UILocaleKeySetting formatKeyLevelNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_SKILL_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Target Level}")]
        public UILocaleKeySetting formatKeySimpleLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_LEVEL);

        [Header("UI Elements")]
        public TextWrapper uiTextAllLevels;
        public UISkillTextPair[] textLevels;
        public DisplayType displayType;
        public bool isBonus;

        private Dictionary<BaseSkill, TextWrapper> cacheTextLevels;
        public Dictionary<BaseSkill, TextWrapper> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<BaseSkill, TextWrapper>();
                    BaseSkill tempSkill;
                    TextWrapper tempTextComponent;
                    foreach (UISkillTextPair textLevel in textLevels)
                    {
                        if (textLevel.skill == null || textLevel.uiText == null)
                            continue;
                        tempSkill = textLevel.skill;
                        tempTextComponent = textLevel.uiText;
                        SetDefaultText(tempTextComponent, tempSkill.Title);
                        cacheTextLevels[tempSkill] = tempTextComponent;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllLevels != null)
                    uiTextAllLevels.gameObject.SetActive(false);

                foreach (KeyValuePair<BaseSkill, TextWrapper> entry in CacheTextLevels)
                {
                    SetDefaultText(entry.Value, entry.Key.Title);
                }
            }
            else
            {
                string tempAllText = string.Empty;
                BaseSkill tempSkill;
                short tempCurrentLevel;
                short tempTargetLevel;
                string tempFormat;
                string tempLevelText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<BaseSkill, short> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempSkill = dataEntry.Key;
                    tempCurrentLevel = 0;
                    tempTargetLevel = dataEntry.Value;
                    string tempCurrentValue;
                    string tempTargetValue;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Get skill level from character
                    if (owningCharacter != null)
                        owningCharacter.GetCaches().Skills.TryGetValue(tempSkill, out tempCurrentLevel);
                    // Use difference format by option 
                    switch (displayType)
                    {
                        case DisplayType.Requirement:
                            // This will show both current character skill level and target level
                            tempFormat = tempCurrentLevel >= tempTargetLevel ?
                                LanguageManager.GetText(formatKeyLevel) :
                                LanguageManager.GetText(formatKeyLevelNotEnough);
                            tempCurrentValue = tempCurrentLevel.ToString("N0");
                            tempTargetValue = tempTargetLevel.ToString("N0");
                            tempLevelText = string.Format(tempFormat, tempSkill.Title, tempCurrentValue, tempTargetValue);
                            break;
                        default:
                            // This will show only target level, so current character skill level will not be shown
                            if (isBonus)
                                tempTargetValue = tempTargetLevel.ToBonusString("N0");
                            else
                                tempTargetValue = tempTargetLevel.ToString("N0");
                            tempLevelText = string.Format(
                                LanguageManager.GetText(formatKeySimpleLevel),
                                tempSkill.Title,
                                tempTargetValue);
                            break;
                    }
                    // Append current skill level text
                    tempAllText += tempLevelText;
                    // Set current skill text to UI
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out tempTextWrapper))
                        tempTextWrapper.text = tempLevelText;
                }

                if (uiTextAllLevels != null)
                {
                    uiTextAllLevels.gameObject.SetActive(!string.IsNullOrEmpty(tempAllText));
                    uiTextAllLevels.text = tempAllText;
                }
            }
        }

        private void SetDefaultText(TextWrapper text, string title)
        {
            switch (displayType)
            {
                case DisplayType.Requirement:
                    text.text = string.Format(
                        LanguageManager.GetText(formatKeyLevel),
                        title,
                        "0", "0");
                    break;
                case DisplayType.Simple:
                    text.text = string.Format(
                        LanguageManager.GetText(formatKeySimpleLevel),
                        title,
                        isBonus ? "+0" : "0");
                    break;
            }
        }
    }
}
