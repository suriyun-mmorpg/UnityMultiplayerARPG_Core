using Insthync.CameraAndInput;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Directional controls for vehicles using the existing entity movement system.</summary>
    public class DefaultVehiclePlayerController : BaseVehiclePlayerController
    {
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private string verticalAxis = "Vertical";
        [SerializeField] private bool cameraRelative = true;

        protected override void OnActivated()
        {
            ResetInput();
        }

        protected override void UpdateControls(float deltaTime)
        {
            if (!CanDrive)
                return;
            Vector3 direction = new Vector3(
                string.IsNullOrEmpty(horizontalAxis) ? 0f : InputManager.GetAxis(horizontalAxis, false), 0f,
                string.IsNullOrEmpty(verticalAxis) ? 0f : InputManager.GetAxis(verticalAxis, false));
            direction = Vector3.ClampMagnitude(direction, 1f);
            if (PlayerController.CurrentGameInstance.DimensionType == DimensionType.Dimension2D)
            {
                direction = new Vector3(direction.x, direction.z, 0f);
                Vehicle.KeyMovement(direction, direction.sqrMagnitude > 0f ? MovementState.Forward : MovementState.None);
                if (direction.sqrMagnitude > 0f)
                    Vehicle.Direction2D = (Vector2)direction;
                return;
            }
            if (cameraRelative && PlayerController.MainCameraTransform != null)
                direction = Quaternion.Euler(0f, PlayerController.MainCameraTransform.eulerAngles.y, 0f) * direction;
            Vehicle.KeyMovement(direction, direction.sqrMagnitude > 0f ? MovementState.Forward : MovementState.None);
            if (direction.sqrMagnitude > 0f)
                Vehicle.SetLookRotation(Quaternion.LookRotation(direction), false);
        }

        protected override void ResetInput()
        {
            if (!CanDrive)
                return;
            Vehicle.KeyMovement(Vector3.zero, MovementState.None);
            Vehicle.SetExtraMovementState(ExtraMovementState.None);
            Vehicle.StopMove();
        }
    }
}
