using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController : BasePlayerCharacterController
    {
        public const int RAYCAST_COLLIDER_SIZE = 32;
        public const int OVERLAP_COLLIDER_SIZE = 32;

        public enum ControllerMode
        {
            Adventure,
            Combat,
        }

        public enum ControllerViewMode
        {
            Tps,
            Fps,
        }

        public enum TurningState
        {
            None,
            Attack,
            Activate,
            UseSkill,
        }

        [SerializeField]
        private ControllerMode mode;
        [SerializeField]
        private ControllerViewMode viewMode;
        [SerializeField]
        private float angularSpeed = 800f;
        [Range(0, 1f)]
        [SerializeField]
        private float turnToTargetDuration = 0.1f;
        [SerializeField]
        private float findTargetRaycastDistance = 16f;
        [SerializeField]
        private bool showConfirmConstructionUI;
        [SerializeField]
        private RectTransform crosshairRect;

        [Header("TPS Settings")]
        [SerializeField]
        private float tpsZoomDistance = 3f;
        [SerializeField]
        private float tpsMinZoomDistance = 3f;
        [SerializeField]
        private float tpsMaxZoomDistance = 3f;
        [SerializeField]
        private Vector3 tpsTargetOffset = new Vector3(0.75f, 1.25f, 0f);
        [SerializeField]
        private float tpsFov = 60f;

        [Header("FPS Settings")]
        [SerializeField]
        private float fpsZoomDistance = 0f;
        [SerializeField]
        private Vector3 fpsTargetOffset = new Vector3(0f, 1.25f, 0f);
        [SerializeField]
        private float fpsFov = 60f;

        public bool IsBlockController { get; protected set; }

        public ControllerMode Mode
        {
            get
            {
                if (viewMode == ControllerViewMode.Fps)
                {
                    // If view mode is fps, controls type must be combat
                    return ControllerMode.Combat;
                }
                return mode;
            }
        }

        public ControllerViewMode ViewMode
        {
            get { return viewMode; }
            set { viewMode = value; }
        }

        public float CameraZoomDistance
        {
            get
            {
                if (ViewMode == ControllerViewMode.Tps)
                    return tpsZoomDistance;
                return fpsZoomDistance;
            }
        }

        public float CameraMinZoomDistance
        {
            get
            {
                if (ViewMode == ControllerViewMode.Tps)
                    return tpsMinZoomDistance;
                return fpsZoomDistance;
            }
        }

        public float CameraMaxZoomDistance
        {
            get
            {
                if (ViewMode == ControllerViewMode.Tps)
                    return tpsMaxZoomDistance;
                return fpsZoomDistance;
            }
        }

        public Vector3 CameraTargetOffset
        {
            get
            {
                if (ViewMode == ControllerViewMode.Tps)
                    return tpsTargetOffset;
                return fpsTargetOffset;
            }
        }

        public float CameraFov
        {
            get
            {
                if (ViewMode == ControllerViewMode.Tps)
                    return tpsFov;
                return fpsFov;
            }
        }

        // Temp data
        ControllerViewMode dirtyViewMode;
        BuildingMaterial tempBuildingMaterial;
        IDamageableEntity tempDamageableEntity;
        BaseGameEntity tempEntity;
        Ray centerRay;
        float centerRayToCharacterDist;
        Vector3 moveDirection;
        Vector3 cameraForward;
        Vector3 cameraRight;
        float inputV;
        float inputH;
        Vector3 moveLookDirection;
        Vector3 targetLookDirection;
        Quaternion tempLookAt;
        TurningState turningState;
        float tempDeltaTime;
        float calculatedTurnDuration;
        float tempCalculateAngle;
        bool tempPressAttackRight;
        bool tempPressAttackLeft;
        bool tempPressWeaponAbility;
        bool tempPressActivate;
        bool tempPressPickupItem;
        bool tempPressReload;
        bool tempPressExitVehicle;
        bool tempPressSwitchEquipWeaponSet;
        bool isLeftHandAttacking;
        GameObject tempGameObject;
        BasePlayerCharacterEntity targetPlayer;
        NpcEntity targetNpc;
        BuildingEntity targetBuilding;
        RaycastHit[] raycasts = new RaycastHit[RAYCAST_COLLIDER_SIZE];
        Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        RaycastHit tempHitInfo;
        Vector3 aimPosition;
        // Crosshair
        public Vector2 CurrentCrosshairSize { get; private set; }
        public CrosshairSetting CurrentCrosshairSetting { get; private set; }
        // Controlling states
        bool isDoingAction;
        bool mustReleaseFireKey;
        Item rightHandWeapon;
        Item leftHandWeapon;
        MovementState movementState;
        public BaseWeaponAbility WeaponAbility { get; private set; }
        public WeaponAbilityState WeaponAbilityState { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);

            if (characterEntity == null)
                return;

            tempLookAt = MovementTransform.rotation;

            SetupEquipWeapons(characterEntity.EquipWeapons);

            characterEntity.onEquipWeaponSetChange += SetupEquipWeapons;
            characterEntity.onSelectableWeaponSetsOperation += SetupEquipWeapons;
            characterEntity.ModelManager.InstantiateFpsModel(CacheGameplayCameraControls.CacheCameraTransform);
        }

        protected override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);

            if (characterEntity == null)
                return;

            characterEntity.onEquipWeaponSetChange -= SetupEquipWeapons;
            characterEntity.onSelectableWeaponSetsOperation -= SetupEquipWeapons;
        }

        protected void SetupEquipWeapons(byte equipWeaponSet)
        {
            SetupEquipWeapons(PlayerCharacterEntity.EquipWeapons);
        }

        protected void SetupEquipWeapons(LiteNetLibManager.LiteNetLibSyncList.Operation operation, int index)
        {
            SetupEquipWeapons(PlayerCharacterEntity.EquipWeapons);
        }

        protected void SetupEquipWeapons(EquipWeapons equipWeapons)
        {
            CurrentCrosshairSetting = PlayerCharacterEntity.GetCrosshairSetting();
            UpdateCrosshair(CurrentCrosshairSetting, -CurrentCrosshairSetting.shrinkPerFrame);

            rightHandWeapon = equipWeapons.GetRightHandWeaponItem();
            leftHandWeapon = equipWeapons.GetLeftHandWeaponItem();
            // Weapon ability will be able to use when equip weapon at main-hand only
            if (rightHandWeapon != null && leftHandWeapon == null)
            {
                if (rightHandWeapon.weaponAbility != WeaponAbility)
                {
                    if (WeaponAbility != null)
                        WeaponAbility.Desetup();
                    WeaponAbility = rightHandWeapon.weaponAbility;
                    if (WeaponAbility != null)
                        WeaponAbility.Setup(this, equipWeapons.rightHand);
                    WeaponAbilityState = WeaponAbilityState.Deactivated;
                }
            }
            else
            {
                if (WeaponAbility != null)
                    WeaponAbility.Desetup();
                WeaponAbility = null;
                WeaponAbilityState = WeaponAbilityState.Deactivated;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();
            if (dirtyViewMode != viewMode)
            {
                dirtyViewMode = viewMode;
                UpdateCameraSettings();
            }

            tempDeltaTime = Time.deltaTime;
            calculatedTurnDuration += tempDeltaTime;

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

            IsBlockController = CacheUISceneGameplay.IsBlockController();
            // Lock cursor when not show UIs

            if (InputManager.useMobileInputOnNonMobile || Application.isMobilePlatform)
            {
                // Control camera by touch-screen
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CacheGameplayCameraControls.updateRotationX = false;
                CacheGameplayCameraControls.updateRotationY = false;
                CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");
            }
            else
            {
                // Control camera by mouse-move
                Cursor.lockState = !IsBlockController ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = IsBlockController;
                CacheGameplayCameraControls.updateRotation = !IsBlockController;
            }
            // Clear selected entity
            SelectedEntity = null;

            // Update crosshair (with states from last update)
            UpdateCrosshair();

            // Clear controlling states from last update
            isDoingAction = false;
            movementState = MovementState.None;
            UpdateActivatedWeaponAbility(tempDeltaTime);

            if (IsBlockController || GenericUtils.IsFocusInputField())
            {
                mustReleaseFireKey = false;
                PlayerCharacterEntity.KeyMovement(Vector3.zero, MovementState.None);
                DeactivateWeaponAbility();
                return;
            }

            // Prepare variables to find nearest raycasted hit point
            centerRay = CacheGameplayCameraControls.CacheCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            centerRayToCharacterDist = Vector3.Distance(centerRay.origin, MovementTransform.position);
            cameraForward = CacheGameplayCameraControls.CacheCameraTransform.forward;
            cameraRight = CacheGameplayCameraControls.CacheCameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            if (CurrentBuildingEntity != null)
                UpdateTarget_BuildingMode();
            else
                UpdateTarget_BattleMode();

            UpdateMovementInputs();

            if (CurrentBuildingEntity != null)
                UpdateInputs_BuildingMode();
            else
                UpdateInputs_BattleMode();

            // Hide Npc UIs when move
            if (moveDirection.magnitude != 0f)
            {
                HideNpcDialogs();
                PlayerCharacterEntity.StopMove();
            }

            // If jumping add jump state
            if (InputManager.GetButtonDown("Jump"))
                movementState |= MovementState.IsJump;

            // If sprinting add is sprinting state
            if (InputManager.GetButton("Sprint"))
                movementState |= MovementState.IsSprinting;

            PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
            UpdateLookAtTarget();
        }

        private void UpdateTarget_BuildingMode()
        {
            // Clear area before next find
            CurrentBuildingEntity.buildingArea = null;
            // Default aim position (aim to sky/space)
            aimPosition = centerRay.origin + centerRay.direction * (centerRayToCharacterDist + gameInstance.buildDistance);
            // Raycast from camera position to center of screen
            int tempCount = PhysicUtils.SortedRaycastNonAlloc3D(centerRay.origin, centerRay.direction, raycasts, findTargetRaycastDistance, Physics.AllLayers);
            float tempDistance;
            BuildingArea buildingArea = null;
            bool hasSnapBuildPosition = false;
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = raycasts[tempCounter];
                tempEntity = tempHitInfo.collider.GetComponentInParent<BuildingEntity>();
                if (tempEntity != null && tempEntity == CurrentBuildingEntity)
                {
                    // Skip because it's raycast to the building that you are going to build
                    continue;
                }

                // Set aim position
                tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                if (IsInFront(tempHitInfo.point))
                {
                    aimPosition = tempHitInfo.point;
                    buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                    if (buildingArea == null ||
                        (buildingArea.buildingEntity != null && buildingArea.buildingEntity == CurrentBuildingEntity) ||
                        !CurrentBuildingEntity.buildingTypes.Contains(buildingArea.buildingType))
                    {
                        // Skip because this area is not allowed to build the building that you are going to build
                        continue;
                    }

                    CurrentBuildingEntity.buildingArea = buildingArea;
                    if (buildingArea.snapBuildingObject)
                    {
                        hasSnapBuildPosition = true;
                        break;
                    }
                }
            }

            if (Vector3.Distance(PlayerCharacterEntity.CacheTransform.position, aimPosition) > gameInstance.buildDistance)
            {
                // Mark as unable to build when the building is far from character
                CurrentBuildingEntity.buildingArea = null;
            }

            if (!hasSnapBuildPosition)
            {
                // There is no snap build position, set building rotation by camera look direction
                CurrentBuildingEntity.CacheTransform.position = aimPosition;
                // Rotate to camera
                Vector3 direction = (aimPosition - CacheGameplayCameraControls.CacheCameraTransform.position).normalized;
                direction.y = 0;
                CurrentBuildingEntity.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void UpdateTarget_BattleMode()
        {
            // Prepare raycast distance / fov
            float attackDistance = 0f;
            float attackFov = 90f;
            // Calculating aim distance, also read attack inputs here
            // Attack inputs will be used to calculate attack distance
            if (CurrentBuildingEntity == null)
            {
                // Attack with right hand weapon
                tempPressAttackRight = GetPrimaryAttackButton();
                if (WeaponAbility == null && leftHandWeapon != null)
                {
                    // Attack with left hand weapon if left hand weapon not empty
                    tempPressAttackLeft = GetSecondaryAttackButton();
                }
                else if (WeaponAbility != null)
                {
                    // Use weapon ability if it can
                    tempPressWeaponAbility = GetSecondaryAttackButtonDown();
                }

                if ((tempPressAttackRight || tempPressAttackLeft) &&
                    turningState == TurningState.None)
                {
                    // So priority is right > left
                    isLeftHandAttacking = !tempPressAttackRight && tempPressAttackLeft;
                }

                // Calculate aim distance by skill or weapon
                if (queueUsingSkill.skill != null && queueUsingSkill.skill.IsAttack())
                {
                    // Increase aim distance by skill attack distance
                    attackDistance = queueUsingSkill.skill.GetCastDistance(PlayerCharacterEntity, queueUsingSkill.level, isLeftHandAttacking);
                    attackFov = queueUsingSkill.skill.GetCastFov(PlayerCharacterEntity, queueUsingSkill.level, isLeftHandAttacking);
                }
                else
                {
                    // Increase aim distance by attack distance
                    attackDistance = PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking);
                    attackFov = PlayerCharacterEntity.GetAttackFov(isLeftHandAttacking);
                }
            }
            // Default aim position (aim to sky/space)
            aimPosition = centerRay.origin + centerRay.direction * (centerRayToCharacterDist + attackDistance);
            // Raycast from camera position to center of screen
            int tempCount = PhysicUtils.SortedRaycastNonAlloc3D(centerRay.origin, centerRay.direction, raycasts, findTargetRaycastDistance, Physics.AllLayers);
            float tempDistance;
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = raycasts[tempCounter];

                // Get distance between character and raycast hit point
                tempDistance = Vector3.Distance(MovementTransform.position, tempHitInfo.point);
                // If this is damageable entity
                tempDamageableEntity = tempHitInfo.collider.GetComponent<IDamageableEntity>();
                if (tempDamageableEntity != null)
                {
                    tempEntity = tempDamageableEntity.Entity;

                    // Target must be damageable, not player character entity, within aim distance and alive
                    if (tempDamageableEntity.ObjectId == PlayerCharacterEntity.ObjectId)
                        continue;

                    // Target must not hidding
                    if (tempDamageableEntity.Entity is BaseCharacterEntity &&
                        (tempDamageableEntity.Entity as BaseCharacterEntity).GetCaches().IsHide)
                        continue;
                    
                    // Entity is in front of character, so this is target
                    if (IsInFront(tempHitInfo.point))
                    {
                        aimPosition = tempHitInfo.point;
                        SelectedEntity = tempEntity;
                        break;
                    }
                }
                // Find item drop entity
                tempEntity = tempHitInfo.collider.GetComponent<ItemDropEntity>();
                if (tempEntity != null && tempDistance <= gameInstance.pickUpItemDistance)
                {
                    // Entity is in front of character, so this is target
                    if (IsInFront(tempHitInfo.point))
                    {
                        SelectedEntity = tempEntity;
                        break;
                    }
                }
                // Find activatable entity (NPC/Building/Mount/Etc)
                tempEntity = tempHitInfo.collider.GetComponent<BaseGameEntity>();
                if (tempEntity != null && tempDistance <= gameInstance.conversationDistance)
                {
                    // Entity is in front of character, so this is target
                    if (IsInFront(tempHitInfo.point))
                    {
                        SelectedEntity = tempEntity;
                        break;
                    }
                }
            }
            // Show target hp/mp
            CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            PlayerCharacterEntity.SetTargetEntity(SelectedEntity);
        }

        private void UpdateMovementInputs()
        {
            PlayerCharacterEntity.Pitch = CacheGameplayCameraControls.CacheCameraTransform.eulerAngles.x;

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            moveDirection = Vector3.zero;
            inputV = InputManager.GetAxis("Vertical", raw);
            inputH = InputManager.GetAxis("Horizontal", raw);
            moveDirection += cameraForward * inputV;
            moveDirection += cameraRight * inputH;
            // Set movement state by inputs
            switch (Mode)
            {
                case ControllerMode.Adventure:
                    if (inputV > 0.5f || inputV < -0.5f || inputH > 0.5f || inputH < -0.5f)
                        movementState = MovementState.Forward;
                    moveLookDirection = moveDirection;
                    break;
                case ControllerMode.Combat:
                    if (inputV > 0.5f)
                        movementState |= MovementState.Forward;
                    else if (inputV < -0.5f)
                        movementState |= MovementState.Backward;
                    if (inputH > 0.5f)
                        movementState |= MovementState.Right;
                    else if (inputH < -0.5f)
                        movementState |= MovementState.Left;
                    moveLookDirection = cameraForward;
                    break;
            }

            if (ViewMode == ControllerViewMode.Fps)
            {
                // Force turn to look direction
                moveLookDirection = cameraForward;
                targetLookDirection = cameraForward;
            }

            // normalize input if it exceeds 1 in combined length:
            if (moveDirection.sqrMagnitude > 1)
                moveDirection.Normalize();
        }

        private void UpdateInputs_BuildingMode()
        {
            mustReleaseFireKey = false;
            // Building
            tempPressAttackRight = GetPrimaryAttackButtonUp();
            if (tempPressAttackRight)
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
                if (moveDirection.magnitude != 0f && ViewMode == ControllerViewMode.Tps)
                    targetLookDirection = moveLookDirection;
            }
        }

        private void UpdateInputs_BattleMode()
        {
            // Have to release fire key, then check press fire key later on next frame
            if (mustReleaseFireKey)
            {
                tempPressAttackRight = false;
                tempPressAttackLeft = false;
                if (!isLeftHandAttacking &&
                    (GetPrimaryAttackButtonUp() ||
                    !GetPrimaryAttackButton()))
                    mustReleaseFireKey = false;
                if (isLeftHandAttacking &&
                    (GetSecondaryAttackButtonUp() ||
                    !GetSecondaryAttackButton()))
                    mustReleaseFireKey = false;
            }

            tempPressActivate = InputManager.GetButtonDown("Activate");
            tempPressPickupItem = InputManager.GetButtonDown("PickUpItem");
            tempPressReload = InputManager.GetButtonDown("Reload");
            tempPressExitVehicle = InputManager.GetButtonDown("ExitVehicle");
            tempPressSwitchEquipWeaponSet = InputManager.GetButtonDown("SwitchEquipWeaponSet");
            if (queueUsingSkill.skill != null || 
                tempPressAttackRight || 
                tempPressAttackLeft || 
                tempPressActivate || 
                PlayerCharacterEntity.IsPlayingActionAnimation())
            {
                // Find forward character / npc / building / warp entity from camera center
                targetPlayer = null;
                targetNpc = null;
                targetBuilding = null;
                if (tempPressActivate && !tempPressAttackRight && !tempPressAttackLeft)
                {
                    if (SelectedEntity is BasePlayerCharacterEntity)
                        targetPlayer = SelectedEntity as BasePlayerCharacterEntity;
                    if (SelectedEntity is NpcEntity)
                        targetNpc = SelectedEntity as NpcEntity;
                    if (SelectedEntity is BuildingEntity)
                        targetBuilding = SelectedEntity as BuildingEntity;
                }
                // While attacking turn character to camera forward
                tempCalculateAngle = Vector3.Angle(MovementTransform.forward, cameraForward);

                if (PlayerCharacterEntity.IsPlayingActionAnimation())
                {
                    // Just look at camera forward while character playing action animation
                    targetLookDirection = cameraForward;
                }
                else if (tempCalculateAngle > 15f && ViewMode == ControllerViewMode.Tps)
                {
                    // Fps mode character always turn to camera forward.
                    // So set turning state for Tps view mode only
                    if (queueUsingSkill.skill != null && queueUsingSkill.skill.IsAttack())
                    {
                        turningState = TurningState.UseSkill;
                    }
                    else if (tempPressAttackRight || tempPressAttackLeft)
                    {
                        turningState = TurningState.Attack;
                    }
                    else if (tempPressActivate)
                    {
                        turningState = TurningState.Activate;
                    }
                    // Calculate turn duration to smoothing character rotation in `UpdateLookAtTarget()`
                    calculatedTurnDuration = (180f - tempCalculateAngle) / 180f * turnToTargetDuration;
                    targetLookDirection = cameraForward;
                    // Set movement state by inputs
                    if (inputV > 0.5f)
                        movementState |= MovementState.Forward;
                    else if (inputV < -0.5f)
                        movementState |= MovementState.Backward;
                    if (inputH > 0.5f)
                        movementState |= MovementState.Right;
                    else if (inputH < -0.5f)
                        movementState |= MovementState.Left;
                }
                else
                {
                    // Attack immediately if character already look at target
                    if (queueUsingSkill.skill != null && queueUsingSkill.skill.IsAttack())
                    {
                        UseSkill(isLeftHandAttacking, aimPosition);
                        isDoingAction = true;
                    }
                    else if (tempPressAttackRight || tempPressAttackLeft)
                    {
                        Attack(isLeftHandAttacking, aimPosition);
                        isDoingAction = true;
                    }
                    else if (tempPressActivate)
                    {
                        Activate();
                    }
                }

                // If skill is not attack skill, use it immediately
                if (queueUsingSkill.skill != null && !queueUsingSkill.skill.IsAttack())
                {
                    UseSkill(isLeftHandAttacking, aimPosition);
                }
            }
            else if (tempPressWeaponAbility)
            {
                switch (WeaponAbilityState)
                {
                    case WeaponAbilityState.Activated:
                    case WeaponAbilityState.Activating:
                        DeactivateWeaponAbility();
                        break;
                    case WeaponAbilityState.Deactivated:
                    case WeaponAbilityState.Deactivating:
                        ActivateWeaponAbility();
                        break;
                }
            }
            else if (tempPressPickupItem)
            {
                // Find for item to pick up
                if (SelectedEntity != null && SelectedEntity is ItemDropEntity)
                    PlayerCharacterEntity.RequestPickupItem((SelectedEntity as ItemDropEntity).ObjectId);
            }
            else if (tempPressReload)
            {
                // Reload ammo when press the button
                ReloadAmmo();
            }
            else if (tempPressExitVehicle)
            {
                // Exit vehicle
                PlayerCharacterEntity.RequestExitVehicle();
            }
            else if (tempPressSwitchEquipWeaponSet)
            {
                // Switch equip weapon set
                PlayerCharacterEntity.RequestSwitchEquipWeaponSet((byte)(PlayerCharacterEntity.EquipWeaponSet + 1));
            }
            else
            {
                // Update move direction
                if (moveDirection.magnitude != 0f && ViewMode == ControllerViewMode.Tps)
                    targetLookDirection = moveLookDirection;
            }

            // Setup releasing state
            if (tempPressAttackRight && rightHandWeapon != null && rightHandWeapon.fireType == FireType.SingleFire)
            {
                // The weapon's fire mode is single fire, so player have to release fire key for next fire
                mustReleaseFireKey = true;
            }
            else if (tempPressAttackLeft && leftHandWeapon != null && leftHandWeapon.fireType == FireType.SingleFire)
            {
                // The weapon's fire mode is single fire, so player have to release fire key for next fire
                mustReleaseFireKey = true;
            }

            // Auto reload
            if (!tempPressAttackRight && !tempPressAttackLeft && !tempPressReload &&
                (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty()))
            {
                // Reload ammo when empty and not press any keys
                ReloadAmmo();
            }
        }

        private void ReloadAmmo()
        {
            // Reload ammo at server
            if (!PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(false);
            else if (!PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(true);
        }

        private void UpdateCrosshair()
        {
            if (isDoingAction)
            {
                UpdateCrosshair(CurrentCrosshairSetting, CurrentCrosshairSetting.expandPerFrameWhileAttacking);
            }
            else if (movementState.HasFlag(MovementState.Forward) ||
                movementState.HasFlag(MovementState.Backward) ||
                movementState.HasFlag(MovementState.Left) ||
                movementState.HasFlag(MovementState.Right) ||
                movementState.HasFlag(MovementState.IsJump))
            {
                UpdateCrosshair(CurrentCrosshairSetting, CurrentCrosshairSetting.expandPerFrameWhileMoving);
            }
            else
            {
                UpdateCrosshair(CurrentCrosshairSetting, -CurrentCrosshairSetting.shrinkPerFrame);
            }
        }

        private void UpdateCrosshair(CrosshairSetting setting, float power)
        {
            if (crosshairRect == null)
                return;

            crosshairRect.gameObject.SetActive(!setting.hidden);
            // Change crosshair size by power
            Vector3 sizeDelta = crosshairRect.sizeDelta;
            sizeDelta.x += power;
            sizeDelta.y += power;
            CurrentCrosshairSize = sizeDelta;
            // Set crosshair size
            crosshairRect.sizeDelta = new Vector2(Mathf.Clamp(CurrentCrosshairSize.x, setting.minSpread, setting.maxSpread), Mathf.Clamp(CurrentCrosshairSize.y, setting.minSpread, setting.maxSpread));
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
            if (ViewMode == ControllerViewMode.Tps)
            {
                if (PlayerCharacterEntity.IsPlayingActionAnimation())
                {
                    // Turn character to look direction immediately
                    // If character playing action animation
                    tempLookAt = Quaternion.LookRotation(targetLookDirection);
                    PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
                    return;
                }
                tempCalculateAngle = Vector3.Angle(tempLookAt * Vector3.forward, targetLookDirection);
                if (turningState != TurningState.None)
                {
                    if (tempCalculateAngle > 0)
                    {
                        // Update rotation when angle difference more than 0
                        tempLookAt = Quaternion.Slerp(tempLookAt, Quaternion.LookRotation(targetLookDirection), calculatedTurnDuration / turnToTargetDuration);
                        PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
                    }
                    else
                    {
                        // Update temp look at to character's rotation
                        tempLookAt = MovementTransform.rotation;
                        // Do actions
                        switch (turningState)
                        {
                            case TurningState.Attack:
                                Attack(isLeftHandAttacking, aimPosition);
                                break;
                            case TurningState.Activate:
                                Activate();
                                break;
                            case TurningState.UseSkill:
                                UseSkill(isLeftHandAttacking, aimPosition);
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
                        PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
                    }
                    else
                    {
                        // Update temp look at to character's rotation
                        tempLookAt = MovementTransform.rotation;
                    }
                }
            }
            else if (ViewMode == ControllerViewMode.Fps)
            {
                // Turn character to look direction immediately
                tempLookAt = Quaternion.LookRotation(targetLookDirection);
                PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
            }
        }

        public override void UseHotkey(int hotkeyIndex, Vector3? aimPosition)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= PlayerCharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;
            ClearQueueUsingSkill();

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            switch (hotkey.type)
            {
                case HotkeyType.Skill:
                    UseSkill(hotkey.relateId, aimPosition);
                    break;
                case HotkeyType.Item:
                    UseItem(hotkey.relateId, aimPosition);
                    break;
            }
        }

        protected void UseSkill(string id, Vector3? aimPosition)
        {
            BaseSkill skill = null;
            short skillLevel = 0;

            if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out skill) || skill == null ||
                !PlayerCharacterEntity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;

            PlayerCharacterEntity.StopMove();
            SetQueueUsingSkill(aimPosition, skill, skillLevel);
        }

        protected void UseItem(string id, Vector3? aimPosition)
        {
            InventoryType inventoryType;
            int itemIndex;
            byte equipWeaponSet;
            CharacterItem characterItem;
            if (PlayerCharacterEntity.IsEquipped(
                id,
                out inventoryType,
                out itemIndex,
                out equipWeaponSet,
                out characterItem))
            {
                PlayerCharacterEntity.RequestUnEquipItem(inventoryType, (short)itemIndex, equipWeaponSet);
                return;
            }

            if (itemIndex < 0)
                return;

            Item item = characterItem.GetItem();
            if (item == null)
                return;

            if (item.IsEquipment())
            {
                PlayerCharacterEntity.RequestEquipItem((short)itemIndex);
            }
            else if (item.IsUsable())
            {
                if (item.IsSkill())
                {
                    PlayerCharacterEntity.StopMove();
                    SetQueueUsingSkill(aimPosition, item.skillLevel.skill, item.skillLevel.level, (short)itemIndex);
                }
                else
                {
                    PlayerCharacterEntity.RequestUseItem((short)itemIndex);
                }
            }
            else if (item.IsBuilding())
            {
                PlayerCharacterEntity.StopMove();
                buildingItemIndex = itemIndex;
                CurrentBuildingEntity = Instantiate(item.buildingEntity);
                CurrentBuildingEntity.SetupAsBuildMode();
                CurrentBuildingEntity.CacheTransform.parent = null;
            }
        }

        public void Attack(bool isLeftHand, Vector3 aimPosition)
        {
            PlayerCharacterEntity.RequestAttack(isLeftHand, aimPosition);
        }

        public void ActivateWeaponAbility()
        {
            if (WeaponAbility == null)
                return;

            if (WeaponAbilityState == WeaponAbilityState.Activated ||
                WeaponAbilityState == WeaponAbilityState.Activating)
                return;

            WeaponAbility.OnPreActivate();
            WeaponAbilityState = WeaponAbilityState.Activating;
        }

        private void UpdateActivatedWeaponAbility(float deltaTime)
        {
            if (WeaponAbility == null)
                return;

            if (WeaponAbilityState == WeaponAbilityState.Activated ||
                WeaponAbilityState == WeaponAbilityState.Deactivated)
                return;

            WeaponAbilityState = WeaponAbility.UpdateActivation(WeaponAbilityState, deltaTime);
        }

        private void DeactivateWeaponAbility()
        {
            if (WeaponAbility == null)
                return;

            if (WeaponAbilityState == WeaponAbilityState.Deactivated ||
                WeaponAbilityState == WeaponAbilityState.Deactivating)
                return;

            WeaponAbility.OnPreDeactivate();
            WeaponAbilityState = WeaponAbilityState.Deactivating;
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

        public void UseSkill(bool isLeftHand, Vector3 defaultAimPosition)
        {
            if (queueUsingSkill.skill != null)
            {
                if (queueUsingSkill.itemIndex >= 0)
                    PlayerCharacterEntity.RequestUseSkillItem(queueUsingSkill.itemIndex, isLeftHand, queueUsingSkill.aimPosition.HasValue ? queueUsingSkill.aimPosition.Value : defaultAimPosition);
                else
                    PlayerCharacterEntity.RequestUseSkill(queueUsingSkill.skill.DataId, isLeftHand, queueUsingSkill.aimPosition.HasValue ? queueUsingSkill.aimPosition.Value : defaultAimPosition);
            }
            ClearQueueUsingSkill();
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(MovementTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempGameObject = overlapColliders[tempCounter].gameObject;
                if (tempGameObject == target)
                    return true;
            }
            return false;
        }

        public bool GetPrimaryAttackButton()
        {
            // Check using hotkey for PC only
            if (!InputManager.useMobileInputOnNonMobile &&
                !Application.isMobilePlatform &&
                UICharacterHotkeys.UsingHotkey != null)
                return false;
            return InputManager.GetButton("Fire1") || InputManager.GetButton("Attack");
        }

        public bool GetSecondaryAttackButton()
        {
            // Check using hotkey for PC only
            if (!InputManager.useMobileInputOnNonMobile &&
                !Application.isMobilePlatform &&
                UICharacterHotkeys.UsingHotkey != null)
                return false;
            return InputManager.GetButton("Fire2");
        }

        public bool GetPrimaryAttackButtonUp()
        {
            return InputManager.GetButtonUp("Fire1") || InputManager.GetButtonUp("Attack");
        }

        public bool GetSecondaryAttackButtonUp()
        {
            return InputManager.GetButtonUp("Fire2");
        }

        public bool GetPrimaryAttackButtonDown()
        {
            return InputManager.GetButtonDown("Fire1") || InputManager.GetButtonDown("Attack");
        }

        public bool GetSecondaryAttackButtonDown()
        {
            return InputManager.GetButtonDown("Fire2");
        }

        public void SetActiveCrosshair(bool isActive)
        {
            if (crosshairRect != null &&
                crosshairRect.gameObject.activeSelf != isActive)
            {
                // Hide crosshair when not active
                crosshairRect.gameObject.SetActive(isActive);
            }
        }

        public void UpdateCameraSettings()
        {
            CacheGameplayCameraControls.CacheCamera.fieldOfView = CameraFov;
            CacheGameplayCameraControls.targetOffset = CameraTargetOffset;
            CacheGameplayCameraControls.zoomDistance = CameraZoomDistance;
            CacheGameplayCameraControls.minZoomDistance = CameraMinZoomDistance;
            CacheGameplayCameraControls.maxZoomDistance = CameraMaxZoomDistance;
            CacheGameplayCameraControls.enableWallHitSpring = ViewMode == ControllerViewMode.Tps ? true : false;
            PlayerCharacterEntity.ModelManager.SetFpsMode(viewMode == ControllerViewMode.Fps);
        }

        public bool IsInFront(Vector3 position)
        {
            return Vector3.Angle(cameraForward, PlayerCharacterEntity.CacheTransform.position - position) > 135f;
        }
    }
}
