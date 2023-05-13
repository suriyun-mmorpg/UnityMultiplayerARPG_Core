using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif

namespace MultiplayerARPG
{
    public partial class HarvestableEntity : DamageableEntity
    {
        [Category(5, "Harvestable Settings")]
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

        [Category("Events")]
        [SerializeField]
        protected UnityEvent onHarvestableDestroy = new UnityEvent();

        public override string EntityTitle
        {
            get
            {
                string title = base.EntityTitle;
                return !string.IsNullOrEmpty(title) ? title : harvestable.Title;
            }
        }
        public override int MaxHp { get { return maxHp; } }
        public float ColliderDetectionRadius { get { return colliderDetectionRadius; } }
        public GameSpawnArea<HarvestableEntity> SpawnArea { get; protected set; }
        public HarvestableEntity SpawnPrefab { get; protected set; }
        public int SpawnLevel { get; protected set; }
        public Vector3 SpawnPosition { get; protected set; }
        public float DestroyDelay { get { return destroyDelay; } }
        public float DestroyRespawnDelay { get { return destroyRespawnDelay; } }

        protected bool _isDestroyed;

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddHarvestables(harvestable);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.harvestableTag;
            gameObject.layer = CurrentGameInstance.harvestableLayer;
            isStaticHitBoxes = true;
            _isDestroyed = false;
        }

        protected virtual void InitStats()
        {
            if (!IsServer)
                return;
            _isDestroyed = false;
            CurrentHp = MaxHp;
        }

        public virtual void SetSpawnArea(GameSpawnArea<HarvestableEntity> spawnArea, HarvestableEntity spawnPrefab, int spawnLevel, Vector3 spawnPosition)
        {
            SpawnArea = spawnArea;
            SpawnPrefab = spawnPrefab;
            SpawnLevel = spawnLevel;
            SpawnPosition = spawnPosition;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            // Initial default data
            InitStats();
            if (SpawnArea == null)
                SpawnPosition = EntityTransform.position;
        }

        [AllRpc]
        protected virtual void AllOnHarvestableDestroy()
        {
            if (onHarvestableDestroy != null)
                onHarvestableDestroy.Invoke();
        }

        public void CallAllOnHarvestableDestroy()
        {
            RPC(AllOnHarvestableDestroy);
        }

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter);
            // Apply damages, won't apply skill damage
            float calculatingTotalDamage = 0f;
            // Harvest type is based on weapon by default
            HarvestType skillHarvestType = HarvestType.BasedOnWeapon;
            if (skill != null && skillLevel > 0)
            {
                skillHarvestType = skill.HarvestType;
            }
            // Get randomizer and random damage
            WeightedRandomizer<ItemDropForHarvestable> itemRandomizer = null;
            switch (skillHarvestType)
            {
                case HarvestType.BasedOnWeapon:
                    {
                        IWeaponItem weaponItem = weapon.GetWeaponItem();
                        HarvestEffectiveness harvestEffectiveness;
                        if (harvestable.CacheHarvestEffectivenesses.TryGetValue(weaponItem.WeaponType, out harvestEffectiveness) &&
                            harvestable.CacheHarvestItems.TryGetValue(weaponItem.WeaponType, out itemRandomizer))
                        {
                            calculatingTotalDamage = weaponItem.HarvestDamageAmount.GetAmount(weapon.level).Random(randomSeed) * harvestEffectiveness.damageEffectiveness;
                        }
                    }
                    break;
                case HarvestType.BasedOnSkill:
                    {
                        SkillHarvestEffectiveness skillHarvestEffectiveness;
                        if (harvestable.CacheSkillHarvestEffectivenesses.TryGetValue(skill, out skillHarvestEffectiveness) &&
                            harvestable.CacheSkillHarvestItems.TryGetValue(skill, out itemRandomizer))
                        {
                            calculatingTotalDamage = skill.HarvestDamageAmount.GetAmount(skillLevel).Random(randomSeed) * skillHarvestEffectiveness.damageEffectiveness;
                        }
                    }
                    break;
            }
            // If found randomizer, random dropping items
            if (skillHarvestType != HarvestType.None && itemRandomizer != null)
            {
                ItemDropForHarvestable receivingItem = itemRandomizer.TakeOne();
                int itemDataId = receivingItem.item.DataId;
                int itemAmount = (int)(receivingItem.amountPerDamage * calculatingTotalDamage);
                bool droppingToGround = collectType == HarvestableCollectType.DropToGround;

                if (attackerCharacter != null)
                {
                    if (attackerCharacter.IncreasingItemsWillOverwhelming(itemDataId, itemAmount))
                        droppingToGround = true;
                    if (!droppingToGround)
                    {
                        GameInstance.ServerGameMessageHandlers.NotifyRewardItem(attackerCharacter.ConnectionId, RewardGivenType.Harvestable, itemDataId, itemAmount);
                        attackerCharacter.IncreaseItems(CharacterItem.Create(itemDataId, 1, itemAmount));
                        attackerCharacter.FillEmptySlots();
                    }
                    attackerCharacter.RewardExp(new Reward() { exp = (int)(harvestable.expPerDamage * calculatingTotalDamage) }, 1, RewardGivenType.Harvestable);
                }
                else
                {
                    // Attacker is not character, always drop item to ground
                    droppingToGround = true;
                }

                if (droppingToGround)
                    ItemDropEntity.DropItem(this, RewardGivenType.Harvestable, CharacterItem.Create(itemDataId, 1, itemAmount), new string[0]);
            }
            // Apply damages
            combatAmountType = CombatAmountType.NormalDamage;
            totalDamage = CurrentGameInstance.GameplayRule.GetTotalDamage(fromPosition, instigator, this, calculatingTotalDamage, weapon, skill, skillLevel);
            if (totalDamage < 0)
                totalDamage = 0;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime = false)
        {
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);
            instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter);
            CurrentGameInstance.GameplayRule.OnHarvestableReceivedDamage(attackerCharacter, this, combatAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);

            if (combatAmountType == CombatAmountType.Miss)
                return;

            // Do something when entity dead
            if (this.IsDead())
                DestroyAndRespawn();
        }

        public virtual void DestroyAndRespawn()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (_isDestroyed)
                return;
            // Mark as destroyed
            _isDestroyed = true;
            // Tell clients that the harvestable destroy to play animation at client
            CallAllOnHarvestableDestroy();
            // Respawning later
            if (SpawnArea != null)
                SpawnArea.Spawn(SpawnPrefab, SpawnLevel, DestroyDelay + DestroyRespawnDelay);
            else if (Identity.IsSceneObject)
                RespawnRoutine(DestroyDelay + DestroyRespawnDelay).Forget();
            // Destroy this entity
            NetworkDestroy(DestroyDelay);
        }

        protected async UniTaskVoid RespawnRoutine(float delay)
        {
            await UniTask.Delay(Mathf.CeilToInt(delay * 1000));
            InitStats();
            Manager.Assets.NetworkSpawnScene(
                Identity.ObjectId,
                SpawnPosition,
                CurrentGameInstance.DimensionType == DimensionType.Dimension3D ? Quaternion.Euler(Vector3.up * Random.Range(0, 360)) : Quaternion.identity);
        }

        public override bool CanReceiveDamageFrom(EntityInfo entityInfo)
        {
            // Harvestable entity can receive damage inside safe area
            return true;
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
