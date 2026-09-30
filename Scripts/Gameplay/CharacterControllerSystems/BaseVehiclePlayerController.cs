using Insthync.CameraAndInput;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>
    /// Local seat controls driven exclusively by BasePlayerCharacterController.
    /// Subclasses implement input handling; vehicle movement/network authority stays on the vehicle.
    /// </summary>
    public abstract class BaseVehiclePlayerController : MonoBehaviour
    {
        [Tooltip("Optional panel, usually initially inactive. It must not contain this component or the player controller.")]
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private string exitVehicleButton = "ExitVehicle";
        [SerializeField] private string cameraRotateButton = "CameraRotate";

        public BasePlayerCharacterController PlayerController { get; private set; }
        public BasePlayerCharacterEntity Character { get; private set; }
        public IVehicleEntity Vehicle { get; private set; }
        public byte SeatIndex { get; private set; }
        public bool IsActive => PlayerController != null;
        public bool CanDrive => !Vehicle.IsNull() && Vehicle.IsDriver(SeatIndex) && Vehicle.Entity.IsOwnerClient;

        private bool _panelWasActive;
        private CursorLockMode _cursorLockMode;
        private bool _cursorVisible;
        private IGameplayCameraController _camera;
        private bool _rotation, _rotationX, _rotationY, _zoom;
        private bool _updatedControls;

        internal void Activate(BasePlayerCharacterController controller, BasePlayerCharacterEntity character,
            IVehicleEntity vehicle, byte seatIndex)
        {
            PlayerController = controller;
            Character = character;
            Vehicle = vehicle;
            SeatIndex = seatIndex;
            _cursorLockMode = Cursor.lockState;
            _cursorVisible = Cursor.visible;
            _camera = controller.GameplayCameraController;
            if (_camera != null)
            {
                _rotation = _camera.UpdateRotation;
                _rotationX = _camera.UpdateRotationX;
                _rotationY = _camera.UpdateRotationY;
                _zoom = _camera.UpdateZoom;
            }
            // A panel must not contain the controller itself: hiding it would disable controls.
            if (controlsPanel != null && !transform.IsChildOf(controlsPanel.transform))
            {
                _panelWasActive = controlsPanel.activeSelf;
                controlsPanel.SetActive(true);
            }
            OnActivated();
        }

        internal void Deactivate()
        {
            if (!IsActive)
                return;
            try
            {
                _updatedControls = false;
                ResetInput();
                OnDeactivated();
            }
            finally
            {
                if (_camera != null)
                {
                    _camera.UpdateRotation = _rotation;
                    _camera.UpdateRotationX = _rotationX;
                    _camera.UpdateRotationY = _rotationY;
                    _camera.UpdateZoom = _zoom;
                }
                Cursor.lockState = _cursorLockMode;
                Cursor.visible = _cursorVisible;
                PlayerController = null;
                Character = null;
                Vehicle = null;
                _camera = null;
                if (controlsPanel != null && !transform.IsChildOf(controlsPanel.transform))
                    controlsPanel.SetActive(_panelWasActive);
            }
        }

        internal void TickUpdate(float deltaTime)
        {
            _updatedControls = false;
            bool blocked = PlayerController.IsVehicleInputBlocked();
            UpdateCameraControls(blocked);
            if (blocked)
            {
                ResetInput();
                return;
            }
            if (!string.IsNullOrEmpty(exitVehicleButton) && InputManager.GetButtonDown(exitVehicleButton))
            {
                ResetInput();
                Character.CallCmdExitVehicle();
                return;
            }
            _updatedControls = true;
            UpdateControls(deltaTime);
        }

        protected virtual void UpdateCameraControls(bool blocked)
        {
            if (_camera != null)
            {
                _camera.UpdateRotationX = false;
                _camera.UpdateRotationY = false;
                _camera.UpdateRotation = !blocked && !string.IsNullOrEmpty(cameraRotateButton) && InputManager.GetButton(cameraRotateButton);
                _camera.UpdateZoom = !blocked;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        internal void TickLateUpdate(float deltaTime)
        {
            if (PlayerController.IsVehicleInputBlocked())
            {
                ResetInput();
                return;
            }
            if (!_updatedControls)
                return;
            _updatedControls = false;
            LateUpdateControls(deltaTime);
        }

        protected virtual void OnDisable()
        {
            if (IsActive)
                PlayerController.ReleaseVehicleController();
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsActive)
                ResetInput();
        }

        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }
        protected abstract void UpdateControls(float deltaTime);
        protected virtual void LateUpdateControls(float deltaTime) { }
        /// <summary>Release held controls, including throttle/boost, without controlling another seat's vehicle.</summary>
        protected abstract void ResetInput();
    }
}
