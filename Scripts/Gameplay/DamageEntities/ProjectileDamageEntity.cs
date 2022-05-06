using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class ProjectileDamageEntity : MissileDamageEntity
    {
        public UnityEvent onProjectileDisappear = new UnityEvent();

        [Header("Configuration")]
        public LayerMask hitLayers;
        [Tooltip("if you don't set it, you better don't change destroy delay.")]
        [FormerlySerializedAs("ProjectileObject")]
        public GameObject projectileObject;
        [Space]
        public bool hasGravity = false;
        [Tooltip("If customGravity is zero, its going to use physics.gravity")]
        public Vector3 customGravity;
        [Space]
        [Tooltip("Angle of shoot.")]
        public bool useAngle = false;
        [Range(0, 89)]
        public float angle;
        [Space]
        [Tooltip("Calculate the speed needed for the arc. Perfect for lock on targets.")]
        public bool recalculateSpeed = false;

        [Header("Prediction Steps")]
        [Tooltip("How many ray casts per frame to detect collisions.")]
        public int predictionStepPerFrame = 6;
        private Vector3 bulletVelocity;

        [Header("Extra Effects")]
        [Tooltip("If you want to activate an effect that is child or instantiate it on client. For 'child' effect, use destroy delay.")]
        public bool instantiateImpact = false;
        [Tooltip("Change direction of the impact effect based on hit normal.")]
        public bool useNormal = false;
        [FormerlySerializedAs("ImpactEffect")]
        public GameObject impactEffect;
        [Tooltip("Perfect for arrows. If you are using 'Child effect', when the projectile despawn, the effect too.")]
        public bool stickTo;
        [Space]
        [Tooltip("This is the effect that spawn if don't hit anything and the end of the max distance.")]
        public bool instantiateDisappear = false;
        public GameObject disappearEffect;

        private Vector3 initialPosition;
        private bool impacted;
        private Vector3 normal;
        private Vector3 hitPos;
        private Vector3 iniImpactEffectPos;

        public override void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            short skillLevel,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            base.Setup(instigator, weapon, damageAmounts, skill, skillLevel, missileDistance, missileSpeed, lockingTarget);

            // Initial configuration
            initialPosition = CacheTransform.position;
            impacted = false;

            // Configuration bullet and effects
            if (projectileObject) projectileObject.SetActive(true);
            if (impactEffect && !instantiateImpact)
            {
                impactEffect.SetActive(false);
                iniImpactEffectPos = impactEffect.transform.localPosition;
            }
            if (disappearEffect && !instantiateDisappear) disappearEffect.SetActive(false);

            // Movement
            Vector3 targetPos = initialPosition + (CacheTransform.forward * missileDistance);

            if (lockingTarget != null && lockingTarget.CurrentHp > 0) targetPos = lockingTarget.GetTransform().position;

            float dist = Vector3.Distance(initialPosition, targetPos);
            float yOffset = -transform.forward.y;

            if (recalculateSpeed) missileSpeed = LaunchSpeed(dist, yOffset, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);

            if (useAngle) CacheTransform.eulerAngles = new Vector3(CacheTransform.eulerAngles.x - angle, CacheTransform.eulerAngles.y, CacheTransform.eulerAngles.z);

            bulletVelocity = CacheTransform.forward * missileSpeed;
        }

        public float LaunchSpeed(float distance, float yOffset, float gravity, float angle)
        {
            float speed = (distance * Mathf.Sqrt(gravity) * Mathf.Sqrt(1 / Mathf.Cos(angle))) / Mathf.Sqrt(2 * distance * Mathf.Sin(angle) + 2 * yOffset * Mathf.Cos(angle));
            return speed;
        }

        protected override void Update()
        {
            /* clear up Missile Duration */
        }

        protected override void FixedUpdate()
        {
            // Don't move if exploded or collided
            if (isExploded || impacted) return;

            Vector3 point1 = CacheTransform.position;
            float stepSize = 1.0f / predictionStepPerFrame;
            // Find hitting objects by future positions
            for (float step = 0; step < 1; step += stepSize)
            {
                if (hasGravity)
                {
                    Vector3 gravity = Physics.gravity;
                    if (customGravity != Vector3.zero) gravity = customGravity;
                    bulletVelocity += gravity * stepSize * Time.deltaTime;
                }

                Vector3 point2 = point1 + bulletVelocity * stepSize * Time.deltaTime;

                int hitCount = 0;
                RaycastHit hit;
                Vector3 origin = point1;
                Vector3 dir = (point2 - point1).normalized;
                float dist = Vector3.Distance(point2, point1);
                switch (hitDetectionMode)
                {
                    case HitDetectionMode.Raycast:
                        hitCount = Physics.RaycastNonAlloc(origin, dir, hits3D, dist, hitLayers);
                        break;
                    case HitDetectionMode.SphereCast:
                        hitCount = Physics.SphereCastNonAlloc(origin, sphereCastRadius, dir, hits3D, dist, hitLayers);
                        break;
                    case HitDetectionMode.BoxCast:
                        hitCount = Physics.BoxCastNonAlloc(origin, boxCastSize * 0.5f, dir, hits3D, CacheTransform.rotation, dist, hitLayers);
                        break;
                }

                for (int i = 0; i < hitCount; ++i)
                {
                    hit = hits3D[i];
                    if (useNormal)
                        normal = hit.normal;
                    hitPos = hit.point;

                    // Hit itself, no impact
                    BaseGameEntity instigatorEntity;
                    if (instigator.Id != null && instigator.TryGetEntity(out instigatorEntity) && instigatorEntity.transform.root == hit.transform.root)
                        continue;

                    Impact(hit.collider.transform.gameObject);
                    if (destroying)
                        return;
                }

                // Moved too far from `initialPosition`
                if (Vector3.Distance(initialPosition, point2) > missileDistance)
                {
                    NoImpact();
                    return;
                }

                point1 = point2;
            }
            CacheTransform.rotation = Quaternion.LookRotation(bulletVelocity);
            CacheTransform.position = point1;
        }

        protected void NoImpact()
        {
            if (destroying)
                return;

            if (disappearEffect && IsClient)
            {
                if (onProjectileDisappear != null)
                    onProjectileDisappear.Invoke();

                if (projectileObject)
                    projectileObject.SetActive(false);

                if (instantiateDisappear)
                    Instantiate(disappearEffect, transform.position, CacheTransform.rotation);
                else
                    disappearEffect.SetActive(true);

                PushBack(destroyDelay);
                destroying = true;
                return;
            }
            PushBack();
            destroying = true;
        }

        protected void Impact(GameObject hitted)
        {
            // Spawn impact effect
            if (impactEffect && IsClient)
            {
                if (projectileObject)
                    projectileObject.SetActive(false);

                if (instantiateImpact)
                {
                    Quaternion rot = Quaternion.identity;
                    if (useNormal) rot = Quaternion.FromToRotation(Vector3.forward, normal);
                    GameObject impact = Instantiate(impactEffect, hitPos, rot);
                    if (stickTo) impact.transform.parent = hitted.transform;

                }
                else
                {
                    if (useNormal) impactEffect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
                    impactEffect.transform.position = hitPos;
                    if (stickTo) impactEffect.transform.parent = hitted.transform;
                    impactEffect.SetActive(true);
                }
            }

            // Check target
            DamageableHitBox target;
            if (FindTargetHitBox(hitted, out target))
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
                impacted = true;
                PushBack(destroyDelay);
                destroying = true;
                return;
            }

            // Hit damageable entity but it is not hitbox, skip it
            if (hitted.GetComponent<DamageableEntity>() != null)
                return;

            if (explodeDistance > 0f)
            {
                // Explode immediately when hit something
                Explode();
            }

            impacted = true;
            PushBack(destroyDelay);
            destroying = true;
        }

        protected override void OnPushBack()
        {
            if (impactEffect && stickTo && !instantiateImpact)
            {
                impactEffect.transform.parent = CacheTransform;
                impactEffect.transform.localPosition = iniImpactEffectPos;
            }
            base.OnPushBack();
        }
    }
}
