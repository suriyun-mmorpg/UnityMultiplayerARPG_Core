using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingObject : MonoBehaviour
{
    [Header("Building Data")]
    [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
    public string buildingType;
    public bool canPlaceOnGround;

    public string Id { get { return name; } }
    public int DataId { get { return name.GenerateHashId(); } }
    
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
}
