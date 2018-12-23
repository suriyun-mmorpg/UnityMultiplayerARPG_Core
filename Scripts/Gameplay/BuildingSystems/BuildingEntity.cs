using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public sealed class BuildingEntity : DamageableEntity, IBuildingSaveData
    {
        [Header("Building Data")]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
        public string buildingType;
        public float characterForwardDistance = 4;
        public int maxHp = 100;

        public override int MaxHp { get { return maxHp; } }

        /// <summary>
        /// Use this as reference for entity to interactive while in play mode
        /// </summary>
        [HideInInspector, System.NonSerialized]
        public BuildingEntity buildingEntity;

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

        public bool isBuildMode { get; private set; }

        private readonly List<BaseGameEntity> triggerEntities = new List<BaseGameEntity>();
        private readonly List<BuildingEntity> triggerBuildings = new List<BuildingEntity>();
        private readonly List<BuildingMaterial> buildingMaterials = new List<BuildingMaterial>();
        private readonly List<BuildingArea> buildingAreas = new List<BuildingArea>();

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = GameInstance.buildingTag;
            gameObject.layer = GameInstance.buildingLayer;

            var materials = GetComponentsInChildren<BuildingMaterial>(true);
            if (materials != null && materials.Length > 0)
            {
                foreach (var material in materials)
                {
                    material.buildingEntity = this;
                    buildingMaterials.Add(material);
                }
            }

            var areas = GetComponentsInChildren<BuildingArea>(true);
            if (areas != null && areas.Length > 0)
            {
                foreach (var area in areas)
                {
                    area.buildingEntity = this;
                    buildingAreas.Add(area);
                }
            }
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
                var canBuild = CanBuild();
                foreach (var buildingMaterial in buildingMaterials)
                {
                    buildingMaterial.CurrentState = canBuild ? BuildingMaterial.State.CanBuild : BuildingMaterial.State.CannotBuild;
                }
            }
            Profiler.EndSample();
        }

        public bool CanBuild()
        {
            if (buildingArea == null || triggerEntities.Count > 0 || HitNonParentObject())
                return false;
            return buildingType.Equals(buildingArea.buildingType);
        }

#if UNITY_EDITOR
        public override void OnBehaviourValidate()
        {
            base.OnBehaviourValidate();
            if (!Application.isPlaying && dataId != name.GenerateHashId())
            {
                dataId = name.GenerateHashId();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif

        public override void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            if (!IsServer || IsDead())
                return;

            base.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
            // TODO: Reduce current hp
        }

        public void SetupAsBuildMode()
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.isTrigger = true;
                // We'll use rigidbody to detect trigger events
                var rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    rigidbody = collider.gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
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
                buildingMaterial.buildingEntity.buildingType == buildingType &&
                !triggerBuildings.Contains(buildingMaterial.buildingEntity))
                triggerBuildings.Add(buildingMaterial.buildingEntity);
        }

        public void TriggerExitBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null && buildingMaterial.buildingEntity != null)
                triggerBuildings.Remove(buildingMaterial.buildingEntity);
        }

        public bool HitNonParentObject()
        {
            foreach (var triggerBuilding in triggerBuildings)
            {
                if (buildingArea != null && triggerBuilding != buildingArea.buildingEntity)
                {
                    if (Vector3.Distance(triggerBuilding.CacheTransform.position, CacheTransform.position) <= 1f)
                        return true;
                }
            }
            return false;
        }
    }
}
