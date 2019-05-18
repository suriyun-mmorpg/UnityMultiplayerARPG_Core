using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentSet : UIBaseEquipmentBonus<EquipmentSetWithEquippedCountTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}, {1} = {Effects}")]
        [Multiline]
        public string formatTitleWithEffects = "<color=#ffa500ff>{0}</color>\n{1}";
        [Tooltip("Format => {0} = {Equip Amount}, {1} = {Effects}")]
        public string formatAppliedEffect = "<color=#ffa500ff>({0}) {1}</color>";
        [Tooltip("Format => {0} = {Equip Amount}, {1} = {Effects}")]
        public string formatUnappliedEffect = "({0}) {1}";

        // TODO: This is deprecated
        [HideInInspector]
        public TextWrapper uiTextAllEffects;

        protected override void Awake()
        {
            base.Awake();
            if (uiTextAllBonus == null && uiTextAllEffects != null)
                uiTextAllBonus = uiTextAllEffects;
        }

        protected override void UpdateData()
        {
            string allBonusText = string.Empty;
            int effectCount = 1;
            string tempText;
            foreach (EquipmentBonus effect in Data.equipmentSet.effects)
            {
                tempText = GetEquipmentBonusText(effect);
                if (!string.IsNullOrEmpty(tempText))
                {
                    if (!string.IsNullOrEmpty(allBonusText))
                        allBonusText += "\n";
                    allBonusText += string.Format(effectCount <= Data.equippedCount ? formatAppliedEffect : formatUnappliedEffect, effectCount.ToString("N0"), tempText);
                }
                ++effectCount;
            }
            if (uiTextAllBonus != null)
            {
                uiTextAllBonus.gameObject.SetActive(!string.IsNullOrEmpty(allBonusText));
                uiTextAllBonus.text = string.Format(formatTitleWithEffects, Data.equipmentSet.Title, allBonusText);
            }
        }
    }
}
