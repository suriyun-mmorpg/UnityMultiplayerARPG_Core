using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public class MissileDamageEntity : BaseDamageEntity
    {
        [SerializeField]
        protected DimensionType dimensionType;
        [SerializeField]
        protected SyncFieldFloat missileSpeed = new SyncFieldFloat();
        protected float missileDistance;

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
            float missileSpeed)
        {
            SetupDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            this.missileDistance = missileDistance;
            this.missileSpeed.Value = missileSpeed;

            if (missileDistance > 0 && missileSpeed > 0)
                NetworkDestroy(missileDistance / missileSpeed);
            else
                NetworkDestroy();
        }

        protected override void EntityFixedUpdate()
        {
            base.EntityFixedUpdate();
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
    }
}
