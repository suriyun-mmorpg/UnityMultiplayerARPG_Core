using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillLevels : UISelectionEntry<Dictionary<Skill, short>>
    {
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string formatLevel = "{0}: {1}/{2}";
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}
        /// </summary>
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string formatLevelNotEnough = "{0}: <color=red>{1}/{2}</color>";
        /// <summary>
        /// Format => {0} = {Skill Title}, {1} = {Target Level}
        /// </summary>
        [Tooltip("Format => {0} = {Skill Title}, {1} = {Target Level}")]
        public string formatSimpleLevel = "{0}: {1}";

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
                        tempTextComponent.text = string.Format(formatLevel, tempSkill.Title, "0", "0");
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
                    entry.Value.text = string.Format(formatLevel, entry.Key.Title, "0", "0");
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
                        // This will show both current character skill level and target amount
                        tempFormat = tempCurrentLevel >= tempTargetLevel ? formatLevel : formatLevelNotEnough;
                        tempLevelText = string.Format(tempFormat, tempSkill.Title, tempCurrentLevel.ToString("N0"), tempTargetLevel.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target level, so current character skill level will not be shown
                        tempLevelText = string.Format(formatSimpleLevel, tempSkill.Title, tempTargetLevel.ToString("N0"));
                    }
                    // Append current attribute amount text
                    tempAllText += tempLevelText;
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
