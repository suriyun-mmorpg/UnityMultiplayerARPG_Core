using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public sealed class HarvestableEntity : DamageableNetworkEntity
    {
        public int maxHp = 100;
        public Harvestable harvestable;
        public float colliderDetectionRadius = 2f;
        public float destroyHideDelay = 2f;
        public float destroyRespawnDelay = 5f;
        public UnityEvent onHarvestableDestroy;

        #region Public data
        [HideInInspector]
        public HarvestableSpawnArea spawnArea;
        [HideInInspector]
        public Vector3 spawnPosition;
        #endregion

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.harvestableTag;
            gameObject.layer = gameInstance.harvestableLayer;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction("OnHarvestableDestroy", new LiteNetLibFunction(() => { onHarvestableDestroy.Invoke(); }));
        }

        public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || weapon == null)
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            // Play hit effect
            if (hitEffectsId == 0)
                hitEffectsId = gameInstance.defaultHitEffects.Id;
            if (hitEffectsId > 0)
                RequestPlayEffect(hitEffectsId);
            // Apply damages
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
            if (IsDead())
            {
                CurrentHp = 0;
                CallNetFunction("OnHarvestableDestroy", FunctionReceivers.All);
                if (spawnArea != null)
                    spawnArea.Spawn(destroyHideDelay + destroyRespawnDelay);
                NetworkDestroy(destroyHideDelay);
            }
        }

        public override void ReceivedDamage(BaseCharacterEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            gameInstance.GameplayRule.OnHarvestableReceivedDamage(attacker, this, combatAmountType, damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CacheTransform.position, colliderDetectionRadius);
        }
    }
}
