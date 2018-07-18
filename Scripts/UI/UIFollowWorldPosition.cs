using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIFollowWorldPosition : MonoBehaviour
{
    public Camera targetCamera;
    public Vector3 targetPosition;
    public float damping = 5f;
    public float snapDistance = 5f;

    public Camera CacheTargetCamera
    {
        get
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            return targetCamera;
        }
    }

    private RectTransform cacheTransform;
    public RectTransform CacheTransform
    {
        get
        {
            if (cacheTransform == null)
                cacheTransform = GetComponent<RectTransform>();
            return cacheTransform;
        }
    }

    private void Start()
    {
        Vector2 wantedPosition = RectTransformUtility.WorldToScreenPoint(CacheTargetCamera, targetPosition);
        CacheTransform.position = wantedPosition;
    }

    private void OnEnable()
    {
        Vector2 wantedPosition = RectTransformUtility.WorldToScreenPoint(CacheTargetCamera, targetPosition);
        CacheTransform.position = wantedPosition;
    }

    private void Update()
    {
        Vector2 wantedPosition = RectTransformUtility.WorldToScreenPoint(CacheTargetCamera, targetPosition);
        if (damping <= 0 || Vector3.Distance(CacheTransform.position, wantedPosition) >= snapDistance)
            CacheTransform.position = wantedPosition;
        else
            CacheTransform.position = Vector3.Slerp(CacheTransform.position, wantedPosition, damping * Time.deltaTime);
    }
}
