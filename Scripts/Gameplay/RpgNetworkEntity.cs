using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class RpgNetworkEntity : LiteNetLibBehaviour
{
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

    private BaseRpgNetworkManager cacheManager;
    public BaseRpgNetworkManager CacheManager
    {
        get
        {
            if (cacheManager == null)
                cacheManager = Manager as BaseRpgNetworkManager;
            return cacheManager;
        }
    }
}
