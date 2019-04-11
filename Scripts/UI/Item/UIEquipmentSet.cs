using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentSet : UIBaseEquipmentBonus<EquipmentSetWithEquippedCountTuple>
    {
        [Header("Equipment Set Format")]
        [Tooltip("Title with Effects Format => {0} = {Title}, {1} = {Effects}")]
        [Multiline]
        public string titleWithEffectsFormat = "<color=#ffa500ff>{0}</color>\n{1}";
        [Tooltip("Effect Format => {0} = {Equip Amount}, {1} = {Effects}")]
        public string appliedEffectFormat = "<color=#ffa500ff>({0}) {1}</color>";
        [Tooltip("Effect Format => {0} = {Equip Amount}, {1} = {Effects}")]
        public string unappliedEffectFormat = "({0}) {1}";

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
                    allBonusText += string.Format(effectCount <= Data.equippedCount ? appliedEffectFormat : unappliedEffectFormat, effectCount.ToString("N0"), tempText);
                }
                ++effectCount;
            }
            if (uiTextAllBonus != null)
            {
                uiTextAllBonus.gameObject.SetActive(!string.IsNullOrEmpty(allBonusText));
                uiTextAllBonus.text = string.Format(titleWithEffectsFormat, Data.equipmentSet.Title, allBonusText);
            }
        }
    }
}
