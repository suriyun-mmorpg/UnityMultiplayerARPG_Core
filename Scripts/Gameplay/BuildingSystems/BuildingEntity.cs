using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;
using LiteNetLibManager;
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
        public float characterForwardDistance = 4;
        public int maxHp = 100;

        public override int MaxHp { get { return maxHp; } }

        /// <summary>
        /// Use this as reference for area to build this object while in build mode
        /// </summary>
        [HideInInspector, System.NonSerialized]
        public BuildingArea buildingArea;

        [Header("Save Data")]
        [SerializeField]
        private SyncFieldString id = new SyncFieldString();
        [SerializeField]
        private SyncFieldString parentId = new SyncFieldString();
        [SerializeField]
        private SyncFieldString creatorId = new SyncFieldString();
        [SerializeField]
        private SyncFieldString creatorName = new SyncFieldString();
        [SerializeField]
        private int dataId;

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

        public int DataId
        {
            get { return dataId; }
            set { }
        }

        public virtual bool Activatable { get { return false; } }
        public bool isBuildMode { get; private set; }

        private readonly List<BaseGameEntity> triggerEntities = new List<BaseGameEntity>();
        private readonly List<TilemapCollider2D> triggerTilemaps = new List<TilemapCollider2D>();
        private readonly List<BuildingMaterial> triggerMaterials = new List<BuildingMaterial>();
        private readonly List<BuildingEntity> children = new List<BuildingEntity>();
        private BuildingMaterial[] buildingMaterials;
        private BuildingArea[] buildingAreas;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.buildingTag;
            gameObject.layer = gameInstance.buildingLayer;

            if (buildingTypes == null)
                buildingTypes = new List<string>();

            if (!string.IsNullOrEmpty(buildingType) && !buildingTypes.Contains(buildingType))
                buildingTypes.Add(buildingType);

            buildingMaterials = GetComponentsInChildren<BuildingMaterial>(true);
            if (buildingMaterials != null && buildingMaterials.Length > 0)
            {
                foreach (BuildingMaterial material in buildingMaterials)
                {
                    material.buildingEntity = this;
                    material.gameObject.tag = gameInstance.buildingTag;
                    material.gameObject.layer = gameInstance.buildingLayer;
                }
            }

            buildingAreas = GetComponentsInChildren<BuildingArea>(true);
            if (buildingAreas != null && buildingAreas.Length > 0)
            {
                foreach (BuildingArea area in buildingAreas)
                {
                    area.buildingEntity = this;
                }
            }
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

        private void OnParentIdChange(bool isInitial, string parentId)
        {
            if (IsServer)
            {
                BuildingEntity parent;
                if (gameManager.TryGetBuildingEntity(id, out parent))
                    parent.AddChildren(this);
            }
        }

        public void AddChildren(BuildingEntity buildingEntity)
        {
            if (!children.Contains(buildingEntity))
                children.Add(buildingEntity);
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("BuildingEntity - Update");
            if (isBuildMode)
            {
                if (buildingArea != null && buildingArea.snapBuildingObject)
                {
                    CacheTransform.position = buildingArea.transform.position;
                    CacheTransform.rotation = buildingArea.transform.rotation;
                }
                bool canBuild = CanBuild();
                foreach (BuildingMaterial buildingMaterial in buildingMaterials)
                {
                    buildingMaterial.CurrentState = canBuild ? BuildingMaterial.State.CanBuild : BuildingMaterial.State.CannotBuild;
                }
            }
            Profiler.EndSample();
        }

        public bool CanBuild()
        {
            if (buildingArea == null || triggerEntities.Count > 0 || triggerMaterials.Count > 0 || triggerTilemaps.Count > 0)
                return false;
            return buildingTypes.Contains(buildingArea.buildingType);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
#if UNITY_EDITOR
            if (!Application.isPlaying && dataId != name.GenerateHashId())
            {
                dataId = name.GenerateHashId();
                EditorUtility.SetDirty(this);
            }
#endif
        }

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
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
                NetworkDestroy();
        }

        public void SetupAsBuildMode()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;
                // Use rigidbody to detect trigger events
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = collider.gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>(true);
            foreach (Collider2D collider in colliders2D)
            {
                collider.isTrigger = true;
                // Use rigidbody to detect trigger events
                Rigidbody2D rigidbody = collider.GetComponent<Rigidbody2D>();
                if (rigidbody == null)
                    rigidbody = collider.gameObject.AddComponent<Rigidbody2D>();
                rigidbody.gravityScale = 0;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            isBuildMode = true;
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
                buildingMaterial.buildingEntity != null &&
                buildingMaterial.buildingEntity != this &&
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
                    if (child == null) continue;
                    child.NetworkDestroy();
                }
            }
        }
    }
}
