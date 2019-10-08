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
        public GameObject targetObjectPrefab;
        [Header("Building Settings")]
        public bool buildGridSnap;
        public float buildGridSize = 4f;
        public bool buildRotationSnap;

        protected Vector3? destination;
        protected Vector3 mouseDownPosition;
        protected float mouseDownTime;
        protected bool isMouseDragOrHoldOrOverUI;
        protected uint lastNpcObjectId;
        
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
        public NearbyEntityDetector activatableEntityDetector { get; protected set; }
        public NearbyEntityDetector itemDropEntityDetector { get; protected set; }
        public NearbyEntityDetector enemyEntityDetector { get; protected set; }
        protected int findingEnemyIndex;
        protected bool isLeftHandAttacking;

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            findingEnemyIndex = -1;
            isLeftHandAttacking = false;
            CurrentBuildingEntity = null;
            
            if (targetObjectPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheTargetObject = Instantiate(targetObjectPrefab);
                CacheTargetObject.SetActive(false);
            }
            // This entity detector will be find entities to use when pressed activate key
            GameObject tempGameObject = new GameObject("_ActivatingEntityDetector");
            activatableEntityDetector = tempGameObject.AddComponent<NearbyEntityDetector>();
            activatableEntityDetector.detectingRadius = gameInstance.conversationDistance;
            activatableEntityDetector.findPlayer = true;
            activatableEntityDetector.findOnlyAlivePlayers = true;
            activatableEntityDetector.findNpc = true;
            activatableEntityDetector.findBuilding = true;
            activatableEntityDetector.findOnlyAliveBuildings = true;
            activatableEntityDetector.findOnlyActivatableBuildings = true;
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

            tempLookAt = MovementTransform.rotation;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (CacheTargetObject != null)
                Destroy(CacheTargetObject.gameObject);
            if (activatableEntityDetector != null)
                Destroy(activatableEntityDetector.gameObject);
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
                ClearQueueUsingSkill();
                ClearQueueUsingSkillItem();
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
                if (Vector3.Distance(destination.Value, MovementTransform.position) < StoppingDistance + 0.5f)
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
                if (character != null && character.CanReceiveDamageFrom(PlayerCharacterEntity))
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
                if (character != PlayerCharacterEntity && character.CanReceiveDamageFrom(PlayerCharacterEntity))
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

            BaseSkill skill = null;
            short skillLevel = 0;

            if (queueUsingSkill.skill != null)
            {
                skill = queueUsingSkill.skill;
                skillLevel = queueUsingSkill.level;
                if (queueUsingSkill.level <= 0)
                {
                    ClearQueueUsingSkill();
                }
            }

            if (queueUsingSkillItem.skill != null)
            {
                skill = queueUsingSkillItem.skill;
                skillLevel = queueUsingSkillItem.level;
                if (queueUsingSkillItem.level <= 0)
                {
                    ClearQueueUsingSkillItem();
                }
            }

            if (skill == null)
            {
                // No using skill, just attack
                return true;
            }

            if (skill.IsAttack())
            {
                attackDistance = skill.GetAttackDistance(PlayerCharacterEntity, skillLevel, isLeftHand);
                attackFov = skill.GetAttackFov(PlayerCharacterEntity, skillLevel, isLeftHand);
            }
            else
            {
                // Stop movement to use non attack skill
                PlayerCharacterEntity.StopMove();
                // Use skill / skill item
                if (queueUsingSkill.skill != null)
                    RequestUsePendingSkill(false);
                else if (queueUsingSkillItem.skill != null)
                    RequestUsePendingSkillItem(false);
                return false;
            }

            // Return true if it's going to attack
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

        public bool RequestUsePendingSkill(bool isLeftHand)
        {
            if (queueUsingSkill.skill != null && PlayerCharacterEntity.CanUseSkill())
            {
                bool canUseSkill = PlayerCharacterEntity.RequestUseSkill(queueUsingSkill.skill.DataId, isLeftHand, queueUsingSkill.aimPosition.HasValue ? queueUsingSkill.aimPosition.Value : GetDefaultAttackAimPosition());
                ClearQueueUsingSkill();
                return canUseSkill;
            }
            return false;
        }

        public bool RequestUsePendingSkillItem(bool isLeftHand)
        {
            if (queueUsingSkillItem.skill != null && PlayerCharacterEntity.CanUseItem() && PlayerCharacterEntity.CanUseSkill())
            {
                bool canUseSkill = PlayerCharacterEntity.RequestUseSkillItem(queueUsingSkillItem.itemIndex, isLeftHand, queueUsingSkillItem.aimPosition.HasValue ? queueUsingSkillItem.aimPosition.Value : GetDefaultAttackAimPosition());
                ClearQueueUsingSkillItem();
                return canUseSkill;
            }
            return false;
        }

        public Vector3 GetDefaultAttackAimPosition()
        {
            // No aim position set, set aim position to forward direction
            Transform damageTransform = PlayerCharacterEntity.GetDamageTransform(PlayerCharacterEntity.GetAvailableWeapon(ref isLeftHandAttacking).GetWeaponItem().WeaponType.damageInfo.damageType, isLeftHandAttacking);
            return damageTransform.position + PlayerCharacterEntity.CacheTransform.forward;
        }
    }
}
