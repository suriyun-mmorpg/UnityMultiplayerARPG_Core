using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UISkillRequirement : UISelectionEntry<SkillTuple>
    {
        [Header("Requirement Format")]
        [Tooltip("Require Level Format => {0} = {Level}")]
        public string requireLevelFormat = "Require Level: {0}";

        [Header("UI Elements")]
        public Text textRequireLevel;
        public TextWrapper uiTextRequireLevel;
        public UISkillLevels uiRequireSkillLevels;

        protected override void UpdateData()
        {
            MigrateUIComponents();

            var skill = Data.skill;
            var level = Data.targetLevel;

            if (uiTextRequireLevel != null)
            {
                if (skill == null)
                    uiTextRequireLevel.gameObject.SetActive(false);
                else
                {
                    uiTextRequireLevel.gameObject.SetActive(true);
                    uiTextRequireLevel.text = string.Format(requireLevelFormat, skill.GetRequireCharacterLevel(level).ToString("N0"));
                }
            }

            if (uiRequireSkillLevels != null)
            {
                if (skill == null)
                    uiRequireSkillLevels.Hide();
                else
                {
                    uiRequireSkillLevels.Show();
                    uiRequireSkillLevels.Data = skill.CacheRequireSkillLevels;
                }
            }
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextRequireLevel = MigrateUIHelpers.SetWrapperToText(textRequireLevel, uiTextRequireLevel);
        }
    }
}
