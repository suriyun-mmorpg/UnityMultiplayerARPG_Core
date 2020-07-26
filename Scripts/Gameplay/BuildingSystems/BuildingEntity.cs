using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;
using LiteNetLibManager;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class BuildingEntity : DamageableEntity, IBuildingSaveData
    {
        [Header("Building Data")]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
        public string buildingType;
        public List<string> buildingTypes;
        [Tooltip("This is a distance that allows a player to build the building")]
        public float buildDistance = 5f;
        [Tooltip("If this is value `TRUE`, this entity will be destroyed when its parent building entity was destroyed")]
        public bool destroyWhenParentDestroyed;
        [Tooltip("Building's max HP. If its HP <= 0, it will be destroyed")]
        public int maxHp = 100;
        [Tooltip("If life time is <= 0, it's unlimit lifetime")]
        public float lifeTime = 0f;
        [Tooltip("Items which will be dropped when building destroyed")]
        public List<ItemAmount> droppingItems;
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onBuildingDestroy` event before it's going to be destroyed from the game.")]
        public float destroyDelay = 2f;
        public UnityEvent onBuildingDestroy;
        public UnityEvent onBuildingConstruct;

        public override int MaxHp { get { return maxHp; } }

        /// <summary>
        /// Use this as reference for area to build this object while in build mode
        /// </summary>
        public BuildingArea BuildingArea { get; set; }

        [Header("Save Data")]
        [SerializeField]
        private SyncFieldString id = new SyncFieldString();
        [SerializeField]
        private SyncFieldString parentId = new SyncFieldString();
        [SerializeField]
        private SyncFieldFloat remainsLifeTime = new SyncFieldFloat();
        [SerializeField]
        private SyncFieldBool isLocked = new SyncFieldBool();
        [SerializeField]
        private SyncFieldString creatorId = new SyncFieldString();
        [SerializeField]
        private SyncFieldString creatorName = new SyncFieldString();

        public string Id
        {
            get { return id; }
            set { id.Value = value; }
        }

        public string ParentId
        {
            get { return parentId; }
            set { parentId.Value = value; }
        }

        public float RemainsLifeTime
        {
            get { return remainsLifeTime; }
            set { remainsLifeTime.Value = value; }
        }

        public bool IsLocked
        {
            get { return isLocked; }
            set { isLocked.Value = value; }
        }

        public string LockPassword
        {
            get;
            set;
        }

        public Vector3 Position
        {
            get { return CacheTransform.position; }
            set { CacheTransform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return CacheTransform.rotation; }
            set { CacheTransform.rotation = value; }
        }

        public string CreatorId
        {
            get { return creatorId; }
            set { creatorId.Value = value; }
        }

        public string CreatorName
        {
            get { return creatorName; }
            set { creatorName.Value = value; }
        }

        public virtual string ExtraData
        {
            get { return string.Empty; }
            set { }
        }

        public virtual bool Activatable { get { return false; } }
        public virtual bool Lockable { get { return false; } }
        public bool IsBuildMode { get; private set; }
        public BasePlayerCharacterEntity Builder { get; private set; }

        // Private variables
        private readonly List<BaseGameEntity> triggerEntities = new List<BaseGameEntity>();
        private readonly List<TilemapCollider2D> triggerTilemaps = new List<TilemapCollider2D>();
        private readonly List<BuildingMaterial> triggerMaterials = new List<BuildingMaterial>();
        private readonly List<BuildingEntity> children = new List<BuildingEntity>();
        private readonly List<BuildingMaterial> buildingMaterials = new List<BuildingMaterial>();
        private bool parentFound;
        private bool isDestroyed;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.buildingTag;
            gameObject.layer = CurrentGameInstance.buildingLayer;
            isDestroyed = false;

            if (buildingTypes == null)
                buildingTypes = new List<string>();

            if (!string.IsNullOrEmpty(buildingType) && !buildingTypes.Contains(buildingType))
                buildingTypes.Add(buildingType);
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("BuildingEntity - Update");
            if (IsBuildMode)
            {
                if (BuildingArea != null && BuildingArea.snapBuildingObject)
                {
                    CacheTransform.position = BuildingArea.transform.position;
                    CacheTransform.rotation = BuildingArea.transform.rotation;
                }
                bool canBuild = CanBuild();
                foreach (BuildingMaterial buildingMaterial in buildingMaterials)
                {
                    if (!buildingMaterial) continue;
                    buildingMaterial.CurrentState = canBuild ? BuildingMaterial.State.CanBuild : BuildingMaterial.State.CannotBuild;
                }
            }
            else
            {
                if (lifeTime > 0f && RemainsLifeTime > 0f)
                {
                    // Reduce remains life time
                    RemainsLifeTime -= Time.deltaTime;
                    if (RemainsLifeTime < 0)
                        RemainsLifeTime = 0f;
                }
            }
            Profiler.EndSample();
        }

        protected override void EntityLateUpdate()
        {
            base.EntityLateUpdate();
            // Setup parent which when it's destroying it will destroy children (chain destroy)
            if (IsServer && !parentFound)
            {
                BuildingEntity parent;
                if (CurrentGameManager.TryGetBuildingEntity(ParentId, out parent))
                {
                    parentFound = true;
                    parent.AddChildren(this);
                }
            }
        }

        public void RegisterMaterial(BuildingMaterial material)
        {
            if (!buildingMaterials.Contains(material))
            {
                Bounds tempLocalBounds = LocalBounds;
                Bounds tempMatLocalBounds = GameplayUtils.MakeLocalBoundsByCollider(material.CacheTransform);
                tempMatLocalBounds.center = material.CacheTransform.position - CacheTransform.position;
                if (tempLocalBounds.extents == Vector3.zero)
                    tempLocalBounds = tempMatLocalBounds;
                else
                    tempLocalBounds.Encapsulate(tempMatLocalBounds);
                LocalBounds = tempLocalBounds;
                buildingMaterials.Add(material);
            }
        }

        public override void OnSetup()
        {
            base.OnSetup();
            RegisterNetFunction(NetFuncOnBuildingDestroy);
            RegisterNetFunction(NetFuncOnBuildingConstruct);
            parentId.onChange += OnParentIdChange;
        }

        private void NetFuncOnBuildingDestroy()
        {
            if (onBuildingDestroy != null)
                onBuildingDestroy.Invoke();
        }

        public void RequestOnBuildingDestroy()
        {
            CallNetFunction(NetFuncOnBuildingDestroy, FunctionReceivers.All);
        }

        private void NetFuncOnBuildingConstruct()
        {
            if (onBuildingConstruct != null)
                onBuildingConstruct.Invoke();
        }

        public void RequestOnBuildingConstruct()
        {
            CallNetFunction(NetFuncOnBuildingConstruct, FunctionReceivers.All);
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            parentId.onChange -= OnParentIdChange;
        }

        private void OnParentIdChange(bool isInitial, string parentId)
        {
            parentFound = false;
        }

        public void AddChildren(BuildingEntity buildingEntity)
        {
            if (!children.Contains(buildingEntity))
                children.Add(buildingEntity);
        }

        public bool CanBuild()
        {
            if (BuildingArea == null || triggerEntities.Count > 0 || triggerMaterials.Count > 0 || triggerTilemaps.Count > 0)
                return false;
            if (BuildingArea.entity != null && !BuildingArea.entity.IsCreator(Builder))
                return false;
            return buildingTypes.Contains(BuildingArea.buildingType);
        }

        public override void ReceiveDamage(IGameEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            if (!IsServer || IsDead())
                return;

            base.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
            float calculatingTotalDamage = 0f;
            if (damageAmounts.Count > 0)
            {
                foreach (KeyValuePair<DamageElement, MinMaxFloat> allDamageAmount in damageAmounts)
                {
                    DamageElement damageElement = allDamageAmount.Key;
                    MinMaxFloat damageAmount = allDamageAmount.Value;
                    calculatingTotalDamage += damageAmount.Random();
                }
            }
            // Apply damages
            int totalDamage = (int)calculatingTotalDamage;
            CurrentHp -= totalDamage;
            ReceivedDamage(attacker, CombatAmountType.NormalDamage, totalDamage);

            // If current hp <= 0, character dead
            if (IsDead())
                Destroy();
        }

        public void Destroy()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (isDestroyed)
                return;
            isDestroyed = true;
            // Tell clients that the building destroy to play animation at client
            RequestOnBuildingDestroy();
            if (droppingItems != null && droppingItems.Count > 0)
            {
                foreach (ItemAmount droppingItem in droppingItems)
                {
                    if (droppingItem.item == null || droppingItem.amount == 0)
                        continue;
                    ItemDropEntity.DropItem(this, CharacterItem.Create(droppingItem.item, 1, droppingItem.amount), new uint[0]);
                }
            }
            NetworkDestroy(destroyDelay);
        }

        public void SetupAsBuildMode(BasePlayerCharacterEntity builder)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
                // Use rigidbody to detect trigger events
                Rigidbody rigidbody = collider.gameObject.GetOrAddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D collider in colliders2D)
            {
                collider.isTrigger = true;
                // Use rigidbody to detect trigger events
                Rigidbody2D rigidbody = collider.gameObject.GetOrAddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            IsBuildMode = true;
            Builder = builder;
        }

        public void TriggerEnterEntity(BaseGameEntity networkEntity)
        {
            if (networkEntity != null && !triggerEntities.Contains(networkEntity))
                triggerEntities.Add(networkEntity);
        }

        public void TriggerExitEntity(BaseGameEntity networkEntity)
        {
            if (networkEntity != null)
                triggerEntities.Remove(networkEntity);
        }

        public void TriggerEnterBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null &&
                buildingMaterial.TargetEntity != null &&
                buildingMaterial.TargetEntity != this &&
                !triggerMaterials.Contains(buildingMaterial))
                triggerMaterials.Add(buildingMaterial);
        }

        public void TriggerExitBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null)
                triggerMaterials.Remove(buildingMaterial);
        }

        public void TriggerEnterTilemap(TilemapCollider2D tilemapCollider)
        {
            if (tilemapCollider != null && !triggerTilemaps.Contains(tilemapCollider))
                triggerTilemaps.Add(tilemapCollider);
        }

        public void TriggerExitTilemap(TilemapCollider2D tilemapCollider)
        {
            if (tilemapCollider != null)
                triggerTilemaps.Remove(tilemapCollider);
        }

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (reasons == LiteNetLibGameManager.DestroyObjectReasons.RequestedToDestroy)
            {
                // Chain destroy
                foreach (BuildingEntity child in children)
                {
                    if (child == null || !child.destroyWhenParentDestroyed) continue;
                    child.Destroy();
                }
                children.Clear();
            }
        }

        public bool IsCreator(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return CreatorId.Equals(playerCharacterEntity.Id);
        }
    }
}
