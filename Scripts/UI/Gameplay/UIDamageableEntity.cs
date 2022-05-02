using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class UIDamageableEntity<T> : UIBaseGameEntity<T>
        where T : DamageableEntity
    {

        [Header("Options")]
        [Tooltip("This is duration before this will be invisible, if this is <= 0f it will always visible")]
        public float visibleWhenHitDuration = 2f;

        [Header("Damageable Entity - UI Elements")]
        public UIGageValue uiGageHp;

        [Header("Damageable Entity - Options")]
        public bool hideWhileDead;

        protected int currentHp;
        protected int maxHp;
        protected float receivedDamageTime;
        protected T previousEntity;

        protected override void OnEnable()
        {
            base.OnEnable();
            receivedDamageTime = 0f;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (previousEntity != null)
                previousEntity.onReceivedDamage -= OnReceivedDamage;
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
            return base.ValidateToUpdateUI() && (!hideWhileDead || !Data.IsDead());
        }

        protected override void UpdateData()
        {
            base.UpdateData();
            if (previousEntity != null)
                previousEntity.onReceivedDamage -= OnReceivedDamage;
            if (Data != null)
                Data.onReceivedDamage += OnReceivedDamage;
            previousEntity = Data;
        }

        private void OnReceivedDamage(
            HitBoxPosition position,
            Vector3 fromPosition,
            IGameEntity attacker,
            CombatAmountType combatAmountType,
            int totalDamage,
            CharacterItem weapon,
            BaseSkill skill,
            short skillLevel)
        {
            receivedDamageTime = Time.unscaledTime;
        }

        protected override void UpdateUI()
        {
            if (!ValidateToUpdateUI())
            {
                CacheCanvas.enabled = false;
                return;
            }

            if (Time.unscaledTime - receivedDamageTime < visibleWhenHitDuration || visibleWhenHitDuration <= 0f)
            {
                CacheCanvas.enabled = true;
                return;
            }

            base.UpdateUI();
        }
    }

    public class UIDamageableEntity : UIDamageableEntity<DamageableEntity> { }
}
