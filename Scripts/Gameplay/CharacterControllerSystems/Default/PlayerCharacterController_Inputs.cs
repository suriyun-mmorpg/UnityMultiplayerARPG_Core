using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        public virtual void UpdateInput()
        {
            if (GenericUtils.IsFocusInputField())
                return;

            if (CacheGameplayCameraControls != null)
            {
                CacheGameplayCameraControls.updateRotationX = false;
                CacheGameplayCameraControls.updateRotationY = false;
                CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");
            }

            if (PlayerCharacterEntity.IsDead())
                return;

            // If it's building something, don't allow to activate NPC/Warp/Pickup Item
            if (CurrentBuildingEntity == null)
            {
                // Activate nearby npcs / players / activable buildings
                if (InputManager.GetButtonDown("Activate"))
                {
                    targetPlayer = null;
                    if (activatableEntityDetector.players.Count > 0)
                        targetPlayer = activatableEntityDetector.players[0];
                    targetNpc = null;
                    if (activatableEntityDetector.npcs.Count > 0)
                        targetNpc = activatableEntityDetector.npcs[0];
                    targetBuilding = null;
                    if (activatableEntityDetector.buildings.Count > 0)
                        targetBuilding = activatableEntityDetector.buildings[0];
                    // Priority Player -> Npc -> Buildings
                    if (targetPlayer != null && CacheUISceneGameplay != null)
                    {
                        // Show dealing, invitation menu
                        CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
                    }
                    else if (targetNpc != null)
                    {
                        // Talk to NPC
                        PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                    }
                    else if (targetBuilding != null)
                    {
                        // Use building
                        ActivateBuilding(targetBuilding);
                    }
                    else
                    {
                        // Enter warp, For some warp portals that `warpImmediatelyWhenEnter` is FALSE
                        PlayerCharacterEntity.RequestEnterWarp();
                    }
                }
                // Pick up nearby items
                if (InputManager.GetButtonDown("PickUpItem"))
                {
                    targetItemDrop = null;
                    if (itemDropEntityDetector.itemDrops.Count > 0)
                        targetItemDrop = itemDropEntityDetector.itemDrops[0];
                    if (targetItemDrop != null)
                        PlayerCharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                }
                // Reload
                if (InputManager.GetButtonDown("Reload"))
                {
                    // Reload ammo when press the button
                    ReloadAmmo();
                }
                // Find target to attack
                if (InputManager.GetButtonDown("FindEnemy"))
                {
                    ++findingEnemyIndex;
                    if (findingEnemyIndex < 0 || findingEnemyIndex >= enemyEntityDetector.characters.Count)
                        findingEnemyIndex = 0;
                    if (enemyEntityDetector.characters.Count > 0)
                    {
                        SetTarget(null);
                        if (!enemyEntityDetector.characters[findingEnemyIndex].GetCaches().IsHide &&
                            !enemyEntityDetector.characters[findingEnemyIndex].IsDead())
                        {
                            SetTarget(enemyEntityDetector.characters[findingEnemyIndex]);
                            if (SelectedEntity != null)
                                targetLookDirection = (SelectedEntity.CacheTransform.position - MovementTransform.position).normalized;
                        }
                    }
                }
                if (InputManager.GetButtonDown("ExitVehicle"))
                {
                    // Exit vehicle
                    PlayerCharacterEntity.RequestExitVehicle();
                }
                if (InputManager.GetButtonDown("SwitchEquipWeaponSet"))
                {
                    // Switch equip weapon set
                    PlayerCharacterEntity.RequestSwitchEquipWeaponSet((byte)(PlayerCharacterEntity.EquipWeaponSet + 1));
                }
                // Auto reload
                if (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                    PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
                {
                    // Reload ammo when empty and not press any keys
                    ReloadAmmo();
                }
            }
            // Update enemy detecting radius to attack distance
            enemyEntityDetector.detectingRadius = PlayerCharacterEntity.GetAttackDistance(false) + lockAttackTargetDistance;
            // Update inputs
            UpdatePointClickInput();
            UpdateWASDInput();
            UpdateBuilding();
        }

        protected void ReloadAmmo()
        {
            // Reload ammo at server
            if (!PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(false);
            else if (!PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(true);
        }

        public virtual void UpdatePointClickInput()
        {
            // If it's building something, not allow point click movement
            if (CurrentBuildingEntity != null)
                return;

            // If it's aiming skills, not allow point click movement
            if (UICharacterHotkeys.UsingHotkey != null)
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
            // Read inputs
            isPointerOverUI = CacheUISceneGameplay != null && CacheUISceneGameplay.IsPointerOverUIObject();
            isMouseDragDetected = (Input.mousePosition - mouseDownPosition).magnitude > DETECT_MOUSE_DRAG_DISTANCE;
            isMouseHoldDetected = Time.unscaledTime - mouseDownTime > DETECT_MOUSE_HOLD_DURATION;
            isMouseHoldAndNotDrag = !isMouseDragDetected && isMouseHoldDetected;
            if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
            {
                // Detected mouse dragging or hold on an UIs
                isMouseDragOrHoldOrOverUI = true;
            }
            // Will set move target when pointer isn't point on an UIs 
            if (!isPointerOverUI && (getMouse || getMouseUp))
            {
                // Clear target
                targetEntity = null;
                targetPosition = null;
                // Prepare temp variables
                Transform tempTransform;
                Vector3 tempVector3;
                bool tempHasMapPosition = false;
                Vector3 tempMapPosition = Vector3.zero;
                float tempHighestY = float.MinValue;
                BuildingMaterial tempBuildingMaterial;
                // If mouse up while cursor point to target (character, item, npc and so on)
                bool mouseUpOnTarget = getMouseUp && !isMouseDragOrHoldOrOverUI && (controllerMode == PlayerCharacterControllerMode.PointClick || controllerMode == PlayerCharacterControllerMode.Both);
                int tempCount = FindClickObjects(out tempVector3);
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempTransform = GetRaycastTransform(tempCounter);
                    // When holding on target, or already enter edit building mode
                    if (isMouseHoldAndNotDrag || IsEditingBuilding)
                    {
                        targetBuilding = null;
                        tempBuildingMaterial = tempTransform.GetComponent<BuildingMaterial>();
                        if (tempBuildingMaterial != null && tempBuildingMaterial.buildingEntity != null)
                            targetBuilding = tempBuildingMaterial.buildingEntity;
                        if (targetBuilding && !targetBuilding.IsDead())
                        {
                            IsEditingBuilding = true;
                            SetTarget(targetBuilding);
                            tempHasMapPosition = false;
                            break;
                        }
                    }
                    // When clicking on target
                    else if (mouseUpOnTarget)
                    {
                        targetPlayer = tempTransform.GetComponent<BasePlayerCharacterEntity>();
                        targetMonster = tempTransform.GetComponent<BaseMonsterCharacterEntity>();
                        targetNpc = tempTransform.GetComponent<NpcEntity>();
                        targetItemDrop = tempTransform.GetComponent<ItemDropEntity>();
                        targetHarvestable = tempTransform.GetComponent<HarvestableEntity>();
                        targetBuilding = null;
                        tempBuildingMaterial = tempTransform.GetComponent<BuildingMaterial>();
                        if (tempBuildingMaterial != null && tempBuildingMaterial.buildingEntity != null)
                            targetBuilding = tempBuildingMaterial.buildingEntity;
                        PlayerCharacterEntity.SetTargetEntity(null);
                        lastNpcObjectId = 0;
                        if (targetPlayer != null && !targetCharacter.GetCaches().IsHide)
                        {
                            // Found activating entity as player character entity
                            SetTarget(targetPlayer);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetMonster != null && !targetCharacter.GetCaches().IsHide && !targetMonster.IsDead())
                        {
                            // Found activating entity as monster character entity
                            SetTarget(targetMonster);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetNpc != null)
                        {
                            // Found activating entity as npc entity
                            SetTarget(targetNpc);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetItemDrop != null)
                        {
                            // Found activating entity as item drop entity
                            SetTarget(targetItemDrop);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetHarvestable != null && !targetHarvestable.IsDead())
                        {
                            // Found activating entity as harvestable entity
                            SetTarget(targetHarvestable);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetBuilding && !targetBuilding.IsDead() && targetBuilding.Activatable)
                        {
                            // Found activating entity as building entity
                            IsEditingBuilding = false;
                            SetTarget(targetBuilding);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (!GetRaycastIsTrigger(tempCounter))
                        {
                            // Set clicked map position, it will be used if no activating entity found
                            tempHasMapPosition = true;
                            tempMapPosition = GetRaycastPoint(tempCounter);
                            if (tempMapPosition.y > tempHighestY)
                                tempHighestY = tempMapPosition.y;
                        }
                    }
                }
                // When clicked on map (Not touch any game entity)
                // - Clear selected target to hide selected entity UIs
                // - Set target position to position where mouse clicked
                if (tempHasMapPosition)
                {
                    SelectedEntity = null;
                    targetPosition = tempMapPosition;
                }
                // When clicked on map (any non-collider position)
                // tempVector3 is come from FindClickObjects()
                // - Clear character target to make character stop doing actions
                // - Clear selected target to hide selected entity UIs
                // - Set target position to position where mouse clicked
                if (gameInstance.DimensionType == DimensionType.Dimension2D && mouseUpOnTarget && tempCount == 0)
                {
                    PlayerCharacterEntity.SetTargetEntity(null);
                    SelectedEntity = null;
                    tempVector3.z = 0;
                    targetPosition = tempVector3;
                }
                // If Found target, do something
                if (targetPosition.HasValue)
                {
                    // Close NPC dialog, when target changes
                    HideNpcDialogs();
                    ClearQueueUsingSkill();
                    ClearQueueUsingSkillItem();

                    // Move to target, will hide destination when target is object
                    if (targetEntity != null)
                    {
                        destination = null;
                    }
                    else
                    {
                        destination = targetPosition.Value;
                        PlayerCharacterEntity.PointClickMovement(targetPosition.Value);
                    }
                }
            }
        }

        protected virtual void SetTarget(BaseGameEntity entity)
        {
            targetPosition = null;
            if (pointClickSetTargetImmediately ||
                (entity != null && SelectedEntity == entity) ||
                (entity != null && entity is ItemDropEntity))
            {
                targetPosition = entity.CacheTransform.position;
                targetEntity = entity;
                PlayerCharacterEntity.SetTargetEntity(entity);
            }
            SelectedEntity = entity;
        }

        protected virtual void ClearTarget()
        {
            targetPosition = null;
            targetEntity = null;
            PlayerCharacterEntity.SetTargetEntity(null);
            SelectedEntity = null;
        }

        public virtual void UpdateWASDInput()
        {
            if (controllerMode != PlayerCharacterControllerMode.WASD &&
                controllerMode != PlayerCharacterControllerMode.Both)
                return;

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
            Vector3 moveDirection = GetMoveDirection(InputManager.GetAxis("Horizontal", raw), InputManager.GetAxis("Vertical", raw));

            if (moveDirection.magnitude != 0f)
            {
                HideNpcDialogs();
                ClearQueueUsingSkill();
                ClearQueueUsingSkillItem();
                FindAndSetBuildingAreaFromCharacterDirection();
            }

            // Attack when player pressed attack button
            if (!UpdateWASDPendingSkill() &&
                !UpdateWASDPendingSkillItem() &&
                InputManager.GetButton("Attack"))
            {
                destination = null;
                PlayerCharacterEntity.StopMove();
                BaseCharacterEntity targetEntity;
                if (TryGetSelectedTargetAsAttackingCharacter(out targetEntity))
                    SetTarget(targetEntity);
                if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                {
                    // Find nearest target and move to the target
                    BaseCharacterEntity nearestTarget = PlayerCharacterEntity
                        .FindNearestAliveCharacter<BaseCharacterEntity>(
                        PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking) + lockAttackTargetDistance,
                        false,
                        true,
                        false);
                    SelectedEntity = nearestTarget;
                    if (nearestTarget != null)
                    {
                        // Set target, then attack later when moved nearby target
                        SetTarget(nearestTarget);
                    }
                    else
                    {
                        // No nearby target, so attack immediately
                        if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, GetDefaultAttackAimPosition()))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else if (!wasdLockAttackTarget)
                {
                    // Find nearest target and set selected target to show character hp/mp UIs
                    BaseCharacterEntity nearestTarget = PlayerCharacterEntity
                        .FindNearestAliveCharacter<BaseCharacterEntity>(
                        PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking),
                        false,
                        true,
                        false,
                        true,
                        PlayerCharacterEntity.GetAttackFov(isLeftHandAttacking));
                    SelectedEntity = nearestTarget;
                    // Not lock target, so not finding target and attack immediately
                    if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, GetDefaultAttackAimPosition()))
                        isLeftHandAttacking = !isLeftHandAttacking;
                }
            }
            // Move
            if (moveDirection.magnitude != 0f)
            {
                PlayerCharacterEntity.StopMove();
                destination = null;
                ClearTarget();
                targetLookDirection = moveDirection.normalized;
            }
            // Always forward
            MovementState movementState = MovementState.Forward;
            if (InputManager.GetButtonDown("Jump"))
                movementState |= MovementState.IsJump;
            PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
        }

        protected bool UpdateWASDPendingSkill()
        {
            if (queueUsingSkill.skill == null)
                return false;

            destination = null;
            PlayerCharacterEntity.StopMove();
            if (queueUsingSkill.level > 0)
            {
                if (queueUsingSkill.skill.IsAttack())
                {
                    BaseCharacterEntity targetEntity;
                    if (TryGetSelectedTargetAsAttackingCharacter(out targetEntity))
                        SetTarget(targetEntity);

                    if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                    {
                        BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(queueUsingSkill.skill.GetAttackDistance(PlayerCharacterEntity, queueUsingSkill.level, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                        if (nearestTarget != null)
                        {
                            // Set target, then use skill later when moved nearby target
                            SetTarget(nearestTarget);
                        }
                        else
                        {
                            // No nearby target, so use skill immediately
                            if (RequestUsePendingSkill(isLeftHandAttacking))
                                isLeftHandAttacking = !isLeftHandAttacking;
                        }
                    }
                    else if (!wasdLockAttackTarget)
                    {
                        // Not lock target, so not finding target and use skill immediately
                        if (RequestUsePendingSkill(isLeftHandAttacking))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    // Not attack skill, so use skill immediately
                    RequestUsePendingSkill(isLeftHandAttacking);
                }
            }
            else
            {
                ClearQueueUsingSkill();
            }

            return true;
        }

        protected bool UpdateWASDPendingSkillItem()
        {
            if (queueUsingSkillItem.skill == null)
                return false;

            destination = null;
            PlayerCharacterEntity.StopMove();
            if (queueUsingSkillItem.level > 0)
            {
                if (queueUsingSkillItem.skill.IsAttack())
                {
                    BaseCharacterEntity targetEntity;
                    if (TryGetSelectedTargetAsAttackingCharacter(out targetEntity))
                        SetTarget(targetEntity);

                    if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                    {
                        BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(queueUsingSkillItem.skill.GetAttackDistance(PlayerCharacterEntity, queueUsingSkillItem.level, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                        if (nearestTarget != null)
                        {
                            // Set target, then use skill later when moved nearby target
                            SetTarget(nearestTarget);
                        }
                        else
                        {
                            // No nearby target, so use skill immediately
                            if (RequestUsePendingSkillItem(isLeftHandAttacking))
                                isLeftHandAttacking = !isLeftHandAttacking;
                        }
                    }
                    else if (!wasdLockAttackTarget)
                    {
                        // Not lock target, so not finding target and use skill immediately
                        if (RequestUsePendingSkillItem(isLeftHandAttacking))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    // Not attack skill, so use skill immediately
                    RequestUsePendingSkillItem(isLeftHandAttacking);
                }
            }
            else
            {
                ClearQueueUsingSkillItem();
            }

            return true;
        }

        public void UpdateBuilding()
        {
            // Current building UI
            UICurrentBuilding uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
            if (uiCurrentBuilding != null)
            {
                if (uiCurrentBuilding.IsVisible() && ActiveBuildingEntity == null)
                    uiCurrentBuilding.Hide();
            }

            // Construct building UI
            UIConstructBuilding uiConstructBuilding = CacheUISceneGameplay.uiConstructBuilding;
            if (uiConstructBuilding != null)
            {
                if (uiConstructBuilding.IsVisible() && CurrentBuildingEntity == null)
                    uiConstructBuilding.Hide();
                if (!uiConstructBuilding.IsVisible() && CurrentBuildingEntity != null)
                    uiConstructBuilding.Show();
            }

            if (CurrentBuildingEntity == null)
                return;

            bool isPointerOverUI = CacheUISceneGameplay != null && CacheUISceneGameplay.IsPointerOverUIObject();
            if (Input.GetMouseButton(0) && !isPointerOverUI)
            {
                mouseDownTime = Time.unscaledTime;
                mouseDownPosition = Input.mousePosition;
                FindAndSetBuildingAreaFromMousePosition();
            }
        }

        public void UpdateFollowTarget()
        {
            // Temp variables
            if (TryGetAttackingCharacter(out targetEnemy))
            {
                if (!targetCharacter.GetCaches().IsHide || targetEnemy.IsDead())
                {
                    ClearQueueUsingSkill();
                    ClearQueueUsingSkillItem();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }

                // Find attack distance and fov, from weapon or skill
                float attackDistance = 0f;
                float attackFov = 0f;
                if (!GetAttackDataOrUseNonAttackSkill(isLeftHandAttacking, out attackDistance, out attackFov))
                    return;

                float actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetEnemy.gameObject, actDistance, gameInstance.characterLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    // Turn character to target
                    targetLookDirection = (targetEnemy.CacheTransform.position - MovementTransform.position).normalized;
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetEnemy.CacheTransform.position))
                    {
                        // If has queue using skill, attack by the skill
                        if (queueUsingSkill.skill != null || queueUsingSkillItem.skill != null)
                        {
                            if (queueUsingSkill.skill != null &&
                                RequestUsePendingSkill(isLeftHandAttacking))
                            {
                                // Change attacking hand after attack requested
                                isLeftHandAttacking = !isLeftHandAttacking;
                            }
                            else if (queueUsingSkillItem.skill != null &&
                                RequestUsePendingSkillItem(isLeftHandAttacking))
                            {
                                // Change attacking hand after attack requested
                                isLeftHandAttacking = !isLeftHandAttacking;
                            }
                            else
                            {
                                // Can't use skill
                                return;
                            }
                        }
                        else if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, targetEnemy.OpponentAimTransform.position))
                        {
                            // Change attacking hand after attack requested
                            isLeftHandAttacking = !isLeftHandAttacking;
                        }
                    }
                }
                else
                    UpdateTargetEntityPosition(targetEnemy);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetPlayer))
            {
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(MovementTransform.position, targetPlayer.CacheTransform.position) <= actDistance)
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
                    ClearQueueUsingSkill();
                    ClearQueueUsingSkillItem();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(MovementTransform.position, targetMonster.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.StopMove();
                    // TODO: do something
                }
                else
                    UpdateTargetEntityPosition(targetMonster);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetNpc))
            {
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(MovementTransform.position, targetNpc.CacheTransform.position) <= actDistance)
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
                float actDistance = gameInstance.pickUpItemDistance - StoppingDistance;
                if (Vector3.Distance(MovementTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                }
                else
                    UpdateTargetEntityPosition(targetItemDrop);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetBuilding))
            {
                UICurrentBuilding uiCurrentBuilding = null;
                if (CacheUISceneGameplay != null)
                    uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(MovementTransform.position, targetBuilding.CacheTransform.position) <= actDistance)
                {
                    PlayerCharacterEntity.StopMove();
                    if (IsEditingBuilding)
                    {
                        // If it's build mode, show destroy menu
                        if (uiCurrentBuilding != null && !uiCurrentBuilding.IsVisible())
                            uiCurrentBuilding.Show();
                    }
                    else
                    {
                        // If it's not build mode, try to activate it
                        ActivateBuilding(targetBuilding);
                        ClearTarget();
                    }
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
                    ClearQueueUsingSkill();
                    ClearQueueUsingSkillItem();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }

                float attackDistance = 0f;
                float attackFov = 0f;
                if (!GetAttackDataOrUseNonAttackSkill(isLeftHandAttacking, out attackDistance, out attackFov))
                    return;
                float actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetHarvestable.gameObject, actDistance, gameInstance.harvestableLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    // Turn character to target
                    targetLookDirection = (targetHarvestable.CacheTransform.position - MovementTransform.position).normalized;
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetHarvestable.CacheTransform.position))
                    {
                        if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, targetHarvestable.OpponentAimTransform.position))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                    UpdateTargetEntityPosition(targetHarvestable);
            }
        }

        protected void UpdateTargetEntityPosition(BaseGameEntity entity)
        {
            if (entity == null)
                return;
            Vector3 targetPosition = entity.CacheTransform.position;
            PlayerCharacterEntity.PointClickMovement(targetPosition);
            targetLookDirection = (targetPosition - MovementTransform.position).normalized;
        }

        public void UpdateLookAtTarget()
        {
            if (destination != null)
                targetLookDirection = (destination.Value - MovementTransform.position).normalized;
            if (Vector3.Angle(tempLookAt * Vector3.forward, targetLookDirection) > 0)
            {
                // Update rotation when angle difference more than 1
                tempLookAt = Quaternion.RotateTowards(tempLookAt, Quaternion.LookRotation(targetLookDirection), Time.deltaTime * angularSpeed);
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
            ClearQueueUsingSkillItem();

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

            // Avoid empty data
            if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out skill) || skill == null ||
                !PlayerCharacterEntity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;

            // Set aim position to use immediately (don't add to queue)
            Vector3 useSkillAimPosition = aimPosition.HasValue ? aimPosition.Value : GetDefaultAttackAimPosition();

            BaseCharacterEntity attackingCharacter;
            if (TryGetAttackingCharacter(out attackingCharacter))
            {
                // If attacking any character, will use skill later
                SetQueueUsingSkill(aimPosition, skill, skillLevel);
            }
            else
            {
                // If not attacking any character, use skill immediately
                if (skill.IsAttack())
                {
                    // Default damage type attacks
                    if (IsLockTarget() && !skill.HasCustomAimControls())
                    {
                        // If attacking any character, will use skill later
                        SetQueueUsingSkill(aimPosition, skill, skillLevel);
                        if (SelectedEntity == null && !(SelectedEntity is BaseCharacterEntity))
                        {
                            // Attacking nearest target
                            BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(skill.GetAttackDistance(PlayerCharacterEntity, skillLevel, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                            if (nearestTarget != null)
                                SetTarget(nearestTarget);
                        }
                    }
                    else
                    {
                        // Not lock target, use it immediately
                        destination = null;
                        PlayerCharacterEntity.StopMove();
                        if (PlayerCharacterEntity.RequestUseSkill(skill.DataId, isLeftHandAttacking, useSkillAimPosition))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    // This is not attack skill, use it immediately
                    destination = null;
                    PlayerCharacterEntity.StopMove();
                    PlayerCharacterEntity.RequestUseSkill(skill.DataId, isLeftHandAttacking, useSkillAimPosition);
                }
            }
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
                    UseSkillItem(item, (short)itemIndex, aimPosition);
                }
                else
                {
                    PlayerCharacterEntity.RequestUseItem((short)itemIndex);
                }
            }
            else if (item.IsBuilding())
            {
                destination = null;
                PlayerCharacterEntity.StopMove();
                buildingItemIndex = itemIndex;
                CurrentBuildingEntity = Instantiate(item.buildingEntity);
                CurrentBuildingEntity.SetupAsBuildMode();
                CurrentBuildingEntity.CacheTransform.parent = null;
                FindAndSetBuildingAreaFromCharacterDirection();
            }
        }

        protected void UseSkillItem(Item item, short itemIndex, Vector3? aimPosition)
        {
            BaseSkill skill = item.skillLevel.skill;
            short skillLevel = item.skillLevel.level;

            // Avoid empty data
            if (skill == null)
                return;

            // Set aim position to use immediately (don't add to queue)
            Vector3 useSkillAimPosition = aimPosition.HasValue ? aimPosition.Value : GetDefaultAttackAimPosition();

            BaseCharacterEntity attackingCharacter;
            if (TryGetAttackingCharacter(out attackingCharacter))
            {
                // If attacking any character, will use skill later
                SetQueueUsingSkillItem(aimPosition, itemIndex, skill, skillLevel);
            }
            else
            {
                // If not attacking any character, use skill immediately
                if (skill.IsAttack())
                {
                    if (IsLockTarget() && !skill.HasCustomAimControls())
                    {
                        // If attacking any character, will use skill later
                        SetQueueUsingSkillItem(aimPosition, itemIndex, skill, skillLevel);
                        if (SelectedEntity == null || !(SelectedEntity is BaseCharacterEntity))
                        {
                            // Attacking nearest target
                            BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(skill.GetAttackDistance(PlayerCharacterEntity, skillLevel, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                            if (nearestTarget != null)
                                SetTarget(nearestTarget);
                        }
                    }
                    else
                    {
                        // Not lock target, use it immediately
                        destination = null;
                        PlayerCharacterEntity.StopMove();
                        if (PlayerCharacterEntity.RequestUseSkillItem(itemIndex, isLeftHandAttacking, useSkillAimPosition))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    // This is not attack skill, use it immediately
                    destination = null;
                    PlayerCharacterEntity.StopMove();
                    PlayerCharacterEntity.RequestUseSkillItem(itemIndex, isLeftHandAttacking, useSkillAimPosition);
                }
            }
        }
    }
}
