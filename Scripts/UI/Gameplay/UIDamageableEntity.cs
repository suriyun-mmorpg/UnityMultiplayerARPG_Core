using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {
        [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}")]
        public string hpFormat = "Hp: {0}/{1}";
        
        public TextWrapper uiTextHp;
        public Image imageHpGage;

        protected override void Update()
        {
            base.Update();

            var currentHp = 0;
            var maxHp = 0;
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
}
