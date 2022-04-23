using UnityEngine;

namespace UtilsComponents
{
    public class CopyCamera : MonoBehaviour
    {
        public Camera copyFromCamera;
        public Camera copyToCamera;

        void Update()
        {
            if (copyFromCamera == null || copyToCamera == null)
                return;
            copyToCamera.orthographic = copyFromCamera.orthographic;
            copyToCamera.orthographicSize = copyFromCamera.orthographicSize;
            copyToCamera.nearClipPlane = copyFromCamera.nearClipPlane;
            copyToCamera.farClipPlane = copyFromCamera.farClipPlane;
            copyToCamera.fieldOfView = copyFromCamera.fieldOfView;
            copyToCamera.rect = copyFromCamera.rect;
            copyToCamera.usePhysicalProperties = copyFromCamera.usePhysicalProperties;
            copyToCamera.focalLength = copyFromCamera.focalLength;
            copyToCamera.sensorSize = copyFromCamera.sensorSize;
            copyToCamera.lensShift = copyFromCamera.lensShift;
        }
    }
}
