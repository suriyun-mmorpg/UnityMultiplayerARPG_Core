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

    [HideInInspector]
    public BuildingEntity buildingEntity;

    [HideInInspector]
    public BuildingArea buildingArea;

    private bool isBuildMode;

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

    private void Awake()
    {
        var materials = GetComponentsInChildren<BuildingMaterial>(true);
        if (materials != null && materials.Length > 0)
            buildingMaterials.AddRange(materials);
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
        if (buildingArea == null)
            return false;
        return buildingType.Equals(buildingArea.buildingType);
    }

    public void SetupAsBuildMode()
    {
        var colliders = GetComponentsInChildren<Collider>(true);
        foreach (var collider in colliders)
        {
            collider.isTrigger = true;
        }
        isBuildMode = true;
    }
}
