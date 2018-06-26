using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Rigidbody))]
    public class MissileDamageEntity : BaseDamageEntity
    {
        protected float missileDistance;
        [SerializeField]
        protected SyncFieldFloat missileSpeed = new SyncFieldFloat();

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

        public void SetupDamage(
            BaseCharacterEntity attacker,
            CharacterItem weapon,
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
            CharacterBuff debuff,
            int hitEffectsId,
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

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            CacheRigidbody.velocity = CacheTransform.forward * missileSpeed.Value;
        }

        private void OnTriggerEnter(Collider other)
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
            if (damageableEntity == null || damageableEntity == attacker || damageableEntity.CurrentHp <= 0)
                return;

            if (attacker is MonsterCharacterEntity && damageableEntity is MonsterCharacterEntity)
                return;

            ApplyDamageTo(damageableEntity);
            NetworkDestroy();
        }
    }
}
