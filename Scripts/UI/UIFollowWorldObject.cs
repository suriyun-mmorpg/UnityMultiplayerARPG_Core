using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIFollowWorldPosition))]
public class UIFollowWorldObject : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;
    public Transform TargetObject
    {
        get { return targetObject; }
        set
        {
            targetObject = value;
            UpdatePosition();
        }
    }

    private UIFollowWorldPosition cachePositionFollower;
    public UIFollowWorldPosition CachePositionFollower
    {
        get
        {
            if (cachePositionFollower == null)
                cachePositionFollower = GetComponent<UIFollowWorldPosition>();
            return cachePositionFollower;
        }
    }

    private void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (TargetObject == null)
            return;

        CachePositionFollower.targetPosition = TargetObject.position;
    }
}
