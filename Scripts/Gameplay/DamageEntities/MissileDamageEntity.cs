using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class MissileDamageEntity : BaseDamageEntity
    {
        public UnityEvent onExploded;
        public UnityEvent onDestroy;
        [Tooltip("If this value more than 0, when it hit anything or it is out of life, it will explode and apply damage to characters in this distance")]
        public float explodeDistance;

        protected float missileDistance;
        [SerializeField]
        protected SyncFieldFloat missileSpeed = new SyncFieldFloat();
        [SerializeField]
        protected SyncFieldBool isExploded = new SyncFieldBool();
        [SerializeField]
        protected SyncFieldPackedUInt lockingTargetId;

        protected IDamageableEntity lockingTarget;
        public IDamageableEntity LockingTarget
        {
            get
            {
                if (lockingTarget == null && lockingTargetId.Value > 0)
                    TryGetEntityByObjectId(lockingTargetId.Value, out lockingTarget);
                return lockingTarget;
            }
            set
            {
                if (!IsServer)
                    return;
                lockingTargetId.Value = value != null ? value.ObjectId : 0;
                lockingTarget = value;
            }
        }

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

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            missileSpeed.deliveryMethod = DeliveryMethod.ReliableSequenced;
            missileSpeed.forOwnerOnly = false;
            isExploded.deliveryMethod = DeliveryMethod.ReliableSequenced;
            isExploded.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            SetupNetElements();
        }

        private void OnIsExplodedChanged(bool isExploded)
        {
            if (isExploded)
            {
                if (onExploded != null)
                    onExploded.Invoke();
            }
        }

        public void SetupDamage(
            IAttackerEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId,
            float missileDistance,
            float missileSpeed,
            IDamageableEntity lockingTarget)
        {
            SetupDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            this.missileDistance = missileDistance;
            this.missileSpeed.Value = missileSpeed;

            if (missileDistance <= 0 && missileSpeed <= 0)
            {
                // Explode immediately when distance and speed is 0
                Explode();
                NetworkDestroy(destroyDelay);
                return;
            }

            LockingTarget = lockingTarget;
            launchTime = Time.unscaledTime;
            missileDuration = missileDistance / missileSpeed;
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (Time.unscaledTime - launchTime > missileDuration)
            {
                Explode();
                NetworkDestroy(destroyDelay);
            }
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            // Don't move if exploded
            if (isExploded.Value)
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

            // Turn to locking target position
            if (LockingTarget != null)
            {
                // Lookat target then do anything when it's in range
                Vector3 lookAtDirection = (LockingTarget.transform.position - CacheTransform.position).normalized;
                // slerp to the desired rotation over time
                if (lookAtDirection.magnitude > 0)
                {
                    Vector3 lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                    lookRotationEuler.x = 0;
                    lookRotationEuler.z = 0;
                    CacheTransform.rotation = Quaternion.Euler(lookRotationEuler);
                }
            }

            if (gameInstance.DimensionType == DimensionType.Dimension2D)
            {
                if (CacheRigidbody2D != null)
                    CacheRigidbody2D.velocity = -CacheTransform.up * missileSpeed.Value;
            }
            else
            {
                if (CacheRigidbody != null)
                    CacheRigidbody.velocity = CacheTransform.forward * missileSpeed.Value;
            }
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
            if (IsHitGroundOrWall(other) || FindAndApplyDamage(other))
            {
                // Explode immediately when hit something
                Explode();
                NetworkDestroy(destroyDelay);
            }
        }

        private bool IsHitGroundOrWall(GameObject other)
        {
            foreach (int layer in gameInstance.groundOrWallLayers)
            {
                if (other.layer == layer)
                    return true;
            }
            return false;
        }

        private bool FindAndApplyDamage(GameObject other)
        {
            if (!IsServer)
                return false;

            IDamageableEntity target = other.GetComponent<IDamageableEntity>();
            
            if (target == null || attacker == null || target.IsDead() || attacker.gameObject == target.gameObject || !target.CanReceiveDamageFrom(attacker))
                return false;

            if (LockingTarget != null && LockingTarget != target)
                return false;

            ApplyDamageTo(target);
            return true;
        }

        private void Explode()
        {
            if (isExploded.Value || !IsServer)
                return;
            isExploded.Value = true;
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

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (reasons == LiteNetLibGameManager.DestroyObjectReasons.RequestedToDestroy)
            {
                if (onDestroy != null)
                    onDestroy.Invoke();
            }
        }
    }
}
