using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class BuildingObject : MonoBehaviour
    {

        [SerializeField]
        private int dataId;
        public int DataId { get { return dataId; } }
        [Header("Generice data")]
        public string title;
        [Header("Building Data")]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
        public string buildingType;
        public float characterForwardDistance = 4;
        public int maxHp = 100;
        [SerializeField]
        private Transform combatTextTransform;

        /// <summary>
        /// Use this as reference for entity to interactive while in play mode
        /// </summary>
        [HideInInspector]
        public BuildingEntity buildingEntity;

        /// <summary>
        /// Use this as reference for area to build this object while in build mode
        /// </summary>
        [HideInInspector]
        public BuildingArea buildingArea;

        public bool isBuildMode { get; private set; }

        private readonly List<RpgNetworkEntity> triggerEntities = new List<RpgNetworkEntity>();
        private readonly List<BuildingObject> triggerBuildings = new List<BuildingObject>();

        public uint EntityObjectId
        {
            get { return buildingEntity == null ? 0 : buildingEntity.ObjectId; }
        }

        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

        public Transform CombatTextTransform
        {
            get
            {
                if (combatTextTransform == null)
                    combatTextTransform = CacheTransform;
                return combatTextTransform;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && dataId != name.GenerateHashId())
            {
                dataId = name.GenerateHashId();
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif

        private readonly List<BuildingMaterial> buildingMaterials = new List<BuildingMaterial>();
        private readonly List<BuildingArea> buildingAreas = new List<BuildingArea>();

        private void Awake()
        {
            var materials = GetComponentsInChildren<BuildingMaterial>(true);
            if (materials != null && materials.Length > 0)
            {
                foreach (var material in materials)
                {
                    material.buildingObject = this;
                    buildingMaterials.Add(material);
                }
            }

            var areas = GetComponentsInChildren<BuildingArea>(true);
            if (areas != null && areas.Length > 0)
            {
                foreach (var area in areas)
                {
                    area.buildingObject = this;
                    buildingAreas.Add(area);
                }
            }
        }

        private void Update()
        {
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
        }

        public bool CanBuild()
        {
            if (buildingArea == null || triggerEntities.Count > 0 || HitNonParentObject())
                return false;
            return buildingType.Equals(buildingArea.buildingType);
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

        public void TriggerEnterEntity(RpgNetworkEntity networkEntity)
        {
            if (networkEntity != null && !triggerEntities.Contains(networkEntity))
                triggerEntities.Add(networkEntity);
        }

        public void TriggerExitEntity(RpgNetworkEntity networkEntity)
        {
            if (networkEntity != null)
                triggerEntities.Remove(networkEntity);
        }

        public void TriggerEnterBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null &&
                buildingMaterial.buildingObject != null &&
                buildingMaterial.buildingObject != this &&
                buildingMaterial.buildingObject.buildingType == buildingType &&
                !triggerBuildings.Contains(buildingMaterial.buildingObject))
                triggerBuildings.Add(buildingMaterial.buildingObject);
        }

        public void TriggerExitBuildingMaterial(BuildingMaterial buildingMaterial)
        {
            if (buildingMaterial != null && buildingMaterial.buildingObject != null)
                triggerBuildings.Remove(buildingMaterial.buildingObject);
        }

        public bool HitNonParentObject()
        {
            foreach (var triggerBuilding in triggerBuildings)
            {
                if (buildingArea != null && triggerBuilding != buildingArea.buildingObject)
                {
                    if (Vector3.Distance(triggerBuilding.CacheTransform.position, CacheTransform.position) <= 1f)
                        return true;
                }
            }
            return false;
        }
    }
}
