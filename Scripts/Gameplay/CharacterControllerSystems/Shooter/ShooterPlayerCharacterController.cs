using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController : BasePlayerCharacterController, IShooterWeaponController, IWeaponAbilityController
    {
        public const byte PAUSE_FIRE_INPUT_FRAMES_AFTER_CONFIRM_BUILD = 3;

        public enum ControllerMode
        {
            Adventure,
            Combat,
        }

        public enum ExtraMoveActiveMode
        {
            None,
            Toggle,
            Hold
        }

        public enum EmptyAmmoAutoReload
        {
            ReloadImmediately,
            ReloadOnKeysReleased,
            DoNotReload,
        }

        [Header("Camera Controls Prefabs")]
        [SerializeField]
        protected FollowCameraControls gameplayCameraPrefab;
        [SerializeField]
        protected FollowCameraControls minimapCameraPrefab;

        [Header("Controller Settings")]
        [SerializeField]
        protected ControllerMode mode;
        [SerializeField]
        protected EmptyAmmoAutoReload emptyAmmoAutoReload;
        [SerializeField]
        protected bool canSwitchViewMode;
        [SerializeField]
        protected ShooterControllerViewMode viewMode;
        [SerializeField]
        protected ExtraMoveActiveMode sprintActiveMode;
        [SerializeField]
        protected ExtraMoveActiveMode walkActiveMode;
        [SerializeField]
        protected ExtraMoveActiveMode crouchActiveMode;
        [SerializeField]
        protected ExtraMoveActiveMode crawlActiveMode;
        [SerializeField]
        protected bool unToggleCrouchWhenJump;
        [SerializeField]
        protected bool unToggleCrawlWhenJump;
        [SerializeField]
        protected float findTargetRaycastDistance = 16f;
        [SerializeField]
        protected bool showConfirmConstructionUI = false;
        [SerializeField]
        protected bool buildRotationSnap;
        [SerializeField]
        protected float buildRotateAngle = 45f;
        [SerializeField]
        protected float buildRotateSpeed = 200f;
        [SerializeField]
        protected RectTransform crosshairRect;
        [SerializeField]
        protected string defaultCameraRotationSpeedScaleSaveKey = "DEFAULT_CAMERA_ROTATION_SPEED_SCALE";

        [Header("TPS Settings")]
        [SerializeField]
        protected float tpsZoomDistance = 3f;
        [SerializeField]
        protected float tpsMinZoomDistance = 3f;
        [SerializeField]
        protected float tpsMaxZoomDistance = 3f;
        [SerializeField]
        protected Vector3 tpsTargetOffset = new Vector3(0.75f, 1.25f, 0f);
        [SerializeField]
        protected Vector3 tpsTargetOffsetWhileCrouching = new Vector3(0.75f, 0.75f, 0f);
        [SerializeField]
        protected Vector3 tpsTargetOffsetWhileCrawling = new Vector3(0.75f, 0.5f, 0f);
        [SerializeField]
        protected float tpsFov = 60f;
        [SerializeField]
        protected float tpsNearClipPlane = 0.3f;
        [SerializeField]
        protected float tpsFarClipPlane = 1000f;
        [SerializeField]
        protected bool turnForwardWhileDoingAction = true;
        [SerializeField]
        [FormerlySerializedAs("stoppedPlayingAttackOrUseSkillAnimationDelay")]
        protected float durationBeforeStopAimming = 0.5f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeed = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWhileSprinting = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWhileWalking = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWhileCrouching = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWileCrawling = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWileSwimming = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        protected float turnSpeedWileDoingAction = 0f;

        [Header("FPS Settings")]
        [SerializeField]
        protected float fpsZoomDistance = 0f;
        [SerializeField]
        protected Vector3 fpsTargetOffset = new Vector3(0f, 0f, 0f);
        [SerializeField]
        protected Vector3 fpsTargetOffsetWhileCrouching = new Vector3(0f, -0.25f, 0f);
        [SerializeField]
        protected Vector3 fpsTargetOffsetWhileCrawling = new Vector3(0f, -0.5f, 0f);
        [SerializeField]
        protected float fpsFov = 40f;
        [SerializeField]
        protected float fpsNearClipPlane = 0.01f;
        [SerializeField]
        protected float fpsFarClipPlane = 1000f;

        [Header("Aim Assist Settings")]
        [SerializeField]
        protected bool enableAimAssist = false;
        [SerializeField]
        protected bool enableAimAssistX = false;
        [SerializeField]
        protected bool enableAimAssistY = true;
        [SerializeField]
        protected bool aimAssistOnFireOnly = true;
        [SerializeField]
        protected float aimAssistRadius = 0.5f;
        [SerializeField]
        protected float aimAssistXSpeed = 20f;
        [SerializeField]
        protected float aimAssistYSpeed = 20f;
        [SerializeField]
        protected bool aimAssistCharacter = true;
        [SerializeField]
        protected bool aimAssistBuilding = false;
        [SerializeField]
        protected bool aimAssistHarvestable = false;

        [Header("Recoil Settings")]
        [SerializeField]
        protected float recoilRateWhileMoving = 1.5f;
        [SerializeField]
        protected float recoilRateWhileSprinting = 2f;
        [SerializeField]
        protected float recoilRateWhileWalking = 1f;
        [SerializeField]
        protected float recoilRateWhileCrouching = 0.5f;
        [SerializeField]
        protected float recoilRateWhileCrawling = 0.5f;
        [SerializeField]
        protected float recoilRateWhileSwimming = 0.5f;

        #region Events
        public System.Action<string, AimPosition> onBeforeUseSkillHotkey;
        public System.Action<string, AimPosition> onBeforeUseItemHotkey;
        #endregion

        public bool IsBlockController { get; protected set; }
        public IShooterGameplayCameraController CacheGameplayCameraController { get; protected set; }
        public IMinimapCameraController CacheMinimapCameraController { get; protected set; }
        public BaseCharacterModel CacheFpsModel { get; protected set; }
        public Vector2 CurrentCrosshairSize { get; protected set; }
        public CrosshairSetting CurrentCrosshairSetting { get; protected set; }
        public BaseWeaponAbility WeaponAbility { get; protected set; }
        public WeaponAbilityState WeaponAbilityState { get; protected set; }

        public ControllerMode Mode
        {
            get
            {
                if (viewMode == ShooterControllerViewMode.Fps)
                {
                    // If view mode is fps, controls type must be combat
                    return ControllerMode.Combat;
                }
                return mode;
            }
        }

        public ShooterControllerViewMode ViewMode
        {
            get { return viewMode; }
            set { viewMode = value; }
        }

        public float CameraZoomDistance
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsZoomDistance : fpsZoomDistance; }
        }

        public float CameraMinZoomDistance
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsMinZoomDistance : fpsZoomDistance; }
        }

        public float CameraMaxZoomDistance
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsMaxZoomDistance : fpsZoomDistance; }
        }

        public float CurrentCameraZoomDistance
        {
            get { return CacheGameplayCameraController.CurrentZoomDistance; }
            set { CacheGameplayCameraController.CurrentZoomDistance = value; }
        }

        public float CurrentCameraMinZoomDistance
        {
            get { return CacheGameplayCameraController.MinZoomDistance; }
            set { CacheGameplayCameraController.MinZoomDistance = value; }
        }

        public float CurrentCameraMaxZoomDistance
        {
            get { return CacheGameplayCameraController.MaxZoomDistance; }
            set { CacheGameplayCameraController.MaxZoomDistance = value; }
        }

        public Vector3 CameraTargetOffset
        {
            get
            {
                if (ViewMode == ShooterControllerViewMode.Tps)
                {
                    if (PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                    {
                        return tpsTargetOffsetWhileCrouching;
                    }
                    else if (PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
                    {
                        return tpsTargetOffsetWhileCrawling;
                    }
                    else
                    {
                        return tpsTargetOffset;
                    }
                }
                else
                {
                    if (PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                    {
                        return fpsTargetOffsetWhileCrouching;
                    }
                    else if (PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
                    {
                        return fpsTargetOffsetWhileCrawling;
                    }
                    else
                    {
                        return fpsTargetOffset;
                    }
                }
            }
        }

        public float CameraFov
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsFov : fpsFov; }
        }

        public float CameraNearClipPlane
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsNearClipPlane : fpsNearClipPlane; }
        }

        public float CameraFarClipPlane
        {
            get { return ViewMode == ShooterControllerViewMode.Tps ? tpsFarClipPlane : fpsFarClipPlane; }
        }

        public float CurrentCameraFov
        {
            get { return CacheGameplayCameraController.CameraFov; }
            set { CacheGameplayCameraController.CameraFov = value; }
        }

        public float CurrentCameraNearClipPlane
        {
            get { return CacheGameplayCameraController.CameraNearClipPlane; }
            set { CacheGameplayCameraController.CameraNearClipPlane = value; }
        }

        public float CurrentCameraFarClipPlane
        {
            get { return CacheGameplayCameraController.CameraFarClipPlane; }
            set { CacheGameplayCameraController.CameraFarClipPlane = value; }
        }

        public float DefaultCameraRotationSpeedScale
        {
            get { return CameraRotationSpeedScaleSetting.GetCameraRotationSpeedScaleByKey(defaultCameraRotationSpeedScaleSaveKey, 1f); }
        }

        public float CameraRotationSpeedScale
        {
            get { return CacheGameplayCameraController.CameraRotationSpeedScale; }
            set { CacheGameplayCameraController.CameraRotationSpeedScale = value; }
        }

        public bool HideCrosshair { get; set; }

        public float CurrentTurnSpeed
        {
            get
            {
                if (PlayerCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
                    return turnSpeedWileSwimming;
                switch (PlayerCharacterEntity.ExtraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        return turnSpeedWhileSprinting;
                    case ExtraMovementState.IsWalking:
                        return turnSpeedWhileWalking;
                    case ExtraMovementState.IsCrouching:
                        return turnSpeedWhileCrouching;
                    case ExtraMovementState.IsCrawling:
                        return turnSpeedWileCrawling;
                }
                return turnSpeed;
            }
        }

        // Input data
        protected InputStateManager activateInput;
        protected InputStateManager pickupItemInput;
        protected InputStateManager reloadInput;
        protected InputStateManager exitVehicleInput;
        protected InputStateManager switchEquipWeaponSetInput;
        protected float lastAimmingTime;
        protected bool updatingInputs;
        // Entity detector
        protected NearbyEntityDetector warpPortalEntityDetector;
        // Temp physic variables
        protected RaycastHit[] raycasts = new RaycastHit[100];
        protected Collider[] overlapColliders = new Collider[200];
        // Temp target
        protected BasePlayerCharacterEntity targetPlayer;
        protected NpcEntity targetNpc;
        protected BuildingEntity targetBuilding;
        protected VehicleEntity targetVehicle;
        protected WarpPortalEntity targetWarpPortal;
        protected ItemsContainerEntity targetItemsContainer;
        // Temp data
        protected IGameEntity tempGameEntity;
        protected Ray centerRay;
        protected float centerOriginToCharacterDistance;
        protected Vector3 moveDirection;
        protected Vector3 cameraForward;
        protected Vector3 cameraRight;
        protected float inputV;
        protected float inputH;
        protected Vector2 normalizedInput;
        protected Vector3 moveLookDirection;
        protected Vector3 targetLookDirection;
        protected bool tempPressAttackRight;
        protected bool tempPressAttackLeft;
        protected bool tempPressWeaponAbility;
        protected bool isLeftHandAttacking;
        protected Vector3 aimTargetPosition;
        protected Vector3 turnDirection;
        // Controlling states
        protected bool toggleSprintOn;
        protected bool toggleWalkOn;
        protected bool toggleCrouchOn;
        protected bool toggleCrawlOn;
        protected ShooterControllerViewMode dirtyViewMode;
        protected IWeaponItem rightHandWeapon;
        protected IWeaponItem leftHandWeapon;
        protected MovementState movementState;
        protected ExtraMovementState extraMovementState;
        protected ShooterControllerViewMode? viewModeBeforeDead;
        protected bool updateAttackingCrosshair;
        protected bool updateAttackedCrosshair;
        protected bool mustReleaseFireKey;
        protected float buildYRotate;
        protected byte pauseFireInputFrames;
        protected bool isAimming;
        protected bool isCharging;

        protected override void Awake()
        {
            base.Awake();
            CacheGameplayCameraController = gameObject.GetOrAddComponent<IShooterGameplayCameraController, ShooterGameplayCameraController>((obj) =>
            {
                ShooterGameplayCameraController castedObj = obj as ShooterGameplayCameraController;
                castedObj.gameplayCameraPrefab = gameplayCameraPrefab;
                castedObj.InitialCameraControls();
            });
            CameraRotationSpeedScale = DefaultCameraRotationSpeedScale;
            CacheMinimapCameraController = gameObject.GetOrAddComponent<IMinimapCameraController, DefaultMinimapCameraController>((obj) =>
            {
                DefaultMinimapCameraController castedObj = obj as DefaultMinimapCameraController;
                castedObj.minimapCameraPrefab = minimapCameraPrefab;
            });
            buildingItemIndex = -1;
            isLeftHandAttacking = false;
            ConstructingBuildingEntity = null;
            activateInput = new InputStateManager("Activate");
            pickupItemInput = new InputStateManager("PickUpItem");
            reloadInput = new InputStateManager("Reload");
            exitVehicleInput = new InputStateManager("ExitVehicle");
            switchEquipWeaponSetInput = new InputStateManager("SwitchEquipWeaponSet");
            // Initialize warp portal entity detector
            GameObject tempGameObject = new GameObject("_WarpPortalEntityDetector");
            warpPortalEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            warpPortalEntityDetector.detectingRadius = CurrentGameInstance.conversationDistance;
            warpPortalEntityDetector.findWarpPortal = true;
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);
            CacheGameplayCameraController.Setup(characterEntity);
            CacheMinimapCameraController.Setup(characterEntity);

            if (characterEntity == null)
                return;

            targetLookDirection = MovementTransform.forward;
            SetupEquipWeapons(characterEntity.EquipWeapons);
            characterEntity.onEquipWeaponSetChange += SetupEquipWeapons;
            characterEntity.onSelectableWeaponSetsOperation += SetupEquipWeapons;
            characterEntity.onLaunchDamageEntity += OnLaunchDamageEntity;
            if (CacheFpsModel != null)
                Destroy(CacheFpsModel.gameObject);
            CacheFpsModel = characterEntity.ModelManager.InstantiateFpsModel(CacheGameplayCameraController.CameraTransform);
            characterEntity.ModelManager.SetIsFps(ViewMode == ShooterControllerViewMode.Fps);
            UpdateViewMode();
        }

        protected override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);
            CacheGameplayCameraController.Desetup(characterEntity);
            CacheMinimapCameraController.Desetup(characterEntity);

            if (characterEntity == null)
                return;

            characterEntity.onEquipWeaponSetChange -= SetupEquipWeapons;
            characterEntity.onSelectableWeaponSetsOperation -= SetupEquipWeapons;
            characterEntity.onLaunchDamageEntity -= OnLaunchDamageEntity;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(CacheGameplayCameraController.gameObject);
            Destroy(CacheMinimapCameraController.gameObject);
            if (warpPortalEntityDetector != null)
                Destroy(warpPortalEntityDetector.gameObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected void SetupEquipWeapons(byte equipWeaponSet)
        {
            SetupEquipWeapons(PlayerCharacterEntity.EquipWeapons);
        }

        protected void SetupEquipWeapons(LiteNetLibManager.LiteNetLibSyncList.Operation operation, int index)
        {
            SetupEquipWeapons(PlayerCharacterEntity.EquipWeapons);
        }

        protected virtual void SetupEquipWeapons(EquipWeapons equipWeapons)
        {
            CurrentCrosshairSetting = PlayerCharacterEntity.GetCrosshairSetting();
            UpdateCrosshair(CurrentCrosshairSetting, false, -CurrentCrosshairSetting.shrinkPerFrame);

            rightHandWeapon = equipWeapons.GetRightHandWeaponItem();
            leftHandWeapon = equipWeapons.GetLeftHandWeaponItem();
            // Weapon ability will be able to use when equip weapon at main-hand only
            if (rightHandWeapon != null && leftHandWeapon == null)
            {
                if (rightHandWeapon.WeaponAbility != WeaponAbility)
                {
                    if (WeaponAbility != null)
                        WeaponAbility.Desetup();
                    WeaponAbility = rightHandWeapon.WeaponAbility;
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
            if (rightHandWeapon == null)
                rightHandWeapon = GameInstance.Singleton.DefaultWeaponItem;
            if (leftHandWeapon == null)
                leftHandWeapon = GameInstance.Singleton.DefaultWeaponItem;
        }

        protected override void Update()
        {
            if (pauseFireInputFrames > 0)
                --pauseFireInputFrames;

            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            CacheMinimapCameraController.FollowingEntityTransform = CameraTargetTransform;
            CacheMinimapCameraController.FollowingGameplayCameraTransform = CacheGameplayCameraController.CameraTransform;

            if (PlayerCharacterEntity.IsDead())
            {
                // Deactivate weapon ability immediately when dead
                if (WeaponAbility != null && WeaponAbilityState != WeaponAbilityState.Deactivated)
                {
                    WeaponAbility.ForceDeactivated();
                    WeaponAbilityState = WeaponAbilityState.Deactivated;
                }
                // Set view mode to TPS when character dead
                if (!viewModeBeforeDead.HasValue)
                    viewModeBeforeDead = ViewMode;
                ViewMode = ShooterControllerViewMode.Tps;
            }
            else
            {
                // Set view mode to view mode before dead when character alive
                if (viewModeBeforeDead.HasValue)
                {
                    ViewMode = viewModeBeforeDead.Value;
                    viewModeBeforeDead = null;
                }
            }

            if (dirtyViewMode != viewMode)
                UpdateViewMode();

            CacheGameplayCameraController.TargetOffset = CameraTargetOffset;
            CacheGameplayCameraController.EnableWallHitSpring = viewMode == ShooterControllerViewMode.Tps;
            CacheGameplayCameraController.FollowingEntityTransform = ViewMode == ShooterControllerViewMode.Fps ? PlayerCharacterEntity.FpsCameraTargetTransform : PlayerCharacterEntity.CameraTargetTransform;

            // Set temp data
            float tempDeltaTime = Time.deltaTime;

            // Update inputs
            activateInput.OnUpdate(tempDeltaTime);
            pickupItemInput.OnUpdate(tempDeltaTime);
            reloadInput.OnUpdate(tempDeltaTime);
            exitVehicleInput.OnUpdate(tempDeltaTime);
            switchEquipWeaponSetInput.OnUpdate(tempDeltaTime);

            // Check is any UIs block controller or not?
            IsBlockController = CacheUISceneGameplay.IsBlockController();

            // Lock cursor when not show UIs
            if (InputManager.useMobileInputOnNonMobile || Application.isMobilePlatform)
            {
                // Control camera by touch-screen
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CacheGameplayCameraController.UpdateRotationX = false;
                CacheGameplayCameraController.UpdateRotationY = false;
                CacheGameplayCameraController.UpdateRotation = InputManager.GetButton("CameraRotate");
                CacheGameplayCameraController.UpdateZoom = !IsBlockController;
            }
            else
            {
                // Control camera by mouse-move
                Cursor.lockState = !IsBlockController ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = IsBlockController;
                CacheGameplayCameraController.UpdateRotation = !IsBlockController;
                CacheGameplayCameraController.UpdateZoom = !IsBlockController;
            }
            // Clear selected entity
            SelectedEntity = null;

            // Update crosshair (with states from last update)
            UpdateCrosshair();

            // Clear controlling states from last update
            movementState = MovementState.None;
            extraMovementState = ExtraMovementState.None;
            CacheGameplayCameraController.CameraRotationSpeedScale = DefaultCameraRotationSpeedScale;
            UpdateWeaponAbilityActivation(tempDeltaTime);

            if (IsBlockController || GenericUtils.IsFocusInputField())
            {
                mustReleaseFireKey = false;
                PlayerCharacterEntity.KeyMovement(Vector3.zero, MovementState.None);
                DeactivateWeaponAbility();
                return;
            }

            // Prepare variables to find nearest raycasted hit point
            centerRay = CacheGameplayCameraController.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            centerOriginToCharacterDistance = Vector3.Distance(centerRay.origin, CacheTransform.position);
            cameraForward = CacheGameplayCameraController.CameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            cameraRight = CacheGameplayCameraController.CameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            // Update look target and aim position
            if (ConstructingBuildingEntity == null)
                UpdateTarget_BattleMode();
            else
                UpdateTarget_BuildMode();

            // Update movement and camera pitch
            UpdateMovementInputs();

            // Update aim position
            PlayerCharacterEntity.AimPosition = PlayerCharacterEntity.GetAttackAimPosition(ref isLeftHandAttacking, aimTargetPosition);

            isAimming = false;
            // Update input
            if (!updatingInputs)
            {
                if (ConstructingBuildingEntity == null)
                    UpdateInputs_BattleMode().Forget();
                else
                    UpdateInputs_BuildMode().Forget();
            }

            // Hide Npc UIs when move
            if (moveDirection.sqrMagnitude > 0f)
                HideNpcDialog();

            // If jumping add jump state
            if (InputManager.GetButtonDown("Jump"))
            {
                if (unToggleCrouchWhenJump && PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                    toggleCrouchOn = false;
                else if (unToggleCrawlWhenJump && PlayerCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
                    toggleCrawlOn = false;
                else
                    movementState |= MovementState.IsJump;
            }
            else if (PlayerCharacterEntity.MovementState.Has(MovementState.IsGrounded))
            {
                if (DetectExtraActive("Sprint", sprintActiveMode, ref toggleSprintOn))
                {
                    extraMovementState = ExtraMovementState.IsSprinting;
                    toggleWalkOn = false;
                    toggleCrouchOn = false;
                    toggleCrawlOn = false;
                }
                if (DetectExtraActive("Walk", walkActiveMode, ref toggleWalkOn))
                {
                    extraMovementState = ExtraMovementState.IsWalking;
                    toggleSprintOn = false;
                    toggleCrouchOn = false;
                    toggleCrawlOn = false;
                }
                if (DetectExtraActive("Crouch", crouchActiveMode, ref toggleCrouchOn))
                {
                    extraMovementState = ExtraMovementState.IsCrouching;
                    toggleSprintOn = false;
                    toggleWalkOn = false;
                    toggleCrawlOn = false;
                }
                if (DetectExtraActive("Crawl", crawlActiveMode, ref toggleCrawlOn))
                {
                    extraMovementState = ExtraMovementState.IsCrawling;
                    toggleSprintOn = false;
                    toggleWalkOn = false;
                    toggleCrouchOn = false;
                }
            }
            if (moveDirection.magnitude > 0f)
            {
                switch (mode)
                {
                    case ControllerMode.Adventure:
                        if (isAimming)
                            movementState |= GameplayUtils.GetMovementStateByDirection(moveDirection, MovementTransform.forward);
                        else
                            movementState |= MovementState.Forward;
                        break;
                    case ControllerMode.Combat:
                        movementState |= GameplayUtils.GetMovementStateByDirection(moveDirection, MovementTransform.forward);
                        break;
                }
            }
            PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
            PlayerCharacterEntity.SetExtraMovementState(extraMovementState);
            UpdateLookAtTarget();

            if (canSwitchViewMode && InputManager.GetButtonDown("SwitchViewMode"))
            {
                switch (ViewMode)
                {
                    case ShooterControllerViewMode.Tps:
                        ViewMode = ShooterControllerViewMode.Fps;
                        break;
                    case ShooterControllerViewMode.Fps:
                        ViewMode = ShooterControllerViewMode.Tps;
                        break;
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (PlayerCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
            {
                // Clear toggled sprint, crouch and crawl
                toggleSprintOn = false;
                toggleWalkOn = false;
                toggleCrouchOn = false;
                toggleCrawlOn = false;
            }
            // Update inputs
            activateInput.OnLateUpdate();
            pickupItemInput.OnLateUpdate();
            reloadInput.OnLateUpdate();
            exitVehicleInput.OnLateUpdate();
            switchEquipWeaponSetInput.OnLateUpdate();
        }

        protected bool DetectExtraActive(string key, ExtraMoveActiveMode activeMode, ref bool state)
        {
            switch (activeMode)
            {
                case ExtraMoveActiveMode.Hold:
                    state = InputManager.GetButton(key);
                    break;
                case ExtraMoveActiveMode.Toggle:
                    if (InputManager.GetButtonDown(key))
                        state = !state;
                    break;
            }
            return state;
        }

        protected virtual void UpdateTarget_BattleMode()
        {
            // Prepare raycast distance / fov
            float attackDistance = 0f;
            bool attacking = false;
            if (IsUsingHotkey())
            {
                mustReleaseFireKey = true;
            }
            else
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

                attacking = tempPressAttackRight || tempPressAttackLeft;
                if (attacking && !PlayerCharacterEntity.IsAttacking && !PlayerCharacterEntity.IsUsingSkill)
                {
                    // Priority is right > left
                    isLeftHandAttacking = !tempPressAttackRight && tempPressAttackLeft;
                }

                // Calculate aim distance by skill or weapon
                if (PlayerCharacterEntity.UsingSkill != null && PlayerCharacterEntity.UsingSkill.IsAttack)
                {
                    // Increase aim distance by skill attack distance
                    attackDistance = PlayerCharacterEntity.UsingSkill.GetCastDistance(PlayerCharacterEntity, PlayerCharacterEntity.UsingSkillLevel, isLeftHandAttacking);
                    attacking = true;
                }
                else if (queueUsingSkill.skill != null && queueUsingSkill.skill.IsAttack)
                {
                    // Increase aim distance by skill attack distance
                    attackDistance = queueUsingSkill.skill.GetCastDistance(PlayerCharacterEntity, queueUsingSkill.level, isLeftHandAttacking);
                    attacking = true;
                }
                else
                {
                    // Increase aim distance by attack distance
                    attackDistance = PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking);
                }
            }
            // Temporary variables
            RaycastHit tempHitInfo;
            float tempDistance;
            // Default aim position (aim to sky/space)
            aimTargetPosition = centerRay.origin + centerRay.direction * (centerOriginToCharacterDistance + attackDistance);
            // Aim to damageable hit boxes (higher priority than other entities)
            // Raycast from camera position to center of screen
            int tempCount = PhysicUtils.SortedRaycastNonAlloc3D(centerRay.origin, centerRay.direction, raycasts, centerOriginToCharacterDistance + attackDistance, Physics.DefaultRaycastLayers);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = raycasts[tempCounter];

                if (tempHitInfo.transform.gameObject.layer == PhysicLayers.TransparentFX ||
                    tempHitInfo.transform.gameObject.layer == PhysicLayers.IgnoreRaycast ||
                    tempHitInfo.transform.gameObject.layer == PhysicLayers.Water)
                {
                    // Skip some layers
                    continue;
                }

                if (tempHitInfo.collider.GetComponent<IUnHittable>() != null)
                {
                    // Don't aim to unhittable objects
                    continue;
                }

                // Get damageable hit box component from hit target
                tempGameEntity = tempHitInfo.collider.GetComponent<DamageableHitBox>();

                if (tempGameEntity == null || !tempGameEntity.Entity || tempGameEntity.Entity.IsHide() ||
                    tempGameEntity.GetObjectId() == PlayerCharacterEntity.ObjectId)
                {
                    // Skip empty game entity / hiddeing entity / controlling player's entity
                    continue;
                }

                // Entity isn't in front of character, so it's not the target
                if (turnForwardWhileDoingAction && !IsInFront(tempHitInfo.point))
                    continue;

                // Skip dead entity while attacking (to allow to use resurrect skills)
                if (attacking && (tempGameEntity as DamageableHitBox).IsDead())
                    continue;

                // Entity is in front of character, so this is target
                aimTargetPosition = tempHitInfo.point;
                SelectedEntity = tempGameEntity.Entity;
                break;
            }

            // Aim to activateable entities if it can't find attacking target
            if (SelectedEntity == null)
            {
                // Default aim position (aim to sky/space)
                aimTargetPosition = centerRay.origin + centerRay.direction * (centerOriginToCharacterDistance + findTargetRaycastDistance);
                // Raycast from camera position to center of screen
                tempCount = PhysicUtils.SortedRaycastNonAlloc3D(centerRay.origin, centerRay.direction, raycasts, centerOriginToCharacterDistance + findTargetRaycastDistance, CurrentGameInstance.GetTargetLayerMask());
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];
                    if (tempHitInfo.collider.GetComponent<IUnHittable>() != null)
                    {
                        // Don't aim to unhittable objects
                        continue;
                    }

                    // Get distance between character and raycast hit point
                    tempDistance = Vector3.Distance(CacheTransform.position, tempHitInfo.point);
                    tempGameEntity = tempHitInfo.collider.GetComponent<IGameEntity>();

                    if (tempGameEntity == null || !tempGameEntity.Entity || tempGameEntity.Entity.IsHide() ||
                        tempGameEntity.GetObjectId() == PlayerCharacterEntity.ObjectId)
                    {
                        // Skip empty game entity / hiddeing entity / controlling player's entity
                        continue;
                    }

                    // Find item drop entity
                    if (tempGameEntity.Entity is ItemDropEntity &&
                        tempDistance <= CurrentGameInstance.pickUpItemDistance)
                    {
                        // Entity is in front of character, so this is target
                        if (!turnForwardWhileDoingAction || IsInFront(tempHitInfo.point))
                            aimTargetPosition = tempHitInfo.point;
                        SelectedEntity = tempGameEntity.Entity;
                        break;
                    }
                    // Find activatable entity (NPC/Building/Mount/Etc)
                    if (tempDistance <= CurrentGameInstance.conversationDistance)
                    {
                        // Entity is in front of character, so this is target
                        if (!turnForwardWhileDoingAction || IsInFront(tempHitInfo.point))
                            aimTargetPosition = tempHitInfo.point;
                        SelectedEntity = tempGameEntity.Entity;
                        break;
                    }
                }
            }

            // Calculate aim direction
            turnDirection = aimTargetPosition - CacheTransform.position;
            turnDirection.y = 0f;
            turnDirection.Normalize();
            // Show target hp/mp
            CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            PlayerCharacterEntity.SetTargetEntity(SelectedEntity);
            // Update aim assist
            CacheGameplayCameraController.EnableAimAssist = enableAimAssist && (tempPressAttackRight || tempPressAttackLeft || !aimAssistOnFireOnly) && !(SelectedEntity is IDamageableEntity);
            CacheGameplayCameraController.EnableAimAssistX = enableAimAssistX;
            CacheGameplayCameraController.EnableAimAssistY = enableAimAssistY;
            CacheGameplayCameraController.AimAssistPlayer = aimAssistCharacter;
            CacheGameplayCameraController.AimAssistBuilding = aimAssistBuilding;
            CacheGameplayCameraController.AimAssistHarvestable = aimAssistHarvestable;
            CacheGameplayCameraController.AimAssistRadius = aimAssistRadius;
            CacheGameplayCameraController.AimAssistXSpeed = aimAssistXSpeed;
            CacheGameplayCameraController.AimAssistYSpeed = aimAssistYSpeed;
            CacheGameplayCameraController.AimAssistMaxAngleFromFollowingTarget = 115f;
        }

        protected virtual void UpdateTarget_BuildMode()
        {
            // Disable aim assist while constucting the building
            CacheGameplayCameraController.EnableAimAssist = false;
        }

        protected virtual void UpdateMovementInputs()
        {
            float pitch = CacheGameplayCameraController.CameraTransform.eulerAngles.x;

            // Update charcter pitch
            PlayerCharacterEntity.Pitch = pitch;

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            moveDirection = Vector3.zero;
            inputV = InputManager.GetAxis("Vertical", raw);
            inputH = InputManager.GetAxis("Horizontal", raw);
            normalizedInput = new Vector2(inputV, inputH).normalized;
            moveDirection += cameraForward * inputV;
            moveDirection += cameraRight * inputH;
            if (moveDirection.sqrMagnitude > 0f)
            {
                if (pitch > 180f)
                    pitch -= 360f;
                moveDirection.y = -pitch / 90f;
            }

            // Set look direction
            switch (Mode)
            {
                case ControllerMode.Adventure:
                    moveLookDirection = moveDirection;
                    moveLookDirection.y = 0f;
                    break;
                case ControllerMode.Combat:
                    moveLookDirection = cameraForward;
                    break;
            }

            if (ViewMode == ShooterControllerViewMode.Fps)
            {
                // Force turn to look direction
                moveLookDirection = cameraForward;
                targetLookDirection = cameraForward;
            }

            moveDirection.Normalize();
        }

        protected virtual async UniTaskVoid UpdateInputs_BattleMode()
        {
            updatingInputs = true;
            // Prepare fire type data
            FireType rightHandFireType = FireType.SingleFire;
            if (rightHandWeapon != null)
            {
                rightHandFireType = rightHandWeapon.FireType;
            }
            // Prepare fire type data
            FireType leftHandFireType = FireType.SingleFire;
            if (leftHandWeapon != null)
            {
                leftHandFireType = leftHandWeapon.FireType;
            }
            // Have to release fire key, then check press fire key later on next frame
            if (mustReleaseFireKey)
            {
                tempPressAttackRight = false;
                tempPressAttackLeft = false;
                // If release fire key while charging, attack
                if (!isLeftHandAttacking &&
                    (GetPrimaryAttackButtonUp() ||
                    !GetPrimaryAttackButton()))
                {
                    mustReleaseFireKey = false;
                    await Aimming();
                    // Button released, start attacking while fire type is fire on release
                    if (rightHandFireType == FireType.FireOnRelease)
                        Attack(isLeftHandAttacking);
                    isCharging = false;
                }
                // If release fire key while charging, attack
                if (isLeftHandAttacking &&
                    (GetSecondaryAttackButtonUp() ||
                    !GetSecondaryAttackButton()))
                {
                    mustReleaseFireKey = false;
                    await Aimming();
                    // Button released, start attacking while fire type is fire on release
                    if (leftHandFireType == FireType.FireOnRelease)
                        Attack(isLeftHandAttacking);
                    isCharging = false;
                }
            }
            bool anyKeyPressed = false;
            if (isCharging ||
                queueUsingSkill.skill != null ||
                tempPressAttackRight ||
                tempPressAttackLeft ||
                activateInput.IsPress ||
                activateInput.IsRelease ||
                activateInput.IsHold ||
                PlayerCharacterEntity.IsPlayingAttackOrUseSkillAnimation())
            {
                anyKeyPressed = true;
                // Find forward character / npc / building / warp entity from camera center
                // Check is character playing action animation to turn character forwarding to aim position
                targetPlayer = null;
                targetNpc = null;
                targetBuilding = null;
                targetVehicle = null;
                targetWarpPortal = null;
                targetItemsContainer = null;
                if (!tempPressAttackRight && !tempPressAttackLeft)
                {
                    if (activateInput.IsHold)
                    {
                        if (SelectedEntity is BuildingEntity)
                        {
                            targetBuilding = SelectedEntity as BuildingEntity;
                        }
                    }
                    else if (activateInput.IsRelease)
                    {
                        if (SelectedEntity == null)
                        {
                            if (warpPortalEntityDetector?.warpPortals.Count > 0)
                            {
                                // It may not able to raycast from inside warp portal, so try to get it from the detector
                                targetWarpPortal = warpPortalEntityDetector.warpPortals[0];
                            }
                        }
                        else
                        {
                            if (SelectedEntity is BasePlayerCharacterEntity)
                            {
                                targetPlayer = SelectedEntity as BasePlayerCharacterEntity;
                            }
                            if (SelectedEntity is NpcEntity)
                            {
                                targetNpc = SelectedEntity as NpcEntity;
                            }
                            if (SelectedEntity is BuildingEntity)
                            {
                                targetBuilding = SelectedEntity as BuildingEntity;
                            }
                            if (SelectedEntity is VehicleEntity)
                            {
                                targetVehicle = SelectedEntity as VehicleEntity;
                            }
                            if (SelectedEntity is WarpPortalEntity)
                            {
                                targetWarpPortal = SelectedEntity as WarpPortalEntity;
                            }
                            if (SelectedEntity is ItemsContainerEntity)
                            {
                                targetItemsContainer = SelectedEntity as ItemsContainerEntity;
                            }
                        }
                    }
                }

                // Update look direction
                if (PlayerCharacterEntity.IsPlayingAttackOrUseSkillAnimation() || isCharging)
                {
                    await Aimming();
                }
                else if (queueUsingSkill.skill != null)
                {
                    await Aimming();
                    UseSkill(isLeftHandAttacking);
                }
                else if (tempPressAttackRight || tempPressAttackLeft)
                {
                    await Aimming();
                    if (!isLeftHandAttacking)
                    {
                        // Fire on release weapons have to release to fire, so when start holding, play weapon charge animation
                        if (rightHandFireType == FireType.FireOnRelease)
                        {
                            isCharging = true;
                            WeaponCharge(isLeftHandAttacking);
                        }
                        else
                        {
                            isCharging = false;
                            Attack(isLeftHandAttacking);
                        }
                    }
                    else
                    {
                        // Fire on release weapons have to release to fire, so when start holding, play weapon charge animation
                        if (leftHandFireType == FireType.FireOnRelease)
                        {
                            isCharging = true;
                            WeaponCharge(isLeftHandAttacking);
                        }
                        else
                        {
                            isCharging = false;
                            Attack(isLeftHandAttacking);
                        }
                    }
                }
                else if (activateInput.IsHold)
                {
                    await Aimming();
                    HoldActivate();
                }
                else if (activateInput.IsRelease)
                {
                    await Aimming();
                    Activate();
                }
            }

            if (tempPressWeaponAbility)
            {
                anyKeyPressed = true;
                // Toggle weapon ability
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

            if (pickupItemInput.IsPress)
            {
                anyKeyPressed = true;
                // Find for item to pick up
                if (SelectedEntity != null && SelectedEntity is ItemDropEntity)
                {
                    PlayerCharacterEntity.CallServerPickupItem(SelectedEntity.ObjectId);
                }
            }

            if (reloadInput.IsPress)
            {
                anyKeyPressed = true;
                // Reload ammo when press the button
                Reload();
            }

            if (exitVehicleInput.IsPress)
            {
                anyKeyPressed = true;
                // Exit vehicle
                PlayerCharacterEntity.CallServerExitVehicle();
            }

            if (switchEquipWeaponSetInput.IsPress)
            {
                anyKeyPressed = true;
                // Switch equip weapon set
                GameInstance.ClientInventoryHandlers.RequestSwitchEquipWeaponSet(new RequestSwitchEquipWeaponSetMessage()
                {
                    equipWeaponSet = (byte)(PlayerCharacterEntity.EquipWeaponSet + 1),
                }, ClientInventoryActions.ResponseSwitchEquipWeaponSet);
            }

            // Setup releasing state
            if (tempPressAttackRight && rightHandFireType != FireType.Automatic)
            {
                // The weapon's fire mode is single fire or fire on release, so player have to release fire key for next fire
                mustReleaseFireKey = true;
            }
            if (tempPressAttackLeft && leftHandFireType != FireType.Automatic)
            {
                // The weapon's fire mode is single fire or fire on release, so player have to release fire key for next fire
                mustReleaseFireKey = true;
            }

            // Reloading
            if (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
            {
                switch (emptyAmmoAutoReload)
                {
                    case EmptyAmmoAutoReload.ReloadImmediately:
                        Reload();
                        break;
                    case EmptyAmmoAutoReload.ReloadOnKeysReleased:
                        // Auto reload when ammo empty
                        if (!tempPressAttackRight && !tempPressAttackLeft && !reloadInput.IsPress)
                        {
                            // Reload ammo when empty and not press any keys
                            Reload();
                        }
                        break;
                }
            }

            // Update look direction
            if (!anyKeyPressed)
            {
                // Update look direction while moving without doing any action
                if (Time.unscaledTime - lastAimmingTime < durationBeforeStopAimming)
                {
                    await Aimming();
                }
                else
                {
                    SetTargetLookDirectionWhileMoving();
                }
            }
            updatingInputs = false;
        }

        protected virtual async UniTaskVoid UpdateInputs_BuildMode()
        {
            SetTargetLookDirectionWhileMoving();
            updatingInputs = false;
            await UniTask.Yield();
        }

        protected virtual void UpdateCrosshair()
        {
            bool isMoving = movementState.Has(MovementState.Forward) ||
                movementState.Has(MovementState.Backward) ||
                movementState.Has(MovementState.Left) ||
                movementState.Has(MovementState.Right) ||
                movementState.Has(MovementState.IsJump);
            if (updateAttackingCrosshair)
            {
                UpdateCrosshair(CurrentCrosshairSetting, true, CurrentCrosshairSetting.expandPerFrameWhileAttacking);
                updateAttackingCrosshair = false;
                updateAttackedCrosshair = true;
            }
            else if (updateAttackedCrosshair)
            {
                UpdateCrosshair(CurrentCrosshairSetting, true, CurrentCrosshairSetting.shrinkPerFrameWhenAttacked);
                updateAttackedCrosshair = false;
            }
            else if (isMoving)
            {
                UpdateCrosshair(CurrentCrosshairSetting, false, CurrentCrosshairSetting.expandPerFrameWhileMoving);
            }
            else
            {
                UpdateCrosshair(CurrentCrosshairSetting, false, -CurrentCrosshairSetting.shrinkPerFrame);
            }
        }

        protected virtual void UpdateCrosshair(CrosshairSetting setting, bool isAttack, float power)
        {
            if (crosshairRect == null)
                return;
            // Show cross hair if weapon's crosshair setting isn't hidden or there is a constructing building
            crosshairRect.gameObject.SetActive((!setting.hidden && !HideCrosshair) || ConstructingBuildingEntity != null);
            // Not active?, don't update
            if (!crosshairRect.gameObject)
                return;
            // Change crosshair size by power
            Vector3 sizeDelta = CurrentCrosshairSize;
            // Expanding
            sizeDelta.x += power;
            sizeDelta.y += power;
            if (!isAttack)
                sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, setting.minSpread, setting.maxSpread), Mathf.Clamp(sizeDelta.y, setting.minSpread, setting.maxSpread));
            crosshairRect.sizeDelta = CurrentCrosshairSize = sizeDelta;
        }

        protected virtual void UpdateRecoil()
        {
            float recoilX;
            float recoilY;
            if (movementState.Has(MovementState.Forward) ||
                movementState.Has(MovementState.Backward) ||
                movementState.Has(MovementState.Left) ||
                movementState.Has(MovementState.Right))
            {
                if (movementState.Has(MovementState.IsUnderWater))
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileSwimming;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileSwimming;
                }
                else if (extraMovementState == ExtraMovementState.IsSprinting)
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileSprinting;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileSprinting;
                }
                else if (extraMovementState == ExtraMovementState.IsWalking)
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileWalking;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileWalking;
                }
                else
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileMoving;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileMoving;
                }
            }
            else if (extraMovementState == ExtraMovementState.IsCrouching)
            {
                recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileCrouching;
                recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileCrouching;
            }
            else if (extraMovementState == ExtraMovementState.IsCrawling)
            {
                recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileCrawling;
                recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileCrawling;
            }
            else
            {
                recoilX = CurrentCrosshairSetting.recoilX;
                recoilY = CurrentCrosshairSetting.recoilY;
            }
            if (recoilX > 0f || recoilY > 0f)
            {
                CacheGameplayCameraController.Recoil(recoilY, Random.Range(-recoilX, recoilX));
            }
        }

        protected void OnLaunchDamageEntity(bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel, int randomSeed, AimPosition aimPosition, Vector3 stagger, HashSet<DamageHitObjectInfo> hitObjectIds)
        {
            UpdateRecoil();
        }

        protected virtual async UniTask Aimming()
        {
            while (!SetTargetLookDirectionWhileDoingAction())
            {
                isAimming = true;
                lastAimmingTime = Time.unscaledTime;
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Return true if it's turned forwarding
        /// </summary>
        /// <returns></returns>
        protected virtual bool SetTargetLookDirectionWhileDoingAction()
        {
            switch (ViewMode)
            {
                case ShooterControllerViewMode.Fps:
                    // Just look at camera forward while character playing action animation
                    targetLookDirection = cameraForward;
                    return PlayerCharacterEntity.CanDoNextAction();
                case ShooterControllerViewMode.Tps:
                    // Just look at camera forward while character playing action animation while `turnForwardWhileDoingAction` is `true`
                    Vector3 doActionLookDirection = turnForwardWhileDoingAction ? cameraForward : turnDirection;
                    if (turnSpeedWileDoingAction > 0f)
                    {
                        Quaternion currentRot = Quaternion.LookRotation(targetLookDirection);
                        Quaternion targetRot = Quaternion.LookRotation(doActionLookDirection);
                        currentRot = Quaternion.Slerp(currentRot, targetRot, turnSpeedWileDoingAction * Time.deltaTime);
                        targetLookDirection = currentRot * Vector3.forward;
                        return Quaternion.Angle(currentRot, targetRot) <= 15f && PlayerCharacterEntity.CanDoNextAction();
                    }
                    else
                    {
                        // Turn immediately because turn speed <= 0
                        targetLookDirection = doActionLookDirection;
                        return PlayerCharacterEntity.CanDoNextAction();
                    }
            }
            return false;
        }

        protected virtual void SetTargetLookDirectionWhileMoving()
        {
            switch (ViewMode)
            {
                case ShooterControllerViewMode.Fps:
                    // Just look at camera forward while character playing action animation
                    targetLookDirection = cameraForward;
                    break;
                case ShooterControllerViewMode.Tps:
                    // Turn character look direction to move direction while moving without doing any action
                    if (moveDirection.sqrMagnitude > 0f)
                    {
                        float currentTurnSpeed = CurrentTurnSpeed;
                        if (currentTurnSpeed > 0f)
                        {
                            Quaternion currentRot = Quaternion.LookRotation(targetLookDirection);
                            Quaternion targetRot = Quaternion.LookRotation(moveLookDirection);
                            currentRot = Quaternion.Slerp(currentRot, targetRot, currentTurnSpeed * Time.deltaTime);
                            targetLookDirection = currentRot * Vector3.forward;
                        }
                        else
                        {
                            // Turn immediately because turn speed <= 0
                            targetLookDirection = moveLookDirection;
                        }
                    }
                    break;
            }
        }

        protected virtual void UpdateLookAtTarget()
        {
            // Turn character to look direction immediately
            PlayerCharacterEntity.SetLookRotation(Quaternion.LookRotation(targetLookDirection));
        }

        public override void UseHotkey(HotkeyType type, string relateId, AimPosition aimPosition)
        {
            ClearQueueUsingSkill();
            switch (type)
            {
                case HotkeyType.Skill:
                    if (onBeforeUseSkillHotkey != null)
                        onBeforeUseSkillHotkey.Invoke(relateId, aimPosition);
                    UseSkill(relateId, aimPosition);
                    break;
                case HotkeyType.Item:
                    if (onBeforeUseItemHotkey != null)
                        onBeforeUseItemHotkey.Invoke(relateId, aimPosition);
                    UseItem(relateId, aimPosition);
                    break;
            }
        }

        protected virtual void UseSkill(string id, AimPosition aimPosition)
        {
            BaseSkill skill;
            short skillLevel;
            if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out skill) || skill == null ||
                !PlayerCharacterEntity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;
            SetQueueUsingSkill(aimPosition, skill, skillLevel);
        }

        protected virtual void UseItem(string id, AimPosition aimPosition)
        {
            int itemIndex;
            BaseItem item;
            int dataId = BaseGameData.MakeDataId(id);
            if (GameInstance.Items.ContainsKey(dataId))
            {
                item = GameInstance.Items[dataId];
                itemIndex = OwningCharacter.IndexOfNonEquipItem(dataId);
            }
            else
            {
                InventoryType inventoryType;
                byte equipWeaponSet;
                CharacterItem characterItem;
                if (PlayerCharacterEntity.IsEquipped(
                    id,
                    out inventoryType,
                    out itemIndex,
                    out equipWeaponSet,
                    out characterItem))
                {
                    GameInstance.ClientInventoryHandlers.RequestUnEquipItem(
                        inventoryType,
                        (short)itemIndex,
                        equipWeaponSet,
                        -1,
                        ClientInventoryActions.ResponseUnEquipArmor,
                        ClientInventoryActions.ResponseUnEquipWeapon);
                    return;
                }
                item = characterItem.GetItem();
            }

            if (itemIndex < 0)
                return;

            if (item == null)
                return;

            if (item.IsEquipment())
            {
                GameInstance.ClientInventoryHandlers.RequestEquipItem(
                        PlayerCharacterEntity,
                        (short)itemIndex,
                        ClientInventoryActions.ResponseEquipArmor,
                        ClientInventoryActions.ResponseEquipWeapon);
            }
            else if (item.IsSkill())
            {
                SetQueueUsingSkill(aimPosition, (item as ISkillItem).UsingSkill, (item as ISkillItem).UsingSkillLevel, (short)itemIndex);
            }
            else if (item.IsBuilding())
            {
                buildingItemIndex = itemIndex;
                if (showConfirmConstructionUI)
                {
                    // Show confirm UI
                    ShowConstructBuildingDialog();
                }
                else
                {
                    // Build when click
                    ConfirmBuild();
                }
                mustReleaseFireKey = true;
            }
            else if (item.IsUsable())
            {
                PlayerCharacterEntity.CallServerUseItem((short)itemIndex);
            }
        }

        public virtual void Attack(bool isLeftHand)
        {
            if (pauseFireInputFrames > 0)
                return;
            // Set this to `TRUE` to update crosshair
            if (PlayerCharacterEntity.Attack(isLeftHand))
                updateAttackingCrosshair = true;
        }

        public virtual void WeaponCharge(bool isLeftHand)
        {
            if (pauseFireInputFrames > 0)
                return;
            PlayerCharacterEntity.StartCharge(isLeftHand);
        }

        public virtual void Reload()
        {
            if (WeaponAbility != null && WeaponAbility.ShouldDeactivateWhenReload)
                WeaponAbility.ForceDeactivated();
            // Reload ammo at server
            if (!PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoFull())
                PlayerCharacterEntity.Reload(false);
            else if (!PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoFull())
                PlayerCharacterEntity.Reload(true);
        }

        public virtual void ActivateWeaponAbility()
        {
            if (WeaponAbility == null)
                return;

            if (WeaponAbilityState == WeaponAbilityState.Activated ||
                WeaponAbilityState == WeaponAbilityState.Activating)
                return;

            WeaponAbility.OnPreActivate();
            WeaponAbilityState = WeaponAbilityState.Activating;
        }

        protected virtual void UpdateWeaponAbilityActivation(float deltaTime)
        {
            if (WeaponAbility == null)
                return;

            WeaponAbilityState = WeaponAbility.UpdateActivation(WeaponAbilityState, deltaTime);
        }

        protected virtual void DeactivateWeaponAbility()
        {
            if (WeaponAbility == null)
                return;

            if (WeaponAbilityState == WeaponAbilityState.Deactivated ||
                WeaponAbilityState == WeaponAbilityState.Deactivating)
                return;

            WeaponAbility.OnPreDeactivate();
            WeaponAbilityState = WeaponAbilityState.Deactivating;
        }

        public virtual void HoldActivate()
        {
            if (targetBuilding != null)
            {
                TargetEntity = targetBuilding;
                ShowCurrentBuildingDialog();
            }
        }

        public virtual void Activate()
        {
            // Priority Player -> Npc -> Buildings
            if (targetPlayer != null)
                CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
            else if (targetNpc != null)
                PlayerCharacterEntity.NpcAction.CallServerNpcActivate(targetNpc.ObjectId);
            else if (targetBuilding != null)
                ActivateBuilding(targetBuilding);
            else if (targetVehicle != null)
                PlayerCharacterEntity.CallServerEnterVehicle(targetVehicle.ObjectId);
            else if (targetWarpPortal != null)
                PlayerCharacterEntity.CallServerEnterWarp(targetWarpPortal.ObjectId);
            else if (targetItemsContainer != null)
                ShowItemsContainerDialog(targetItemsContainer);
        }

        public virtual void UseSkill(bool isLeftHand)
        {
            if (pauseFireInputFrames > 0)
                return;
            if (queueUsingSkill.skill != null)
            {
                if (queueUsingSkill.itemIndex >= 0)
                {
                    PlayerCharacterEntity.UseSkillItem(queueUsingSkill.itemIndex, isLeftHand, SelectedEntityObjectId, queueUsingSkill.aimPosition);
                }
                else
                {
                    PlayerCharacterEntity.UseSkill(queueUsingSkill.skill.DataId, isLeftHand, SelectedEntityObjectId, queueUsingSkill.aimPosition);
                }
            }
            ClearQueueUsingSkill();
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(CacheTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                if (overlapColliders[tempCounter].gameObject == target)
                    return true;
            }
            return false;
        }

        public bool IsUsingHotkey()
        {
            // Check using hotkey for PC only
            if (!InputManager.useMobileInputOnNonMobile &&
                !Application.isMobilePlatform &&
                UICharacterHotkeys.UsingHotkey != null)
            {
                return true;
            }
            return false;
        }

        public virtual bool GetPrimaryAttackButton()
        {
            return InputManager.GetButton("Fire1") || InputManager.GetButton("Attack");
        }

        public virtual bool GetSecondaryAttackButton()
        {
            return InputManager.GetButton("Fire2");
        }

        public virtual bool GetPrimaryAttackButtonUp()
        {
            return InputManager.GetButtonUp("Fire1") || InputManager.GetButtonUp("Attack");
        }

        public virtual bool GetSecondaryAttackButtonUp()
        {
            return InputManager.GetButtonUp("Fire2");
        }

        public virtual bool GetPrimaryAttackButtonDown()
        {
            return InputManager.GetButtonDown("Fire1") || InputManager.GetButtonDown("Attack");
        }

        public virtual bool GetSecondaryAttackButtonDown()
        {
            return InputManager.GetButtonDown("Fire2");
        }

        public virtual void UpdateViewMode()
        {
            dirtyViewMode = viewMode;
            UpdateCameraSettings();
            // Update camera zoom distance when change view mode only, to allow zoom controls
            CurrentCameraMinZoomDistance = CameraMinZoomDistance;
            CurrentCameraMaxZoomDistance = CameraMaxZoomDistance;
            CurrentCameraZoomDistance = CameraZoomDistance;
        }

        public virtual void UpdateCameraSettings()
        {
            CurrentCameraFov = CameraFov;
            CurrentCameraNearClipPlane = CameraNearClipPlane;
            CurrentCameraFarClipPlane = CameraFarClipPlane;
            PlayerCharacterEntity.ModelManager.SetIsFps(viewMode == ShooterControllerViewMode.Fps);
        }

        public virtual bool IsInFront(Vector3 position)
        {
            return Vector3.Angle(cameraForward, position - CacheTransform.position) < 115f;
        }

        public override AimPosition UpdateBuildAimControls(Vector2 aimAxes, BuildingEntity prefab)
        {
            // Instantiate constructing building
            if (ConstructingBuildingEntity == null)
            {
                InstantiateConstructingBuilding(prefab);
                buildYRotate = 0f;
            }
            // Rotate by keys
            Vector3 buildingAngles = Vector3.zero;
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                if (buildRotationSnap)
                {
                    if (InputManager.GetButtonDown("RotateLeft"))
                        buildYRotate -= buildRotateAngle;
                    if (InputManager.GetButtonDown("RotateRight"))
                        buildYRotate += buildRotateAngle;
                    // Make Y rotation set to 0, 90, 180
                    buildingAngles.y = buildYRotate = Mathf.Round(buildYRotate / buildRotateAngle) * buildRotateAngle;
                }
                else
                {
                    float deltaTime = Time.deltaTime;
                    if (InputManager.GetButton("RotateLeft"))
                        buildYRotate -= buildRotateSpeed * deltaTime;
                    if (InputManager.GetButton("RotateRight"))
                        buildYRotate += buildRotateSpeed * deltaTime;
                    // Rotate by set angles
                    buildingAngles.y = buildYRotate;
                }
                ConstructingBuildingEntity.BuildYRotation = buildYRotate;
            }
            // Clear area before next find
            ConstructingBuildingEntity.BuildingArea = null;
            // Default aim position (aim to sky/space)
            aimTargetPosition = centerRay.origin + centerRay.direction * (centerOriginToCharacterDistance + ConstructingBuildingEntity.BuildDistance - BuildingEntity.BUILD_DISTANCE_BUFFER);
            // Raycast from camera position to center of screen
            FindConstructingBuildingArea(centerRay, centerOriginToCharacterDistance + ConstructingBuildingEntity.BuildDistance - BuildingEntity.BUILD_DISTANCE_BUFFER);
            // Not hit ground
            if (!ConstructingBuildingEntity.HitSurface)
            {
                // Find nearest grounded position
                FindConstructingBuildingArea(new Ray(aimTargetPosition, Vector3.down), 100f);
            }
            if (!ConstructingBuildingEntity.IsPositionInBuildDistance(CacheTransform.position, aimTargetPosition))
            {
                // Clamp building position to be within build distance
                aimTargetPosition = GameplayUtils.ClampPosition(CacheTransform.position, aimTargetPosition, ConstructingBuildingEntity.BuildDistance - BuildingEntity.BUILD_DISTANCE_BUFFER);
            }
            // Place constructing building
            if ((ConstructingBuildingEntity.BuildingArea && !ConstructingBuildingEntity.BuildingArea.snapBuildingObject) ||
                !ConstructingBuildingEntity.BuildingArea)
            {
                // Place the building on the ground when the building area is not snapping
                // Or place it anywhere if there is no building area
                // It's also no snapping build area, so set building rotation by camera look direction
                ConstructingBuildingEntity.Position = aimTargetPosition;
                // Rotate to camera
                Vector3 direction = aimTargetPosition - CacheGameplayCameraController.CameraTransform.position;
                direction.y = 0f;
                direction.Normalize();
                ConstructingBuildingEntity.CacheTransform.eulerAngles = Quaternion.LookRotation(direction).eulerAngles + (Vector3.up * buildYRotate);
            }
            return AimPosition.CreatePosition(ConstructingBuildingEntity.Position);
        }

        protected int FindConstructingBuildingArea(Ray ray, float distance)
        {
            ConstructingBuildingEntity.BuildingArea = null;
            ConstructingBuildingEntity.HitSurface = false;
            int tempCount = PhysicUtils.SortedRaycastNonAlloc3D(ray.origin, ray.direction, raycasts, distance, CurrentGameInstance.GetBuildLayerMask());
            RaycastHit tempHitInfo;
            BuildingEntity buildingEntity;
            BuildingArea buildingArea;
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = raycasts[tempCounter];
                if (ConstructingBuildingEntity.CacheTransform.root == tempHitInfo.transform.root)
                {
                    // Hit collider which is part of constructing building entity, skip it
                    continue;
                }

                aimTargetPosition = tempHitInfo.point;

                if (!IsInFront(tempHitInfo.point))
                {
                    // Skip because this position is not allowed to build the building
                    continue;
                }

                // Find ground position from upper position
                Vector3 raycastOrigin = tempHitInfo.point + Vector3.up * 100f * 0.5f;
                RaycastHit[] groundHits = Physics.RaycastAll(raycastOrigin, Vector3.down, 100f, CurrentGameInstance.GetBuildLayerMask());
                for (int j = 0; j < groundHits.Length; ++j)
                {
                    if (groundHits[j].transform == tempHitInfo.transform)
                        aimTargetPosition = groundHits[j].point;
                }

                buildingEntity = tempHitInfo.transform.root.GetComponent<BuildingEntity>();
                buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                if ((buildingArea == null || !ConstructingBuildingEntity.BuildingTypes.Contains(buildingArea.buildingType))
                    && buildingEntity == null)
                {
                    // Hit surface which is not building area or building entity
                    ConstructingBuildingEntity.BuildingArea = null;
                    ConstructingBuildingEntity.HitSurface = true;
                    break;
                }

                if (buildingArea == null || !ConstructingBuildingEntity.BuildingTypes.Contains(buildingArea.buildingType))
                {
                    // Skip because this area is not allowed to build the building that you are going to build
                    continue;
                }

                // Found building area which can construct the building
                ConstructingBuildingEntity.BuildingArea = buildingArea;
                ConstructingBuildingEntity.HitSurface = true;
                break;
            }
            return tempCount;
        }

        public override void FinishBuildAimControls(bool isCancel)
        {
            if (isCancel)
                CancelBuild();
        }

        public override void ConfirmBuild()
        {
            base.ConfirmBuild();
            pauseFireInputFrames = PAUSE_FIRE_INPUT_FRAMES_AFTER_CONFIRM_BUILD;
        }
    }
}
