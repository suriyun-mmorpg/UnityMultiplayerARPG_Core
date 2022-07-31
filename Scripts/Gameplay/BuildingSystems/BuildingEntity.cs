using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;
using LiteNetLibManager;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class BuildingEntity : DamageableEntity, IBuildingSaveData, IActivatePressActivatableEntity, IClickActivatableEntity, IHoldClickActivatableEntity
    {
        public const float BUILD_DISTANCE_BUFFER = 0.1f;

        [Category(5, "Building Settings")]
        [SerializeField]
        [Tooltip("If this is `TRUE` this building entity will be able to build on any surface. But when constructing, if player aimming on building area it will place on building area")]
        protected bool canBuildOnAnySurface = false;

        [SerializeField]
        [Tooltip("If this is `TRUE` this building entity will be able to build on limited surface hit normal angle (default up angle is 90)")]
        protected bool limitSurfaceHitNormalAngle = false;

        [SerializeField]
        protected float limitSurfaceHitNormalAngleMin = 80f;

        [SerializeField]
        protected float limitSurfaceHitNormalAngleMax = 100f;

        [HideInInspector]
        [SerializeField]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish. This is a part of `buildingTypes`, just keep it for backward compatibility.")]
        protected string buildingType = string.Empty;

        [SerializeField]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish.")]
        protected List<string> buildingTypes = new List<string>();

        [SerializeField]
        [Tooltip("This is a distance that allows a player to build the building")]
        protected float buildDistance = 5f;

        [SerializeField]
        [Tooltip("If this is value `TRUE`, this entity will be destroyed when its parent building entity was destroyed")]
        protected bool destroyWhenParentDestroyed = false;

        [SerializeField]
        [Tooltip("Building's max HP. If its HP <= 0, it will be destroyed")]
        protected int maxHp = 100;

        [SerializeField]
        [Tooltip("If life time is <= 0, it's unlimit lifetime")]
        protected float lifeTime = 0f;

        [SerializeField]
        [Tooltip("Items which will be dropped when building destroyed")]
        protected List<ItemAmount> droppingItems = new List<ItemAmount>();

        [SerializeField]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onBuildingDestroy` event before it's going to be destroyed from the game.")]
        protected float destroyDelay = 2f;

        [SerializeField]
        protected InputField.ContentType passwordContentType = InputField.ContentType.Pin;
        public InputField.ContentType PasswordContentType { get { return passwordContentType; } }

        [SerializeField]
        protected int passwordLength = 6;
        public int PasswordLength { get { return passwordLength; } }

        [Category("Events")]
        [SerializeField]
        protected UnityEvent onBuildingDestroy = new UnityEvent();
        [SerializeField]
        protected UnityEvent onBuildingConstruct = new UnityEvent();

        public bool CanBuildOnAnySurface { get { return canBuildOnAnySurface; } }
        public bool LimitSurfaceHitNormalAngle { get { return limitSurfaceHitNormalAngle; } }
        public float LimitSurfaceHitNormalAngleMin { get { return limitSurfaceHitNormalAngleMin; } }
        public float LimitSurfaceHitNormalAngleMax { get { return limitSurfaceHitNormalAngleMax; } }
        public List<string> BuildingTypes { get { return buildingTypes; } }
        public float BuildDistance { get { return buildDistance; } }
        public float BuildYRotation { get; set; }
        public override int MaxHp { get { return maxHp; } }
        public float LifeTime { get { return lifeTime; } }

        /// <summary>
        /// Use this as reference for area to build this object while in build mode
        /// </summary>
        public BuildingArea BuildingArea { get; set; }

        /// <summary>
        /// Use this as reference for hit surface state while in build mode
        /// </summary>
        public bool HitSurface { get; set; }

        /// <summary>
        /// Use this as reference for hit surface normal while in build mode
        /// </summary>
        public Vector3 HitSurfaceNormal { get; set; }

        [Category("Sync Fields")]
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

        protected readonly List<BaseGameEntity> triggerEntities = new List<BaseGameEntity>();
        protected readonly List<TilemapCollider2D> triggerTilemaps = new List<TilemapCollider2D>();
        protected readonly List<BuildingMaterial> triggerMaterials = new List<BuildingMaterial>();
        protected readonly List<NoConstructionArea> triggerNoConstructionAreas = new List<NoConstructionArea>();
        protected readonly List<BuildingEntity> children = new List<BuildingEntity>();
        protected readonly List<BuildingMaterial> buildingMaterials = new List<BuildingMaterial>();
        protected bool parentFound;
        protected bool isDestroyed;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.buildingTag;
            gameObject.layer = CurrentGameInstance.buildingLayer;
            isStaticHitBoxes = true;
            isDestroyed = false;
            MigrateBuildingType();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (MigrateBuildingType())
                EditorUtility.SetDirty(this);
        }
#endif

        protected bool MigrateBuildingType()
        {
            if (!string.IsNullOrEmpty(buildingType) && !buildingTypes.Contains(buildingType))
            {
                buildingTypes.Add(buildingType);
                buildingType = string.Empty;
                return true;
            }
            return false;
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
                    if (BuildingArea.allowRotateInSocket)
                    {
                        CacheTransform.localEulerAngles = new Vector3(
                            CacheTransform.localEulerAngles.x,
                            CacheTransform.localEulerAngles.y + BuildYRotation,
                            CacheTransform.localEulerAngles.z);
                    }
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
                if (IsServer && lifeTime > 0f)
                {
                    // Reduce remains life time
                    RemainsLifeTime -= Time.deltaTime;
                    if (RemainsLifeTime < 0)
                    {
                        // Destroy building
                        RemainsLifeTime = 0f;
                        Destroy();
                    }
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
                if (GameInstance.ServerBuildingHandlers.TryGetBuilding(ParentId, out parent))
                {
                    parentFound = true;
                    parent.AddChildren(this);
                }
            }
        }

        public void RegisterMaterial(BuildingMaterial material)
        {
            if (!buildingMaterials.Contains(material))
                buildingMaterials.Add(material);
        }

        public override void OnSetup()
        {
            base.OnSetup();
            parentId.onChange += OnParentIdChange;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            parentId.onChange -= OnParentIdChange;
        }

        [AllRpc]
        private void AllOnBuildingDestroy()
        {
            if (onBuildingDestroy != null)
                onBuildingDestroy.Invoke();
        }

        [AllRpc]
        private void AllOnBuildingConstruct()
        {
            if (onBuildingConstruct != null)
                onBuildingConstruct.Invoke();
        }

        public void CallAllOnBuildingDestroy()
        {
            RPC(AllOnBuildingDestroy);
        }

        public void CallAllOnBuildingConstruct()
        {
            RPC(AllOnBuildingConstruct);
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

        public bool IsPositionInBuildDistance(Vector3 builderPosition, Vector3 placePosition)
        {
            return Vector3.Distance(builderPosition, placePosition) <= BuildDistance;
        }

        public bool CanBuild()
        {
            if (Builder == null)
            {
                // Builder destroyed?
                return false;
            }
            if (!IsPositionInBuildDistance(Builder.CacheTransform.position, CacheTransform.position))
            {
                // Too far from buildiner?
                return false;
            }
            if (triggerEntities.Count > 0 || triggerMaterials.Count > 0 || triggerTilemaps.Count > 0 || triggerNoConstructionAreas.Count > 0)
            {
                // Triggered something?
                return false;
            }
            if (LimitSurfaceHitNormalAngle && CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                float angle = GameplayUtils.GetPitchByDirection(HitSurfaceNormal);
                if (angle < LimitSurfaceHitNormalAngleMin || angle > LimitSurfaceHitNormalAngleMax)
                    return false;
            }
            if (BuildingArea != null)
            {
                // Must build on building area
                return BuildingArea.AllowToBuild(this);
            }
            else
            {
                // Can build on any surface and it hit surface?
                return canBuildOnAnySurface && HitSurface;
            }
        }

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            // Calculate damages
            float calculatingTotalDamage = 0f;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                calculatingTotalDamage += damageAmounts[damageElement].Random(randomSeed);
            }
            // Apply damages
            combatAmountType = CombatAmountType.NormalDamage;
            totalDamage = CurrentGameInstance.GameplayRule.GetTotalDamage(fromPosition, instigator, this, calculatingTotalDamage, weapon, skill, skillLevel);
            if (totalDamage < 0)
                totalDamage = 0;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel, CharacterBuff buff)
        {
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel, buff);

            if (combatAmountType == CombatAmountType.Miss)
                return;

            // Do something when entity dead
            if (this.IsDead())
                Destroy();
        }

        public virtual void Destroy()
        {
            if (!IsServer)
                return;
            CurrentHp = 0;
            if (isDestroyed)
                return;
            isDestroyed = true;
            // Tell clients that the building destroy to play animation at client
            CallAllOnBuildingDestroy();
            // Drop items
            if (droppingItems != null && droppingItems.Count > 0)
            {
                foreach (ItemAmount droppingItem in droppingItems)
                {
                    if (droppingItem.item == null || droppingItem.amount == 0)
                        continue;
                    ItemDropEntity.DropItem(this, CharacterItem.Create(droppingItem.item, 1, droppingItem.amount), new string[0]);
                }
            }
            // Destroy this entity
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
            if (networkEntity != null &&
                !triggerEntities.Contains(networkEntity) &&
                networkEntity.Entity != Entity)
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
                buildingMaterial.BuildingEntity != this &&
                !triggerMaterials.Contains(buildingMaterial) &&
                buildingMaterial.Entity != Entity)
                triggerMaterials.Add(buildingMaterial);
        }

        public void TriggerExitBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null)
                triggerMaterials.Remove(buildingMaterial);
        }

        public void TriggerEnterTilemap(TilemapCollider2D tilemapCollider)
        {
            if (tilemapCollider != null &&
                !triggerTilemaps.Contains(tilemapCollider))
                triggerTilemaps.Add(tilemapCollider);
        }

        public void TriggerExitTilemap(TilemapCollider2D tilemapCollider)
        {
            if (tilemapCollider != null)
                triggerTilemaps.Remove(tilemapCollider);
        }

        public void TriggerEnterNoConstructionArea(NoConstructionArea noConstructionArea)
        {
            if (noConstructionArea != null &&
                !triggerNoConstructionAreas.Contains(noConstructionArea))
                triggerNoConstructionAreas.Add(noConstructionArea);
        }

        public void TriggerExitNoConstructionArea(NoConstructionArea noConstructionArea)
        {
            if (noConstructionArea != null)
                triggerNoConstructionAreas.Remove(noConstructionArea);
        }

        public override void OnNetworkDestroy(byte reasons)
        {
            base.OnNetworkDestroy(reasons);
            if (reasons == DestroyObjectReasons.RequestedToDestroy)
            {
                // Chain destroy
                foreach (BuildingEntity child in children)
                {
                    if (child == null || !child.destroyWhenParentDestroyed) continue;
                    child.Destroy();
                }
                children.Clear();
                CurrentGameManager.DestroyBuildingEntity(Id);
            }
        }

        public bool IsCreator(IPlayerCharacterData playerCharacter)
        {
            return playerCharacter != null && IsCreator(playerCharacter.Id);
        }

        public bool IsCreator(string playerCharacterId)
        {
            return CreatorId.Equals(playerCharacterId);
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.conversationDistance;
        }

        public virtual bool ShouldBeAttackTarget()
        {
            return false;
        }

        public virtual bool CanKeyPressActivate()
        {
            return !this.IsDead() && Activatable;
        }

        public virtual void OnKeyPressActivate()
        {
            // Do nothing, override this function to do something
        }

        public virtual bool CanClickActivate()
        {
            return !this.IsDead() && Activatable;
        }

        public virtual void OnClickActivate()
        {
            // Do nothing, override this function to do something
        }

        public virtual bool CanHoldClickActivate()
        {
            return !this.IsDead();
        }

        public virtual void OnHoldClickActivate()
        {
            BaseUISceneGameplay.Singleton.ShowCurrentBuildingDialog(this);
        }
    }
}
