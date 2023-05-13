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
        /// <param name="throwForce">Calculated throw force</param>
        /// <param name="lifetime">Calculated life time</param>
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
            float throwForce,
            float lifetime)
        {
            Setup(instigator, weapon, simulateSeed, triggerIndex, spreadIndex, damageAmounts, skill, skillLevel, onHit);
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
                CacheRigidbody2D.velocity = Vector2.zero;
                CacheRigidbody2D.angularVelocity = 0f;
                CacheRigidbody2D.AddForce(CacheTransform.forward * _throwForce, ForceMode2D.Impulse);
            }
            else
            {
                CacheRigidbody.velocity = Vector3.zero;
                CacheRigidbody.angularVelocity = Vector3.zero;
                CacheRigidbody.AddForce(CacheTransform.forward * _throwForce, ForceMode.Impulse);
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

            if (!canApplyDamageToUser && target.GetObjectId() == _instigator.ObjectId)
            {
                target = null;
                return false;
            }

            if (!canApplyDamageToAllies && target.DamageableEntity is BaseCharacterEntity characterEntity && characterEntity.IsAlly(_instigator))
            {
                target = null;
                return false;
            }

            return true;
        }

        protected virtual bool FindAndApplyDamage(GameObject other, HashSet<uint> alreadyHitObjects)
        {
            if (FindTargetHitBox(other, out DamageableHitBox target) && !alreadyHitObjects.Contains(target.GetObjectId()))
            {
                target.ReceiveDamageWithoutConditionCheck(CacheTransform.position, _instigator, _damageAmounts, _weapon, _skill, _skillLevel, Random.Range(0, 255));
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
                _alreadyHitObjects.Clear();
                Collider2D[] colliders2D = Physics2D.OverlapCircleAll(CacheTransform.position, explodeDistance);
                foreach (Collider2D collider in colliders2D)
                {
                    FindAndApplyDamage(collider.gameObject, _alreadyHitObjects);
                }
            }
            else
            {
                _alreadyHitObjects.Clear();
                Collider[] colliders = Physics.OverlapSphere(CacheTransform.position, explodeDistance);
                foreach (Collider collider in colliders)
                {
                    FindAndApplyDamage(collider.gameObject, _alreadyHitObjects);
                }
            }
        }
    }
}
