using System.Collections.Generic;
using System.Text;
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

        [Header("Options")]
        public DisplayType displayType;
        public bool includeEquipmentsForCurrentLevels;
        public bool isBonus;
        public bool inactiveIfLevelZero;

        private Dictionary<BaseSkill, UISkillTextPair> cacheTextLevels;
        public Dictionary<BaseSkill, UISkillTextPair> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<BaseSkill, UISkillTextPair>();
                    BaseSkill tempSkill;
                    foreach (UISkillTextPair componentPair in textLevels)
                    {
                        if (componentPair.skill == null || componentPair.uiText == null)
                            continue;
                        tempSkill = componentPair.skill;
                        SetDefaultValue(componentPair);
                        cacheTextLevels[tempSkill] = componentPair;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            // Reset number
            foreach (UISkillTextPair entry in CacheTextLevels.Values)
            {
                SetDefaultValue(entry);
            }
            // Set number by updated data
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllLevels != null)
                    uiTextAllLevels.SetGameObjectActive(false);
            }
            else
            {
                // Prepare attribute data
                IPlayerCharacterData character = GameInstance.PlayingCharacter;
                Dictionary<BaseSkill, short> currentSkillLevels = new Dictionary<BaseSkill, short>();
                if (character != null)
                    currentSkillLevels = character.GetSkills(includeEquipmentsForCurrentLevels);
                // In-loop temp data
                StringBuilder tempAllText = new StringBuilder();
                BaseSkill tempSkill;
                short tempCurrentLevel;
                short tempTargetLevel;
                string tempFormat;
                string tempLevelText;
                UISkillTextPair tempComponentPair;
                foreach (KeyValuePair<BaseSkill, short> dataEntry in Data)
                {
                    if (dataEntry.Key == null)
                        continue;
                    // Set temp data
                    tempSkill = dataEntry.Key;
                    tempCurrentLevel = 0;
                    tempTargetLevel = dataEntry.Value;
                    string tempCurrentValue;
                    string tempTargetValue;
                    // Get skill level from character
                    currentSkillLevels.TryGetValue(tempSkill, out tempCurrentLevel);
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
                    if (dataEntry.Value != 0)
                    {
                        // Add new line if text is not empty
                        if (tempAllText.Length > 0)
                            tempAllText.Append('\n');
                        tempAllText.Append(tempLevelText);
                    }
                    // Set current skill text to UI
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out tempComponentPair))
                    {
                        tempComponentPair.uiText.text = tempLevelText;
                        if (tempComponentPair.root != null)
                            tempComponentPair.root.SetActive(!inactiveIfLevelZero || tempTargetLevel != 0);
                    }
                }

                if (uiTextAllLevels != null)
                {
                    uiTextAllLevels.SetGameObjectActive(tempAllText.Length > 0);
                    uiTextAllLevels.text = tempAllText.ToString();
                }
            }
        }

        private void SetDefaultValue(UISkillTextPair componentPair)
        {
            switch (displayType)
            {
                case DisplayType.Requirement:
                    componentPair.uiText.text = string.Format(
                        LanguageManager.GetText(formatKeyLevel),
                        componentPair.skill.Title,
                        "0", "0");
                    break;
                case DisplayType.Simple:
                    componentPair.uiText.text = string.Format(
                        LanguageManager.GetText(formatKeySimpleLevel),
                        componentPair.skill.Title,
                        isBonus ? 0.ToBonusString("N0") : "0");
                    break;
            }
            if (componentPair.imageIcon != null)
                componentPair.imageIcon.sprite = componentPair.skill.icon;
            if (inactiveIfLevelZero && componentPair.root != null)
                componentPair.root.SetActive(false);
        }
    }
}
