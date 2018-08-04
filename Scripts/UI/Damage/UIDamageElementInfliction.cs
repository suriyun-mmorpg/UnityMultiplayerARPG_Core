using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIDamageElementInfliction : UISelectionEntry<DamageElementInflictionTuple>
    {
        [Tooltip("Default Element Infliction Format => {1} = {Rate}")]
        public string defaultElementInflictionFormat = "Inflict {1}% damage";
        [Tooltip("Infliction Format => {0} = {Element title}, {1} = {Rate}")]
        public string inflictionFormat = "Inflict {1}% as {0} damage";

        [Header("UI Elements")]
        public Text textInfliction;
        public TextWrapper uiTextInfliction;

        protected override void UpdateData()
        {
            if (uiTextInfliction != null)
            {
                var element = Data.damageElement;
                var rate = Data.infliction;
                var format = element == GameInstance.Singleton.DefaultDamageElement ? defaultElementInflictionFormat : inflictionFormat;
                uiTextInfliction.text = string.Format(format, element.title, (rate * 100f).ToString("N0"));
            }
        }

        [ContextMenu("Update UI Components")]
        public void UpdateUIComponents()
        {
            uiTextInfliction = UIWrapperHelpers.SetWrapperToText(textInfliction, uiTextInfliction);
        }
    }
}
