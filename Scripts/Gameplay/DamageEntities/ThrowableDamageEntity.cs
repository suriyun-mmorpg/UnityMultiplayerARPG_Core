using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class ThrowableDamageEntity : BaseDamageEntity
    {
        public bool canApplyDamageToUser;
        public bool canApplyDamageToAllies;
        public float destroyDelay;
        public UnityEvent onExploded;
        public UnityEvent onDestroy;
        public float explodeDistance;

        public Rigidbody CacheRigidbody { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }

        protected float _throwForce;
        protected float _lifetime;
        protected bool _isExploded;
        protected float _throwedTime;
        protected bool _destroying;
        protected IgnoreColliderManager _ignoreColliderManager;

        protected override void Awake()
        {
            base.Awake();
            CacheRigidbody = GetComponent<Rigidbody>();
            CacheRigidbody2D = GetComponent<Rigidbody2D>();
            _ignoreColliderManager = new IgnoreColliderManager(GetComponentsInChildren<Collider>(), GetComponentsInChildren<Collider2D>());
        }

        /// <summary>
        /// Setup this component data
        /// </summary>
        /// <param name="instigator">Weapon's or skill's instigator who to spawn this to attack enemy</param>
        /// <param name="weapon">Weapon which was used to attack enemy</param>
        /// <param name="damageAmounts">Calculated damage amounts</param>
        /// <param name="skill">Skill which was used to attack enemy</param>
        /// <param name="skillLevel">Level of the skill</param>
        /// <param name="throwForce">Calculated throw force</param>
        /// <param name="lifetime">Calculated life time</param>
        public virtual void Setup(
            EntityInfo instigator,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            float throwForce,
            float lifetime)
        {
            Setup(instigator, weapon, damageAmounts, skill, skillLevel);
            _throwForce = throwForce;
            _lifetime = lifetime;

            if (lifetime <= 0)
            {
                // Explode immediately when lifetime is 0
                Explode();
                PushBack(destroyDelay);
                _destroying = true;
                return;
            }
            _isExploded = false;
            _destroying = false;
            _throwedTime = Time.unscaledTime;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                _ignoreColliderManager.ResetAndSetIgnoreColliders(instigator);
                CacheRigidbody2D.velocity = Vector2.zero;
                CacheRigidbody2D.angularVelocity = 0f;
                CacheRigidbody2D.AddForce(CacheTransform.forward * throwForce, ForceMode2D.Impulse);
            }
            else
            {
                _ignoreColliderManager.ResetAndSetIgnoreCollider2Ds(instigator);
                CacheRigidbody.velocity = Vector3.zero;
                CacheRigidbody.angularVelocity = Vector3.zero;
                CacheRigidbody.AddForce(CacheTransform.forward * throwForce, ForceMode.Impulse);
            }
        }

        protected virtual void Update()
        {
            if (_destroying)
                return;

            if (Time.unscaledTime - _throwedTime >= _lifetime)
            {
                Explode();
                PushBack(destroyDelay);
                _destroying = true;
            }
        }

        protected override void OnPushBack()
        {
            if (onDestroy != null)
                onDestroy.Invoke();
            base.OnPushBack();
        }

        protected virtual bool FindTargetHitBox(GameObject other, out DamageableHitBox target)
        {
            target = null;

            if (!other.GetComponent<IUnHittable>().IsNull())
                return false;

            target = other.GetComponent<DamageableHitBox>();

            if (target == null || target.IsDead() || target.IsImmune || target.IsInSafeArea)
            {
                target = null;
                return false;
            }

            if (!canApplyDamageToUser && target.GetObjectId() == instigator.ObjectId)
            {
                target = null;
                return false;
            }

            if (!canApplyDamageToAllies && target.DamageableEntity is BaseCharacterEntity characterEntity && characterEntity.IsAlly(instigator))
            {
                target = null;
                return false;
            }

            return true;
        }

        protected virtual bool FindAndApplyDamage(GameObject other, HashSet<uint> alreadyHitObjects)
        {
            DamageableHitBox target;
            if (FindTargetHitBox(other, out target) && !alreadyHitObjects.Contains(target.GetObjectId()))
            {
                target.ReceiveDamageWithoutConditionCheck(CacheTransform.position, instigator, damageAmounts, weapon, skill, skillLevel, Random.Range(0, 255));
                alreadyHitObjects.Add(target.GetObjectId());
                return true;
            }
            return false;
        }

        protected virtual void Explode()
        {
            if (_isExploded)
                return;

            _isExploded = true;

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
                HashSet<uint> alreadyHitObjects = new HashSet<uint>();
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(CacheTransform.position, explodeDistance);
                foreach (Collider2D collider in colliders2D)
                {
                    FindAndApplyDamage(collider.gameObject, alreadyHitObjects);
                }
            }
            else
            {
                HashSet<uint> alreadyHitObjects = new HashSet<uint>();
                Collider[] colliders = Physics.OverlapSphere(CacheTransform.position, explodeDistance);
                foreach (Collider collider in colliders)
                {
                    FindAndApplyDamage(collider.gameObject, alreadyHitObjects);
                }
            }
        }
    }
}
