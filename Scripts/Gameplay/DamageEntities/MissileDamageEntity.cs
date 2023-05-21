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
        public float sphereCastRadius = 1f;
        public Vector3 boxCastSize = Vector3.one;
        public float destroyDelay;
        public UnityEvent onExploded;
        public UnityEvent onDestroy;
        [Tooltip("If this value more than 0, when it hit anything or it is out of life, it will explode and apply damage to characters in this distance")]
        public float explodeDistance;

        public Rigidbody CacheRigidbody { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }

        protected bool _isExploded;
        protected float _missileDistance;
        protected float _missileSpeed;
        protected IDamageableEntity _lockingTarget;
        protected float _launchTime;
        protected float _missileDuration;
        protected bool _destroying;
        protected float? _lagMoveSpeedRate;
        protected Vector3? _previousPosition;
        protected RaycastHit2D[] _hits2D = new RaycastHit2D[8];
        protected RaycastHit[] _hits3D = new RaycastHit[8];
        protected readonly HashSet<uint> _alreadyHitObjects = new HashSet<uint>();

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
        /// <param name="simulateSeed">Launch random seed</param>
        /// <param name="triggerIndex"></param>
        /// <param name="spreadIndex"></param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="onHit">Action when hit</param>
        /// <param name="missileDistance">Calculated missile distance</param>
        /// <param name="missileSpeed">Calculated missile speed</param>
        /// <param name="lockingTarget">Locking target, if this is empty it can hit any entities</param>
        public virtual void Setup(
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
            Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, onHit);
            _missileDistance = missileDistance;
            _missileSpeed = missileSpeed;

            if (missileDistance <= 0 && missileSpeed <= 0)
            {
                // Explode immediately when distance and speed is 0
                Explode();
                PushBack(destroyDelay);
                _destroying = true;
                return;
            }
            _lockingTarget = lockingTarget;
            _isExploded = false;
            _destroying = false;
            _launchTime = Time.unscaledTime;
            _missileDuration = (missileDistance / missileSpeed) + 0.1f;
            _alreadyHitObjects.Clear();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _previousPosition = CacheTransform.position;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Color defaultColor = Gizmos.color;
            Gizmos.color = Color.green;
            switch (hitDetectionMode)
            {
                case HitDetectionMode.SphereCast:
                    Gizmos.DrawWireSphere(transform.position, sphereCastRadius);
                    break;
                case HitDetectionMode.BoxCast:
                    Gizmos.DrawWireCube(transform.position, boxCastSize);
                    break;
            }
            Gizmos.color = defaultColor;
        }
#endif

        protected virtual void Update()
        {
            if (_destroying)
                return;

            if (Time.unscaledTime - _launchTime >= _missileDuration)
            {
                Explode();
                PushBack(destroyDelay);
                _destroying = true;
            }

            HitDetect();
        }

        /// <summary>
        /// RayCast/SphereCast/BoxCast from current position back to previous frame position to detect hit target
        /// </summary>
        public virtual void HitDetect()
        {
            if (!_destroying)
            {
                if (_previousPosition.HasValue)
                {
                    int hitCount = 0;
                    int layerMask = GameInstance.Singleton.GetDamageEntityHitLayerMask();
                    Vector3 dir = (CacheTransform.position - _previousPosition.Value).normalized;
                    float dist = Vector3.Distance(CacheTransform.position, _previousPosition.Value);
                    // Raycast to previous position to check is it hitting something or not
                    // If hit, explode
                    switch (hitDetectionMode)
                    {
                        case HitDetectionMode.Raycast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.RaycastNonAlloc(_previousPosition.Value, dir, _hits2D, dist, layerMask);
                            else
                                hitCount = Physics.RaycastNonAlloc(_previousPosition.Value, dir, _hits3D, dist, layerMask);
                            break;
                        case HitDetectionMode.SphereCast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.CircleCastNonAlloc(_previousPosition.Value, sphereCastRadius, dir, _hits2D, dist, layerMask);
                            else
                                hitCount = Physics.SphereCastNonAlloc(_previousPosition.Value, sphereCastRadius, dir, _hits3D, dist, layerMask);
                            break;
                        case HitDetectionMode.BoxCast:
                            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
                                hitCount = Physics2D.BoxCastNonAlloc(_previousPosition.Value, new Vector2(boxCastSize.x, boxCastSize.y), Vector2.SignedAngle(Vector2.zero, dir), dir, _hits2D, dist, layerMask);
                            else
                                hitCount = Physics.BoxCastNonAlloc(_previousPosition.Value, boxCastSize * 0.5f, dir, _hits3D, CacheTransform.rotation, dist, layerMask);
                            break;
                    }
                    for (int i = 0; i < hitCount; ++i)
                    {
                        if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D && _hits2D[i].transform != null)
                            TriggerEnter(_hits2D[i].transform.gameObject);
                        if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D && _hits3D[i].transform != null)
                            TriggerEnter(_hits3D[i].transform.gameObject);
                    }
                }
                _previousPosition = CacheTransform.position;
            }
        }

        protected virtual void FixedUpdate()
        {
            // Don't move if exploded
            if (_isExploded)
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

            float currentMissileSpeed = CalculateCurrentMoveSpeed(_missileSpeed, Time.fixedDeltaTime);
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                if (CacheRigidbody2D != null)
                    CacheRigidbody2D.velocity = -CacheTransform.up * currentMissileSpeed;
            }
            else
            {
                if (CacheRigidbody != null)
                    CacheRigidbody.velocity = CacheTransform.forward * currentMissileSpeed;
            }
        }

        protected float CalculateCurrentMoveSpeed(float maxMoveSpeed, float deltaTime)
        {
            // Adjust speed by rtt
            if (!IsServer && _instigator.TryGetEntity(out BaseGameEntity entity) && entity.IsOwnerClient)
            {
                float rtt = 0.001f * CurrentGameManager.Rtt;
                float acc = 1f / rtt * deltaTime * 0.5f;
                if (!_lagMoveSpeedRate.HasValue)
                    _lagMoveSpeedRate = 0f;
                if (_lagMoveSpeedRate < 1f)
                    _lagMoveSpeedRate += acc;
                if (_lagMoveSpeedRate > 1f)
                    _lagMoveSpeedRate = 1f;
                return maxMoveSpeed * _lagMoveSpeedRate.Value;
            }
            // TODO: Adjust other's client move speed by rtt
            return maxMoveSpeed;
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
            _previousPosition = null;
            if (onDestroy != null)
                onDestroy.Invoke();
            base.OnPushBack();
        }

        protected virtual void TriggerEnter(GameObject other)
        {
            if (_destroying)
                return;

            if (!other.GetComponent<IUnHittable>().IsNull())
                return;

            if (FindTargetHitBox(other, out DamageableHitBox target))
            {
                // Hit a hitbox
                if (explodeDistance <= 0f && !_alreadyHitObjects.Contains(target.GetObjectId()))
                {
                    // If this is not going to explode, just apply damage to target
                    _alreadyHitObjects.Add(target.GetObjectId());
                    ApplyDamageTo(target);
                }
                else
                {
                    // Explode immediately when hit something
                    Explode();
                }
                PushBack(destroyDelay);
                _destroying = true;
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
                _destroying = true;
                return;
            }
        }

        protected virtual bool FindTargetHitBox(GameObject other, out DamageableHitBox target)
        {
            target = null;

            if (!other.GetComponent<IUnHittable>().IsNull())
                return false;

            target = other.GetComponent<DamageableHitBox>();

            if (target == null || target.IsDead() || target.GetObjectId() == _instigator.ObjectId || !target.CanReceiveDamageFrom(_instigator))
            {
                target = null;
                return false;
            }

            if (_lockingTarget != null && _lockingTarget.GetObjectId() != target.GetObjectId())
            {
                target = null;
                return false;
            }

            return true;
        }

        protected virtual bool FindAndApplyDamage(GameObject other, HashSet<uint> alreadyHitObjects)
        {
            if (FindTargetHitBox(other, out DamageableHitBox target) && !_alreadyHitObjects.Contains(target.GetObjectId()))
            {
                _alreadyHitObjects.Add(target.GetObjectId());
                ApplyDamageTo(target);
                return true;
            }
            return false;
        }

        protected virtual void Explode()
        {
            if (_isExploded)
                return;

            _isExploded = true;

            // Explode when distance > 0
            if (explodeDistance <= 0f)
                return;

            if (onExploded != null)
                onExploded.Invoke();

            ExplodeApplyDamage();
        }

        protected virtual void ExplodeApplyDamage()
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(CacheTransform.position, explodeDistance);
                foreach (Collider2D collider in colliders2D)
                {
                    FindAndApplyDamage(collider.gameObject, _alreadyHitObjects);
                }
            }
            else
            {
                Collider[] colliders = Physics.OverlapSphere(CacheTransform.position, explodeDistance);
                foreach (Collider collider in colliders)
                {
                    FindAndApplyDamage(collider.gameObject, _alreadyHitObjects);
                }
            }
        }
    }
}
