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
        [FormerlySerializedAs("turnSpeedWileCrawling")]
        protected float turnSpeedWhileCrawling = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        [FormerlySerializedAs("turnSpeedWileSwimming")]
        protected float turnSpeedWhileSwimming = 0f;
        [SerializeField]
        [Tooltip("Use this to turn character smoothly, Set this <= 0 to turn immediately")]
        [FormerlySerializedAs("turnSpeedWileDoingAction")]
        protected float turnSpeedWhileDoingAction = 0f;

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
        /// <summary>
        /// RelateId (string), AimPosition (AimPosition)
        /// </summary>
        public event System.Action<string, AimPosition> onBeforeUseSkillHotkey;
        /// <summary>
        /// RelateId (string), AimPosition (AimPosition)
        /// </summary>
        public event System.Action<string, AimPosition> onAfterUseSkillHotkey;
        /// <summary>
        /// RelateId (string), AimPosition (AimPosition)
        /// </summary>
        public event System.Action<string, AimPosition> onBeforeUseItemHotkey;
        /// <summary>
        /// RelateId (string), AimPosition (AimPosition)
        /// </summary>
        public event System.Action<string, AimPosition> onAfterUseItemHotkey;
        #endregion

        public byte HotkeyEquipWeaponSet { get; set; }
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
                    if (PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                    {
                        return tpsTargetOffsetWhileCrouching;
                    }
                    else if (PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
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
                    if (PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                    {
                        return fpsTargetOffsetWhileCrouching;
                    }
                    else if (PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
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
                if (PlayingCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
                    return turnSpeedWhileSwimming;
                switch (PlayingCharacterEntity.ExtraMovementState)
                {
                    case ExtraMovementState.IsSprinting:
                        return turnSpeedWhileSprinting;
                    case ExtraMovementState.IsWalking:
                        return turnSpeedWhileWalking;
                    case ExtraMovementState.IsCrouching:
                        return turnSpeedWhileCrouching;
                    case ExtraMovementState.IsCrawling:
                        return turnSpeedWhileCrawling;
                }
                return turnSpeed;
            }
        }

        // Input data
        protected InputStateManager _activateInput;
        protected InputStateManager _pickupItemInput;
        protected InputStateManager _reloadInput;
        protected InputStateManager _exitVehicleInput;
        protected InputStateManager _switchEquipWeaponSetInput;
        protected float _lastAimmingTime;
        protected bool _updatingInputs;
        // Entity detector
        protected NearbyEntityDetector _warpPortalEntityDetector;
        // Temp physic variables
        protected RaycastHit[] _raycasts = new RaycastHit[100];
        protected Collider[] _overlapColliders = new Collider[200];
        // Temp target
        protected IActivatableEntity _activatableEntity;
        protected IHoldActivatableEntity _holdActivatableEntity;
        // Temp data
        protected Ray _centerRay;
        protected float _centerOriginToCharacterDistance;
        protected Vector3 _moveDirection;
        protected Vector3 _cameraForward;
        protected Vector3 _cameraRight;
        protected float _inputV;
        protected float _inputH;
        protected Vector2 _normalizedInput;
        protected Vector3 _moveLookDirection;
        protected Vector3 _targetLookDirection;
        protected bool _tempPressAttackRight;
        protected bool _tempPressAttackLeft;
        protected bool _tempPressWeaponAbility;
        protected bool _isLeftHandAttacking;
        protected Vector3 _aimTargetPosition;
        protected Vector3 _turnDirection;
        // Controlling states
        protected bool _toggleSprintOn;
        protected bool _toggleWalkOn;
        protected bool _toggleCrouchOn;
        protected bool _toggleCrawlOn;
        protected ShooterControllerViewMode _dirtyViewMode;
        protected IWeaponItem _rightHandWeapon;
        protected IWeaponItem _leftHandWeapon;
        protected MovementState _movementState;
        protected ExtraMovementState _extraMovementState;
        protected ShooterControllerViewMode? _viewModeBeforeDead;
        protected bool _updateAttackingCrosshair;
        protected bool _updateAttackedCrosshair;
        protected bool _mustReleaseFireKey;
        protected byte _pauseFireInputFrames;
        protected bool _isAimming;
        protected bool _isCharging;
        protected bool _isAlreadyReloaded;

        protected override void Awake()
        {
            base.Awake();
            // Initial gameplay camera controller
            CacheGameplayCameraController = gameObject.GetOrAddComponent<IShooterGameplayCameraController, ShooterGameplayCameraController>((obj) =>
            {
                ShooterGameplayCameraController castedObj = obj as ShooterGameplayCameraController;
                castedObj.SetData(gameplayCameraPrefab);
            });
            CacheGameplayCameraController.Init();
            CameraRotationSpeedScale = DefaultCameraRotationSpeedScale;
            // Initial minimap camera controller
            CacheMinimapCameraController = gameObject.GetOrAddComponent<IMinimapCameraController, DefaultMinimapCameraController>((obj) =>
            {
                DefaultMinimapCameraController castedObj = obj as DefaultMinimapCameraController;
                castedObj.SetData(minimapCameraPrefab);
            });
            CacheMinimapCameraController.Init();
            // Initial build aim controller
            BuildAimController = gameObject.GetOrAddComponent<IShooterBuildAimController, ShooterBuildAimController>((obj) =>
            {
                ShooterBuildAimController castedObj = obj as ShooterBuildAimController;
                castedObj.SetData(buildRotationSnap, buildRotateAngle, buildRotateSpeed);
            });
            BuildAimController.Init();
            // Initial area skill aim controller
            AreaSkillAimController = gameObject.GetOrAddComponent<IAreaSkillAimController, ShooterAreaSkillAimController>();

            _buildingItemIndex = -1;
            _isLeftHandAttacking = false;
            ConstructingBuildingEntity = null;
            _activateInput = new InputStateManager("Activate");
            _pickupItemInput = new InputStateManager("PickUpItem");
            _reloadInput = new InputStateManager("Reload");
            _exitVehicleInput = new InputStateManager("ExitVehicle");
            _switchEquipWeaponSetInput = new InputStateManager("SwitchEquipWeaponSet");

            // Initialize warp portal entity detector
            GameObject tempGameObject = new GameObject("_WarpPortalEntityDetector");
            _warpPortalEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            _warpPortalEntityDetector.detectingRadius = CurrentGameInstance.conversationDistance;
            _warpPortalEntityDetector.findWarpPortal = true;
        }

        protected override async void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);
            CacheGameplayCameraController.Setup(characterEntity);
            CacheMinimapCameraController.Setup(characterEntity);

            if (characterEntity == null)
                return;

            _targetLookDirection = MovementTransform.forward;
            SetupEquipWeapons(characterEntity.EquipWeapons);
            characterEntity.onEquipWeaponSetChange += SetupEquipWeapons;
            characterEntity.onSelectableWeaponSetsOperation += SetupEquipWeapons;
            characterEntity.onLaunchDamageEntity += OnLaunchDamageEntity;
            if (CacheFpsModel != null)
                Destroy(CacheFpsModel.gameObject);
            CacheFpsModel = characterEntity.ModelManager.InstantiateFpsModel(CacheGameplayCameraController.CameraTransform);
            await UniTask.NextFrame();
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
            if (_warpPortalEntityDetector != null)
                Destroy(_warpPortalEntityDetector.gameObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected void SetupEquipWeapons(byte equipWeaponSet)
        {
            SetupEquipWeapons(PlayingCharacterEntity.EquipWeapons);
        }

        protected void SetupEquipWeapons(LiteNetLibManager.LiteNetLibSyncList.Operation operation, int index)
        {
            SetupEquipWeapons(PlayingCharacterEntity.EquipWeapons);
        }

        protected virtual void SetupEquipWeapons(EquipWeapons equipWeapons)
        {
            CurrentCrosshairSetting = PlayingCharacterEntity.GetCrosshairSetting();
            UpdateCrosshair(CurrentCrosshairSetting, false, -CurrentCrosshairSetting.shrinkPerFrame);

            _rightHandWeapon = equipWeapons.GetRightHandWeaponItem();
            _leftHandWeapon = equipWeapons.GetLeftHandWeaponItem();
            // Weapon ability will be able to use when equip weapon at main-hand only
            if (_rightHandWeapon != null && _leftHandWeapon == null)
            {
                if (_rightHandWeapon.WeaponAbility != WeaponAbility)
                {
                    if (WeaponAbility != null)
                        WeaponAbility.Desetup();
                    WeaponAbility = _rightHandWeapon.WeaponAbility;
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
            if (_rightHandWeapon == null)
                _rightHandWeapon = GameInstance.Singleton.DefaultWeaponItem;
            if (_leftHandWeapon == null)
                _leftHandWeapon = GameInstance.Singleton.DefaultWeaponItem;
        }

        protected override void Update()
        {
            if (_pauseFireInputFrames > 0)
                --_pauseFireInputFrames;

            if (PlayingCharacterEntity == null || !PlayingCharacterEntity.IsOwnerClient)
                return;

            CacheMinimapCameraController.FollowingEntityTransform = CameraTargetTransform;
            CacheMinimapCameraController.FollowingGameplayCameraTransform = CacheGameplayCameraController.CameraTransform;

            if (PlayingCharacterEntity.IsDead())
            {
                // Deactivate weapon ability immediately when dead
                if (WeaponAbility != null && WeaponAbilityState != WeaponAbilityState.Deactivated)
                {
                    WeaponAbility.ForceDeactivated();
                    WeaponAbilityState = WeaponAbilityState.Deactivated;
                }
                // Set view mode to TPS when character dead
                if (!_viewModeBeforeDead.HasValue)
                    _viewModeBeforeDead = ViewMode;
                ViewMode = ShooterControllerViewMode.Tps;
            }
            else
            {
                // Set view mode to view mode before dead when character alive
                if (_viewModeBeforeDead.HasValue)
                {
                    ViewMode = _viewModeBeforeDead.Value;
                    _viewModeBeforeDead = null;
                }
            }

            if (_dirtyViewMode != viewMode)
                UpdateViewMode();

            CacheGameplayCameraController.TargetOffset = CameraTargetOffset;
            CacheGameplayCameraController.EnableWallHitSpring = viewMode == ShooterControllerViewMode.Tps;
            CacheGameplayCameraController.FollowingEntityTransform = ViewMode == ShooterControllerViewMode.Fps ? PlayingCharacterEntity.FpsCameraTargetTransform : PlayingCharacterEntity.CameraTargetTransform;

            // Set temp data
            float tempDeltaTime = Time.deltaTime;

            // Update inputs
            _activateInput.OnUpdate(tempDeltaTime);
            _pickupItemInput.OnUpdate(tempDeltaTime);
            _reloadInput.OnUpdate(tempDeltaTime);
            _exitVehicleInput.OnUpdate(tempDeltaTime);
            _switchEquipWeaponSetInput.OnUpdate(tempDeltaTime);

            // Check is any UIs block controller or not?
            bool isBlockController = UISceneGameplay.IsBlockController();

            // Lock cursor when not show UIs
            if (GameInstance.Singleton.IsMobileTestInEditor() || Application.isMobilePlatform)
            {
                // Control camera by touch-screen
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                CacheGameplayCameraController.UpdateRotationX = InputManager.GetButton("CameraRotate");
                CacheGameplayCameraController.UpdateRotationY = InputManager.GetButton("CameraRotate") && PlayingCharacterEntity.CanTurn();
                CacheGameplayCameraController.UpdateRotation = false;
                CacheGameplayCameraController.UpdateZoom = !isBlockController;
            }
            else
            {
                // Control camera by mouse-move
                Cursor.lockState = !isBlockController ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = isBlockController;
                CacheGameplayCameraController.UpdateRotationX = !isBlockController;
                CacheGameplayCameraController.UpdateRotationY = !isBlockController && PlayingCharacterEntity.CanTurn();
                CacheGameplayCameraController.UpdateRotation = false;
                CacheGameplayCameraController.UpdateZoom = !isBlockController;
            }
            isBlockController |= GenericUtils.IsFocusInputField();

            // Clear selected entity
            SelectedEntity = null;

            // Update crosshair (with states from last update)
            UpdateCrosshair();

            // Clear controlling states from last update
            _movementState = MovementState.None;
            _extraMovementState = ExtraMovementState.None;
            CacheGameplayCameraController.CameraRotationSpeedScale = DefaultCameraRotationSpeedScale;
            UpdateWeaponAbilityActivation(tempDeltaTime);

            // Prepare variables to find nearest raycasted hit point
            _centerRay = CacheGameplayCameraController.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            _centerOriginToCharacterDistance = Vector3.Distance(_centerRay.origin, EntityTransform.position);
            _cameraForward = CacheGameplayCameraController.CameraTransform.forward;
            _cameraForward.y = 0f;
            _cameraForward.Normalize();
            _cameraRight = CacheGameplayCameraController.CameraTransform.right;
            _cameraRight.y = 0f;
            _cameraRight.Normalize();

            // Update look target and aim position
            if (ConstructingBuildingEntity == null)
                UpdateTarget_BattleMode(isBlockController);
            else
                UpdateTarget_BuildMode(isBlockController);

            // Update movement inputs
            if (isBlockController)
            {
                // Clear movement inputs
                _moveDirection = Vector3.zero;
                DeactivateWeaponAbility();
            }
            else
            {
                // Update movement and camera pitch
                UpdateMovementInputs();
            }

            // Update aim position
            bool isCharacterTurnForwarding = Mathf.Abs(PlayingCharacterEntity.EntityTransform.eulerAngles.y - CacheGameplayCameraController.CameraTransform.eulerAngles.y) < 15f;
            bool isAimingToCenterOfScreen = Mode == ControllerMode.Combat || turnForwardWhileDoingAction || isCharacterTurnForwarding;
            if (isAimingToCenterOfScreen)
            {
                // Aim to center of screen
                PlayingCharacterEntity.AimPosition = PlayingCharacterEntity.GetAttackAimPosition(ref _isLeftHandAttacking, _aimTargetPosition);
            }
            else
            {
                // Aim to character direction
                Vector3 direction = PlayingCharacterEntity.EntityTransform.forward;
                Vector3 angles = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
                angles = new Vector3(CacheGameplayCameraController.CameraTransform.eulerAngles.x, angles.y, angles.z);
                direction = Quaternion.Euler(angles) * Vector3.forward;
                PlayingCharacterEntity.AimPosition = PlayingCharacterEntity.GetAttackAimPositionByDirection(ref _isLeftHandAttacking, direction, false);
            }

            _isAimming = false;
            // Update input
            if (!_updatingInputs)
            {
                if (ConstructingBuildingEntity == null)
                    UpdateInputs_BattleMode(isBlockController).Forget();
                else
                    UpdateInputs_BuildMode(isBlockController).Forget();
            }

            // Hide Npc UIs when move
            if (_moveDirection.sqrMagnitude > 0f)
                HideNpcDialog();

            // If jumping add jump state
            if (!isBlockController)
            {
                if (InputManager.GetButtonDown("Jump"))
                {
                    if (unToggleCrouchWhenJump && PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrouching)
                        _toggleCrouchOn = false;
                    else if (unToggleCrawlWhenJump && PlayingCharacterEntity.ExtraMovementState == ExtraMovementState.IsCrawling)
                        _toggleCrawlOn = false;
                    else
                        _movementState |= MovementState.IsJump;
                }
                else if (PlayingCharacterEntity.MovementState.Has(MovementState.IsGrounded))
                {
                    if (DetectExtraActive("Sprint", sprintActiveMode, ref _toggleSprintOn))
                    {
                        _extraMovementState = ExtraMovementState.IsSprinting;
                        _toggleWalkOn = false;
                        _toggleCrouchOn = false;
                        _toggleCrawlOn = false;
                    }
                    if (DetectExtraActive("Walk", walkActiveMode, ref _toggleWalkOn))
                    {
                        _extraMovementState = ExtraMovementState.IsWalking;
                        _toggleSprintOn = false;
                        _toggleCrouchOn = false;
                        _toggleCrawlOn = false;
                    }
                    if (DetectExtraActive("Crouch", crouchActiveMode, ref _toggleCrouchOn))
                    {
                        _extraMovementState = ExtraMovementState.IsCrouching;
                        _toggleSprintOn = false;
                        _toggleWalkOn = false;
                        _toggleCrawlOn = false;
                    }
                    if (DetectExtraActive("Crawl", crawlActiveMode, ref _toggleCrawlOn))
                    {
                        _extraMovementState = ExtraMovementState.IsCrawling;
                        _toggleSprintOn = false;
                        _toggleWalkOn = false;
                        _toggleCrouchOn = false;
                    }
                }
            }
            if (_moveDirection.magnitude > 0f)
            {
                switch (mode)
                {
                    case ControllerMode.Adventure:
                        if (_isAimming)
                            _movementState |= GameplayUtils.GetMovementStateByDirection(_moveDirection, MovementTransform.forward);
                        else
                            _movementState |= MovementState.Forward;
                        break;
                    case ControllerMode.Combat:
                        _movementState |= GameplayUtils.GetMovementStateByDirection(_moveDirection, MovementTransform.forward);
                        break;
                }
            }
            PlayingCharacterEntity.KeyMovement(_moveDirection, _movementState);
            PlayingCharacterEntity.SetExtraMovementState(_extraMovementState);
            PlayingCharacterEntity.SetSmoothTurnSpeed(0f);
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
            if (PlayingCharacterEntity.MovementState.Has(MovementState.IsUnderWater))
            {
                // Clear toggled sprint, crouch and crawl
                _toggleSprintOn = false;
                _toggleWalkOn = false;
                _toggleCrouchOn = false;
                _toggleCrawlOn = false;
            }
            // Update inputs
            _activateInput.OnLateUpdate();
            _pickupItemInput.OnLateUpdate();
            _reloadInput.OnLateUpdate();
            _exitVehicleInput.OnLateUpdate();
            _switchEquipWeaponSetInput.OnLateUpdate();
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

        protected virtual void UpdateTarget_BattleMode(bool isBlockController)
        {
            // Prepare raycast distance / fov
            float attackDistance = 0f;
            bool attacking = false;
            if (IsUsingHotkey())
            {
                _mustReleaseFireKey = true;
            }
            else
            {
                // Attack with right hand weapon
                _tempPressAttackRight = !isBlockController && GetPrimaryAttackButton();
                if (WeaponAbility == null && _leftHandWeapon != null)
                {
                    // Attack with left hand weapon if left hand weapon not empty
                    _tempPressAttackLeft = !isBlockController && GetSecondaryAttackButton();
                }
                else if (WeaponAbility != null)
                {
                    // Use weapon ability if it can
                    _tempPressWeaponAbility = !isBlockController && GetSecondaryAttackButtonDown();
                }

                attacking = _tempPressAttackRight || _tempPressAttackLeft;
                if (attacking && !PlayingCharacterEntity.IsAttacking && !PlayingCharacterEntity.IsUsingSkill)
                {
                    // Priority is right > left
                    _isLeftHandAttacking = !_tempPressAttackRight && _tempPressAttackLeft;
                }

                // Calculate aim distance by skill or weapon
                if (PlayingCharacterEntity.UsingSkill != null && PlayingCharacterEntity.UsingSkill.IsAttack)
                {
                    // Increase aim distance by skill attack distance
                    attackDistance = PlayingCharacterEntity.UsingSkill.GetCastDistance(PlayingCharacterEntity, PlayingCharacterEntity.UsingSkillLevel, _isLeftHandAttacking);
                    attacking = true;
                }
                else if (_queueUsingSkill.skill != null && _queueUsingSkill.skill.IsAttack)
                {
                    // Increase aim distance by skill attack distance
                    attackDistance = _queueUsingSkill.skill.GetCastDistance(PlayingCharacterEntity, _queueUsingSkill.level, _isLeftHandAttacking);
                    attacking = true;
                }
                else
                {
                    // Increase aim distance by attack distance
                    attackDistance = PlayingCharacterEntity.GetAttackDistance(_isLeftHandAttacking);
                }
            }
            // Temporary variables
            DamageableHitBox tempHitBox;
            RaycastHit tempHitInfo;
            float tempDistance;
            // Default aim position (aim to sky/space)
            _aimTargetPosition = _centerRay.origin + _centerRay.direction * (_centerOriginToCharacterDistance + attackDistance);
            // Aim to damageable hit boxes (higher priority than other entities)
            // Raycast from camera position to center of screen
            int tempCount = PhysicUtils.SortedRaycastNonAlloc3D(_centerRay.origin, _centerRay.direction, _raycasts, _centerOriginToCharacterDistance + attackDistance, Physics.DefaultRaycastLayers);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = _raycasts[tempCounter];

                if (tempHitInfo.transform.gameObject.layer == PhysicLayers.TransparentFX ||
                    tempHitInfo.transform.gameObject.layer == PhysicLayers.IgnoreRaycast ||
                    tempHitInfo.transform.gameObject.layer == PhysicLayers.Water)
                {
                    // Skip some layers
                    continue;
                }

                if (!tempHitInfo.collider.GetComponent<IUnHittable>().IsNull())
                {
                    // Don't aim to unhittable objects
                    continue;
                }

                // Get damageable hit box component from hit target
                tempHitBox = tempHitInfo.collider.GetComponent<DamageableHitBox>();

                if (tempHitBox == null || !tempHitBox.Entity || tempHitBox.IsHide() ||
                    tempHitBox.GetObjectId() == PlayingCharacterEntity.ObjectId)
                {
                    // Skip empty game entity / hidding entity / controlling player's entity
                    continue;
                }

                // Entity isn't in front of character, so it's not the target
                if (turnForwardWhileDoingAction && !IsInFront(tempHitInfo.point))
                    continue;

                // Skip dead entity while attacking (to allow to use resurrect skills)
                if (attacking && tempHitBox.IsDead())
                    continue;

                // Entity is in front of character, so this is target
                if (tempHitBox.CanReceiveDamageFrom(PlayingCharacterEntity.GetInfo()))
                    _aimTargetPosition = tempHitInfo.point;
                SelectedEntity = tempHitBox.Entity;
                break;
            }

            // Aim to activateable entities if it can't find attacking target
            if (SelectedGameEntity == null && !attacking)
            {
                SelectedEntity = null;
                IGameEntity tempGameEntity;
                IBaseActivatableEntity tempActivatableEntity;
                // Raycast from camera position to center of screen
                tempCount = PhysicUtils.SortedRaycastNonAlloc3D(_centerRay.origin, _centerRay.direction, _raycasts, _centerOriginToCharacterDistance + findTargetRaycastDistance, CurrentGameInstance.GetTargetLayerMask());
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = _raycasts[tempCounter];
                    if (!tempHitInfo.collider.GetComponent<IUnHittable>().IsNull())
                    {
                        // Don't aim to unhittable objects
                        continue;
                    }

                    // Get distance between character and raycast hit point
                    tempDistance = Vector3.Distance(EntityTransform.position, tempHitInfo.point);
                    tempGameEntity = tempHitInfo.collider.GetComponent<IGameEntity>();

                    if (tempGameEntity.IsNull() || tempGameEntity.IsHide() ||
                        tempGameEntity.GetObjectId() == PlayingCharacterEntity.ObjectId)
                    {
                        // Skip empty game entity / hiddeing entity / controlling player's entity
                        continue;
                    }

                    tempActivatableEntity = tempGameEntity as IBaseActivatableEntity;
                    if (tempActivatableEntity != null && tempDistance <= tempActivatableEntity.GetActivatableDistance())
                    {
                        // Entity is in front of character, so this is target
                        SelectedEntity = tempGameEntity.Entity;
                        break;
                    }
                }
            }

            // Calculate aim direction
            _turnDirection = _aimTargetPosition - EntityTransform.position;
            _turnDirection.y = 0f;
            _turnDirection.Normalize();
            // Show target hp/mp
            UISceneGameplay.SetTargetEntity(SelectedGameEntity);
            PlayingCharacterEntity.SetTargetEntity(SelectedGameEntity);
            // Update aim assist
            CacheGameplayCameraController.EnableAimAssist = enableAimAssist && (_tempPressAttackRight || _tempPressAttackLeft || !aimAssistOnFireOnly) && !(SelectedGameEntity is IDamageableEntity);
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

        protected virtual void UpdateTarget_BuildMode(bool isBlockController)
        {
            // Disable aim assist while constucting the building
            CacheGameplayCameraController.EnableAimAssist = false;
            // Update build aim controller
            (BuildAimController as IShooterBuildAimController)?.UpdateCameraLookData(_centerRay, _centerOriginToCharacterDistance, _cameraForward, _cameraRight);
        }

        protected virtual void UpdateMovementInputs()
        {
            float pitch = CacheGameplayCameraController.CameraTransform.eulerAngles.x;

            // Update charcter pitch
            PlayingCharacterEntity.Pitch = pitch;

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !GameInstance.Singleton.IsMobileTestInEditor() && !Application.isMobilePlatform && !GameInstance.Singleton.IsConsoleTestInEditor() && !Application.isConsolePlatform;
            _moveDirection = Vector3.zero;
            _inputV = InputManager.GetAxis("Vertical", raw);
            _inputH = InputManager.GetAxis("Horizontal", raw);
            _normalizedInput = new Vector2(_inputV, _inputH).normalized;
            _moveDirection += _cameraForward * _inputV;
            _moveDirection += _cameraRight * _inputH;
            if (_moveDirection.sqrMagnitude > 0f)
            {
                if (pitch > 180f)
                    pitch -= 360f;
                _moveDirection.y = -pitch / 90f;
            }

            // Set look direction
            switch (Mode)
            {
                case ControllerMode.Adventure:
                    _moveLookDirection = _moveDirection;
                    _moveLookDirection.y = 0f;
                    break;
                case ControllerMode.Combat:
                    _moveLookDirection = _cameraForward;
                    break;
            }

            if (ViewMode == ShooterControllerViewMode.Fps)
            {
                // Force turn to look direction
                _moveLookDirection = _cameraForward;
                _targetLookDirection = _cameraForward;
            }

            _moveDirection.Normalize();
        }

        protected virtual async UniTaskVoid UpdateInputs_BattleMode(bool isBlockController)
        {
            _updatingInputs = true;
            // Prepare fire type data
            FireType rightHandFireType = FireType.SingleFire;
            if (_rightHandWeapon != null)
            {
                rightHandFireType = _rightHandWeapon.FireType;
            }
            // Prepare fire type data
            FireType leftHandFireType = FireType.SingleFire;
            if (_leftHandWeapon != null)
            {
                leftHandFireType = _leftHandWeapon.FireType;
            }
            // Have to release fire key, then check press fire key later on next frame
            if (_mustReleaseFireKey)
            {
                _tempPressAttackRight = false;
                _tempPressAttackLeft = false;
                bool isButtonReleased;
                // If release fire key while charging, attack
                isButtonReleased = isBlockController || GetPrimaryAttackButtonUp() || !GetPrimaryAttackButton();
                if (!_isLeftHandAttacking && isButtonReleased)
                {
                    _mustReleaseFireKey = false;
                    await Aimming();
                    // Button released, start attacking while fire type is fire on release
                    if (rightHandFireType == FireType.FireOnRelease)
                        Attack(ref _isLeftHandAttacking);
                    _isCharging = false;
                }
                // If release fire key while charging, attack
                isButtonReleased = isBlockController || GetSecondaryAttackButtonUp() || !GetSecondaryAttackButton();
                if (_isLeftHandAttacking && isButtonReleased)
                {
                    _mustReleaseFireKey = false;
                    await Aimming();
                    // Button released, start attacking while fire type is fire on release
                    if (leftHandFireType == FireType.FireOnRelease)
                        Attack(ref _isLeftHandAttacking);
                    _isCharging = false;
                }
            }
            // Controller blocked, so don't do anything
            if (isBlockController)
            {
                _updatingInputs = false;
                return;
            }

            bool anyKeyPressed = false;
            if (_isCharging ||
                _queueUsingSkill.skill != null ||
                _tempPressAttackRight ||
                _tempPressAttackLeft ||
                _activateInput.IsPress ||
                _activateInput.IsRelease ||
                _activateInput.IsHold ||
                PlayingCharacterEntity.IsPlayingAttackOrUseSkillAnimation())
            {
                anyKeyPressed = true;
                // Find activatable entities in front of playing character from camera center
                // Check the playing character is playing action animation to turn character forwarding to aim position
                if (!_tempPressAttackRight && !_tempPressAttackLeft)
                {
                    if (_activateInput.IsHold)
                    {
                        _holdActivatableEntity = null;
                        if (SelectedEntity is IHoldActivatableEntity)
                        {
                            _holdActivatableEntity = SelectedEntity as IHoldActivatableEntity;
                        }
                    }
                    else if (_activateInput.IsRelease)
                    {
                        _activatableEntity = null;
                        if (SelectedEntity == null)
                        {
                            if (_warpPortalEntityDetector?.warpPortals.Count > 0)
                            {
                                // It may not able to raycast from inside warp portal, so try to get it from the detector
                                _activatableEntity = _warpPortalEntityDetector.warpPortals[0];
                            }
                        }
                        else
                        {
                            if (SelectedEntity is IActivatableEntity)
                            {
                                _activatableEntity = SelectedEntity as IActivatableEntity;
                            }
                        }
                    }
                }

                // Update look direction
                if (PlayingCharacterEntity.IsPlayingAttackOrUseSkillAnimation() || _isCharging)
                {
                    await Aimming();
                }
                else if (_queueUsingSkill.skill != null)
                {
                    await Aimming();
                    UseSkill(_isLeftHandAttacking);
                }
                else if (_tempPressAttackRight || _tempPressAttackLeft)
                {
                    await Aimming();
                    if (!_isLeftHandAttacking)
                    {
                        // Fire on release weapons have to release to fire, so when start holding, play weapon charge animation
                        if (rightHandFireType == FireType.FireOnRelease)
                        {
                            _isCharging = true;
                            WeaponCharge(ref _isLeftHandAttacking);
                        }
                        else
                        {
                            _isCharging = false;
                            Attack(ref _isLeftHandAttacking);
                        }
                    }
                    else
                    {
                        // Fire on release weapons have to release to fire, so when start holding, play weapon charge animation
                        if (leftHandFireType == FireType.FireOnRelease)
                        {
                            _isCharging = true;
                            WeaponCharge(ref _isLeftHandAttacking);
                        }
                        else
                        {
                            _isCharging = false;
                            Attack(ref _isLeftHandAttacking);
                        }
                    }
                }
                else if (_activateInput.IsHold)
                {
                    if (_holdActivatableEntity != null && _holdActivatableEntity.CanHoldActivate())
                    {
                        await Aimming();
                        HoldActivate();
                    }
                }
                else if (_activateInput.IsRelease)
                {
                    if (_activatableEntity != null && _activatableEntity.CanActivate())
                    {
                        await Aimming();
                        Activate();
                    }
                }
            }

            if (_tempPressWeaponAbility)
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

            if (_pickupItemInput.IsPress)
            {
                anyKeyPressed = true;
                // Find for item to pick up
                if (SelectedEntity is IPickupActivatableEntity)
                {
                    IPickupActivatableEntity pickupActivatableEntity = SelectedEntity as IPickupActivatableEntity;
                    if (pickupActivatableEntity != null && pickupActivatableEntity.CanPickupActivate())
                        pickupActivatableEntity.OnPickupActivate();
                }
            }

            if (_reloadInput.IsPress)
            {
                anyKeyPressed = true;
                // Reload ammo when press the button
                Reload(true);
            }

            if (_exitVehicleInput.IsPress)
            {
                anyKeyPressed = true;
                // Exit vehicle
                PlayingCharacterEntity.CallServerExitVehicle();
            }

            if (_switchEquipWeaponSetInput.IsPress)
            {
                anyKeyPressed = true;
                // Switch equip weapon set
                GameInstance.ClientInventoryHandlers.RequestSwitchEquipWeaponSet(new RequestSwitchEquipWeaponSetMessage()
                {
                    equipWeaponSet = (byte)(PlayingCharacterEntity.EquipWeaponSet + 1),
                }, ClientInventoryActions.ResponseSwitchEquipWeaponSet);
            }

            // Setup releasing state
            if (_tempPressAttackRight && rightHandFireType != FireType.Automatic)
            {
                // The weapon's fire mode is single fire or fire on release, so player have to release fire key for next fire
                _mustReleaseFireKey = true;
            }
            if (_tempPressAttackLeft && leftHandFireType != FireType.Automatic)
            {
                // The weapon's fire mode is single fire or fire on release, so player have to release fire key for next fire
                _mustReleaseFireKey = true;
            }

            // Reloading
            if (PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
            {
                switch (emptyAmmoAutoReload)
                {
                    case EmptyAmmoAutoReload.ReloadImmediately:
                        Reload();
                        break;
                    case EmptyAmmoAutoReload.ReloadOnKeysReleased:
                        // Auto reload when ammo empty
                        if (!_tempPressAttackRight && !_tempPressAttackLeft && !_reloadInput.IsPress)
                        {
                            // Reload ammo when empty and not press any keys
                            Reload();
                        }
                        break;
                }
            }
            else
            {
                // Ammo filled, set reloaded state to `FALSE` to make it reload auto-reload later again
                _isAlreadyReloaded = false;
            }

            // Update look direction
            if (!anyKeyPressed)
            {
                // Update look direction while moving without doing any action
                if (Time.unscaledTime - _lastAimmingTime < durationBeforeStopAimming)
                {
                    await Aimming();
                }
                else
                {
                    SetTargetLookDirectionWhileMoving();
                }
            }
            _updatingInputs = false;
        }

        protected virtual async UniTaskVoid UpdateInputs_BuildMode(bool isBlockController)
        {
            SetTargetLookDirectionWhileMoving();
            _updatingInputs = false;
            await UniTask.Yield();
        }

        protected virtual void UpdateCrosshair()
        {
            bool isMoving = _movementState.Has(MovementState.Forward) ||
                _movementState.Has(MovementState.Backward) ||
                _movementState.Has(MovementState.Left) ||
                _movementState.Has(MovementState.Right) ||
                _movementState.Has(MovementState.IsJump);
            if (_updateAttackingCrosshair)
            {
                UpdateCrosshair(CurrentCrosshairSetting, true, CurrentCrosshairSetting.expandPerFrameWhileAttacking);
                _updateAttackingCrosshair = false;
                _updateAttackedCrosshair = true;
            }
            else if (_updateAttackedCrosshair)
            {
                UpdateCrosshair(CurrentCrosshairSetting, true, CurrentCrosshairSetting.shrinkPerFrameWhenAttacked);
                _updateAttackedCrosshair = false;
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
            if (_movementState.Has(MovementState.Forward) ||
                _movementState.Has(MovementState.Backward) ||
                _movementState.Has(MovementState.Left) ||
                _movementState.Has(MovementState.Right))
            {
                if (_movementState.Has(MovementState.IsUnderWater))
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileSwimming;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileSwimming;
                }
                else if (_extraMovementState == ExtraMovementState.IsSprinting)
                {
                    recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileSprinting;
                    recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileSprinting;
                }
                else if (_extraMovementState == ExtraMovementState.IsWalking)
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
            else if (_extraMovementState == ExtraMovementState.IsCrouching)
            {
                recoilX = CurrentCrosshairSetting.recoilX * recoilRateWhileCrouching;
                recoilY = CurrentCrosshairSetting.recoilY * recoilRateWhileCrouching;
            }
            else if (_extraMovementState == ExtraMovementState.IsCrawling)
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

        public void OnLaunchDamageEntity(
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition)
        {
            UpdateRecoil();
        }

        protected virtual async UniTask Aimming()
        {
            while (!SetTargetLookDirectionWhileDoingAction())
            {
                _isAimming = true;
                _lastAimmingTime = Time.unscaledTime;
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
                    _targetLookDirection = _cameraForward;
                    return PlayingCharacterEntity.CanDoNextAction();
                case ShooterControllerViewMode.Tps:
                    // Just look at camera forward while character playing action animation while `turnForwardWhileDoingAction` is `true`
                    Vector3 doActionLookDirection = turnForwardWhileDoingAction ? _cameraForward : (_moveLookDirection.sqrMagnitude > 0f ? _moveLookDirection : _targetLookDirection);
                    if (turnSpeedWhileDoingAction > 0f)
                    {
                        Quaternion currentRot = Quaternion.LookRotation(_targetLookDirection);
                        Quaternion targetRot = Quaternion.LookRotation(doActionLookDirection);
                        currentRot = Quaternion.Slerp(currentRot, targetRot, turnSpeedWhileDoingAction * Time.deltaTime);
                        _targetLookDirection = currentRot * Vector3.forward;
                        return Quaternion.Angle(currentRot, targetRot) <= 15f && PlayingCharacterEntity.CanDoNextAction();
                    }
                    else
                    {
                        // Turn immediately because turn speed <= 0
                        _targetLookDirection = doActionLookDirection;
                        return PlayingCharacterEntity.CanDoNextAction();
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
                    _targetLookDirection = _cameraForward;
                    break;
                case ShooterControllerViewMode.Tps:
                    // Turn character look direction to move direction while moving without doing any action
                    if (_moveDirection.sqrMagnitude > 0f)
                    {
                        float currentTurnSpeed = CurrentTurnSpeed;
                        if (currentTurnSpeed > 0f)
                        {
                            Quaternion currentRot = Quaternion.LookRotation(_targetLookDirection);
                            Quaternion targetRot = Quaternion.LookRotation(_moveLookDirection);
                            currentRot = Quaternion.Slerp(currentRot, targetRot, currentTurnSpeed * Time.deltaTime);
                            _targetLookDirection = currentRot * Vector3.forward;
                        }
                        else
                        {
                            // Turn immediately because turn speed <= 0
                            _targetLookDirection = _moveLookDirection;
                        }
                    }
                    break;
            }
        }

        protected virtual void UpdateLookAtTarget()
        {
            // Turn character to look direction immediately
            PlayingCharacterEntity.SetLookRotation(Quaternion.LookRotation(_targetLookDirection));
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
                    if (onAfterUseSkillHotkey != null)
                        onAfterUseSkillHotkey.Invoke(relateId, aimPosition);
                    break;
                case HotkeyType.Item:
                    HotkeyEquipWeaponSet = PlayingCharacterEntity.EquipWeaponSet;
                    if (onBeforeUseItemHotkey != null)
                        onBeforeUseItemHotkey.Invoke(relateId, aimPosition);
                    UseItem(relateId, aimPosition);
                    if (onAfterUseItemHotkey != null)
                        onAfterUseItemHotkey.Invoke(relateId, aimPosition);
                    break;
            }
        }

        protected virtual void UseSkill(string id, AimPosition aimPosition)
        {
            if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out BaseSkill skill) || skill == null ||
                !PlayingCharacterEntity.GetCaches().Skills.TryGetValue(skill, out int skillLevel))
                return;
            SetQueueUsingSkill(aimPosition, skill, skillLevel);
        }

        protected virtual void UseItem(string id, AimPosition aimPosition)
        {
            int itemIndex;
            int dataId = BaseGameData.MakeDataId(id);
            if (GameInstance.Items.TryGetValue(dataId, out BaseItem item))
            {
                itemIndex = GameInstance.PlayingCharacterEntity.IndexOfNonEquipItem(dataId);
            }
            else
            {
                if (PlayingCharacterEntity.IsEquipped(
                    id,
                    out InventoryType inventoryType,
                    out itemIndex,
                    out byte equipWeaponSet,
                    out CharacterItem characterItem))
                {
                    GameInstance.ClientInventoryHandlers.RequestUnEquipItem(
                        inventoryType,
                        itemIndex,
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
                        PlayingCharacterEntity,
                        itemIndex,
                        HotkeyEquipWeaponSet,
                        ClientInventoryActions.ResponseEquipArmor,
                        ClientInventoryActions.ResponseEquipWeapon);
            }
            else if (item.IsSkill())
            {
                SetQueueUsingSkill(aimPosition, (item as ISkillItem).UsingSkill, (item as ISkillItem).UsingSkillLevel, itemIndex);
            }
            else if (item.IsBuilding())
            {
                _buildingItemIndex = itemIndex;
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
                _mustReleaseFireKey = true;
            }
            else if (item.IsUsable())
            {
                PlayingCharacterEntity.CallServerUseItem(itemIndex);
            }
        }

        public virtual void Attack(ref bool isLeftHand)
        {
            if (_pauseFireInputFrames > 0)
                return;
            // Set this to `TRUE` to update crosshair
            if (PlayingCharacterEntity.Attack(ref isLeftHand))
                _updateAttackingCrosshair = true;
        }

        public virtual void WeaponCharge(ref bool isLeftHand)
        {
            if (_pauseFireInputFrames > 0)
                return;
            PlayingCharacterEntity.StartCharge(ref isLeftHand);
        }

        public virtual void Reload(bool forceReload = false)
        {
            if (!forceReload && _isAlreadyReloaded)
                return;
            if (WeaponAbility != null && WeaponAbility.ShouldDeactivateWhenReload)
                WeaponAbility.ForceDeactivated();
            // Reload ammo at server
            if (!PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoFull() && PlayingCharacterEntity.EquipWeapons.rightHand.HasAmmoToReload(PlayingCharacterEntity))
                PlayingCharacterEntity.Reload(false);
            else if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull() && PlayingCharacterEntity.EquipWeapons.leftHand.HasAmmoToReload(PlayingCharacterEntity))
                PlayingCharacterEntity.Reload(true);
            _isAlreadyReloaded = true;
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
            if (_holdActivatableEntity != null && _holdActivatableEntity.CanHoldActivate())
                _holdActivatableEntity.OnHoldActivate();
        }

        public virtual void Activate()
        {
            if (_activatableEntity != null && _activatableEntity.CanActivate())
                _activatableEntity.OnActivate();
        }

        public virtual void UseSkill(bool isLeftHand)
        {
            if (_pauseFireInputFrames > 0)
                return;
            if (_queueUsingSkill.skill != null)
            {
                if (_queueUsingSkill.itemIndex >= 0)
                {
                    PlayingCharacterEntity.UseSkillItem(_queueUsingSkill.itemIndex, isLeftHand, SelectedGameEntityObjectId, _queueUsingSkill.aimPosition);
                }
                else
                {
                    PlayingCharacterEntity.UseSkill(_queueUsingSkill.skill.DataId, isLeftHand, SelectedGameEntityObjectId, _queueUsingSkill.aimPosition);
                }
            }
            ClearQueueUsingSkill();
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, _overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(EntityTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                if (_overlapColliders[tempCounter].gameObject == target)
                    return true;
            }
            return false;
        }

        public bool IsUsingHotkey()
        {
            // Check using hotkey for PC only
            return !InputManager.UseMobileInput() && UICharacterHotkeys.UsingHotkey != null;
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
            _dirtyViewMode = viewMode;
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
            PlayingCharacterEntity.ModelManager.SetIsFps(viewMode == ShooterControllerViewMode.Fps);
        }

        public virtual bool IsInFront(Vector3 position)
        {
            return Vector3.Angle(_cameraForward, position - EntityTransform.position) < 115f;
        }

        public override void ConfirmBuild()
        {
            base.ConfirmBuild();
            _pauseFireInputFrames = PAUSE_FIRE_INPUT_FRAMES_AFTER_CONFIRM_BUILD;
        }

        public override bool ShouldShowActivateButtons()
        {
            return SelectedGameEntity is IActivatableEntity castedEntity && castedEntity.CanActivate();
        }

        public override bool ShouldShowHoldActivateButtons()
        {
            return SelectedGameEntity is IHoldActivatableEntity castedEntity && castedEntity.CanHoldActivate();
        }

        public override bool ShouldShowPickUpButtons()
        {
            return SelectedGameEntity is IPickupActivatableEntity castedEntity && castedEntity.CanPickupActivate();
        }
    }
}
