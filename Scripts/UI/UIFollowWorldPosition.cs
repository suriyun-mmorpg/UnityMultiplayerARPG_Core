using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[RequireComponent(typeof(RectTransform))]
public class UIFollowWorldPosition : MonoBehaviour
{
    public Camera targetCamera;
    public Vector3 targetPosition;
    public float damping = 5f;
    public float snapDistance = 5f;

    private Vector3 wantedPosition;
    private Vector3 oldTargetPosition;
    private Vector3 oldCameraPosition;
    private Quaternion oldCameraRotation;
    private TransformAccessArray followJobTransforms;
    private UIFollowWorldPositionJob followJob;
    private JobHandle followJobHandle;

    public RectTransform CacheTransform { get; private set; }
    public Transform CacheCameraTransform { get; private set; }

    private bool SetupCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera != null)
            {
                CacheCameraTransform = targetCamera.transform;
                CacheTransform.position = RectTransformUtility.WorldToScreenPoint(targetCamera, targetPosition);
            }
        }
        return targetCamera != null;
    }

    private void OnEnable()
    {
        CacheTransform = GetComponent<RectTransform>();
        SetupCamera();
        followJobTransforms = new TransformAccessArray(new Transform[] { CacheTransform });
    }

    private void OnDisable()
    {
        followJobTransforms.Dispose();
        followJobHandle.Complete();
    }

    private void Update()
    {
        followJobHandle.Complete();

        if (!SetupCamera())
            return;

        // Find wanted position only when it needed
        if (!oldTargetPosition.Equals(targetPosition) ||
            !CacheCameraTransform.position.Equals(oldCameraPosition) ||
            !CacheCameraTransform.rotation.Equals(oldCameraRotation))
            wantedPosition = RectTransformUtility.WorldToScreenPoint(targetCamera, targetPosition);

        oldTargetPosition = targetPosition;
        oldCameraPosition = CacheCameraTransform.position;
        oldCameraRotation = CacheCameraTransform.rotation;

        followJob = new UIFollowWorldPositionJob()
        {
            wantedPosition = wantedPosition,
            damping = damping,
            snapDistance = snapDistance,
            deltaTime = Time.deltaTime,
        };
        followJobHandle = followJob.Schedule(followJobTransforms);
        JobHandle.ScheduleBatchedJobs();
    }
}

public struct UIFollowWorldPositionJob : IJobParallelForTransform
{
    public Vector2 wantedPosition;
    public float damping;
    public float snapDistance;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (damping <= 0 || Vector3.Distance(transform.position, wantedPosition) >= snapDistance)
            transform.position = wantedPosition;
        else
            transform.position = Vector3.Slerp(transform.position, wantedPosition, damping * deltaTime);
    }
}