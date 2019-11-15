using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class MissileDamageEntity : BaseDamageEntity
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

        private Rigidbody cacheRigidbody;
        public Rigidbody CacheRigidbody
        {
            get
            {
                if (cacheRigidbody == null)
                    cacheRigidbody = GetComponent<Rigidbody>();
                return cacheRigidbody;
            }
        }

        private Rigidbody2D cacheRigidbody2D;
        public Rigidbody2D CacheRigidbody2D
        {
            get
            {
                if (cacheRigidbody2D == null)
                    cacheRigidbody2D = GetComponent<Rigidbody2D>();
                return cacheRigidbody2D;
            }
        }

        private float launchTime;
        private float missileDuration;
        private bool destroying;

        private void Awake()
        {
            gameObject.layer = 2;   // Ignore raycast
        }

        public void Setup(
            IAttackerEntity attacker,
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
                Destroy(gameObject, destroyDelay);
                destroying = true;
                return;
            }

            this.skill = skill;
            this.lockingTarget = lockingTarget;
            launchTime = Time.unscaledTime;
            missileDuration = missileDistance / missileSpeed;
        }

        private void Update()
        {
            if (destroying)
                return;

            if (Time.unscaledTime - launchTime > missileDuration)
            {
                Explode();
                Destroy(gameObject, destroyDelay);
                destroying = true;
            }
        }

        private void FixedUpdate()
        {
            // Don't move if exploded
            if (isExploded)
            {
                if (gameInstance.DimensionType == DimensionType.Dimension2D)
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

            if (gameInstance.DimensionType == DimensionType.Dimension2D)
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

        private void OnDestroy()
        {
            if (onDestroy != null)
                onDestroy.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            if (destroying)
                return;

            if (attacker != null && attacker.Entity.gameObject == other)
                return;

            IDamageableEntity target = null;
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
                Destroy(gameObject, destroyDelay);
                destroying = true;
                return;
            }

            // Don't hits: TransparentFX, IgnoreRaycast, character, item
            if (other.layer != 1 &&
                other.layer != 2 &&
                other.layer != gameInstance.characterLayer &&
                other.layer != gameInstance.itemDropLayer)
            {
                if (explodeDistance > 0f)
                {
                    // Explode immediately when hit something
                    Explode();
                }
                Destroy(gameObject, destroyDelay);
                destroying = true;
                return;
            }
        }

        private bool FindTargetEntity(GameObject other, out IDamageableEntity target)
        {
            target = null;

            if (attacker != null && attacker.Entity.gameObject == other)
                return false;

            target = other.GetComponent<IDamageableEntity>();

            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(attacker))
                return false;

            if (lockingTarget != null && lockingTarget != target)
                return false;

            return true;
        }

        private bool FindAndApplyDamage(GameObject other)
        {
            IDamageableEntity target;
            if (FindTargetEntity(other, out target))
            {
                ApplyDamageTo(target);
                return true;
            }
            return false;
        }

        private void Explode()
        {
            if (isExploded || !IsServer)
                return;

            isExploded = true;

            // Explode when distance > 0
            if (explodeDistance <= 0f)
                return;

            if (onExploded != null)
                onExploded.Invoke();

            ExplodeApplyDamage();
        }

        private void ExplodeApplyDamage()
        {
            if (gameInstance.DimensionType == DimensionType.Dimension2D)
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
