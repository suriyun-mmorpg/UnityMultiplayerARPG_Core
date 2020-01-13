using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilsComponents
{
    public class Billboard : MonoBehaviour
    {
        public Camera targetCamera;

        public Transform CacheTransform { get; private set; }
        public Transform CacheCameraTransform { get; private set; }

        private void OnEnable()
        {
            CacheTransform = transform;
            SetupCamera();
        }

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

        private void LateUpdate()
        {
            if (!SetupCamera())
                return;
            CacheTransform.rotation = Quaternion.Euler(Quaternion.LookRotation(CacheCameraTransform.forward, CacheCameraTransform.up).eulerAngles);
        }
    }
}
