using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BuildingObject : MonoBehaviour
{
    [Header("Building Data")]
    [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
    public string buildingType;
    public float characterForwardDistance = 4;
    public int maxHp = 100;

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

    private readonly List<Component> triggerComponents = new List<Component>();

    public uint EntityObjectId
    {
        get { return buildingEntity == null ? 0 : buildingEntity.ObjectId; }
    }

    [SerializeField]
    private int dataId;
    public int DataId { get { return dataId; } }
    
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
        if (buildingArea != null && buildingArea.snapBuildingObject)
        {
            CacheTransform.position = buildingArea.transform.position;
            CacheTransform.rotation = buildingArea.transform.rotation;
        }
        if (isBuildMode)
        {
            var canBuild = CanBuild();
            foreach (var buildingMaterial in buildingMaterials)
            {
                buildingMaterial.CurrentState = canBuild ? BuildingMaterial.State.CanBuild : BuildingMaterial.State.CannotBuild;
            }
        }
    }

    public bool CanBuild()
    {
        if (buildingArea == null || triggerComponents.Count > 0)
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

    public void AddTriggerEntity(RpgNetworkEntity networkEntity)
    {
        if (networkEntity != null && !triggerComponents.Contains(networkEntity))
            triggerComponents.Add(networkEntity);
    }

    public void RemoveTriggerEntity(RpgNetworkEntity networkEntity)
    {
        if (networkEntity != null)
            triggerComponents.Remove(networkEntity);
    }

    public void AddTriggerBuildingMaterial(BuildingMaterial buildingMaterial)
    {
        if (buildingMaterial != null && buildingMaterial.buildingObject != null && !triggerComponents.Contains(buildingMaterial.buildingObject))
            triggerComponents.Add(buildingMaterial.buildingObject);
    }

    public void RemoveTriggerBuildingMaterial(BuildingMaterial buildingMaterial)
    {
        if (buildingMaterial != null && buildingMaterial.buildingObject != null)
            triggerComponents.Remove(buildingMaterial.buildingObject);
    }
}
