using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UIDamageElementInfliction : UISelectionEntry<DamageElementInflictionTuple>
    {
        [Header("UI Elements")]
        public TextWrapper uiTextInfliction;

        protected override void UpdateData()
        {
            if (uiTextInfliction != null)
            {
                DamageElement element = Data.damageElement;
                uiTextInfliction.text = string.Format(
                    element == GameInstance.Singleton.DefaultDamageElement ?
                        LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION.ToString()) :
                        LanguageManager.GetText(UILocaleKeys.UI_FORMAT_DAMAGE_INFLICTION_AS_ELEMENTAL.ToString()),
                    element.Title,
                    (Data.infliction * 100f).ToString("N0"));
            }
        }
    }
}
