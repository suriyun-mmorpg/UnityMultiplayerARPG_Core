using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {
        [Header("Damageable Entity - UI Elements")]
        // HP
        public UIGageValue uiGageHp;

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
            if (uiGageHp != null)
                uiGageHp.Update(currentHp, maxHp);
        }

        protected override bool ValidateToUpdateUI()
        {
            return base.ValidateToUpdateUI() && !Data.IsDead();
        }
    }

    public class UIDamageableEntity : UIDamageableEntity<DamageableEntity> { }
}
