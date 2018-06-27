using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{

    public sealed class HarvestableEntity : DamageableNetworkEntity
    {
        public Harvestable harvestable;
        public int maxHp = 100;
        public float colliderDetectionRadius = 2f;
        public float respawnDelay = 5f;

        #region Public data
        [HideInInspector]
        public HarvestableSpawnArea spawnArea;
        [HideInInspector]
        public Vector3 spawnPosition;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.harvestableTag;
            gameObject.layer = gameInstance.harvestableLayer;
        }

        public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
        {
            if (!IsServer || CurrentHp <= 0 || weapon == null)
                return;

            var totalDamage = 0;
            var weaponItem = weapon.GetWeaponItem();
            HarvestEffectiveness harvestEffectiveness;
            WeightedRandomizer<ItemDropByWeight> itemRandomizer;
            if (harvestable.CacheHarvestEffectivenesses.TryGetValue(weaponItem.weaponType, out harvestEffectiveness) &&
                harvestable.CacheHarvestItems.TryGetValue(weaponItem.weaponType, out itemRandomizer))
            {
                totalDamage = (int)(weaponItem.harvestDamageAmount.GetAmount(weapon.level).Random() * harvestEffectiveness.damageEffectiveness);
                var receivingItem = itemRandomizer.TakeOne();
                var dataId = receivingItem.item.DataId;
                var amount = (short)(receivingItem.amountPerDamage * totalDamage);
                if (!attacker.IncreasingItemsWillOverwhelming(dataId, amount))
                    attacker.IncreaseItems(dataId, 1, amount);
            }
            CurrentHp -= totalDamage;
            ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage);
            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
                if (spawnArea != null)
                    spawnArea.Spawn(respawnDelay);
                NetworkDestroy();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CacheTransform.position, colliderDetectionRadius);
        }
    }
}
