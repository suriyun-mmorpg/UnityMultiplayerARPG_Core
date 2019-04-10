using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISkillLevels : UISelectionEntry<Dictionary<Skill, short>>
    {
        [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string levelFormat = "{0}: {1}/{2}";
        [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string levelNotEnoughFormat = "{0}: <color=red>{1}/{2}</color>";
        [Tooltip("Skill Level Format without Current Level => {0} = {Skill title}, {1} = {Target Level}")]
        public string simpleLevelFormat = "{0}: {1}";

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
                    foreach (UISkillTextPair textLevel in textLevels)
                    {
                        if (textLevel.skill == null || textLevel.uiText == null)
                            continue;
                        Skill key = textLevel.skill;
                        TextWrapper textComp = textLevel.uiText;
                        textComp.text = string.Format(levelFormat, key.Title, "0", "0");
                        cacheTextLevels[key] = textComp;
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
                    entry.Value.text = string.Format(levelFormat, entry.Key.Title, "0", "0");
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
                    tempSkill = dataEntry.Key;
                    tempTargetLevel = dataEntry.Value;
                    if (tempSkill == null || tempTargetLevel == 0)
                        continue;
                    if (!string.IsNullOrEmpty(tempAllText))
                        tempAllText += "\n";
                    tempCurrentLevel = 0;
                    if (owningCharacter != null)
                        owningCharacter.CacheSkills.TryGetValue(tempSkill, out tempCurrentLevel);
                    if (showAsRequirement)
                    {
                        tempFormat = tempCurrentLevel >= tempTargetLevel ? levelFormat : levelNotEnoughFormat;
                        tempLevelText = string.Format(tempFormat, tempSkill.Title, tempCurrentLevel.ToString("N0"), tempTargetLevel.ToString("N0"));
                    }
                    else
                    {
                        // This will show only target level, so current character skill level will not be shown
                        tempLevelText = string.Format(simpleLevelFormat, tempSkill.Title, tempTargetLevel.ToString("N0"));
                    }
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
