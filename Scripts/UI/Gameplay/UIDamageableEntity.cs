using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {
        [Header("Damageable Entity - String Formats")]
        [Tooltip("Format => {0} = {Current Hp}, {1} = {Max Hp}")]
        public string formatKeyHp = UILocaleKeys.UI_FORMAT_CURRENT_HP.ToString();

        [Header("Damageable Entity - UI Elements")]
        public TextWrapper uiTextHp;
        public Image imageHpGage;
        protected int currentHp;
        protected int maxHp;

        [Header("Options")]
        [Tooltip("Visible when hit duration for non owning character")]
        public float visibleWhenHitDuration = 2f;

        protected override void Update()
        {
            base.Update();

            if (!CacheCanvas.enabled)
                return;

            currentHp = 0;
            maxHp = 0;
            if (Data != null)
            {
                currentHp = Data.CurrentHp;
                maxHp = Data.MaxHp;
            }

            if (uiTextHp != null)
            {
                uiTextHp.text = string.Format(
                    LanguageManager.GetText(formatKeyHp),
                    currentHp.ToString("N0"),
                    maxHp.ToString("N0"));
            }

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;
        }

        protected override bool ValidateToUpdateUI()
        {
            return base.ValidateToUpdateUI() && !Data.IsDead();
        }
    }

    public class UIDamageableEntity : UIDamageableEntity<DamageableEntity> { }
}
