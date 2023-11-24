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
            HitRegisterData hitRegisterData,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            base.Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, hitRegisterData, missileDistance, missileSpeed, lockingTarget);

            // Initial configuration
            _initialPosition = CacheTransform.position;

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

        public override void HitDetect()
        {
            if (Destroying)
                return;

            if (!_previousPosition.HasValue)
                return;

            int hitCount = 0;
            Vector3 dir = (CacheTransform.position - _previousPosition.Value).normalized;
            float dist = Vector3.Distance(CacheTransform.position, _previousPosition.Value);
            // Raycast to previous position to check is it hitting something or not
            // If hit, explode
            switch (hitDetectionMode)
            {
                case HitDetectionMode.Raycast:
                    hitCount = Physics.RaycastNonAlloc(_previousPosition.Value, dir, _hits3D, dist, hitLayers);
                    break;
                case HitDetectionMode.SphereCast:
                    hitCount = Physics.SphereCastNonAlloc(_previousPosition.Value, sphereCastRadius, dir, _hits3D, dist, hitLayers);
                    break;
                case HitDetectionMode.BoxCast:
                    hitCount = Physics.BoxCastNonAlloc(_previousPosition.Value, boxCastSize * 0.5f, dir, _hits3D, CacheTransform.rotation, dist, hitLayers);
                    break;
            }

            RaycastHit hit;
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

                Impact(hit.collider.gameObject);

                // Already hit something
                if (Destroying)
                    break;
            }
        }

        protected override void Update()
        {
            if (Destroying)
                return;

            if (hasGravity)
            {
                Vector3 gravity = Physics.gravity;
                if (customGravity != Vector3.zero)
                    gravity = customGravity;
                _bulletVelocity += gravity * Time.deltaTime;
            }

            HitDetect();

            if (Destroying)
                return;

            CacheTransform.rotation = Quaternion.LookRotation(_bulletVelocity);
            CacheTransform.position += _bulletVelocity * Time.deltaTime;

            // Moved too far from `_initialPosition`
            if (Vector3.Distance(_initialPosition, CacheTransform.position) > _missileDistance && Time.unscaledTime - _launchTime >= _missileDuration)
                NoImpact();
        }

        protected void NoImpact()
        {
            if (Destroying)
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
                Destroying = true;
                return;
            }
            PushBack();
            Destroying = true;
        }

        protected void Impact(GameObject hitted)
        {
            // Check target
            if (FindTargetHitBox(hitted, true, out DamageableHitBox target))
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

            PushBack(destroyDelay);
            Destroying = true;
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
