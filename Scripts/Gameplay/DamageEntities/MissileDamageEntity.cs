using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class MissileDamageEntity : BaseDamageEntity
    {
        public enum HitDetectionMode
        {
            Raycast,
            SphereCast,
            BoxCast,
        }
        public HitDetectionMode hitDetectionMode = HitDetectionMode.Raycast;
        public float sphereRadius = 1f;
        public Vector3 boxSize = Vector3.one;
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
        protected Vector3? previousPosition;
        protected RaycastHit2D[] hits2D = new RaycastHit2D[8];
        protected RaycastHit[] hits3D = new RaycastHit[8];

        protected override void Awake()
        {
            base.Awake();
            CacheRigidbody = GetComponent<Rigidbody>();
            CacheRigidbody2D = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="instigator">Weapon's or skill's instigator who to spawn this to attack enemy</param>
        /// <param name="weapon">Weapon which was used to attack enemy</param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="missileDistance">Calculated missile distance</param>
        /// <param name="missileSpeed">Calculated missile speed</param>
        /// <param name="lockingTarget">Locking target, if this is empty it can hit any entities</param>
        public virtual void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            Setup(instigator, weapon, damageAmounts, skill, skillLevel);
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

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Color defaultColor = Gizmos.color;
            Gizmos.color = Color.green;
            switch (hitDetectionMode)
            {
                case HitDetectionMode.SphereCast:
                    Gizmos.DrawWireSphere(transform.position, sphereRadius);
                    break;
                case HitDetectionMode.BoxCast:
                    Gizmos.DrawWireCube(transform.position, boxSize);
                    break;
            }
            Gizmos.color = defaultColor;
        }
#endif

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

            if (!destroying)
            {
                if (previousPosition.HasValue)
                {
                    int hitCount = 0;
                    int layerMask = GameInstance.Singleton.GetDamageEntityHitLayerMask();
                    Vector3 dir = (previousPosition.Value - CacheTransform.position).normalized;
                    float dist = Vector3.Distance(CacheTransform.position, previousPosition.Value);
                    // Raycast to previous position to check is it hitting something or not
                    // If hit, explode
                    switch (hitDetectionMode)
                    {
                        case HitDetectionMode.Raycast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.RaycastNonAlloc(previousPosition.Value, dir, hits2D, dist, layerMask);
                            else
                                hitCount = Physics.RaycastNonAlloc(previousPosition.Value, dir, hits3D, dist, layerMask);
                            break;
                        case HitDetectionMode.SphereCast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.CircleCastNonAlloc(previousPosition.Value, sphereRadius, dir, hits2D, dist, layerMask);
                            else
                                hitCount = Physics.SphereCastNonAlloc(previousPosition.Value, sphereRadius, dir, hits3D, dist, layerMask);
                            break;
                        case HitDetectionMode.BoxCast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.BoxCastNonAlloc(previousPosition.Value, new Vector2(boxSize.x, boxSize.y), Vector2.SignedAngle(Vector2.zero, dir), dir, hits2D, dist, layerMask);
                            else
                                hitCount = Physics.BoxCastNonAlloc(previousPosition.Value, boxSize * 0.5f, dir, hits3D, CacheTransform.rotation, dist, layerMask);
                            break;
                    }
                    for (int i = 0; i < hitCount; ++i)
                    {
                        if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D && hits2D[i].transform != null)
                            TriggerEnter(hits2D[i].transform.gameObject);
                        if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D && hits3D[i].transform != null)
                            TriggerEnter(hits3D[i].transform.gameObject);
                    }
                }
                previousPosition = CacheTransform.position;
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
            previousPosition = null;
            if (onDestroy != null)
                onDestroy.Invoke();
            base.OnPushBack();
        }

        protected virtual void TriggerEnter(GameObject other)
        {
            if (destroying)
                return;

            if (other.GetComponent<IUnHittable>() != null)
                return;

            DamageableHitBox target;
            if (FindTargetHitBox(other, out target))
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

            // Must hit walls or grounds to explode
            // So if it hit item drop, character, building, harvestable and other ignore raycasting objects, it won't explode
            if (!CurrentGameInstance.IsDamageableLayer(other.layer) &&
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

        protected virtual bool FindTargetHitBox(GameObject other, out DamageableHitBox target)
        {
            target = null;

            if (other.GetComponent<IUnHittable>() != null)
                return false;

            target = other.GetComponent<DamageableHitBox>();

            if (target == null || target.IsDead() || !target.CanReceiveDamageFrom(instigator))
                return false;

            BaseGameEntity instigatorEntity;
            if (instigator.TryGetEntity(out instigatorEntity) && instigatorEntity == target.Entity)
                return false;

            if (lockingTarget != null && lockingTarget.GetObjectId() != target.GetObjectId())
                return false;

            return true;
        }

        protected virtual bool FindAndApplyDamage(GameObject other)
        {
            DamageableHitBox target;
            if (FindTargetHitBox(other, out target))
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
