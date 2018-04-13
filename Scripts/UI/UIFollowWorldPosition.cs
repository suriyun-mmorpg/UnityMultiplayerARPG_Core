using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class UIFollowWorldPosition : MonoBehaviour
{
    public Vector3 targetPosition;
    private bool alreadyShown;
    private CanvasGroup cacheCanvasGroup;
    public CanvasGroup CacheCanvasGroup
    {
        get
        {
            if (cacheCanvasGroup == null)
                cacheCanvasGroup = GetComponent<CanvasGroup>();
            return cacheCanvasGroup;
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

    private void Awake()
    {
        CacheCanvasGroup.alpha = 0;
    }

    private void LateUpdate()
    {
        UpdatePosition();
        if (!alreadyShown)
        {
            CacheCanvasGroup.alpha = 1;
            alreadyShown = true;
        }
    }

    public void UpdatePosition()
    {
        Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetPosition);
        CacheTransform.position = pos;
    }
}
