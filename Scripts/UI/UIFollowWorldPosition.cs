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

    private Vector3? wantedPosition;
    private bool updatedOnce;
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
                CacheCameraTransform = targetCamera.transform;
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
        if (!SetupCamera())
            return;

        wantedPosition = RectTransformUtility.WorldToScreenPoint(targetCamera, targetPosition);
    }

    private void LateUpdate()
    {
        if (!wantedPosition.HasValue)
            return;

        followJobHandle.Complete();
        followJob = new UIFollowWorldPositionJob()
        {
            wantedPosition = wantedPosition.Value,
            damping = damping,
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
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (damping <= 0)
            transform.position = wantedPosition;
        else
            transform.position = Vector3.Lerp(transform.position, wantedPosition, damping * deltaTime);
    }
}