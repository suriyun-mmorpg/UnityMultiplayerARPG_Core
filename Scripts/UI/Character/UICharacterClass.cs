using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterClass : UISelectionEntry<BaseCharacter>
    {
        /// <summary>
        /// Format => {0} = {Title}
        /// </summary>
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public string formatTitle = "{0}";
        /// <summary>
        /// Format => {0} = {Description}
        /// </summary>
        [Tooltip("Format => {0} = {Description}")]
        public string formatDescription = "{0}";

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;
        public UICharacterStats uiStats;
        public UIAttributeAmounts uiAttributes;
        public UIResistanceAmounts uiResistances;
        public UISkillLevels uiSkills;

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = string.Format(formatTitle, Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);

            if (uiTextDescription != null)
                uiTextDescription.text = string.Format(formatDescription, Data == null ? LanguageManager.GetUnknowDescription() : Data.Description);

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }

            if (uiStats != null)
                uiStats.Data = Data.GetCharacterStats(1);

            if (uiAttributes != null)
                uiAttributes.Data = Data.GetCharacterAttributes(1);

            if (uiResistances != null)
                uiResistances.Data = Data.GetCharacterResistances(1);

            if (uiSkills != null)
                uiSkills.Data = Data.CacheSkillLevels;
        }
    }
}
