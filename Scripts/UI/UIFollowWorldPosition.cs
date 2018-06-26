using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class UIFollowWorldPosition : MonoBehaviour
{
    public Vector3 targetPosition;
    public float damping = 5f;

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
        Vector2 wantedPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPosition);
        CacheTransform.position = wantedPosition;
    }

    private void Update()
    {
        Vector2 wantedPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPosition);
        CacheTransform.position = Vector3.Slerp(CacheTransform.position, wantedPosition, damping * Time.deltaTime);
    }
}
