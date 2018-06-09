using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;
    public Camera CacheTargetCamera
    {
        get
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            return targetCamera;
        }
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

    void Update()
    {
        if (CacheTargetCamera != null)
            CacheTransform.rotation = Quaternion.Euler(Quaternion.LookRotation(CacheTargetCamera.transform.forward, CacheTargetCamera.transform.up).eulerAngles);
    }
}
