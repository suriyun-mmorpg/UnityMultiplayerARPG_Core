using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        bool isBlockController;
        BuildingMaterial tempBuildingMaterial;
        BaseGameEntity tempEntity;
        BaseGameEntity foundEntity;
        Vector3 moveLookDirection;
        Vector3 targetLookDirection;
        Quaternion tempLookAt;
        TurningState turningState;
        float turnTimeCounter;
        float tempCalculateAngle;
        bool tempPressAttack;
        bool tempPressActivate;
        bool tempPressPickupItem;
        int tempCount;
        int tempCounter;
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
            if (currentBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiConstructBuilding.IsVisible())
                    CacheUISceneGameplay.uiConstructBuilding.Hide();
            }
            if (activeBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiCurrentBuilding.IsVisible())
                    CacheUISceneGameplay.uiCurrentBuilding.Hide();
            }

            isBlockController = CacheUISceneGameplay.IsBlockController();
            // Lock cursor when not show UIs
            Cursor.lockState = !isBlockController ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isBlockController;

            CacheGameplayCameraControls.updateRotation = !isBlockController;

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
            foundEntity = null;
            // Find for enemy character
            if (currentBuildingEntity == null)
            {
                tempCount = Physics.RaycastNonAlloc(ray, raycasts, aimDistance);
                for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
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
                    if (tempEntity != null && tempEntity == PlayerCharacterEntity)
                        continue;
                    // Set aim position and found target
                    aimPosition = tempHitInfo.point;
                    if (tempEntity != null)
                        foundEntity = tempEntity;
                }
                // Show target hp/mp
                CacheUISceneGameplay.SetTargetEntity(foundEntity);
            }
            else
            {
                // Clear area before next find
                currentBuildingEntity.buildingArea = null;
                // Find for position to construction building
                bool foundSnapBuildPosition = false;
                tempCount = Physics.RaycastNonAlloc(ray, raycasts, gameInstance.buildDistance);
                BuildingArea buildingArea = null;
                for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];
                    tempEntity = tempHitInfo.collider.GetComponentInParent<BuildingEntity>();
                    if (tempEntity != null && tempEntity == currentBuildingEntity)
                        continue;

                    buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                    if (buildingArea == null || (buildingArea.buildingEntity != null && buildingArea.buildingEntity == currentBuildingEntity))
                        continue;

                    // Set aim position
                    aimPosition = tempHitInfo.point;

                    if (currentBuildingEntity.buildingType.Equals(buildingArea.buildingType))
                    {
                        currentBuildingEntity.buildingArea = buildingArea;
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
                    currentBuildingEntity.CacheTransform.position = aimPosition;
                    // Rotate to camera
                    Vector3 direction = (aimPosition - CacheGameplayCameraControls.CacheCameraTransform.position).normalized;
                    direction.y = 0;
                    currentBuildingEntity.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            // If mobile platforms, don't receive input raw to make it smooth
            MovementFlag movementState = MovementFlag.None;
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

            if (currentBuildingEntity != null)
            {
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
                // Not building / attacking
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
                        if (foundEntity is BasePlayerCharacterEntity)
                            targetPlayer = foundEntity as BasePlayerCharacterEntity;
                        if (foundEntity is NpcEntity)
                            targetNpc = foundEntity as NpcEntity;
                        if (foundEntity is BuildingEntity)
                            targetBuilding = foundEntity as BuildingEntity;
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
                        }
                        else if (tempPressAttack)
                        {
                            Attack(aimPosition);
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
                    if (foundEntity != null)
                        PlayerCharacterEntity.RequestPickupItem((foundEntity as ItemDropEntity).ObjectId);
                }
                else
                {
                    // Update move direction
                    if (moveDirection.magnitude != 0f)
                        targetLookDirection = moveLookDirection;
                }
            }

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
                            Attack(aimPosition);
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
            currentBuildingEntity = null;

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            Skill skill = hotkey.GetSkill();
            if (skill != null)
            {
                int skillIndex = PlayerCharacterEntity.IndexOfSkill(skill.DataId);
                if (skillIndex >= 0)
                {
                    if (PlayerCharacterEntity.Skills[skillIndex].CanUse(PlayerCharacterEntity))
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
                        currentBuildingEntity = Instantiate(item.buildingEntity);
                        currentBuildingEntity.SetupAsBuildMode();
                        currentBuildingEntity.CacheTransform.parent = null;
                        // TODO: Build character by cursor position
                    }
                }
            }
        }

        public void Attack(Vector3 aimPosition)
        {
            PlayerCharacterEntity.RequestAttack(aimPosition);
        }

        public void Activate()
        {
            // Priority Player -> Npc -> Buildings
            if (targetPlayer != null && CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
            else if (targetNpc != null)
                PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
            else if (targetBuilding != null)
            {
                activeBuildingEntity = targetBuilding;
                if (!CacheUISceneGameplay.uiCurrentBuilding.IsVisible())
                    CacheUISceneGameplay.uiCurrentBuilding.Show();
            }
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
            tempCount = OverlapObjects(CharacterTransform.position, actDistance, layerMask);
            for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempGameObject = overlapColliders[tempCounter].gameObject;
                if (tempGameObject == target)
                    return true;
            }
            return false;
        }
    }
}
