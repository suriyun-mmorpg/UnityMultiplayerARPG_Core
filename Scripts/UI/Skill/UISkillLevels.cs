using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillLevels : UISelectionEntry<Dictionary<Skill, short>>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string formatKeyLevel = UILocaleKeys.UI_FORMAT_CURRENT_SKILL.ToString();
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string formatKeyLevelNotEnough = UILocaleKeys.UI_FORMAT_CURRENT_SKILL_NOT_ENOUGH.ToString();
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Target Level}")]
        public string formatKeySimpleLevel = UILocaleKeys.UI_FORMAT_SKILL_LEVEL.ToString();

        [Header("UI Elements")]
        public TextWrapper uiTextAllLevels;
        public UISkillTextPair[] textLevels;
        public bool showAsRequirement;

        private Dictionary<Skill, TextWrapper> cacheTextLevels;
        public Dictionary<Skill, TextWrapper> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<Skill, TextWrapper>();
                    Skill tempSkill;
                    TextWrapper tempTextComponent;
                    foreach (UISkillTextPair textLevel in textLevels)
                    {
                        if (textLevel.skill == null || textLevel.uiText == null)
                            continue;
                        tempSkill = textLevel.skill;
                        tempTextComponent = textLevel.uiText;
                        tempTextComponent.text = string.Format(
                            LanguageManager.GetText(formatKeyLevel),
                            tempSkill.Title,
                            "0",
                            "0");
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

                foreach (KeyValuePair<Skill, TextWrapper> entry in CacheTextLevels)
                {
                    entry.Value.text = string.Format(
                        LanguageManager.GetText(formatKeyLevel),
                        entry.Key.Title,
                        "0",
                        "0");
                }
            }
            else
            {
                string tempAllText = string.Empty;
                Skill tempSkill;
                short tempCurrentLevel;
                short tempTargetLevel;
                string tempFormat;
                string tempLevelText;
                TextWrapper tempTextWrapper;
                foreach (KeyValuePair<Skill, short> dataEntry in Data)
                {
                    if (dataEntry.Key == null || dataEntry.Value == 0)
                        continue;
                    // Set temp data
                    tempSkill = dataEntry.Key;
                    tempTargetLevel = dataEntry.Value;
                    tempCurrentLevel = 0;
                    // Add new line if text is not empty
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    // Get skill level from character
                    if (owningCharacter != null)
                        owningCharacter.CacheSkills.TryGetValue(tempSkill, out tempCurrentLevel);
                    // Use difference format by option 
                    if (showAsRequirement)
                    {
                        // This will show both current character skill level and target level
                        tempFormat = tempCurrentLevel >= tempTargetLevel ?
                            LanguageManager.GetText(formatKeyLevel) :
                            LanguageManager.GetText(formatKeyLevelNotEnough);
                        tempLevelText = string.Format(tempFormat, tempSkill.Title, tempCurrentLevel.ToString("N0"), tempTargetLevel.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target level, so current character skill level will not be shown
                        tempLevelText = string.Format(
                            LanguageManager.GetText(formatKeySimpleLevel),
                            tempSkill.Title,
                            tempTargetLevel.ToString("N0"));
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
    }
}
