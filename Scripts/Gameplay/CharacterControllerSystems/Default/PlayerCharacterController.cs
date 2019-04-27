using System.Collections;
using System.Collections.Generic;
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

        public struct UsingSkillData
        {
            public Vector3? aimPosition;
            public int dataId;
            public UsingSkillData(Vector3? aimPosition, int dataId)
            {
                this.aimPosition = aimPosition;
                this.dataId = dataId;
            }
        }

        public const float DETECT_MOUSE_DRAG_DISTANCE = 10f;
        public const float DETECT_MOUSE_HOLD_DURATION = 1f;
        public float angularSpeed = 800f;
        public PlayerCharacterControllerMode controllerMode;
        [Tooltip("Set this to TRUE to find nearby enemy and look to it while attacking when `Controller Mode` is `WASD`")]
        public bool wasdLockAttackTarget;
        [Tooltip("This will be used to find nearby enemy when `Controller Mode` is `Point Click` or when `Wasd Lock Attack Target` is `TRUE`")]
        public float lockAttackTargetDistance = 10f;
        [Tooltip("Set this to TRUE to move to target immediately when clicked on target, if this is FALSE it will not move to target immediately")]
        public bool pointClickSetTargetImmediately;
        public FollowCameraControls gameplayCameraPrefab;
        public GameObject targetObjectPrefab;
        [Header("Building Settings")]
        public bool buildGridSnap;
        public float buildGridSize = 4f;
        public bool buildRotationSnap;

        protected Vector3? destination;
        protected UsingSkillData? queueUsingSkill;
        protected Vector3 mouseDownPosition;
        protected float mouseDownTime;
        protected bool isMouseDragOrHoldOrOverUI;
        protected uint lastNpcObjectId;

        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        public GameObject CacheTargetObject { get; protected set; }

        protected BaseGameEntity targetEntity;
        protected Vector3? targetPosition;
        // Optimizing garbage collection
        protected bool getMouseUp;
        protected bool getMouseDown;
        protected bool getMouse;
        protected bool isPointerOverUI;
        protected bool isMouseDragDetected;
        protected bool isMouseHoldDetected;
        protected bool isMouseHoldAndNotDrag;
        protected BaseCharacterEntity targetCharacter;
        protected BaseCharacterEntity targetEnemy;
        protected BasePlayerCharacterEntity targetPlayer;
        protected BaseMonsterCharacterEntity targetMonster;
        protected NpcEntity targetNpc;
        protected ItemDropEntity targetItemDrop;
        protected BuildingEntity targetBuilding;
        protected HarvestableEntity targetHarvestable;
        protected Quaternion tempLookAt;
        protected Vector3 targetLookDirection;
        protected NearbyEntityDetector activatingEntityDetector;
        protected NearbyEntityDetector itemDropEntityDetector;
        protected NearbyEntityDetector enemyEntityDetector;
        protected int findingEnemyIndex;
        protected bool isLeftHandAttacking;

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            findingEnemyIndex = -1;
            isLeftHandAttacking = false;
            CurrentBuildingEntity = null;

            if (gameplayCameraPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheGameplayCameraControls = Instantiate(gameplayCameraPrefab);
            }
            if (targetObjectPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheTargetObject = Instantiate(targetObjectPrefab);
                CacheTargetObject.SetActive(false);
            }
            // This entity detector will be find entities to use when pressed activate key
            GameObject tempGameObject = new GameObject("_ActivatingEntityDetector");
            activatingEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            activatingEntityDetector.detectingRadius = gameInstance.conversationDistance;
            activatingEntityDetector.findPlayer = true;
            activatingEntityDetector.findOnlyAlivePlayers = true;
            activatingEntityDetector.findNpc = true;
            activatingEntityDetector.findBuilding = true;
            activatingEntityDetector.findOnlyAliveBuildings = true;
            activatingEntityDetector.findOnlyActivatableBuildings = true;
            // This entity detector will be find item drop entities to use when pressed pickup key
            tempGameObject = new GameObject("_ItemDropEntityDetector");
            itemDropEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            itemDropEntityDetector.detectingRadius = gameInstance.pickUpItemDistance;
            itemDropEntityDetector.findItemDrop = true;
            // This entity detector will be find item drop entities to use when pressed pickup key
            tempGameObject = new GameObject("_EnemyEntityDetector");
            enemyEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            enemyEntityDetector.findPlayer = true;
            enemyEntityDetector.findOnlyAlivePlayers = true;
            enemyEntityDetector.findPlayerToAttack = true;
            enemyEntityDetector.findMonster = true;
            enemyEntityDetector.findOnlyAliveMonsters = true;
            enemyEntityDetector.findMonsterToAttack = true;
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
            if (CacheTargetObject != null)
                Destroy(CacheTargetObject.gameObject);
            if (activatingEntityDetector != null)
                Destroy(activatingEntityDetector.gameObject);
            if (itemDropEntityDetector != null)
                Destroy(itemDropEntityDetector.gameObject);
            if (enemyEntityDetector != null)
                Destroy(enemyEntityDetector.gameObject);
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();

            if (CacheTargetObject != null)
                CacheTargetObject.gameObject.SetActive(destination.HasValue);

            if (PlayerCharacterEntity.IsDead())
            {
                queueUsingSkill = null;
                destination = null;
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetEntity(null);
                CancelBuild();
            }
            else
            {
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            }

            if (destination.HasValue)
            {
                if (CacheTargetObject != null)
                    CacheTargetObject.transform.position = destination.Value;
                if (Vector3.Distance(destination.Value, CharacterTransform.position) < StoppingDistance + 0.5f)
                    destination = null;
            }

            UpdateInput();
            UpdateFollowTarget();
            UpdateLookAtTarget();
        }

        private Vector3 GetBuildingPlacePosition(Vector3 position)
        {
            if (buildGridSnap)
                position = new Vector3(Mathf.Round(position.x / buildGridSize) * buildGridSize, position.y, Mathf.Round(position.z / buildGridSize) * buildGridSize);
            return position;
        }

        private Vector3 GetBuildingPlaceEulerAngles(Vector3 eulerAngles)
        {
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            // Make Y rotation set to 0, 90, 180
            if (buildRotationSnap)
                eulerAngles.y = Mathf.Round(eulerAngles.y / 90) * 90;
            return eulerAngles;
        }

        public bool TryGetSelectedTargetAsAttackingCharacter(out BaseCharacterEntity character)
        {
            character = null;
            if (SelectedEntity != null)
            {
                character = SelectedEntity as BaseCharacterEntity;
                if (character != null &&
                    !character.IsAlly(PlayerCharacterEntity))
                    return true;
                else
                    character = null;
            }
            return false;
        }

        public bool TryGetAttackingCharacter(out BaseCharacterEntity character)
        {
            character = null;
            if (PlayerCharacterEntity.TryGetTargetEntity(out character))
            {
                if (character != PlayerCharacterEntity && !character.IsAlly(PlayerCharacterEntity))
                    return true;
                else
                    character = null;
            }
            return false;
        }

        public bool GetAttackDataOrUseNonAttackSkill(bool isLeftHand, out float attackDistance, out float attackFov)
        {
            attackDistance = PlayerCharacterEntity.GetAttackDistance(isLeftHand);
            attackFov = PlayerCharacterEntity.GetAttackFov(isLeftHand);
            if (queueUsingSkill.HasValue)
            {
                Skill skill = null;
                if (GameInstance.Skills.TryGetValue(queueUsingSkill.Value.dataId, out skill) && skill != null)
                {
                    if (skill.IsAttack())
                    {
                        attackDistance = PlayerCharacterEntity.GetSkillAttackDistance(skill, isLeftHand);
                        attackFov = PlayerCharacterEntity.GetSkillAttackFov(skill, isLeftHand);
                    }
                    else
                    {
                        // Stop movement to use non attack skill
                        PlayerCharacterEntity.StopMove();
                        RequestUsePendingSkill(false, null);
                        return false;
                    }
                }
                else
                    queueUsingSkill = null;
            }
            // Return true if going to attack
            return true;
        }

        public bool IsLockTarget()
        {
            return controllerMode == PlayerCharacterControllerMode.Both ||
                controllerMode == PlayerCharacterControllerMode.PointClick ||
                (controllerMode == PlayerCharacterControllerMode.WASD && wasdLockAttackTarget);
        }

        public Vector3 GetMoveDirection(float horizontalInput, float verticalInput)
        {
            Vector3 moveDirection = Vector3.zero;
            switch (gameInstance.DimensionType)
            {
                case DimensionType.Dimension3D:
                    Vector3 forward = Camera.main.transform.forward;
                    Vector3 right = Camera.main.transform.right;
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

        public void RequestUsePendingSkill(bool isLeftHand, Vector3? aimPosition)
        {
            if (queueUsingSkill.HasValue && PlayerCharacterEntity.CanUseSkill())
            {
                UsingSkillData queueUsingSkillValue = queueUsingSkill.Value;
                if (queueUsingSkillValue.aimPosition.HasValue)
                    aimPosition = queueUsingSkillValue.aimPosition.Value;
                if (aimPosition.HasValue)
                    PlayerCharacterEntity.RequestUseSkill(queueUsingSkillValue.dataId, isLeftHand, aimPosition.Value);
                else
                    PlayerCharacterEntity.RequestUseSkill(queueUsingSkillValue.dataId, isLeftHand);
                queueUsingSkill = null;
            }
        }

        public void RequestEquipItem(short itemIndex)
        {
            PlayerCharacterEntity.RequestEquipItem(itemIndex);
        }

        public void RequestUseItem(short itemIndex)
        {
            PlayerCharacterEntity.RequestUseItem(itemIndex);
        }
    }
}
