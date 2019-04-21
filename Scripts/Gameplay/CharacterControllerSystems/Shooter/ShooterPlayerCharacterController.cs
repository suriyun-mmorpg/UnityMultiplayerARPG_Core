using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController : BasePlayerCharacterController
    {
        public const int RAYCAST_COLLIDER_SIZE = 32;
        public const int OVERLAP_COLLIDER_SIZE = 32;

        public enum Mode
        {
            Adventure,
            Combat,
        }

        public enum TurningState
        {
            None,
            Attack,
            Activate,
            UseSkill,
        }

        public Mode mode;
        public float angularSpeed = 800f;
        [Range(0, 1f)]
        public float turnToTargetDuration = 0.1f;
        public FollowCameraControls gameplayCameraPrefab;
        public bool showConfirmConstructionUI;
        public RectTransform crosshairRect;
        public Image zoomCrosshairImage;
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        bool isBlockController;
        BuildingMaterial tempBuildingMaterial;
        BaseGameEntity tempEntity;
        Vector3 moveLookDirection;
        Vector3 targetLookDirection;
        Quaternion tempLookAt;
        TurningState turningState;
        float turnTimeCounter;
        float tempCalculateAngle;
        bool tempPressAttack;
        bool tempPressActivate;
        bool tempPressPickupItem;
        GameObject tempGameObject;
        BasePlayerCharacterEntity targetPlayer;
        NpcEntity targetNpc;
        BuildingEntity targetBuilding;
        RaycastHit[] raycasts = new RaycastHit[RAYCAST_COLLIDER_SIZE];
        Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        RaycastHit tempHitInfo;
        Skill queueSkill;
        Skill usingSkill;
        Vector3 aimPosition;
        Vector3 actionLookDirection;
        Vector2 currentCrosshairSize;
        CrosshairSetting currentCrosshairSetting;
        bool isAttacking;
        MovementFlag movementState;

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;
            zoomCrosshairImage.preserveAspect = true;

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

            tempLookAt = characterEntity.CacheTransform.rotation;
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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();
            UpdateLookAtTarget();
            turnTimeCounter += Time.deltaTime;

            // Hide construction UI
            if (CurrentBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiConstructBuilding.IsVisible())
                    CacheUISceneGameplay.uiConstructBuilding.Hide();
            }
            if (ActiveBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiCurrentBuilding.IsVisible())
                    CacheUISceneGameplay.uiCurrentBuilding.Hide();
            }

            isBlockController = CacheUISceneGameplay.IsBlockController();
            // Lock cursor when not show UIs
            Cursor.lockState = !isBlockController ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isBlockController;

            CacheGameplayCameraControls.updateRotation = !isBlockController;
            // Clear selected entity
            SelectedEntity = null;

            // Update crosshair (with states from last update)
            currentCrosshairSetting = GetCrosshairSetting();
            if (isAttacking)
            {
                UpdateCrosshair(currentCrosshairSetting, currentCrosshairSetting.spreadPowerWhileAttacking);
            }
            else if (movementState.HasFlag(MovementFlag.Forward) ||
                movementState.HasFlag(MovementFlag.Backward) ||
                movementState.HasFlag(MovementFlag.Left) ||
                movementState.HasFlag(MovementFlag.Right) ||
                movementState.HasFlag(MovementFlag.IsJump))
            {
                UpdateCrosshair(currentCrosshairSetting, currentCrosshairSetting.spreadPowerWhileMoving);
            }
            else
            {
                UpdateCrosshair(currentCrosshairSetting, -currentCrosshairSetting.spreadDecreasePower);
            }

            // Clear state from last update
            isAttacking = false;
            movementState = MovementFlag.None;

            if (isBlockController || GenericUtils.IsFocusInputField())
            {
                PlayerCharacterEntity.KeyMovement(Vector3.zero, MovementFlag.None);
                return;
            }

            // Find target character
            Ray ray = CacheGameplayCameraControls.CacheCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 forward = CacheGameplayCameraControls.CacheCameraTransform.forward;
            Vector3 right = CacheGameplayCameraControls.CacheCameraTransform.right;
            float distanceFromOrigin = Vector3.Distance(ray.origin, PlayerCharacterEntity.CacheTransform.position);
            float aimDistance = distanceFromOrigin;
            // Calculate aim distance by skill or weapon
            if (queueSkill != null && queueSkill.IsAttack())
                aimDistance += PlayerCharacterEntity.GetSkillAttackDistance(queueSkill);
            else
                aimDistance += PlayerCharacterEntity.GetAttackDistance();
            actionLookDirection = aimPosition = ray.origin + ray.direction * aimDistance;
            actionLookDirection.y = PlayerCharacterEntity.CacheTransform.position.y;
            actionLookDirection = actionLookDirection - PlayerCharacterEntity.CacheTransform.position;
            actionLookDirection.Normalize();
            // Prepare variables to find nearest raycasted hit point
            float tempDistance;
            float tempNearestDistance = float.MaxValue;
            // Find for enemy character
            if (CurrentBuildingEntity == null)
            {
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, aimDistance);
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];

                    tempEntity = tempHitInfo.collider.GetComponent<BaseGameEntity>();
                    // Find building entity from building material
                    if (tempEntity == null)
                    {
                        tempBuildingMaterial = tempHitInfo.collider.GetComponent<BuildingMaterial>();
                        if (tempBuildingMaterial != null)
                            tempEntity = tempBuildingMaterial.buildingEntity;
                    }
                    if (tempEntity == null || tempEntity == PlayerCharacterEntity)
                        continue;
                    // Set aim position and found target
                    tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                    if (tempDistance < tempNearestDistance)
                    {
                        tempNearestDistance = tempDistance;
                        aimPosition = tempHitInfo.point;
                        if (tempEntity != null)
                            SelectedEntity = tempEntity;
                    }
                }
                // Show target hp/mp
                CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            }
            else
            {
                // Clear area before next find
                CurrentBuildingEntity.buildingArea = null;
                // Find for position to construction building
                bool foundSnapBuildPosition = false;
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, gameInstance.buildDistance);
                BuildingArea buildingArea = null;
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];
                    tempEntity = tempHitInfo.collider.GetComponentInParent<BuildingEntity>();
                    if (tempEntity == null || tempEntity == CurrentBuildingEntity)
                        continue;

                    buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                    if (buildingArea == null || 
                        (buildingArea.buildingEntity != null && buildingArea.buildingEntity == CurrentBuildingEntity) ||
                        !CurrentBuildingEntity.buildingType.Equals(buildingArea.buildingType))
                        continue;

                    // Set aim position
                    tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                    if (tempDistance < tempNearestDistance)
                    {
                        aimPosition = tempHitInfo.point;
                        CurrentBuildingEntity.buildingArea = buildingArea;
                        if (buildingArea.snapBuildingObject)
                        {
                            foundSnapBuildPosition = true;
                            break;
                        }
                    }
                }
                // Update building position
                if (!foundSnapBuildPosition)
                {
                    CurrentBuildingEntity.CacheTransform.position = aimPosition;
                    // Rotate to camera
                    Vector3 direction = (aimPosition - CacheGameplayCameraControls.CacheCameraTransform.position).normalized;
                    direction.y = 0;
                    CurrentBuildingEntity.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            Vector3 moveDirection = Vector3.zero;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            float inputV = InputManager.GetAxis("Vertical", raw);
            float inputH = InputManager.GetAxis("Horizontal", raw);
            moveDirection += forward * inputV;
            moveDirection += right * inputH;
            // Set movement state by inputs
            switch (mode)
            {
                case Mode.Adventure:
                    if (inputV > 0.5f || inputV < -0.5f || inputH > 0.5f || inputH < -0.5f)
                        movementState = MovementFlag.Forward;
                    moveLookDirection = moveDirection;
                    break;
                case Mode.Combat:
                    moveDirection += forward * inputV;
                    moveDirection += right * inputH;
                    if (inputV > 0.5f)
                        movementState |= MovementFlag.Forward;
                    else if (inputV < -0.5f)
                        movementState |= MovementFlag.Backward;
                    if (inputH > 0.5f)
                        movementState |= MovementFlag.Right;
                    else if (inputH < -0.5f)
                        movementState |= MovementFlag.Left;
                    moveLookDirection = actionLookDirection;
                    break;
            }

            // normalize input if it exceeds 1 in combined length:
            if (moveDirection.sqrMagnitude > 1)
                moveDirection.Normalize();

            if (CurrentBuildingEntity != null)
            {
                // Building
                tempPressAttack = InputManager.GetButtonUp("Fire1");
                if (tempPressAttack)
                {
                    if (showConfirmConstructionUI)
                    {
                        // Show confirm UI
                        if (!CacheUISceneGameplay.uiConstructBuilding.IsVisible())
                            CacheUISceneGameplay.uiConstructBuilding.Show();
                    }
                    else
                    {
                        // Build when click
                        ConfirmBuild();
                    }
                }
                else
                {
                    // Update move direction
                    if (moveDirection.magnitude != 0f)
                        targetLookDirection = moveLookDirection;
                }
            }
            else
            {
                // Not building so it is attacking
                tempPressAttack = InputManager.GetButton("Fire1");
                tempPressActivate = InputManager.GetButtonUp("Activate");
                tempPressPickupItem = InputManager.GetButtonUp("PickUpItem");
                if (queueSkill != null || tempPressAttack || tempPressActivate || PlayerCharacterEntity.IsPlayingActionAnimation())
                {
                    // Find forward character / npc / building / warp entity from camera center
                    targetPlayer = null;
                    targetNpc = null;
                    targetBuilding = null;
                    if (tempPressActivate && !tempPressAttack)
                    {
                        if (SelectedEntity is BasePlayerCharacterEntity)
                            targetPlayer = SelectedEntity as BasePlayerCharacterEntity;
                        if (SelectedEntity is NpcEntity)
                            targetNpc = SelectedEntity as NpcEntity;
                        if (SelectedEntity is BuildingEntity)
                            targetBuilding = SelectedEntity as BuildingEntity;
                    }
                    // While attacking turn to camera forward
                    tempCalculateAngle = Vector3.Angle(PlayerCharacterEntity.CacheTransform.forward, actionLookDirection);
                    if (tempCalculateAngle > 15f)
                    {
                        if (queueSkill != null && queueSkill.IsAttack())
                            turningState = TurningState.UseSkill;
                        else if (tempPressAttack)
                            turningState = TurningState.Attack;
                        else if (tempPressActivate)
                            turningState = TurningState.Activate;
                        turnTimeCounter = ((180f - tempCalculateAngle) / 180f) * turnToTargetDuration;
                        targetLookDirection = actionLookDirection;
                        // Set movement state by inputs
                        if (inputV > 0.5f)
                            movementState |= MovementFlag.Forward;
                        else if (inputV < -0.5f)
                            movementState |= MovementFlag.Backward;
                        if (inputH > 0.5f)
                            movementState |= MovementFlag.Right;
                        else if (inputH < -0.5f)
                            movementState |= MovementFlag.Left;
                    }
                    else
                    {
                        // Attack immediately if character already look at target
                        if (queueSkill != null && queueSkill.IsAttack())
                        {
                            UseSkill(aimPosition);
                            isAttacking = true;
                        }
                        else if (tempPressAttack)
                        {
                            Attack();
                            isAttacking = true;
                        }
                        else if (tempPressActivate)
                        {
                            Activate();
                        }
                    }
                    // If skill is not attack skill, use it immediately
                    if (queueSkill != null && !queueSkill.IsAttack())
                        UseSkill();
                    queueSkill = null;
                }
                else if (tempPressPickupItem)
                {
                    // Find for item to pick up
                    if (SelectedEntity != null)
                        PlayerCharacterEntity.RequestPickupItem((SelectedEntity as ItemDropEntity).ObjectId);
                }
                else
                {
                    // Update move direction
                    if (moveDirection.magnitude != 0f)
                        targetLookDirection = moveLookDirection;
                }
            }
            SetAimPosition(aimPosition);

            // Hide Npc UIs when move
            if (moveDirection.magnitude != 0f)
            {
                HideNpcDialogs();
                PlayerCharacterEntity.StopMove();
                PlayerCharacterEntity.SetTargetEntity(null);
            }
            // If jumping add jump state
            if (InputManager.GetButtonDown("Jump"))
                movementState |= MovementFlag.IsJump;

            PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
        }

        private void UpdateCrosshair(CrosshairSetting setting, float power)
        {
            currentCrosshairSize = crosshairRect.sizeDelta;
            // Change crosshair size by power
            currentCrosshairSize.x += power;
            currentCrosshairSize.y += power;
            // Set crosshair size
            crosshairRect.sizeDelta = new Vector2(Mathf.Clamp(currentCrosshairSize.x, setting.minSpread, setting.maxSpread), Mathf.Clamp(currentCrosshairSize.y, setting.minSpread, setting.maxSpread));
        }

        private CrosshairSetting GetCrosshairSetting()
        {
            return PlayerCharacterEntity.GetCrosshairSetting();
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

        protected void UpdateLookAtTarget()
        {
            tempCalculateAngle = Vector3.Angle(tempLookAt * Vector3.forward, targetLookDirection);
            if (turningState != TurningState.None)
            {
                if (tempCalculateAngle > 0)
                {
                    // Update rotation when angle difference more than 0
                    tempLookAt = Quaternion.Slerp(tempLookAt, Quaternion.LookRotation(targetLookDirection), turnTimeCounter / turnToTargetDuration);
                    PlayerCharacterEntity.UpdateYRotation(tempLookAt.eulerAngles.y);
                }
                else
                {
                    switch (turningState)
                    {
                        case TurningState.Attack:
                            Attack();
                            break;
                        case TurningState.Activate:
                            Activate();
                            break;
                        case TurningState.UseSkill:
                            UseSkill(aimPosition);
                            break;
                    }
                    turningState = TurningState.None;
                }
            }
            else
            {
                if (tempCalculateAngle > 0)
                {
                    // Update rotation when angle difference more than 0
                    tempLookAt = Quaternion.RotateTowards(tempLookAt, Quaternion.LookRotation(targetLookDirection), Time.deltaTime * angularSpeed);
                    PlayerCharacterEntity.UpdateYRotation(tempLookAt.eulerAngles.y);
                }
            }
        }

        public override void UseHotkey(int hotkeyIndex)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= PlayerCharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            Skill skill = hotkey.GetSkill();
            if (skill != null)
            {
                short skillLevel;
                if (PlayerCharacterEntity.CacheSkills.TryGetValue(skill, out skillLevel))
                {
                    if (skill.CanUse(PlayerCharacterEntity, skillLevel))
                    {
                        PlayerCharacterEntity.StopMove();
                        queueSkill = skill;
                    }
                }
            }
            Item item = hotkey.GetItem();
            if (item != null)
            {
                int itemIndex = PlayerCharacterEntity.IndexOfNonEquipItem(item.DataId);
                if (itemIndex >= 0)
                {
                    if (item.IsEquipment())
                        PlayerCharacterEntity.RequestEquipItem((short)itemIndex);
                    else if (item.IsPotion() || item.IsPet())
                        PlayerCharacterEntity.RequestUseItem((short)itemIndex);
                    else if (item.IsBuilding())
                    {
                        PlayerCharacterEntity.StopMove();
                        buildingItemIndex = itemIndex;
                        CurrentBuildingEntity = Instantiate(item.buildingEntity);
                        CurrentBuildingEntity.SetupAsBuildMode();
                        CurrentBuildingEntity.CacheTransform.parent = null;
                        // TODO: Build character by cursor position
                    }
                }
            }
        }

        public void SetAimPosition(Vector3 aimPosition)
        {
            PlayerCharacterEntity.RequestSetAimPosition(aimPosition);
        }

        public void Attack()
        {
            PlayerCharacterEntity.RequestAttack();
        }

        public void Activate()
        {
            // Priority Player -> Npc -> Buildings
            if (targetPlayer != null && CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
            else if (targetNpc != null)
                PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
            else if (targetBuilding != null)
                ActivateBuilding(targetBuilding);
        }

        public void UseSkill()
        {
            if (queueSkill == null)
                return;
            PlayerCharacterEntity.RequestUseSkill(queueSkill.DataId);
        }

        public void UseSkill(Vector3 aimPosition)
        {
            if (queueSkill == null)
                return;
            PlayerCharacterEntity.RequestUseSkill(queueSkill.DataId, aimPosition);
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(CharacterTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempGameObject = overlapColliders[tempCounter].gameObject;
                if (tempGameObject == target)
                    return true;
            }
            return false;
        }
    }
}
