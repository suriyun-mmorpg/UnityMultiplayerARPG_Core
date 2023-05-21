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

        [Header("Extra Effects")]
        [Tooltip("If you want to activate an effect that is child or instantiate it on client. For 'child' effect, use destroy delay.")]
        public bool instantiateImpact = false;
        [FormerlySerializedAs("ImpactEffect")]
        public GameObject impactEffect;
        [Tooltip("Change direction of the impact effect based on hit normal.")]
        public bool useNormal = false;
        [Tooltip("Perfect for arrows. If you are using 'Child effect', when the projectile despawn, the effect too.")]
        [FormerlySerializedAs("stickTo")]
        public bool stickToHitObject;
        [Space]
        [Tooltip("This is the effect that spawn if don't hit anything and the end of the max distance.")]
        public bool instantiateDisappear = false;
        public GameObject disappearEffect;

        private Vector3 _initialPosition;
        private Vector3 _defaultImpactEffectPosition;
        private bool _impacted;
        private Vector3 _bulletVelocity;
        private Vector3 _normal;
        private Vector3 _hitPos;

        public override void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            DamageHitDelegate onHit,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            base.Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, onHit, missileDistance, missileSpeed, lockingTarget);

            // Initial configuration
            _initialPosition = CacheTransform.position;
            _impacted = false;

            // Configuration bullet and effects
            if (projectileObject)
                projectileObject.SetActive(true);

            if (impactEffect && !instantiateImpact)
            {
                impactEffect.SetActive(false);
                _defaultImpactEffectPosition = impactEffect.transform.localPosition;
            }

            if (disappearEffect && !instantiateDisappear)
                disappearEffect.SetActive(false);

            // Movement
            Vector3 targetPos = _initialPosition + (CacheTransform.forward * missileDistance);
            if (lockingTarget != null && lockingTarget.CurrentHp > 0)
                targetPos = lockingTarget.GetTransform().position;

            float dist = Vector3.Distance(_initialPosition, targetPos);
            float yOffset = -transform.forward.y;

            if (recalculateSpeed)
                missileSpeed = LaunchSpeed(dist, yOffset, Physics.gravity.magnitude, angle * Mathf.Deg2Rad);

            if (useAngle)
                CacheTransform.eulerAngles = new Vector3(CacheTransform.eulerAngles.x - angle, CacheTransform.eulerAngles.y, CacheTransform.eulerAngles.z);

            _bulletVelocity = CacheTransform.forward * missileSpeed;
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
            if (_isExploded || _impacted) 
                return;

            Vector3 point1 = CacheTransform.position;
            float stepSize = 1.0f / predictionStepPerFrame;
            // Find hitting objects by future positions
            for (float step = 0; step < 1; step += stepSize)
            {
                if (hasGravity)
                {
                    Vector3 gravity = Physics.gravity;
                    if (customGravity != Vector3.zero)
                        gravity = customGravity;
                    _bulletVelocity += gravity * stepSize * Time.deltaTime;
                }

                Vector3 point2 = point1 + _bulletVelocity * stepSize * Time.deltaTime;

                int hitCount = 0;
                RaycastHit hit;
                Vector3 origin = point1;
                Vector3 dir = (point2 - point1).normalized;
                float dist = Vector3.Distance(point2, point1);
                switch (hitDetectionMode)
                {
                    case HitDetectionMode.Raycast:
                        hitCount = Physics.RaycastNonAlloc(origin, dir, _hits3D, dist, hitLayers);
                        break;
                    case HitDetectionMode.SphereCast:
                        hitCount = Physics.SphereCastNonAlloc(origin, sphereCastRadius, dir, _hits3D, dist, hitLayers);
                        break;
                    case HitDetectionMode.BoxCast:
                        hitCount = Physics.BoxCastNonAlloc(origin, boxCastSize * 0.5f, dir, _hits3D, CacheTransform.rotation, dist, hitLayers);
                        break;
                }

                for (int i = 0; i < hitCount; ++i)
                {
                    hit = _hits3D[i];
                    if (!hit.transform.gameObject.GetComponent<IUnHittable>().IsNull())
                        continue;

                    if (useNormal)
                        _normal = hit.normal;
                    _hitPos = hit.point;

                    // Hit itself, no impact
                    if (_instigator.Id != null && _instigator.TryGetEntity(out BaseGameEntity instigatorEntity) && instigatorEntity.transform.root == hit.transform.root)
                        continue;

                    Impact(hit.collider.transform.gameObject);

                    // Already hit something
                    if (_destroying)
                        return;
                }

                // Moved too far from `initialPosition`
                if (Vector3.Distance(_initialPosition, point2) > _missileDistance)
                {
                    NoImpact();
                    return;
                }

                point1 = point2;
            }
            CacheTransform.rotation = Quaternion.LookRotation(_bulletVelocity);
            CacheTransform.position = point1;
        }

        protected void NoImpact()
        {
            if (_destroying)
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
                _destroying = true;
                return;
            }
            PushBack();
            _destroying = true;
        }

        protected void Impact(GameObject hitted)
        {
            // Check target
            if (FindTargetHitBox(hitted, out DamageableHitBox target))
            {
                // Hit a hitbox
                if (explodeDistance <= 0f && !_alreadyHitObjects.Contains(target.GetObjectId()))
                {
                    // If this is not going to explode, just apply damage to target
                    _alreadyHitObjects.Add(target.GetObjectId());
                    ApplyDamageTo(target);
                }
                OnHit(hitted);
                return;
            }

            // Hit damageable entity but it is not hitbox, skip it
            if (hitted.GetComponent<DamageableEntity>() != null)
                return;

            // Hit ground, wall, tree, etc.
            OnHit(hitted);
        }

        protected void OnHit(GameObject hitted)
        {
            // Spawn impact effect
            if (impactEffect && IsClient)
            {
                if (projectileObject)
                    projectileObject.SetActive(false);

                if (instantiateImpact)
                {
                    Quaternion hitRot = Quaternion.identity;
                    if (useNormal)
                        hitRot = Quaternion.FromToRotation(Vector3.forward, _normal);
                    GameObject newImpactEffect = Instantiate(impactEffect, _hitPos, hitRot);
                    if (stickToHitObject)
                        newImpactEffect.transform.parent = hitted.transform;
                    newImpactEffect.SetActive(true);
                }
                else
                {
                    if (useNormal)
                        impactEffect.transform.rotation = Quaternion.FromToRotation(Vector3.forward, _normal);
                    impactEffect.transform.position = _hitPos;
                    if (stickToHitObject)
                        impactEffect.transform.parent = hitted.transform;
                    impactEffect.SetActive(true);
                }
            }

            // Hit something
            if (explodeDistance > 0f)
            {
                // Explode immediately when hit something
                Explode();
            }

            _impacted = true;
            PushBack(destroyDelay);
            _destroying = true;
        }

        protected override void OnPushBack()
        {
            if (impactEffect && stickToHitObject && !instantiateImpact)
            {
                impactEffect.transform.parent = CacheTransform;
                impactEffect.transform.localPosition = _defaultImpactEffectPosition;
            }
            base.OnPushBack();
        }
    }
}
