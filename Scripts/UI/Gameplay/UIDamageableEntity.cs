using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {
        [Header("Damageable Entity - Display Format")]
        [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}, {2} = {Hp Label}")]
        public string hpFormat = "{2}: {0}/{1}";

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
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"), LanguageManager.GetText(UILocaleKeys.UI_LABEL_HP.ToString()));

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
