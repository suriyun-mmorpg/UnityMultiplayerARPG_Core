using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class MissileDamageEntity : BaseDamageEntity
    {
        public float destroyDelay;
        public UnityEvent onExploded;
        public UnityEvent onDestroy;
        [Tooltip("If this value more than 0, when it hit anything or it is out of life, it will explode and apply damage to characters in this distance")]
        public float explodeDistance;

        protected float missileDistance;
        protected float missileSpeed;
        protected bool isExploded;
        protected IDamageableEntity lockingTarget;

        public Rigidbody CacheRigidbody { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }

        protected float launchTime;
        protected float missileDuration;
        protected bool destroying;

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = PhysicLayers.IgnoreRaycast;
            CacheRigidbody = GetComponent<Rigidbody>();
            CacheRigidbody2D = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="attacker">Attacker entity who use weapon or skill to spawn this to attack enemy</param>
        /// <param name="weapon">Weapon which was used to attack enemy</param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="missileDistance">Calculated missile distance</param>
        /// <param name="missileSpeed">Calculated missile speed</param>
        /// <param name="lockingTarget">Locking target, if this is empty it can hit any entities</param>
        public virtual void Setup(
            IGameEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            Setup(attacker, weapon, damageAmounts, skill, skillLevel);
            this.missileDistance = missileDistance;
            this.missileSpeed = missileSpeed;

            if (missileDistance <= 0 && missileSpeed <= 0)
            {
                // Explode immediately when distance and speed is 0
                Explode();
                PushBack(destroyDelay);
                destroying = true;
                return;
            }
            this.lockingTarget = lockingTarget;
            isExploded = false;
            destroying = false;
            launchTime = Time.unscaledTime;
            missileDuration = (missileDistance / missileSpeed) + 0.1f;
        }

        protected virtual void Update()
        {
            if (destroying)
                return;

            if (Time.unscaledTime - launchTime >= missileDuration)
            {
                Explode();
                PushBack(destroyDelay);
                destroying = true;
            }
        }

        protected virtual void FixedUpdate()
        {
            // Don't move if exploded
            if (isExploded)
            {
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                {
                    if (CacheRigidbody2D != null)
                        CacheRigidbody2D.velocity = Vector2.zero;
                }
                else
                {
                    if (CacheRigidbody != null)
                        CacheRigidbody.velocity = Vector3.zero;
                }
                return;
            }

            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                if (CacheRigidbody2D != null)
                    CacheRigidbody2D.velocity = -CacheTransform.up * missileSpeed;
            }
            else
            {
                if (CacheRigidbody != null)
                    CacheRigidbody.velocity = CacheTransform.forward * missileSpeed;
            }
        }

        protected override void OnPushBack()
        {
            if (onDestroy != null)
                onDestroy.Invoke();
            base.OnPushBack();
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
            if (destroying)
                return;

            if (other.layer == PhysicLayers.TransparentFX ||
                other.layer == PhysicLayers.IgnoreRaycast ||
                other.layer == PhysicLayers.Water)
                return;

            if (other.GetComponent<IUnHittable>() != null)
                return;

            if (attacker != null && attacker.GetGameObject() == other)
                return;

            IDamageableEntity target;
            if (FindTargetEntity(other, out target))
            {
                if (explodeDistance > 0f)
                {
                    // Explode immediately when hit something
                    Explode();
                }
                else
                {
                    // If this is not going to explode, just apply damage to target
                    ApplyDamageTo(target);
                }
                PushBack(destroyDelay);
                destroying = true;
                return;
            }

            // Hit walls or grounds → Explode
            if (other.layer != CurrentGameInstance.characterLayer &&
                other.layer != CurrentGameInstance.itemDropLayer &&
                !CurrentGameInstance.IgnoreRaycastLayersValues.Contains(other.layer))
            {
                if (explodeDistance > 0f)
                {
                    // Explode immediately when hit something
                    Explode();
                }
                PushBack(destroyDelay);
                destroying = true;
                return;
            }
        }

        protected virtual bool FindTargetEntity(GameObject other, out IDamageableEntity target)
        {
            target = null;

            if (attacker != null && attacker.GetGameObject() == other)
                return false;

            target = other.GetComponent<IDamageableEntity>();

            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(attacker))
                return false;

            if (lockingTarget != null && lockingTarget != target)
                return false;

            return true;
        }

        protected virtual bool FindAndApplyDamage(GameObject other)
        {
            IDamageableEntity target;
            if (FindTargetEntity(other, out target))
            {
                ApplyDamageTo(target);
                return true;
            }
            return false;
        }

        protected virtual void Explode()
        {
            if (isExploded)
                return;

            isExploded = true;

            // Explode when distance > 0
            if (explodeDistance <= 0f)
                return;

            if (onExploded != null)
                onExploded.Invoke();

            if (!IsServer)
                return;

            ExplodeApplyDamage();
        }

        protected virtual void ExplodeApplyDamage()
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(CacheTransform.position, explodeDistance);
                foreach (Collider2D collider in colliders2D)
                {
                    FindAndApplyDamage(collider.gameObject);
                }
            }
            else
            {
                Collider[] colliders = Physics.OverlapSphere(CacheTransform.position, explodeDistance);
                foreach (Collider collider in colliders)
                {
                    FindAndApplyDamage(collider.gameObject);
                }
            }
        }
    }
}
