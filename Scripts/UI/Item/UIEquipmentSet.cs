using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIEquipmentSet : UIBaseEquipmentBonus<EquipmentSetWithEquippedCountTuple>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Set Title}, {1} = {List Of Effect}")]
        [Multiline]
        public string formatKeySet = UILocaleKeys.UI_FORMAT_EQUIPMENT_SET.ToString();
        [Tooltip("Format => {0} = {Equip Amount}, {1} = {List Of Bonus}")]
        public string formatKeyAppliedEffect = UILocaleKeys.UI_FORMAT_EQUIPMENT_SET_APPLIED_EFFECT.ToString();
        [Tooltip("Format => {0} = {Equip Amount}, {1} = {List Of Bonus}")]
        public string formatKeyUnappliedEffect = UILocaleKeys.UI_FORMAT_EQUIPMENT_SET_UNAPPLIED_EFFECT.ToString();

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
                    allBonusText += string.Format(
                        effectCount <= Data.equippedCount ?
                            LanguageManager.GetText(formatKeyAppliedEffect) :
                            LanguageManager.GetText(formatKeyUnappliedEffect),
                        effectCount.ToString("N0"),
                        tempText);
                }
                ++effectCount;
            }

            if (uiTextAllBonus != null)
            {
                uiTextAllBonus.gameObject.SetActive(!string.IsNullOrEmpty(allBonusText));
                uiTextAllBonus.text = string.Format(
                    LanguageManager.GetText(formatKeySet),
                    Data.equipmentSet.Title,
                    allBonusText);
            }
        }
    }
}
