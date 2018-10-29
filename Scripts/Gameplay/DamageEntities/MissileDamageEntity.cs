using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class MissileDamageEntity : BaseDamageEntity
    {
        public DimensionType dimensionType;
        public UnityEvent onDestroy;
        protected float missileDistance;
        [SerializeField]
        protected SyncFieldFloat missileSpeed = new SyncFieldFloat();
        [SerializeField]
        protected SyncFieldPackedUInt lockingTargetId;

        protected DamageableNetworkEntity lockingTarget;
        public DamageableNetworkEntity LockingTarget
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

        public void SetupDamage(
            BaseCharacterEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            uint hitEffectsId,
            float missileDistance,
            float missileSpeed,
            DamageableNetworkEntity lockingTarget)
        {
            SetupDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            this.missileDistance = missileDistance;
            this.missileSpeed.Value = missileSpeed;

            if (missileDistance <= 0 && missileSpeed <= 0)
            {
                NetworkDestroy();
                return;
            }

            LockingTarget = lockingTarget;
            NetworkDestroy(missileDistance / missileSpeed);
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
            // Turn to locking target position
            if (LockingTarget != null)
            {
                // Lookat target then do anything when it's in range
                var lookAtDirection = (LockingTarget.CacheTransform.position - CacheTransform.position).normalized;
                // slerp to the desired rotation over time
                if (lookAtDirection.magnitude > 0)
                {
                    var lookRotationEuler = Quaternion.LookRotation(lookAtDirection).eulerAngles;
                    lookRotationEuler.x = 0;
                    lookRotationEuler.z = 0;
                    CacheTransform.rotation = Quaternion.Euler(lookRotationEuler);
                }
            }

            switch (dimensionType)
            {
                case DimensionType.Dimension3D:
                    if (CacheRigidbody != null)
                        CacheRigidbody.velocity = CacheTransform.forward * missileSpeed.Value;
                    break;
                case DimensionType.Dimension2D:
                    if (CacheRigidbody2D != null)
                        CacheRigidbody2D.velocity = -CacheTransform.up * missileSpeed.Value;
                    break;
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
            if (!IsServer)
                return;

            var damageableEntity = other.GetComponent<DamageableNetworkEntity>();
            // Try to find damageable entity by building object materials
            if (damageableEntity == null)
            {
                var buildingMaterial = other.GetComponent<BuildingMaterial>();
                if (buildingMaterial != null && buildingMaterial.buildingEntity != null)
                    damageableEntity = buildingMaterial.buildingEntity;
            }
            if (damageableEntity == null || damageableEntity == attacker || damageableEntity.IsDead())
                return;

            if (attacker is BaseMonsterCharacterEntity && damageableEntity is BaseMonsterCharacterEntity)
                return;

            ApplyDamageTo(damageableEntity);
            NetworkDestroy();
        }

        public override void OnNetworkDestroy(DestroyObjectReasons reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (reasons == DestroyObjectReasons.RequestedToDestroy && onDestroy != null)
                onDestroy.Invoke();
        }
    }
}
