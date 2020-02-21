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
        public HarvestableCollectType collectType;
        [Tooltip("Radius to detect other entities to avoid spawn this harvestable nearby other entities")]
        public float colliderDetectionRadius = 2f;
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onHarvestableDestroy` event before it's going to be destroyed from the game.")]
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
            gameObject.tag = CurrentGameInstance.harvestableTag;
            gameObject.layer = CurrentGameInstance.harvestableLayer;
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
            RegisterNetFunction(NetFuncOnHarvestableDestroy);
            InitStats();
        }

        private void NetFuncOnHarvestableDestroy()
        {
            if (onHarvestableDestroy != null)
                onHarvestableDestroy.Invoke();
        }

        public override void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead() || weapon == null)
                return;

            base.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);

            BaseCharacterEntity attackerCharacter = null;
            if (attacker != null && attacker.Entity is BaseCharacterEntity)
                attackerCharacter = attacker.Entity as BaseCharacterEntity;

            // Apply damages
            int totalDamage = 0;
            IWeaponItem weaponItem = weapon.GetWeaponItem();
            HarvestEffectiveness harvestEffectiveness;
            WeightedRandomizer<ItemDropByWeight> itemRandomizer;
            if (harvestable.CacheHarvestEffectivenesses.TryGetValue(weaponItem.WeaponType, out harvestEffectiveness) &&
                harvestable.CacheHarvestItems.TryGetValue(weaponItem.WeaponType, out itemRandomizer))
            {
                totalDamage = (int)(weaponItem.HarvestDamageAmount.GetAmount(weapon.level).Random() * harvestEffectiveness.damageEffectiveness);
                ItemDropByWeight receivingItem = itemRandomizer.TakeOne();
                int dataId = receivingItem.item.DataId;
                short amount = (short)(receivingItem.amountPerDamage * totalDamage);
                bool droppingToGround = collectType == HarvestableCollectType.DropToGround;

                if (attackerCharacter != null)
                {
                    if (attackerCharacter.IncreasingItemsWillOverwhelming(dataId, amount))
                        droppingToGround = true;
                    if (!droppingToGround)
                    {
                        attackerCharacter.IncreaseItems(CharacterItem.Create(dataId, 1, amount));
                        attackerCharacter.FillEmptySlots();
                    }
                    attackerCharacter.RewardExp(new Reward() { exp = totalDamage * harvestable.expPerDamage }, 1, RewardGivenType.Harvestable);
                }
                else
                {
                    // Attacker is not character, always drop item to ground
                    droppingToGround = true;
                }

                if (droppingToGround)
                    ItemDropEntity.DropItem(this, CharacterItem.Create(dataId, 1, amount), new uint[0]);
            }

            CurrentHp -= totalDamage;
            ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage);

            if (IsDead())
            {
                CurrentHp = 0;
                CallNetFunction(NetFuncOnHarvestableDestroy, FunctionReceivers.All);
                DestroyAndRespawn();
            }
        }

        public void DestroyAndRespawn()
        {
            if (!IsServer)
                return;

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
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                spawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public override void ReceivedDamage(IGameEntity attacker, CombatAmountType combatAmountType, int damage)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage);
            if (attacker != null && attacker.Entity is BaseCharacterEntity)
                CurrentGameInstance.GameplayRule.OnHarvestableReceivedDamage(attacker.Entity as BaseCharacterEntity, this, combatAmountType, damage);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, colliderDetectionRadius);
        }
    }
}
