using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class AreaDamageEntity : BaseDamageEntity
    {
        public bool canApplyDamageToUser;
        public UnityEvent onDestroy;

        protected float applyDuration;
        protected float lastAppliedTime;
        protected readonly Dictionary<uint, DamageableHitBox> receivingDamageHitBoxes = new Dictionary<uint, DamageableHitBox>();

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="instigator">Weapon's or skill's instigator who to spawn this to attack enemy</param>
        /// <param name="weapon">Weapon which was used to attack enemy</param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="areaDuration"></param>
        /// <param name="applyDuration"></param>
        public virtual void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            float areaDuration,
            float applyDuration)
        {
            base.Setup(instigator, weapon, damageAmounts, skill, skillLevel);
            PushBack(areaDuration);
            this.applyDuration = applyDuration;
            lastAppliedTime = Time.unscaledTime;
        }

        protected virtual void Update()
        {
            if (Time.unscaledTime - lastAppliedTime >= applyDuration)
            {
                lastAppliedTime = Time.unscaledTime;
                foreach (DamageableHitBox hitBox in receivingDamageHitBoxes.Values)
                {
                    if (hitBox == null)
                        continue;

                    ApplyDamageTo(hitBox);
                }
            }
        }

        public override void ApplyDamageTo(DamageableHitBox target)
        {
            if (canApplyDamageToUser)
            {
                if (!IsServer || target == null || target.IsDead() || target.IsImmune || instigator.IsInSafeArea)
                    return;
                target.ReceiveDamageWithoutConditionCheck(CacheTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, Random.Range(0, 255));
                return;
            }
            base.ApplyDamageTo(target);
        }

        protected override void OnPushBack()
        {
            if (onDestroy != null)
                onDestroy.Invoke();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        protected virtual void TriggerEnter(GameObject other)
        {
            DamageableHitBox target = other.GetComponent<DamageableHitBox>();
            if (target == null)
                return;

            if (receivingDamageHitBoxes.ContainsKey(target.GetObjectId()))
                return;

            receivingDamageHitBoxes.Add(target.GetObjectId(), target);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        protected virtual void TriggerExit(GameObject other)
        {
            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            if (target == null)
                return;

            if (!receivingDamageHitBoxes.ContainsKey(target.GetObjectId()))
                return;

            receivingDamageHitBoxes.Remove(target.GetObjectId());
        }
    }
}
