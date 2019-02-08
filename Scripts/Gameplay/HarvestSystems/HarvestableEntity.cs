using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public sealed class HarvestableEntity : DamageableEntity
    {
        public int maxHp = 100;
        public Harvestable harvestable;
        public float colliderDetectionRadius = 2f;
        [HideInInspector]
        public float destroyHideDelay = 2f;
        public float destroyDelay = 2f;
        public float destroyRespawnDelay = 5f;
        public UnityEvent onHarvestableDestroy;

        public override string Title { get { return harvestable.Title; } set { } }
        public override int MaxHp { get { return maxHp; } }
        public HarvestableSpawnArea spawnArea { get; private set; }
        public Vector3 spawnPosition { get; private set; }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.harvestableTag;
            gameObject.layer = gameInstance.harvestableLayer;
            MigrateFields();
        }

        protected override void EntityStart()
        {
            base.EntityStart();
            InitStats();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (MigrateFields())
                EditorUtility.SetDirty(this);
#endif
        }

        private bool MigrateFields()
        {
            bool hasChanges = false;
            if (destroyHideDelay > 0)
            {
                destroyDelay = destroyHideDelay;
                destroyHideDelay = -1;
                hasChanges = true;
            }
            return hasChanges;
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
            RegisterNetFunction(NetFuncOnHarvestableDestroy);
            InitStats();
        }

        private void NetFuncOnHarvestableDestroy()
        {
            if (onHarvestableDestroy != null)
                onHarvestableDestroy.Invoke();
        }

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead() || weapon == null)
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            BaseCharacterEntity attackerCharacter = attacker as BaseCharacterEntity;
            // Play hit effect
            if (hitEffectsId == 0)
                hitEffectsId = gameInstance.DefaultHitEffects.Id;
            if (hitEffectsId > 0)
                RequestPlayEffect(hitEffectsId);
            // Apply damages
            int totalDamage = 0;
            Item weaponItem = weapon.GetWeaponItem();
            HarvestEffectiveness harvestEffectiveness;
            WeightedRandomizer<ItemDropByWeight> itemRandomizer;
            if (harvestable.CacheHarvestEffectivenesses.TryGetValue(weaponItem.weaponType, out harvestEffectiveness) &&
                harvestable.CacheHarvestItems.TryGetValue(weaponItem.weaponType, out itemRandomizer))
            {
                totalDamage = (int)(weaponItem.harvestDamageAmount.GetAmount(weapon.level).Random() * harvestEffectiveness.damageEffectiveness);
                ItemDropByWeight receivingItem = itemRandomizer.TakeOne();
                int dataId = receivingItem.item.DataId;
                short amount = (short)(receivingItem.amountPerDamage * totalDamage);
                if (!attackerCharacter.IncreasingItemsWillOverwhelming(dataId, amount))
                    attackerCharacter.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
            }
            CurrentHp -= totalDamage;
            ReceivedDamage(attackerCharacter, CombatAmountType.NormalDamage, totalDamage);

            if (IsDead())
            {
                CurrentHp = 0;
                CallNetFunction(NetFuncOnHarvestableDestroy, FunctionReceivers.All);
                DestroyAndRespawn();
            }
        }

        public void DestroyAndRespawn()
        {
            if (spawnArea != null)
                spawnArea.Spawn(destroyDelay + destroyRespawnDelay);
            else
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(destroyDelay);
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(destroyDelay + destroyRespawnDelay);
            InitStats();
            Manager.Assets.NetworkSpawn(Identity.HashAssetId, 
                spawnPosition,
                gameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity, 
                Identity.ObjectId, 
                Identity.ConnectionId);
        }

        public override void ReceivedDamage(IAttackerEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            if (attacker is BaseCharacterEntity)
                gameInstance.GameplayRule.OnHarvestableReceivedDamage(attacker as BaseCharacterEntity, this, combatAmountType, damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CacheTransform.position, colliderDetectionRadius);
        }
    }
}
