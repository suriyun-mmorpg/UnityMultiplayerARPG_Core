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
                    if (ActivatableEntityDetector.players.Count > 0)
                        targetPlayer = ActivatableEntityDetector.players[0];
                    targetNpc = null;
                    if (ActivatableEntityDetector.npcs.Count > 0)
                        targetNpc = ActivatableEntityDetector.npcs[0];
                    targetBuilding = null;
                    if (ActivatableEntityDetector.buildings.Count > 0)
                        targetBuilding = ActivatableEntityDetector.buildings[0];
                    // Priority Player -> Npc -> Buildings
                    if (targetPlayer != null && CacheUISceneGameplay != null)
                    {
                        // Show dealing, invitation menu
                        SelectedEntity = targetPlayer;
                        CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
                    }
                    else if (targetNpc != null)
                    {
                        // Talk to NPC
                        SelectedEntity = targetNpc;
                        PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                    }
                    else if (targetBuilding != null)
                    {
                        // Use building
                        SelectedEntity = targetBuilding;
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
                    if (ItemDropEntityDetector.itemDrops.Count > 0)
                        targetItemDrop = ItemDropEntityDetector.itemDrops[0];
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
                    if (findingEnemyIndex < 0 || findingEnemyIndex >= EnemyEntityDetector.characters.Count)
                        findingEnemyIndex = 0;
                    if (EnemyEntityDetector.characters.Count > 0)
                    {
                        SetTarget(null, TargetActionType.Attack);
                        if (!EnemyEntityDetector.characters[findingEnemyIndex].GetCaches().IsHide &&
                            !EnemyEntityDetector.characters[findingEnemyIndex].IsDead())
                        {
                            SetTarget(EnemyEntityDetector.characters[findingEnemyIndex], TargetActionType.Attack);
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
                if (InputManager.GetButtonDown("Sprint"))
                {
                    isSprinting = !isSprinting;
                    PlayerCharacterEntity.SetExtraMovement(isSprinting ? MovementState.IsSprinting : MovementState.None);
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
            EnemyEntityDetector.detectingRadius = PlayerCharacterEntity.GetAttackDistance(false) + lockAttackTargetDistance;
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
                ClearTarget(true);
                // Prepare temp variables
                Transform tempTransform;
                Vector3 tempVector3;
                bool tempHasMapPosition = false;
                Vector3 tempMapPosition = Vector3.zero;
                float tempHighestY = float.MinValue;
                BuildingMaterial tempBuildingMaterial;
                // If mouse up while cursor point to target (character, item, npc and so on)
                bool mouseUpOnTarget = getMouseUp && !isMouseDragOrHoldOrOverUI;
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
                            SetTarget(targetBuilding, TargetActionType.Undefined);
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
                        lastNpcObjectId = 0;
                        if (targetPlayer != null && !targetPlayer.GetCaches().IsHide)
                        {
                            // Found activating entity as player character entity
                            SetTarget(targetPlayer, TargetActionType.Attack);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetMonster != null && !targetMonster.GetCaches().IsHide)
                        {
                            // Found activating entity as monster character entity
                            SetTarget(targetMonster, TargetActionType.Attack);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetNpc != null)
                        {
                            // Found activating entity as npc entity
                            SetTarget(targetNpc, TargetActionType.Undefined);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetItemDrop != null)
                        {
                            // Found activating entity as item drop entity
                            SetTarget(targetItemDrop, TargetActionType.Undefined);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetHarvestable != null && !targetHarvestable.IsDead())
                        {
                            // Found activating entity as harvestable entity
                            SetTarget(targetHarvestable, TargetActionType.Undefined);
                            tempHasMapPosition = false;
                            break;
                        }
                        else if (targetBuilding && !targetBuilding.IsDead() && targetBuilding.Activatable)
                        {
                            // Found activating entity as building entity
                            IsEditingBuilding = false;
                            SetTarget(targetBuilding, TargetActionType.Undefined);
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
                    ClearTarget();
                    tempVector3.z = 0;
                    targetPosition = tempVector3;
                }
                // If Found target, do something
                if (targetPosition.HasValue)
                {
                    if (controllerMode == PlayerCharacterControllerMode.WASD)
                    {
                        destination = null;
                    }
                    else
                    {
                        // Close NPC dialog, when target changes
                        HideNpcDialogs();
                        ClearQueueUsingSkill();

                        // Move to target, will hide destination when target is object
                        if (TargetEntity != null)
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
        }

        protected virtual void SetTarget(BaseGameEntity entity, TargetActionType targetActionType, bool checkControllerMode = true)
        {
            targetPosition = null;
            if (checkControllerMode && controllerMode == PlayerCharacterControllerMode.WASD)
            {
                this.targetActionType = targetActionType;
                destination = null;
                SelectedEntity = entity;
                return;
            }
            if (pointClickSetTargetImmediately ||
                (entity != null && SelectedEntity == entity) ||
                (entity != null && entity is ItemDropEntity))
            {
                this.targetActionType = targetActionType;
                destination = null;
                TargetEntity = entity;
                PlayerCharacterEntity.SetTargetEntity(entity);
            }
            SelectedEntity = entity;
        }

        protected virtual void ClearTarget(bool exceptSelectedTarget = false)
        {
            if (!exceptSelectedTarget)
                SelectedEntity = null;
            TargetEntity = null;
            PlayerCharacterEntity.SetTargetEntity(null);
            targetPosition = null;
            targetActionType = TargetActionType.Undefined;
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
                FindAndSetBuildingAreaFromCharacterDirection();
            }

            // Attack when player pressed attack button
            if (queueUsingSkill.skill != null)
                UpdateWASDPendingSkill(queueUsingSkill.skill, queueUsingSkill.level);
            else if (InputManager.GetButton("Attack"))
                UpdateWASDAttack();

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

        protected void UpdateWASDAttack()
        {
            destination = null;
            PlayerCharacterEntity.StopMove();
            BaseCharacterEntity targetEntity;

            if (TryGetSelectedTargetAsAttackingEntity(out targetEntity))
                SetTarget(targetEntity, TargetActionType.Attack, false);

            if (IsLockTarget() && !TryGetAttackingCharacter(out targetEntity))
            {
                // Find nearest target and move to the target
                SelectedEntity = PlayerCharacterEntity
                    .FindNearestAliveCharacter<BaseCharacterEntity>(
                    PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking) + lockAttackTargetDistance,
                    false,
                    true,
                    false);
                if (SelectedEntity != null)
                {
                    // Set target, then attack later when moved nearby target
                    SetTarget(SelectedEntity, TargetActionType.Attack, false);
                }
                else
                {
                    // No nearby target, so attack immediately
                    if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, GetDefaultAttackAimPosition()))
                        isLeftHandAttacking = !isLeftHandAttacking;
                }
            }
            else if (!IsLockTarget())
            {
                // Find nearest target and set selected target to show character hp/mp UIs
                SelectedEntity = PlayerCharacterEntity
                    .FindNearestAliveCharacter<BaseCharacterEntity>(
                    PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking),
                    false,
                    true,
                    false,
                    true,
                    PlayerCharacterEntity.GetAttackFov(isLeftHandAttacking));
                // Not lock target, so not finding target and attack immediately
                if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, GetDefaultAttackAimPosition()))
                    isLeftHandAttacking = !isLeftHandAttacking;
            }
        }

        protected void UpdateWASDPendingSkill(BaseSkill skill, short skillLevel)
        {
            destination = null;
            PlayerCharacterEntity.StopMove();
            BaseCharacterEntity targetEntity;

            if (skill.IsAttack())
            {
                if (TryGetSelectedTargetAsAttackingEntity(out targetEntity))
                    SetTarget(targetEntity, TargetActionType.Attack, false);

                if (IsLockTarget() && !TryGetAttackingCharacter(out targetEntity))
                {
                    BaseCharacterEntity nearestTarget = PlayerCharacterEntity
                        .FindNearestAliveCharacter<BaseCharacterEntity>(
                        skill.GetCastDistance(PlayerCharacterEntity, skillLevel, isLeftHandAttacking) + lockAttackTargetDistance,
                        false,
                        true,
                        false);
                    if (nearestTarget != null)
                    {
                        // Set target, then use skill later when moved nearby target
                        SetTarget(nearestTarget, TargetActionType.Attack, false);
                    }
                    else
                    {
                        // No nearby target, so use skill immediately
                        if (RequestUsePendingSkill(isLeftHandAttacking))
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else if (!IsLockTarget())
                {
                    // Not lock target, so not finding target and use skill immediately
                    if (RequestUsePendingSkill(isLeftHandAttacking))
                        isLeftHandAttacking = !isLeftHandAttacking;
                }
            }
            else
            {
                // Not attack skill, so use skill immediately
                if (skill.RequiredTarget())
                {
                    if (IsLockTarget())
                    {
                        // Let's update follow target do it
                        return;
                    }
                    // TODO: Check is target nearby or not
                    ClearQueueUsingSkill();
                }
                else
                {
                    // Target not required, use skill immediately
                    RequestUsePendingSkill(isLeftHandAttacking);
                }
            }
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
            if (!IsLockTarget())
                return;

            if (TryGetAttackingCharacter(out targetCharacter))
            {
                if (targetCharacter.GetCaches().IsHide || targetCharacter.IsDead())
                {
                    ClearQueueUsingSkill();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }

                if (queueUsingSkill.skill != null && !queueUsingSkill.skill.IsAttack())
                {
                    // Try use non-attack skill
                    PlayerCharacterEntity.StopMove();
                    RequestUsePendingSkill(false);
                    return;
                }

                // Find attack distance and fov, from weapon or skill
                float attackDistance = 0f;
                float attackFov = 0f;
                GetAttackDistanceAndFov(isLeftHandAttacking, out attackDistance, out attackFov);

                float actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetCharacter.gameObject, actDistance, gameInstance.characterLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    // Set direction to turn character to target
                    targetLookDirection = (targetCharacter.CacheTransform.position - MovementTransform.position).normalized;
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetCharacter.CacheTransform.position))
                    {
                        // If has queue using skill, attack by the skill
                        if (queueUsingSkill.skill != null)
                        {
                            if (RequestUsePendingSkill(isLeftHandAttacking))
                            {
                                // Change attacking hand after attack requested
                                isLeftHandAttacking = !isLeftHandAttacking;
                            }
                            return;
                        }
                        else if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, targetCharacter.OpponentAimTransform.position))
                        {
                            // Change attacking hand after attack requested
                            isLeftHandAttacking = !isLeftHandAttacking;
                            return;
                        }
                    }
                }
                else
                    UpdateTargetEntityPosition(targetCharacter);
            }
            else if (TryGetUsingSkillCharacter(out targetCharacter))
            {
                // Find attack distance and fov, from weapon or skill
                float castDistance = 0f;
                float castFov = 0f;
                GetUseSkillDistanceAndFov(out castDistance, out castFov);

                float actDistance = castDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (targetCharacter == PlayerCharacterEntity || Vector3.Distance(MovementTransform.position, targetCharacter.CacheTransform.position) <= actDistance)
                {
                    // Stop movement to use skill
                    PlayerCharacterEntity.StopMove();
                    // Set direction to turn character to target
                    targetLookDirection = (targetCharacter.CacheTransform.position - MovementTransform.position).normalized;
                    if (targetCharacter == PlayerCharacterEntity || PlayerCharacterEntity.IsPositionInFov(castFov, targetCharacter.CacheTransform.position))
                    {
                        if (queueUsingSkill.skill != null)
                        {
                            if (RequestUsePendingSkill(false))
                                targetActionType = TargetActionType.Undefined;
                            return;
                        }
                        else
                        {
                            // Can't use skill
                            targetActionType = TargetActionType.Undefined;
                            ClearQueueUsingSkill();
                            return;
                        }
                    }
                }
                else
                    UpdateTargetEntityPosition(targetCharacter);
            }
            else if (PlayerCharacterEntity.TryGetTargetEntity(out targetCharacter))
            {
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (targetCharacter == PlayerCharacterEntity || Vector3.Distance(MovementTransform.position, targetCharacter.CacheTransform.position) <= actDistance)
                {
                    // Stop movement to do something
                    PlayerCharacterEntity.StopMove();
                }
                else
                    UpdateTargetEntityPosition(targetCharacter);
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
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }

                // Find attack distance and fov, from weapon
                float attackDistance = 0f;
                float attackFov = 0f;
                GetAttackDistanceAndFov(isLeftHandAttacking, out attackDistance, out attackFov);

                float actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (FindTarget(targetHarvestable.gameObject, actDistance, gameInstance.harvestableLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    // Set direction to turn character to target
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
            if (PlayerCharacterEntity.IsPlayingActionAnimation())
                return;
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
            UseSkill(
                skill,
                skillLevel,
                () => SetQueueUsingSkill(aimPosition, skill, skillLevel),
                () => PlayerCharacterEntity.RequestUseSkill(skill.DataId, isLeftHandAttacking, useSkillAimPosition));
        }

        protected void UseSkill(
            BaseSkill skill,
            short skillLevel,
            VoidAction setQueueFunction,
            BoolAction useFunction)
        {

            BaseCharacterEntity attackingCharacter;
            if (TryGetAttackingCharacter(out attackingCharacter))
            {
                // If attacking any character, will use skill later
                setQueueFunction();
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
                        setQueueFunction();
                        if (SelectedEntity == null && !(SelectedEntity is BaseCharacterEntity))
                        {
                            // Attacking nearest target
                            BaseCharacterEntity nearestTarget = PlayerCharacterEntity
                                .FindNearestAliveCharacter<BaseCharacterEntity>(
                                skill.GetCastDistance(PlayerCharacterEntity, skillLevel, isLeftHandAttacking) + lockAttackTargetDistance,
                                false,
                                true,
                                false);
                            if (nearestTarget != null)
                                SetTarget(nearestTarget, TargetActionType.Attack);
                        }
                    }
                    else
                    {
                        // Not lock target, use it immediately
                        destination = null;
                        PlayerCharacterEntity.StopMove();
                        if (useFunction())
                            isLeftHandAttacking = !isLeftHandAttacking;
                    }
                }
                else
                {
                    // This is not attack skill, use it immediately
                    destination = null;
                    PlayerCharacterEntity.StopMove();
                    if (skill.RequiredTarget())
                    {
                        setQueueFunction();
                        if (IsLockTarget())
                            SetTarget(SelectedEntity, TargetActionType.UseSkill, false);
                    }
                    else
                    {
                        useFunction();
                    }
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
                    // Set aim position to use immediately (don't add to queue)
                    Vector3 useSkillAimPosition = aimPosition.HasValue ? aimPosition.Value : GetDefaultAttackAimPosition();
                    BaseSkill skill = item.skillLevel.skill;
                    short skillLevel = item.skillLevel.level;
                    UseSkill(
                        skill,
                        skillLevel,
                        () => SetQueueUsingSkill(aimPosition, skill, skillLevel, (short)itemIndex),
                        () => PlayerCharacterEntity.RequestUseSkillItem((short)itemIndex, isLeftHandAttacking, useSkillAimPosition));
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
    }
}
