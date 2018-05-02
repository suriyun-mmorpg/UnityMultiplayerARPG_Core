using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

public class RpgNetworkEntity : LiteNetLibBehaviour
{
    public string title;
    public Text textTitle;

    public virtual string Title { get { return title; } }

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

    protected virtual void LateUpdate()
    {
        if (textTitle != null)
            textTitle.text = Title;
    }
}
