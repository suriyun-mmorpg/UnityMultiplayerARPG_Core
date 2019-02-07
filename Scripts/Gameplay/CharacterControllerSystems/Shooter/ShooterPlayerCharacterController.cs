using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController : BasePlayerCharacterController
    {
        public const int RAYCAST_COLLIDER_SIZE = 32;
        public const int OVERLAP_COLLIDER_SIZE = 32;

        public enum TurningState
        {
            None,
            Attack,
            Activate,
            UseSkill,
        }
        public float angularSpeed = 800f;
        [Range(0, 1f)]
        public float turnToTargetDuration = 0.1f;
        public FollowCameraControls gameplayCameraPrefab;
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        bool isBlockController;
        BaseGameEntity tempEntity;
        BaseGameEntity foundEntity;
        Vector3 targetLookDirection;
        Quaternion tempLookAt;
        TurningState turningState;
        float turnTimeCounter;
        float tempCalculateAngle;
        bool tempPressAttack;
        bool tempPressActivate;
        int tempCount;
        int tempCounter;
        GameObject tempGameObject;
        BasePlayerCharacterEntity targetPlayer;
        NpcEntity targetNpc;
        RaycastHit[] raycasts = new RaycastHit[RAYCAST_COLLIDER_SIZE];
        Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        RaycastHit tempHitInfo;
        Skill queueSkill;
        Skill usingSkill;
        Vector3 aimPosition;

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

            isBlockController = CacheUISceneGameplay.IsBlockController();
            // Lock cursor when not show UIs
            Cursor.lockState = !isBlockController ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = isBlockController;

            CacheGameplayCameraControls.updateRotation = !isBlockController;

            if (isBlockController || GenericUtils.IsFocusInputField())
            {
                PlayerCharacterEntity.KeyMovement(Vector3.zero, false);
                return;
            }

            // Find target character
            Vector3 forward = CacheGameplayCameraControls.CacheCameraTransform.forward;
            Vector3 right = CacheGameplayCameraControls.CacheCameraTransform.right;
            Ray ray = CacheGameplayCameraControls.CacheCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            float distanceFromOrigin = Vector3.Distance(ray.origin, PlayerCharacterEntity.CacheTransform.position);
            float aimDistance = distanceFromOrigin;
            if (queueSkill != null && queueSkill.IsAttack())
                aimDistance += PlayerCharacterEntity.GetSkillAttackDistance(queueSkill);
            else
                aimDistance += PlayerCharacterEntity.GetAttackDistance();
            aimPosition = ray.origin + ray.direction * aimDistance;
            foundEntity = null;
            tempCount = Physics.RaycastNonAlloc(ray, raycasts, aimDistance);
            for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempHitInfo = raycasts[tempCounter];
                aimPosition = tempHitInfo.point;
                tempEntity = tempHitInfo.collider.GetComponent<BaseGameEntity>();
                if (tempEntity != PlayerCharacterEntity)
                {
                    foundEntity = tempEntity;
                    break;
                }
            }

            // Show target hp/mp
            CacheUISceneGameplay.SetTargetEntity(foundEntity);

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            Vector3 moveDirection = Vector3.zero;
            Vector3 actionDirection = Vector3.zero;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            moveDirection += forward * InputManager.GetAxis("Vertical", raw);
            moveDirection += right * InputManager.GetAxis("Horizontal", raw);
            actionDirection += forward * 1f;

            // normalize input if it exceeds 1 in combined length:
            if (moveDirection.sqrMagnitude > 1)
                moveDirection.Normalize();

            // normalize input if it exceeds 1 in combined length:
            if (actionDirection.sqrMagnitude > 1)
                actionDirection.Normalize();

            tempPressAttack = InputManager.GetButton("Fire1");
            tempPressActivate = InputManager.GetButtonDown("Activate");
            if (queueSkill != null || tempPressAttack || tempPressActivate || PlayerCharacterEntity.IsPlayingActionAnimation())
            {
                // Find forward character / npc / building / warp entity from camera center
                targetPlayer = null;
                targetNpc = null;
                if (tempPressActivate && !tempPressAttack)
                {
                    if (foundEntity is BasePlayerCharacterEntity)
                        targetPlayer = foundEntity as BasePlayerCharacterEntity;
                    if (foundEntity is NpcEntity)
                        targetNpc = foundEntity as NpcEntity;
                }
                // While attacking turn to camera forward
                tempCalculateAngle = Vector3.Angle(PlayerCharacterEntity.CacheTransform.forward, actionDirection);
                if (tempCalculateAngle > 15f)
                {
                    if (queueSkill != null && queueSkill.IsAttack())
                        turningState = TurningState.UseSkill;
                    else if (tempPressAttack)
                        turningState = TurningState.Attack;
                    else if (tempPressActivate)
                        turningState = TurningState.Activate;
                    turnTimeCounter = ((180f - tempCalculateAngle) / 180f) * turnToTargetDuration;
                    targetLookDirection = actionDirection;
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

                if (queueSkill != null && !queueSkill.IsAttack())
                {
                    // If it is not attack skill, use it immediately
                    UseSkill();
                }
                queueSkill = null;
            }
            else
            {
                if (moveDirection.magnitude != 0f)
                    targetLookDirection = moveDirection;
            }

            // Hide Npc UIs when move
            if (moveDirection.magnitude != 0f)
            {
                if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                    CacheUISceneGameplay.uiNpcDialog.Hide();
                PlayerCharacterEntity.StopMove();
                PlayerCharacterEntity.SetTargetEntity(null);
            }
            PlayerCharacterEntity.KeyMovement(moveDirection, InputManager.GetButtonDown("Jump"));
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
