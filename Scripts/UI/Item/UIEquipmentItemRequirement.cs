using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentItemRequirement : UISelectionEntry<Item>
    {
        [Header("Requirement Format")]
        [Tooltip("Require Level Format => {0} = {Level}")]
        public string requireLevelFormat = "Require Level: {0}";
        [Tooltip("Require Class Format => {0} = {Class title}")]
        public string requireClassFormat = "Require Class: {0}";

        [Header("UI Elements")]
        public TextWrapper uiTextRequireLevel;
        public TextWrapper uiTextRequireClass;
        public UIAttributeAmounts uiRequireAttributeAmounts;

        protected override void UpdateData()
        {
            var equipmentItem = Data;

            if (uiTextRequireLevel != null)
            {
                if (equipmentItem == null || equipmentItem.requirement.level <= 0)
                    uiTextRequireLevel.gameObject.SetActive(false);
                else
                {
                    uiTextRequireLevel.gameObject.SetActive(true);
                    uiTextRequireLevel.text = string.Format(requireLevelFormat, equipmentItem.requirement.level.ToString("N0"));
                }
            }

            if (uiTextRequireClass != null)
            {
                if (equipmentItem == null || equipmentItem.requirement.character == null)
                    uiTextRequireClass.gameObject.SetActive(false);
                else
                {
                    uiTextRequireClass.gameObject.SetActive(true);
                    uiTextRequireClass.text = string.Format(requireClassFormat, equipmentItem.requirement.character.title);
                }
            }

            if (uiRequireAttributeAmounts != null)
            {
                if (equipmentItem == null)
                    uiRequireAttributeAmounts.Hide();
                else
                {
                    uiRequireAttributeAmounts.Show();
                    uiRequireAttributeAmounts.Data = equipmentItem.CacheRequireAttributeAmounts;
                }
            }
        }
    }
}
