using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using LiteNetLibManager;

public enum PlayerCharacterControllerMode
{
    PointClick,
    WASD,
    Both,
}

public class PlayerCharacterController : BasePlayerCharacterController
{
    public const float DETECT_MOUSE_DRAG_DISTANCE = 10f;
    public const float DETECT_MOUSE_HOLD_DURATION = 1f;
    public PlayerCharacterControllerMode controllerMode;
    public bool wasdLockAttackTarget;
    public float lockAttackTargetDistance = 10f;
    public struct UsingSkillData
    {
        public Vector3 position;
        public int skillIndex;
        public UsingSkillData(Vector3 position, int skillIndex)
        {
            this.position = position;
            this.skillIndex = skillIndex;
        }
    }
    protected Vector3? destination;
    protected UsingSkillData? queueUsingSkill;
    protected Vector3 mouseDownPosition;
    protected float mouseDownTime;
    protected bool isMouseDragOrHoldOrOverUI;
    protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

    protected override void Update()
    {
        base.Update();

        if (CacheTargetObject != null)
            CacheTargetObject.gameObject.SetActive(destination.HasValue);

        if (CacheCharacterEntity.CurrentHp <= 0)
        {
            queueUsingSkill = null;
            destination = null;
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetTargetCharacter(null);
        }
        else
        {
            BaseCharacterEntity targetCharacter = null;
            CacheCharacterEntity.TryGetTargetEntity(out targetCharacter);
            if (CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetTargetCharacter(targetCharacter);
        }

        if (destination.HasValue)
        {
            var destinationValue = destination.Value;
            if (CacheTargetObject != null)
                CacheTargetObject.transform.position = destinationValue;
            if (Vector3.Distance(destinationValue, CacheCharacterTransform.position) < stoppingDistance + 0.5f)
                destination = null;
        }
        
        UpdateInput();
        UpdateFollowTarget();
    }

    protected virtual void UpdateInput()
    {
        if (!CacheCharacterEntity.IsOwnerClient)
            return;

        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");

        if (CacheCharacterEntity.CurrentHp <= 0)
            return;

        switch (controllerMode)
        {
            case PlayerCharacterControllerMode.PointClick:
                UpdatePointClickInput();
                break;
            case PlayerCharacterControllerMode.WASD:
                UpdateWASDInput();
                break;
            default:
                UpdatePointClickInput();
                UpdateWASDInput();
                break;
        }

        // Activate nearby npcs
        if (InputManager.GetButtonDown("Activate"))
        {
            var foundEntities = Physics.OverlapSphere(CacheCharacterTransform.position, gameInstance.conversationDistance, gameInstance.characterLayer.Mask);
            foreach (var foundEntity in foundEntities)
            {
                var npcEntity = foundEntity.GetComponent<NpcEntity>();
                if (npcEntity != null)
                {
                    CacheCharacterEntity.RequestNpcActivate(npcEntity.ObjectId);
                    break;
                }
            }
        }
        // Pick up nearby items
        if (InputManager.GetButtonDown("PickUpItem"))
        {
            var foundEntities = Physics.OverlapSphere(CacheCharacterTransform.position, gameInstance.pickUpItemDistance, gameInstance.itemDropLayer.Mask);
            foreach (var foundEntity in foundEntities)
            {
                var itemDropEntity = foundEntity.GetComponent<ItemDropEntity>();
                if (itemDropEntity != null)
                {
                    CacheCharacterEntity.RequestPickupItem(itemDropEntity.ObjectId);
                    break;
                }
            }
        }
    }

    protected void UpdatePointClickInput()
    {
        var isPointerOverUI = CacheUISceneGameplay != null && CacheUISceneGameplay.IsPointerOverUIObject();
        if (Input.GetMouseButtonDown(0))
        {
            isMouseDragOrHoldOrOverUI = false;
            mouseDownTime = Time.unscaledTime;
            mouseDownPosition = Input.mousePosition;
        }
        var isMouseDragDetected = (Input.mousePosition - mouseDownPosition).magnitude > DETECT_MOUSE_DRAG_DISTANCE;
        var isMouseHoldDetected = Time.unscaledTime - mouseDownTime > DETECT_MOUSE_HOLD_DURATION;
        if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
            isMouseDragOrHoldOrOverUI = true;
        if (!isPointerOverUI && Input.GetMouseButtonUp(0) && !isMouseDragOrHoldOrOverUI)
        {
            var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
            CacheCharacterEntity.SetTargetEntity(null);
            LiteNetLibIdentity targetIdentity = null;
            Vector3? targetPosition = null;
            RaycastHit[] hits = Physics.RaycastAll(targetCamera.ScreenPointToRay(Input.mousePosition), 100f);
            foreach (var hit in hits)
            {
                var hitTransform = hit.transform;
                targetPosition = hit.point;
                var playerEntity = hitTransform.GetComponent<PlayerCharacterEntity>();
                var monsterEntity = hitTransform.GetComponent<MonsterCharacterEntity>();
                var npcEntity = hitTransform.GetComponent<NpcEntity>();
                var itemDropEntity = hitTransform.GetComponent<ItemDropEntity>();
                CacheCharacterEntity.SetTargetEntity(null);
                if (playerEntity != null && playerEntity.CurrentHp > 0)
                {
                    targetPosition = playerEntity.CacheTransform.position;
                    targetIdentity = playerEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(playerEntity);
                    break;
                }
                else if (monsterEntity != null && monsterEntity.CurrentHp > 0)
                {
                    targetPosition = monsterEntity.CacheTransform.position;
                    targetIdentity = monsterEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(monsterEntity);
                    break;
                }
                else if (npcEntity != null)
                {
                    targetPosition = npcEntity.CacheTransform.position;
                    targetIdentity = npcEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(npcEntity);
                    break;
                }
                else if (itemDropEntity != null)
                {
                    targetPosition = itemDropEntity.CacheTransform.position;
                    targetIdentity = itemDropEntity.Identity;
                    CacheCharacterEntity.SetTargetEntity(itemDropEntity);
                    break;
                }
            }
            if (targetPosition.HasValue)
            {
                if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                    CacheUISceneGameplay.uiNpcDialog.Hide();
                queueUsingSkill = null;
                if (targetIdentity != null)
                    destination = null;
                else
                {
                    destination = targetPosition.Value;
                    CacheCharacterEntity.PointClickMovement(targetPosition.Value);
                }
            }
        }
    }

    protected void UpdateWASDInput()
    {
        if (CacheCharacterEntity.IsPlayingActionAnimation())
        {
            CacheCharacterEntity.StopMove();
            return;
        }

        var horizontalInput = InputManager.GetAxis("Horizontal", false);
        var verticalInput = InputManager.GetAxis("Vertical", false);
        var jumpInput = InputManager.GetButtonDown("Jump");

        var moveDirection = Vector3.zero;
        var cameraTransform = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera.transform : Camera.main.transform;
        if (cameraTransform != null)
        {
            moveDirection += cameraTransform.forward * verticalInput;
            moveDirection += cameraTransform.right * horizontalInput;
        }
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;

        if (moveDirection.magnitude > 0.1f && CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
            CacheUISceneGameplay.uiNpcDialog.Hide();

        if (queueUsingSkill.HasValue)
        {
            destination = null;
            CacheCharacterEntity.StopMove();
            var queueUsingSkillValue = queueUsingSkill.Value;
            var characterSkill = CacheCharacterEntity.Skills[queueUsingSkillValue.skillIndex];
            var skill = characterSkill.GetSkill();
            if (skill != null)
            {
                if (skill.IsAttack())
                {
                    BaseCharacterEntity targetEntity;
                    if (wasdLockAttackTarget && !CacheCharacterEntity.TryGetTargetEntity(out targetEntity))
                    {
                        var nearestTarget = FindNearestAliveCharacter<MonsterCharacterEntity>(CacheCharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance);
                        if (nearestTarget != null)
                            CacheCharacterEntity.SetTargetEntity(nearestTarget);
                        else
                            RequestUsePendingSkill();
                    }
                    else if (!wasdLockAttackTarget)
                        RequestUsePendingSkill();
                }
                else
                    RequestUsePendingSkill();
            }
            else
                queueUsingSkill = null;
        }
        else if (InputManager.GetButton("Attack"))
        {
            destination = null;
            CacheCharacterEntity.StopMove();
            BaseCharacterEntity targetEntity;
            if (wasdLockAttackTarget && !CacheCharacterEntity.TryGetTargetEntity(out targetEntity))
            {
                var nearestTarget = FindNearestAliveCharacter<MonsterCharacterEntity>(CacheCharacterEntity.GetAttackDistance() + lockAttackTargetDistance);
                if (nearestTarget != null)
                    CacheCharacterEntity.SetTargetEntity(nearestTarget);
                else
                    RequestAttack();
            }
            else if (!wasdLockAttackTarget)
                RequestAttack();
        }
        else
        {
            if (moveDirection.magnitude > 0)
            {
                if (CacheCharacterEntity.HasNavPaths)
                    CacheCharacterEntity.StopMove();
                destination = null;
                CacheCharacterEntity.SetTargetEntity(null);
            }
            CacheCharacterEntity.KeyMovement(moveDirection, jumpInput);
        }
    }

    protected void UpdateFollowTarget()
    {
        // Temp variables
        BaseCharacterEntity targetEnemy;
        PlayerCharacterEntity targetPlayer;
        NpcEntity targetNpc;
        ItemDropEntity targetItemDrop;
        if (TryGetAttackingCharacter(out targetEnemy))
        {
            if (targetEnemy.CurrentHp <= 0)
            {
                queueUsingSkill = null;
                CacheCharacterEntity.SetTargetEntity(null);
                CacheCharacterEntity.StopMove();
                return;
            }

            if (CacheCharacterEntity.IsPlayingActionAnimation())
            {
                CacheCharacterEntity.StopMove();
                return;
            }

            // Find attack distance and fov, from weapon or skill
            var attackDistance = CacheCharacterEntity.GetAttackDistance();
            var attackFov = CacheCharacterEntity.GetAttackFov();
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                var characterSkill = CacheCharacterEntity.Skills[queueUsingSkillValue.skillIndex];
                var skill = characterSkill.GetSkill();
                if (skill != null)
                {
                    if (skill.IsAttack())
                    {
                        attackDistance = CacheCharacterEntity.GetSkillAttackDistance(skill);
                        attackFov = CacheCharacterEntity.GetSkillAttackFov(skill);
                    }
                    else
                    {
                        // Stop movement to use non attack skill
                        CacheCharacterEntity.StopMove();
                        RequestUsePendingSkill();
                        return;
                    }
                }
                else
                    queueUsingSkill = null;
            }
            var actDistance = attackDistance;
            actDistance -= actDistance * 0.1f;
            actDistance -= stoppingDistance;
            actDistance += targetEnemy.CacheCapsuleCollider.radius;
            if (Vector3.Distance(CacheCharacterTransform.position, targetEnemy.CacheTransform.position) <= actDistance)
            {
                // Stop movement to attack
                CacheCharacterEntity.StopMove();
                var halfFov = attackFov * 0.5f;
                var targetDir = (CacheCharacterTransform.position - targetEnemy.CacheTransform.position).normalized;
                var angle = Vector3.Angle(targetDir, CacheCharacterTransform.forward);
                if (angle < 180 + halfFov && angle > 180 - halfFov)
                {
                    // If has queue using skill, attack by the skill
                    if (queueUsingSkill.HasValue)
                        RequestUsePendingSkill();
                    else
                        RequestAttack();

                    /** Hint: Uncomment these to make it attack one time and stop 
                    //  when reached target and doesn't pressed on mouse like as diablo
                    if (CacheUISceneGameplay.IsPointerOverUIObject() || !Input.GetMouseButtonUp(0))
                    {
                        queueUsingSkill = null;
                        CacheCharacterEntity.SetTargetEntity(null);
                        StopPointClickMove();
                    }
                    */
                }
            }
            else
                UpdateTargetEntityPosition(targetEnemy);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetPlayer))
        {
            if (targetPlayer.CurrentHp <= 0)
            {
                queueUsingSkill = null;
                CacheCharacterEntity.SetTargetEntity(null);
                CacheCharacterEntity.StopMove();
                return;
            }
            var actDistance = gameInstance.conversationDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetPlayer.CacheTransform.position) <= actDistance)
            {
                CacheCharacterEntity.StopMove();
                // TODO: do something
            }
            else
                UpdateTargetEntityPosition(targetPlayer);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetNpc))
        {
            var actDistance = gameInstance.conversationDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetNpc.CacheTransform.position) <= actDistance)
            {
                CacheCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                CacheCharacterEntity.StopMove();
                CacheCharacterEntity.SetTargetEntity(null);
            }
            else
                UpdateTargetEntityPosition(targetNpc);
        }
        else if (CacheCharacterEntity.TryGetTargetEntity(out targetItemDrop))
        {
            var actDistance = gameInstance.pickUpItemDistance - stoppingDistance;
            if (Vector3.Distance(CacheCharacterTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
            {
                CacheCharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                CacheCharacterEntity.StopMove();
                CacheCharacterEntity.SetTargetEntity(null);
            }
            else
                UpdateTargetEntityPosition(targetItemDrop);
        }
    }

    protected void UpdateTargetEntityPosition(RpgNetworkEntity entity)
    {
        if (entity == null)
            return;

        var targetPosition = entity.CacheTransform.position;
        CacheCharacterEntity.PointClickMovement(targetPosition);
    }

    protected T FindNearestAliveCharacter<T>(float distance) where T : BaseCharacterEntity
    {
        T result = null;
        var colliders = Physics.OverlapSphere(CacheCharacterTransform.position, distance, gameInstance.characterLayer.Mask);
        if (colliders != null && colliders.Length > 0)
        {
            float tempDistance;
            T tempEntity;
            float nearestDistance = float.MaxValue;
            T nearestEntity = null;
            foreach (var collider in colliders)
            {
                tempEntity = collider.GetComponent<T>();
                if (tempEntity == null || tempEntity.CurrentHp <= 0)
                    continue;

                tempDistance = Vector3.Distance(CacheCharacterTransform.position, tempEntity.CacheTransform.position);
                if (tempDistance < nearestDistance)
                {
                    nearestDistance = tempDistance;
                    nearestEntity = tempEntity;
                }
            }
            result = nearestEntity;
        }
        return result;
    }

    public void RequestAttack()
    {
        CacheCharacterEntity.RequestAttack();
    }

    public void RequestUseSkill(Vector3 position, int skillIndex)
    {
        CacheCharacterEntity.RequestUseSkill(position, skillIndex);
    }

    public void RequestUsePendingSkill()
    {
        if (queueUsingSkill.HasValue)
        {
            var queueUsingSkillValue = queueUsingSkill.Value;
            RequestUseSkill(queueUsingSkillValue.position, queueUsingSkillValue.skillIndex);
            queueUsingSkill = null;
        }
    }

    public void RequestUseItem(int itemIndex)
    {
        if (CacheCharacterEntity.CurrentHp > 0)
            CacheCharacterEntity.RequestUseItem(itemIndex);
    }

    public override void UseHotkey(int hotkeyIndex)
    {
        if (hotkeyIndex < 0 || hotkeyIndex >= CacheCharacterEntity.hotkeys.Count)
            return;

        var hotkey = CacheCharacterEntity.hotkeys[hotkeyIndex];
        var skill = hotkey.GetSkill();
        if (skill != null)
        {
            var skillIndex = CacheCharacterEntity.IndexOfSkill(skill.Id);
            if (skillIndex >= 0 && skillIndex < CacheCharacterEntity.skills.Count)
            {
                BaseCharacterEntity attackingCharacter;
                if (TryGetAttackingCharacter(out attackingCharacter))
                    queueUsingSkill = new UsingSkillData(CacheCharacterTransform.position, skillIndex);
                else if ((controllerMode == PlayerCharacterControllerMode.WASD || controllerMode == PlayerCharacterControllerMode.Both) || skill.IsAttack())
                    queueUsingSkill = new UsingSkillData(CacheCharacterTransform.position, skillIndex);
                else if (CacheCharacterEntity.skills[skillIndex].CanUse(CacheCharacterEntity))
                {
                    destination = null;
                    queueUsingSkill = null;
                    CacheCharacterEntity.StopMove();
                    RequestUseSkill(CacheCharacterTransform.position, skillIndex);
                }
            }
        }
        var item = hotkey.GetItem();
        if (item != null)
        {
            var itemIndex = CacheCharacterEntity.IndexOfNonEquipItem(item.Id);
            if (itemIndex >= 0 && itemIndex < CacheCharacterEntity.nonEquipItems.Count)
                RequestUseItem(itemIndex);
        }
    }

    public bool TryGetAttackingCharacter(out BaseCharacterEntity character)
    {
        character = null;
        if (CacheCharacterEntity.TryGetTargetEntity(out character))
        {
            // TODO: Returning Pvp characters
            if (character is MonsterCharacterEntity)
                return true;
            else
                character = null;
        }
        return false;
    }
}
