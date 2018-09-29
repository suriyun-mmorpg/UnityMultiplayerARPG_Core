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
        
        public HarvestableSpawnArea spawnArea { get; private set; }
        public Vector3 spawnPosition { get; private set; }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.harvestableTag;
            gameObject.layer = gameInstance.harvestableLayer;
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            InitStats();
        }

        private void InitStats()
        {
            if (IsServer)
            {
                if (spawnArea == null)
                    spawnPosition = CacheTransform.position;

                CurrentHp = maxHp;
            }
        }

        public void SetSpawnArea(HarvestableSpawnArea spawnArea, Vector3 spawnPosition)
        {
            this.spawnArea = spawnArea;
            this.spawnPosition = spawnPosition;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction("OnHarvestableDestroy", new LiteNetLibFunction(() => { onHarvestableDestroy.Invoke(); }));
            InitStats();
        }

        public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || weapon == null)
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            // Play hit effect
            if (hitEffectsId == 0)
                hitEffectsId = gameInstance.DefaultHitEffects.Id;
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
                DestroyAndRespawn();
            }
        }

        public void DestroyAndRespawn()
        {
            if (spawnArea != null)
                spawnArea.Spawn(destroyHideDelay + destroyRespawnDelay);
            else
                Manager.StartCoroutine(RespawnRoutine(destroyHideDelay + destroyRespawnDelay, Manager, Identity.HashAssetId, spawnPosition));

            NetworkDestroy(destroyHideDelay);
        }

        private static IEnumerator RespawnRoutine(float delay, LiteNetLibGameManager manager, int hashAssetId, Vector3 spawnPosition)
        {
            yield return new WaitForSecondsRealtime(delay);
            var identity = manager.Assets.NetworkSpawn(hashAssetId, spawnPosition, Quaternion.Euler(Vector3.up * Random.Range(0, 360)));
            if (identity != null)
                identity.GetComponent<BaseMonsterCharacterEntity>();
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
