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
    public bool canPlaceOnGround;

    [HideInInspector]
    public BuildingEntity buildingEntity;

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
}
