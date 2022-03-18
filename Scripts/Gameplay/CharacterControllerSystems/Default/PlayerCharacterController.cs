using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController : BasePlayerCharacterController
    {
        public enum PlayerCharacterControllerMode
        {
            PointClick,
            WASD,
            Both,
        }

        public enum TargetActionType
        {
            Activate,
            Attack,
            UseSkill,
            ViewOptions,
        }

        public const float DETECT_MOUSE_DRAG_DISTANCE_SQUARED = 100f;
        public const float DETECT_MOUSE_HOLD_DURATION = 1f;

        [Header("Camera Controls Prefabs")]
        [SerializeField]
        protected FollowCameraControls gameplayCameraPrefab;
        [SerializeField]
        protected FollowCameraControls minimapCameraPrefab;

        [Header("Controller Settings")]
        [SerializeField]
        protected PlayerCharacterControllerMode controllerMode;
        [Tooltip("Set this to `TRUE` to find nearby enemy and follow it to attack while `Controller Mode` is `WASD`")]
        [SerializeField]
        protected bool wasdLockAttackTarget;
        [Tooltip("This will be used to find nearby enemy while `Controller Mode` is `Point Click` or when `Wasd Lock Attack Target` is `TRUE`")]
        [SerializeField]
        protected float lockAttackTargetDistance = 10f;
        [Tooltip("This will be used to clear selected target when character move with WASD keys and far from target")]
        [SerializeField]
        protected float wasdClearTargetDistance = 15f;
        [Tooltip("Set this to TRUE to move to target immediately when clicked on target, if this is FALSE it will not move to target immediately")]
        [SerializeField]
        protected bool pointClickSetTargetImmediately;
        [Tooltip("Set this to TRUE to interrupt casting skill when click on ground to move")]
        [SerializeField]
        protected bool pointClickInterruptCastingSkill;
        [Tooltip("The object which will represent where character is moving to")]
        [SerializeField]
        protected GameObject targetObjectPrefab;

        [Header("Building Settings")]
        [SerializeField]
        protected bool buildGridSnap;
        [SerializeField]
        protected Vector3 buildGridOffsets = Vector3.zero;
        [SerializeField]
        protected float buildGridSize = 4f;
        [SerializeField]
        protected bool buildRotationSnap;
        [SerializeField]
        protected float buildRotateAngle = 45f;
        [SerializeField]
        protected float buildRotateSpeed = 200f;

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
        public NearbyEntityDetector ActivatableEntityDetector { get; protected set; }
        public NearbyEntityDetector ItemDropEntityDetector { get; protected set; }
        public NearbyEntityDetector EnemyEntityDetector { get; protected set; }
        public IGameplayCameraController CacheGameplayCameraController { get; protected set; }
        public IMinimapCameraController CacheMinimapCameraController { get; protected set; }
        public GameObject CacheTargetObject { get; protected set; }

        // Input & control states variables
        protected bool getMouseUp;
        protected bool getMouseDown;
        protected bool getMouse;
        protected bool isPointerOverUI;
        protected bool isMouseDragDetected;
        protected bool isMouseHoldDetected;
        protected bool isMouseHoldAndNotDrag;
        protected bool isSprinting;
        protected bool isWalking;
        protected Vector3? destination;
        protected Vector3 mouseDownPosition;
        protected float mouseDownTime;
        protected bool isMouseDragOrHoldOrOverUI;
        protected Vector3? targetPosition;
        protected TargetActionType targetActionType;
        protected IPhysicFunctions physicFunctions;
        protected DamageableEntity targetDamageable;
        protected BasePlayerCharacterEntity targetPlayer;
        protected BaseMonsterCharacterEntity targetMonster;
        protected NpcEntity targetNpc;
        protected ItemDropEntity targetItemDrop;
        protected BuildingEntity targetBuilding;
        protected VehicleEntity targetVehicle;
        protected WarpPortalEntity targetWarpPortal;
        protected HarvestableEntity targetHarvestable;
        protected ItemsContainerEntity targetItemsContainer;
        protected Vector3 previousPointClickPosition = Vector3.positiveInfinity;
        protected int findingEnemyIndex;
        protected bool isLeftHandAttacking;
        protected bool isFollowingTarget;
        protected bool didActionOnTarget;
        protected float buildYRotate;

        protected override void Awake()
        {
            base.Awake();
            CacheGameplayCameraController = gameObject.GetOrAddComponent<IGameplayCameraController, DefaultGameplayCameraController>((obj) =>
            {
                DefaultGameplayCameraController castedObj = obj as DefaultGameplayCameraController;
                castedObj.gameplayCameraPrefab = gameplayCameraPrefab;
                castedObj.InitialCameraControls();
            });
            CacheMinimapCameraController = gameObject.GetOrAddComponent<IMinimapCameraController, DefaultMinimapCameraController>((obj) =>
            {
                DefaultMinimapCameraController castedObj = obj as DefaultMinimapCameraController;
                castedObj.minimapCameraPrefab = minimapCameraPrefab;
            });
            buildingItemIndex = -1;
            findingEnemyIndex = -1;
            isLeftHandAttacking = false;
            ConstructingBuildingEntity = null;

            if (targetObjectPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheTargetObject = Instantiate(targetObjectPrefab);
                CacheTargetObject.SetActive(false);
            }
            // This entity detector will be find entities to use when pressed activate key
            GameObject tempGameObject = new GameObject("_ActivatingEntityDetector");
            ActivatableEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            ActivatableEntityDetector.detectingRadius = CurrentGameInstance.conversationDistance;
            ActivatableEntityDetector.findPlayer = true;
            ActivatableEntityDetector.findOnlyAlivePlayers = true;
            ActivatableEntityDetector.findNpc = true;
            ActivatableEntityDetector.findBuilding = true;
            ActivatableEntityDetector.findOnlyAliveBuildings = true;
            ActivatableEntityDetector.findOnlyActivatableBuildings = true;
            ActivatableEntityDetector.findVehicle = true;
            ActivatableEntityDetector.findWarpPortal = true;
            // This entity detector will be find item drop entities to use when pressed pickup key
            tempGameObject = new GameObject("_ItemDropEntityDetector");
            ItemDropEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            ItemDropEntityDetector.detectingRadius = CurrentGameInstance.pickUpItemDistance;
            ItemDropEntityDetector.findItemDrop = true;
            ItemDropEntityDetector.findItemsContainer = true;
            // This entity detector will be find item drop entities to use when pressed pickup key
            tempGameObject = new GameObject("_EnemyEntityDetector");
            EnemyEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            EnemyEntityDetector.findPlayer = true;
            EnemyEntityDetector.findOnlyAlivePlayers = true;
            EnemyEntityDetector.findPlayerToAttack = true;
            EnemyEntityDetector.findMonster = true;
            EnemyEntityDetector.findOnlyAliveMonsters = true;
            EnemyEntityDetector.findMonsterToAttack = true;
            // Initial physic functions
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
                physicFunctions = new PhysicFunctions(512);
            else
                physicFunctions = new PhysicFunctions2D(512);
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);
            CacheGameplayCameraController.Setup(characterEntity);
            CacheMinimapCameraController.Setup(characterEntity);
        }

        protected override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);
            CacheGameplayCameraController.Desetup(characterEntity);
            CacheMinimapCameraController.Desetup(characterEntity);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(CacheGameplayCameraController.gameObject);
            Destroy(CacheMinimapCameraController.gameObject);
            if (CacheTargetObject != null)
                Destroy(CacheTargetObject.gameObject);
            if (ActivatableEntityDetector != null)
                Destroy(ActivatableEntityDetector.gameObject);
            if (ItemDropEntityDetector != null)
                Destroy(ItemDropEntityDetector.gameObject);
            if (EnemyEntityDetector != null)
                Destroy(EnemyEntityDetector.gameObject);
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            CacheGameplayCameraController.FollowingEntityTransform = CameraTargetTransform;
            CacheMinimapCameraController.FollowingEntityTransform = CameraTargetTransform;
            CacheMinimapCameraController.FollowingGameplayCameraTransform = CacheGameplayCameraController.CameraTransform;

            if (CacheTargetObject != null)
                CacheTargetObject.gameObject.SetActive(destination.HasValue);

            if (PlayerCharacterEntity.IsDead())
            {
                ClearQueueUsingSkill();
                destination = null;
                isFollowingTarget = false;
                CancelBuild();
                CacheUISceneGameplay.SetTargetEntity(null);
            }
            else
            {
                CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            }

            if (destination.HasValue)
            {
                if (CacheTargetObject != null)
                    CacheTargetObject.transform.position = destination.Value;
                if (Vector3.Distance(destination.Value, MovementTransform.position) < StoppingDistance + 0.5f)
                    destination = null;
            }

            UpdateInput();
            UpdateFollowTarget();
            PlayerCharacterEntity.AimPosition = PlayerCharacterEntity.GetAttackAimPosition(ref isLeftHandAttacking);
        }

        private Vector3 GetBuildingPlacePosition(Vector3 position)
        {
            if (CurrentGameInstance.DimensionType == DimensionType.Dimension3D)
            {
                if (buildGridSnap)
                    position = new Vector3(Mathf.Round(position.x / buildGridSize) * buildGridSize, position.y, Mathf.Round(position.z / buildGridSize) * buildGridSize) + buildGridOffsets;
            }
            else
            {
                if (buildGridSnap)
                    position = new Vector3(Mathf.Round(position.x / buildGridSize) * buildGridSize, Mathf.Round(position.y / buildGridSize) * buildGridSize) + buildGridOffsets;
            }
            return position;
        }

        public bool TryGetSelectedTargetAsAttackingEntity(out BaseCharacterEntity character)
        {
            character = null;
            if (SelectedEntity != null)
            {
                character = SelectedEntity as BaseCharacterEntity;
                if (character == null ||
                    character == PlayerCharacterEntity ||
                    !character.CanReceiveDamageFrom(PlayerCharacterEntity.GetInfo()))
                {
                    character = null;
                    return false;
                }
                return true;
            }
            return false;
        }

        public bool TryGetAttackingEntity<T>(out T entity)
            where T : DamageableEntity
        {
            if (!TryGetDoActionEntity(out entity, TargetActionType.Attack))
                return false;
            if (entity == PlayerCharacterEntity || !entity.CanReceiveDamageFrom(PlayerCharacterEntity.GetInfo()))
            {
                entity = null;
                return false;
            }
            return true;
        }

        public bool TryGetUsingSkillEntity<T>(out T entity)
            where T : DamageableEntity
        {
            if (!TryGetDoActionEntity(out entity, TargetActionType.UseSkill))
                return false;
            if (queueUsingSkill.skill == null)
            {
                entity = null;
                return false;
            }
            return true;
        }

        public bool TryGetDoActionEntity<T>(out T entity, TargetActionType actionType = TargetActionType.Activate)
            where T : BaseGameEntity
        {
            entity = null;
            if (targetActionType != actionType)
                return false;
            if (TargetEntity == null)
                return false;
            entity = TargetEntity as T;
            if (entity == null)
                return false;
            return true;
        }

        public void GetAttackDistanceAndFov(bool isLeftHand, out float attackDistance, out float attackFov)
        {
            attackDistance = PlayerCharacterEntity.GetAttackDistance(isLeftHand);
            attackFov = PlayerCharacterEntity.GetAttackFov(isLeftHand);
            attackDistance -= PlayerCharacterEntity.StoppingDistance;
        }

        public void GetUseSkillDistanceAndFov(bool isLeftHand, out float castDistance, out float castFov)
        {
            castDistance = CurrentGameInstance.conversationDistance;
            castFov = 360f;
            if (queueUsingSkill.skill != null)
            {
                // If skill is attack skill, set distance and fov by skill
                castDistance = queueUsingSkill.skill.GetCastDistance(PlayerCharacterEntity, queueUsingSkill.level, isLeftHand);
                castFov = queueUsingSkill.skill.GetCastFov(PlayerCharacterEntity, queueUsingSkill.level, isLeftHand);
            }
            castDistance -= PlayerCharacterEntity.StoppingDistance;
        }

        public Vector3 GetMoveDirection(float horizontalInput, float verticalInput)
        {
            Vector3 moveDirection = Vector3.zero;
            switch (CurrentGameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    Vector3 forward = CacheGameplayCameraController.CameraTransform.forward;
                    Vector3 right = CacheGameplayCameraController.CameraTransform.right;
                    forward.y = 0f;
                    right.y = 0f;
                    forward.Normalize();
                    right.Normalize();
                    moveDirection += forward * verticalInput;
                    moveDirection += right * horizontalInput;
                    // normalize input if it exceeds 1 in combined length:
                    if (moveDirection.sqrMagnitude > 1)
                        moveDirection.Normalize();
                    break;
                case DimensionType.Dimension2D:
                    moveDirection = new Vector2(horizontalInput, verticalInput);
                    break;
            }
            return moveDirection;
        }

        public void RequestAttack()
        {
            if (PlayerCharacterEntity.Attack(isLeftHandAttacking))
                isLeftHandAttacking = !isLeftHandAttacking;
        }

        public void RequestUsePendingSkill()
        {
            if (PlayerCharacterEntity.IsDead() ||
                PlayerCharacterEntity.Dealing.DealingState != DealingState.None)
            {
                ClearQueueUsingSkill();
                return;
            }

            if (queueUsingSkill.skill != null &&
                !PlayerCharacterEntity.IsPlayingActionAnimation() &&
                !PlayerCharacterEntity.IsAttacking &&
                !PlayerCharacterEntity.IsUsingSkill)
            {
                if (queueUsingSkill.itemIndex >= 0)
                {
                    if (PlayerCharacterEntity.UseSkillItem(queueUsingSkill.itemIndex, isLeftHandAttacking, SelectedEntityObjectId, queueUsingSkill.aimPosition))
                    {
                        isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    if (PlayerCharacterEntity.UseSkill(queueUsingSkill.skill.DataId, isLeftHandAttacking, SelectedEntityObjectId, queueUsingSkill.aimPosition))
                    {
                        isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                ClearQueueUsingSkill();
            }
        }
    }
}
