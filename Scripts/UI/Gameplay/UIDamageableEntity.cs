using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {
        [Header("Damageable Entity - Display Format")]
        [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}")]
        public string hpFormat = "Hp: {0}/{1}";

        [Header("Damageable Entity - UI Elements")]
        public TextWrapper uiTextHp;
        public Image imageHpGage;
        protected int currentHp;
        protected int maxHp;

        protected override void Update()
        {
            base.Update();

            currentHp = 0;
            maxHp = 0;
            if (Data != null)
            {
                currentHp = Data.CurrentHp;
                maxHp = Data.MaxHp;
            }

            if (uiTextHp != null)
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;
        }
    }

    public class UIDamageableEntity : UIDamageableEntity<DamageableEntity>
    {
        protected override void UpdateData()
        {
        }
    }
}
