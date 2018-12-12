using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        protected virtual void UpdateInput()
        {
            var fields = ComponentCollector.Get(typeof(InputFieldWrapper));
            foreach (var field in fields)
            {
                if (((InputFieldWrapper)field).isFocused)
                {
                    destination = null;
                    PlayerCharacterEntity.StopMove();
                    return;
                }
            }

            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");

            if (PlayerCharacterEntity.IsDead())
                return;

            // If it's building something, don't allow to activate NPC/Warp/Pickup Item
            if (currentBuildingEntity == null)
            {
                // Activate nearby npcs / players / activable buildings
                if (InputManager.GetButtonDown("Activate"))
                {
                    targetPlayer = null;
                    targetNpc = null;
                    tempCount = OverlapObjects(CharacterTransform.position, gameInstance.conversationDistance, gameInstance.characterLayer.Mask);
                    for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                    {
                        tempGameObject = GetOverlapObject(tempCounter);
                        if (targetPlayer == null)
                        {
                            targetPlayer = tempGameObject.GetComponent<BasePlayerCharacterEntity>();
                            if (targetPlayer == PlayerCharacterEntity)
                                targetPlayer = null;
                        }
                        if (targetNpc == null)
                            targetNpc = tempGameObject.GetComponent<NpcEntity>();
                    }
                    // Priority Player -> Npc -> Buildings
                    if (targetPlayer != null && CacheUISceneGameplay != null)
                        CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
                    else if (targetNpc != null)
                        PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                    if (overlapColliders.Length == 0)
                        PlayerCharacterEntity.RequestEnterWarp();
                }
                // Pick up nearby items
                if (InputManager.GetButtonDown("PickUpItem"))
                {
                    tempCount = OverlapObjects(CharacterTransform.position, gameInstance.pickUpItemDistance, gameInstance.itemDropLayer.Mask);
                    for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                    {
                        tempGameObject = GetOverlapObject(tempCounter);
                        targetItemDrop = tempGameObject.GetComponent<ItemDropEntity>();
                        if (targetItemDrop != null)
                        {
                            PlayerCharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                            break;
                        }
                    }
                }
            }
            UpdatePointClickInput();
            UpdateWASDInput();
            UpdateBuilding();
        }

        protected void UpdatePointClickInput()
        {
            // If it's building something, not allow point click movement
            if (currentBuildingEntity != null)
                return;
            getMouseDown = Input.GetMouseButtonDown(0);
            getMouseUp = Input.GetMouseButtonUp(0);
            getMouse = Input.GetMouseButton(0);
            if (getMouseDown)
            {
                isMouseDragOrHoldOrOverUI = false;
                mouseDownTime = Time.unscaledTime;
                mouseDownPosition = Input.mousePosition;
            }
            isPointerOverUI = CacheUISceneGameplay != null && CacheUISceneGameplay.IsPointerOverUIObject();
            isMouseDragDetected = (Input.mousePosition - mouseDownPosition).magnitude > DETECT_MOUSE_DRAG_DISTANCE;
            isMouseHoldDetected = Time.unscaledTime - mouseDownTime > DETECT_MOUSE_HOLD_DURATION;
            isMouseHoldAndNotDrag = !isMouseDragDetected && isMouseHoldDetected;
            if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
                isMouseDragOrHoldOrOverUI = true;
            if (!isPointerOverUI && (getMouse || getMouseUp))
            {
                PlayerCharacterEntity.SetTargetEntity(null);
                LiteNetLibIdentity targetIdentity = null;
                Vector3? targetPosition = null;
                var mouseUpOnTarget = getMouseUp &&
                        !isMouseDragOrHoldOrOverUI &&
                        (controllerMode == PlayerCharacterControllerMode.PointClick || controllerMode == PlayerCharacterControllerMode.Both);
                tempCount = FindClickObjects(out tempVector3);
                for (tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempTransform = GetRaycastTransform(tempCounter);
                    // When clicking on target
                    if (mouseUpOnTarget)
                    {
                        targetPlayer = tempTransform.GetComponent<BasePlayerCharacterEntity>();
                        targetMonster = tempTransform.GetComponent<BaseMonsterCharacterEntity>();
                        targetNpc = tempTransform.GetComponent<NpcEntity>();
                        targetItemDrop = tempTransform.GetComponent<ItemDropEntity>();
                        targetHarvestable = tempTransform.GetComponent<HarvestableEntity>();
                        targetPosition = GetRaycastPoint(tempCounter);
                        PlayerCharacterEntity.SetTargetEntity(null);
                        lastNpcObjectId = 0;
                        if (targetPlayer != null && !targetPlayer.IsDead())
                        {
                            targetPosition = targetPlayer.CacheTransform.position;
                            targetIdentity = targetPlayer.Identity;
                            PlayerCharacterEntity.SetTargetEntity(targetPlayer);
                            break;
                        }
                        else if (targetMonster != null && !targetMonster.IsDead())
                        {
                            targetPosition = targetMonster.CacheTransform.position;
                            targetIdentity = targetMonster.Identity;
                            PlayerCharacterEntity.SetTargetEntity(targetMonster);
                            break;
                        }
                        else if (targetNpc != null)
                        {
                            targetPosition = targetNpc.CacheTransform.position;
                            targetIdentity = targetNpc.Identity;
                            PlayerCharacterEntity.SetTargetEntity(targetNpc);
                            break;
                        }
                        else if (targetItemDrop != null)
                        {
                            targetPosition = targetItemDrop.CacheTransform.position;
                            targetIdentity = targetItemDrop.Identity;
                            PlayerCharacterEntity.SetTargetEntity(targetItemDrop);
                            break;
                        }
                        else if (targetHarvestable != null && !targetHarvestable.IsDead())
                        {
                            targetPosition = targetHarvestable.CacheTransform.position;
                            targetIdentity = targetHarvestable.Identity;
                            PlayerCharacterEntity.SetTargetEntity(targetHarvestable);
                            break;
                        }
                    }
                    // When holding on target
                    else if (isMouseHoldAndNotDrag)
                    {
                        var buildingMaterial = tempTransform.GetComponent<BuildingMaterial>();
                        PlayerCharacterEntity.SetTargetEntity(null);
                        if (buildingMaterial != null && buildingMaterial.buildingEntity != null && !buildingMaterial.buildingEntity.IsDead())
                        {
                            targetPosition = buildingMaterial.buildingEntity.CacheTransform.position;
                            targetIdentity = buildingMaterial.buildingEntity.Identity;
                            PlayerCharacterEntity.SetTargetEntity(buildingMaterial.buildingEntity);
                            break;
                        }
                    }
                }
                // When clicking on map (any non-collider position)
                // tempWorldPoint is come from FindClickObjects()
                if (dimensionType == DimensionType.Dimension2D && mouseUpOnTarget && !targetPosition.HasValue)
                {
                    tempVector3.z = 0;
                    targetPosition = tempVector3;
                }
                // If Found target, do something
                if (targetPosition.HasValue)
                {
                    // Close NPC dialog, when target changes
                    if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                        CacheUISceneGameplay.uiNpcDialog.Hide();
                    // Clear queue using skill
                    queueUsingSkill = null;
                    // Move to target, will hide destination when target is object
                    if (targetIdentity != null)
                        destination = null;
                    else
                    {
                        destination = targetPosition.Value;
                        PlayerCharacterEntity.PointClickMovement(targetPosition.Value);
                    }
                }
            }
        }

        protected void UpdateWASDInput()
        {
            if (controllerMode != PlayerCharacterControllerMode.WASD &&
                controllerMode != PlayerCharacterControllerMode.Both)
                return;

            if (PlayerCharacterEntity.IsPlayingActionAnimation())
            {
                PlayerCharacterEntity.StopMove();
                return;
            }

            // If mobile platforms, don't receive input raw to make it smooth
            var raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            var moveDirection = GetMoveDirection(InputManager.GetAxis("Horizontal", raw), InputManager.GetAxis("Vertical", raw));

            if (moveDirection.magnitude != 0f)
            {
                if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                    CacheUISceneGameplay.uiNpcDialog.Hide();
                FindAndSetBuildingAreaFromCharacterDirection();
            }

            // For WASD mode, Using skill when player pressed hotkey
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                destination = null;
                PlayerCharacterEntity.StopMove();
                Skill skill = null;
                if (GameInstance.Skills.TryGetValue(queueUsingSkillValue.dataId, out skill) && skill != null)
                {
                    if (skill.IsAttack())
                    {
                        BaseCharacterEntity targetEntity;
                        if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                        {
                            var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance, false, true, false);
                            if (nearestTarget != null)
                                PlayerCharacterEntity.SetTargetEntity(nearestTarget);
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
            // Attack when player pressed attack button
            else if (InputManager.GetButton("Attack"))
            {
                destination = null;
                PlayerCharacterEntity.StopMove();
                BaseCharacterEntity targetEntity;
                if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                {
                    var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetAttackDistance() + lockAttackTargetDistance, false, true, false);
                    if (nearestTarget != null)
                        PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                    else
                        RequestAttack();
                }
                else if (!wasdLockAttackTarget)
                    RequestAttack();
            }
            // Move
            else
            {
                if (moveDirection.magnitude != 0f)
                {
                    PlayerCharacterEntity.StopMove();
                    destination = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                }
                PlayerCharacterEntity.KeyMovement(moveDirection, InputManager.GetButtonDown("Jump"));
            }
        }

        protected void UpdateBuilding()
        {
            // Current building UI
            BuildingEntity currentBuilding;
            var uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
            if (uiCurrentBuilding != null)
            {
                if (uiCurrentBuilding.IsVisible() && !PlayerCharacterEntity.TryGetTargetEntity(out currentBuilding))
                    uiCurrentBuilding.Hide();
            }

            // Construct building UI
            var uiConstructBuilding = CacheUISceneGameplay.uiConstructBuilding;
            if (uiConstructBuilding != null)
            {
                if (uiConstructBuilding.IsVisible() && currentBuildingEntity == null)
                    uiConstructBuilding.Hide();
                if (!uiConstructBuilding.IsVisible() && currentBuildingEntity != null)
                    uiConstructBuilding.Show();
            }

            if (currentBuildingEntity == null)
                return;

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
                FindAndSetBuildingAreaFromMousePosition();
        }

        protected void UpdateFollowTarget()
        {
            // Temp variables
            if (TryGetAttackingCharacter(out targetEnemy))
            {
                if (targetEnemy.IsDead())
                {
                    queueUsingSkill = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                    PlayerCharacterEntity.StopMove();
                    return;
                }

                if (PlayerCharacterEntity.IsPlayingActionAnimation())
                {
                    PlayerCharacterEntity.StopMove();
                    return;
                }

                // Find attack distance and fov, from weapon or skill
                var attackDistance = 0f;
                var attackFov = 0f;
                if (!GetAttackDistanceAndFov(out attackDistance, out attackFov))
                    return;
                var actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetEnemy.gameObject, actDistance, gameInstance.characterLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetEnemy.CacheTransform.position))
                    {
                        // If has queue using skill, attack by the skill
                        if (queueUsingSkill.HasValue)
                            RequestUsePendingSkill();
                        else
                            RequestAttack();
                    }
                }
                else
                    UpdateTargetEntityPosition(targetEnemy);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetPlayer))
            {
                if (targetPlayer.IsDead())
                {
                    queueUsingSkill = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                    PlayerCharacterEntity.StopMove();
                    return;
                }
                var actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetPlayer.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.StopMove();
                    // TODO: do something
                }
                else
                    UpdateTargetEntityPosition(targetPlayer);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetMonster))
            {
                if (targetMonster.IsDead())
                {
                    queueUsingSkill = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                    PlayerCharacterEntity.StopMove();
                    return;
                }
                var actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetMonster.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.StopMove();
                    // TODO: do something
                }
                else
                    UpdateTargetEntityPosition(targetMonster);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetNpc))
            {
                var actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetNpc.CacheTransform.position) <= actDistance)
                {
                    if (lastNpcObjectId != targetNpc.ObjectId)
                    {
                        PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                        lastNpcObjectId = targetNpc.ObjectId;
                    }
                    PlayerCharacterEntity.StopMove();
                }
                else
                    UpdateTargetEntityPosition(targetNpc);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetItemDrop))
            {
                var actDistance = gameInstance.pickUpItemDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                    PlayerCharacterEntity.StopMove();
                    PlayerCharacterEntity.SetTargetEntity(null);
                }
                else
                    UpdateTargetEntityPosition(targetItemDrop);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetBuilding))
            {
                var uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
                var actDistance = gameInstance.buildDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetBuilding.CacheTransform.position) <= actDistance)
                {
                    if (uiCurrentBuilding != null && !uiCurrentBuilding.IsVisible())
                        uiCurrentBuilding.Show();
                    PlayerCharacterEntity.StopMove();
                }
                else
                {
                    UpdateTargetEntityPosition(targetBuilding);
                    if (uiCurrentBuilding != null && uiCurrentBuilding.IsVisible())
                        uiCurrentBuilding.Hide();
                }
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetHarvestable))
            {
                if (targetHarvestable.IsDead())
                {
                    queueUsingSkill = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                    PlayerCharacterEntity.StopMove();
                    return;
                }

                var attackDistance = 0f;
                var attackFov = 0f;
                if (!GetAttackDistanceAndFov(out attackDistance, out attackFov))
                    return;
                var actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetHarvestable.gameObject, actDistance, gameInstance.harvestableLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetHarvestable.CacheTransform.position))
                        RequestAttack();
                }
                else
                    UpdateTargetEntityPosition(targetHarvestable);
            }
        }

        protected void UpdateTargetEntityPosition(BaseGameEntity entity)
        {
            if (entity == null)
                return;

            var targetPosition = entity.CacheTransform.position;
            PlayerCharacterEntity.PointClickMovement(targetPosition);
        }

        public override void UseHotkey(int hotkeyIndex)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= PlayerCharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            currentBuildingEntity = null;

            var hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            var skill = hotkey.GetSkill();
            if (skill != null)
            {
                var skillIndex = PlayerCharacterEntity.IndexOfSkill(skill.DataId);
                if (skillIndex >= 0)
                {
                    BaseCharacterEntity attackingCharacter;
                    if (TryGetAttackingCharacter(out attackingCharacter))
                    {
                        // If attacking any character, will use skill later
                        queueUsingSkill = new UsingSkillData(null, skill.DataId);
                    }
                    else if (PlayerCharacterEntity.Skills[skillIndex].CanUse(PlayerCharacterEntity))
                    {
                        // If not attacking any character, use skill immediately
                        if (skill.IsAttack() && IsLockTarget())
                        {
                            // If attacking any character, will use skill later
                            queueUsingSkill = new UsingSkillData(null, skill.DataId);
                            var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance, false, true, false);
                            if (nearestTarget != null)
                                PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                        }
                        else
                        {
                            destination = null;
                            PlayerCharacterEntity.StopMove();
                            RequestUseSkill(CharacterTransform.position, skill.DataId);
                        }
                    }
                }
            }
            var item = hotkey.GetItem();
            if (item != null)
            {
                var itemIndex = PlayerCharacterEntity.IndexOfNonEquipItem(item.DataId);
                if (itemIndex >= 0)
                {
                    if (item.IsEquipment())
                        RequestEquipItem((ushort)itemIndex);
                    else if (item.IsPotion() || item.IsPet())
                        RequestUseItem((ushort)itemIndex);
                    else if (item.IsBuilding())
                    {
                        destination = null;
                        PlayerCharacterEntity.StopMove();
                        buildingItemIndex = itemIndex;
                        currentBuildingEntity = Instantiate(item.buildingEntity);
                        currentBuildingEntity.SetupAsBuildMode();
                        currentBuildingEntity.CacheTransform.parent = null;
                        FindAndSetBuildingAreaFromCharacterDirection();
                    }
                }
            }
        }
    }
}
