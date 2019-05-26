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
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public TextWrapper uiTextHp;
        [HideInInspector] // TODO: This is deprecated, it will be removed later
        public Image imageHpGage;
        public UIGageValue uiGageHp;

        protected int currentHp;
        protected int maxHp;

        [Header("Options")]
        [Tooltip("Visible when hit duration for non owning character")]
        public float visibleWhenHitDuration = 2f;

        protected override void Awake()
        {
            base.Awake();
            MigrateUIGageValue();
        }

        protected void OnValidate()
        {
#if UNITY_EDITOR
            if (MigrateUIGageValue())
                EditorUtility.SetDirty(this);
#endif
        }

        protected virtual bool MigrateUIGageValue()
        {
            return UIGageValue.Migrate(ref uiGageHp, ref uiTextHp, ref imageHpGage);
        }

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
