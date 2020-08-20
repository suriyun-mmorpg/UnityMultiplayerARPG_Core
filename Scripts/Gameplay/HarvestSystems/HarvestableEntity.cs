using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif

namespace MultiplayerARPG
{
    public partial class HarvestableEntity : DamageableEntity
    {
        [SerializeField]
        protected int maxHp = 100;
        [SerializeField]
        protected Harvestable harvestable;
        [SerializeField]
        protected HarvestableCollectType collectType;
        [SerializeField]
        [Tooltip("Radius to detect other entities to avoid spawn this harvestable nearby other entities")]
        protected float colliderDetectionRadius = 2f;
        [SerializeField]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onHarvestableDestroy` event before it's going to be destroyed from the game.")]
        protected float destroyDelay = 2f;
        [SerializeField]
        protected float destroyRespawnDelay = 5f;
        [SerializeField]
        protected UnityEvent onHarvestableDestroy;

        public override string Title { get { return harvestable.Title; } set { } }
        public override int MaxHp { get { return maxHp; } }
        public float ColliderDetectionRadius { get { return colliderDetectionRadius; } }
        public HarvestableSpawnArea SpawnArea { get; protected set; }
        public Vector3 SpawnPosition { get; protected set; }

        // Private variables
        private bool isDestroyed;

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddHarvestables(new Harvestable[] { harvestable });
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.harvestableTag;
            gameObject.layer = CurrentGameInstance.harvestableLayer;
            isDestroyed = false;
        }

        protected virtual void InitStats()
        {
            if (!IsServer)
                return;

            CurrentHp = maxHp;
        }

        public virtual void SetSpawnArea(HarvestableSpawnArea spawnArea, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPosition = spawnPosition;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction(NetFuncOnHarvestableDestroy);
            // Initial default data
            InitStats();
            if (SpawnArea == null)
                SpawnPosition = CacheTransform.position;
        }

        protected virtual void NetFuncOnHarvestableDestroy()
        {
            if (onHarvestableDestroy != null)
                onHarvestableDestroy.Invoke();
        }

        public void RequestOnHarvestableDestroy()
        {
            CallNetFunction(NetFuncOnHarvestableDestroy, FunctionReceivers.All);
        }

        public override void ReceiveDamage(IGameEntity attacker, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead() || weapon == null)
                return;

            base.ReceiveDamage(attacker, damageAmounts, weapon, skill, skillLevel);

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
                        CurrentGameManager.SendNotifyRewardItem(attackerCharacter.ConnectionId, dataId, amount);
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
            ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage, weapon, skill, skillLevel);

            if (IsDead())
                DestroyAndRespawn();
        }

        public void DestroyAndRespawn()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (isDestroyed)
                return;
            isDestroyed = true;
            // Tell clients that the harvestable destroy to play animation at client
            RequestOnHarvestableDestroy();
            if (SpawnArea != null)
                SpawnArea.Spawn(destroyDelay + destroyRespawnDelay);
            else if (Identity.IsSceneObject)
                Manager.StartCoroutine(RespawnRoutine());

            NetworkDestroy(destroyDelay);
        }

        protected IEnumerator RespawnRoutine()
        {
            yield return new WaitForSecondsRealtime(destroyDelay + destroyRespawnDelay);
            isDestroyed = false;
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public override bool CanReceiveDamageFrom(IGameEntity attacker)
        {
            // Harvestable entity can receive damage inside safe area
            return true;
        }

        public override void ReceivedDamage(IGameEntity attacker, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            base.ReceivedDamage(attacker, combatAmountType, damage, weapon, skill, skillLevel);
            if (attacker != null && attacker.Entity is BaseCharacterEntity)
                CurrentGameInstance.GameplayRule.OnHarvestableReceivedDamage(attacker.Entity as BaseCharacterEntity, this, combatAmountType, damage, weapon, skill, skillLevel);
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmos();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, colliderDetectionRadius);
        }
#endif
    }
}
