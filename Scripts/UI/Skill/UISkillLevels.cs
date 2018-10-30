using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISkillLevels : UISelectionEntry<Dictionary<Skill, short>>
    {
        [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string levelFormat = "{0}: {1}/{2}";
        [Tooltip("Skill Level Format => {0} = {Skill title}, {1} = {Current Level}, {2} = {Target Level}")]
        public string levelNotReachTargetFormat = "{0}: <color=red>{1}/{2}</color>";

        [Header("UI Elements")]
        public TextWrapper uiTextAllLevels;
        public UISkillTextPair[] textLevels;

        private Dictionary<Skill, TextWrapper> cacheTextLevels;
        public Dictionary<Skill, TextWrapper> CacheTextLevels
        {
            get
            {
                if (cacheTextLevels == null)
                {
                    cacheTextLevels = new Dictionary<Skill, TextWrapper>();
                    foreach (var textLevel in textLevels)
                    {
                        if (textLevel.skill == null || textLevel.uiText == null)
                            continue;
                        var key = textLevel.skill;
                        var textComp = textLevel.uiText;
                        textComp.text = string.Format(levelFormat, key.title, "0", "0");
                        cacheTextLevels[key] = textComp;
                    }
                }
                return cacheTextLevels;
            }
        }

        protected override void UpdateData()
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (Data == null || Data.Count == 0)
            {
                if (uiTextAllLevels != null)
                    uiTextAllLevels.gameObject.SetActive(false);

                foreach (var textLevel in CacheTextLevels)
                {
                    var element = textLevel.Key;
                    textLevel.Value.text = string.Format(levelFormat, element.title, "0", "0");
                }
            }
            else
            {
                var text = "";
                foreach (var dataEntry in Data)
                {
                    var skill = dataEntry.Key;
                    var targetLevel = dataEntry.Value;
                    if (skill == null || targetLevel == 0)
                        continue;
                    if (!string.IsNullOrEmpty(text))
                        text += "\n";
                    short currentLevel = 0;
                    if (owningCharacter != null)
                        owningCharacter.CacheSkills.TryGetValue(skill, out currentLevel);
                    var format = currentLevel >= targetLevel ? levelFormat : levelNotReachTargetFormat;
                    var amountText = string.Format(format, skill.title, currentLevel.ToString("N0"), targetLevel.ToString("N0"));
                    text += amountText;
                    TextWrapper cacheTextAmount;
                    if (CacheTextLevels.TryGetValue(dataEntry.Key, out cacheTextAmount))
                        cacheTextAmount.text = amountText;
                }
                if (uiTextAllLevels != null)
                {
                    uiTextAllLevels.gameObject.SetActive(!string.IsNullOrEmpty(text));
                    uiTextAllLevels.text = text;
                }
            }
        }
    }
}
