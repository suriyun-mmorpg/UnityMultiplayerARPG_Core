using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultGameplayCameraController : MonoBehaviour, IGameplayCameraController
    {
        public FollowCameraControls gameplayCameraPrefab = null;
        public bool aimAssistPlayer = true;
        public bool aimAssistMonster = true;
        public bool aimAssistBuilding = false;
        public bool aimAssistHarvestable = false;
        public BasePlayerCharacterEntity PlayerCharacterEntity { get; protected set; }
        public FollowCameraControls CameraControls { get; protected set; }
        public Camera Camera { get { return CameraControls.CacheCamera; } }
        public Transform CameraTransform { get { return CameraControls.CacheCameraTransform; } }
        public Transform FollowingEntityTransform { get; set; }
        public Vector3 TargetOffset { get { return CameraControls.targetOffset; } set { CameraControls.targetOffset = value; } }
        public float CameraFov { get { return Camera.fieldOfView; } set { Camera.fieldOfView = value; } }
        public float CameraNearClipPlane { get { return Camera.nearClipPlane; } set { Camera.nearClipPlane = value; } }
        public float CameraFarClipPlane { get { return Camera.farClipPlane; } set { Camera.farClipPlane = value; } }
        public float MinZoomDistance { get { return CameraControls.minZoomDistance; } set { CameraControls.minZoomDistance = value; } }
        public float MaxZoomDistance { get { return CameraControls.maxZoomDistance; } set { CameraControls.maxZoomDistance = value; } }
        public float CurrentZoomDistance { get { return CameraControls.zoomDistance; } set { CameraControls.zoomDistance = value; } }
        public bool EnableWallHitSpring { get { return CameraControls.enableWallHitSpring; } set { CameraControls.enableWallHitSpring = value; } }
        public bool UpdateRotation { get { return CameraControls.updateRotation; } set { CameraControls.updateRotation = value; } }
        public bool UpdateRotationX { get { return CameraControls.updateRotationX; } set { CameraControls.updateRotationX = value; } }
        public bool UpdateRotationY { get { return CameraControls.updateRotationY; } set { CameraControls.updateRotationY = value; } }
        public bool UpdateZoom { get { return CameraControls.updateZoom; } set { CameraControls.updateZoom = value; } }

        protected bool cameraControlsInitialized = false;

        protected virtual void Start()
        {
            InitialCameraControls();
        }

        public virtual void InitialCameraControls()
        {
            if (cameraControlsInitialized)
                return;
            cameraControlsInitialized = true;
            if (gameplayCameraPrefab == null)
            {
                Debug.LogWarning("`gameplayCameraPrefab` is empty, `DefaultGameplayCameraController` component is disabling.");
                enabled = false;
            }
            CameraControls = Instantiate(gameplayCameraPrefab);
        }

        protected virtual void OnDestroy()
        {
            if (CameraControls != null)
                Destroy(CameraControls.gameObject);
        }

        protected virtual void Update()
        {
            CameraControls.target = FollowingEntityTransform;
        }

        public virtual void Setup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = characterEntity;
        }

        public virtual void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            PlayerCharacterEntity = null;
            FollowingEntityTransform = null;
        }
    }
}
