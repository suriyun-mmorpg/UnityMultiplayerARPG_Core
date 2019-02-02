using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController : BasePlayerCharacterController
    {
        public float mouseXSensitivity = 5f;
        public float targetRaycastDistance = 100f;
        public FollowCameraControls gameplayCameraPrefab;
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        RaycastHit[] tempHitInfos;
        DamageableEntity tempEntity;
        DamageableEntity foundEntity;
        Quaternion updatingCharacterRotation;

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            currentBuildingEntity = null;

            if (gameplayCameraPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheGameplayCameraControls = Instantiate(gameplayCameraPrefab);
            }
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);

            if (characterEntity == null)
                return;

            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = characterEntity.CacheTransform;

            updatingCharacterRotation = characterEntity.CacheTransform.rotation;
        }

        protected override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);

            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = null;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (CacheGameplayCameraControls != null)
                Destroy(CacheGameplayCameraControls.gameObject);
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();

            // Lock cursor when not show UIs
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            CacheGameplayCameraControls.updateRotation = false;
            CacheGameplayCameraControls.updateRotationY = true;
            CacheGameplayCameraControls.useTargetYRotation = true;

            // Find target character
            Ray ray = CacheGameplayCameraControls.CacheCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            foundEntity = null;
            tempHitInfos = Physics.RaycastAll(ray, targetRaycastDistance);
            foreach (RaycastHit hitInfo in tempHitInfos)
            {
                tempEntity = hitInfo.collider.GetComponent<DamageableEntity>();
                if (tempEntity != PlayerCharacterEntity)
                {
                    foundEntity = tempEntity;
                    break;
                }
            }
            // Set aim target at server
            PlayerCharacterEntity.RequestUpdateAimDirection(ray.direction);
            float yRot = InputManager.GetAxis("Mouse X", false) * mouseXSensitivity;
            updatingCharacterRotation *= Quaternion.Euler(0f, yRot, 0f);

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            Vector3 moveDirection = GetMoveDirection(InputManager.GetAxis("Horizontal", raw), InputManager.GetAxis("Vertical", raw));

            // normalize input if it exceeds 1 in combined length:
            if (moveDirection.sqrMagnitude > 1)
                moveDirection.Normalize();

            // Hide Npc UIs when move
            if (moveDirection.magnitude != 0f)
            {
                if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                    CacheUISceneGameplay.uiNpcDialog.Hide();
            }

            // Move
            if (moveDirection.magnitude != 0f)
            {
                PlayerCharacterEntity.StopMove();
                PlayerCharacterEntity.SetTargetEntity(null);
            }

            PlayerCharacterEntity.KeyMovement(moveDirection, InputManager.GetButtonDown("Jump"));
            PlayerCharacterEntity.UpdateYRotation(updatingCharacterRotation.eulerAngles.y);

            // Show target hp/mp
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetTargetEntity(foundEntity);
        }

        public Vector3 GetMoveDirection(float horizontalInput, float verticalInput)
        {
            Vector3 moveDirection = Vector3.zero;
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            moveDirection += forward * verticalInput;
            moveDirection += right * horizontalInput;
            return moveDirection;
        }

        public override void UseHotkey(int hotkeyIndex)
        {

        }
    }
}
