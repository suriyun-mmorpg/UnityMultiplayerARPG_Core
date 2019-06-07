using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        protected virtual void UpdateInput()
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
                    if (activatingEntityDetector.players.Count > 0)
                        targetPlayer = activatingEntityDetector.players[0];
                    targetNpc = null;
                    if (activatingEntityDetector.npcs.Count > 0)
                        targetNpc = activatingEntityDetector.npcs[0];
                    targetBuilding = null;
                    if (activatingEntityDetector.buildings.Count > 0)
                        targetBuilding = activatingEntityDetector.buildings[0];
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
                        if (!enemyEntityDetector.characters[findingEnemyIndex].IsDead())
                        {
                            SetTarget(enemyEntityDetector.characters[findingEnemyIndex]);
                            if (SelectedEntity != null)
                                targetLookDirection = (SelectedEntity.CacheTransform.position - PlayerCharacterEntity.CacheTransform.position).normalized;
                        }
                    }
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

        protected virtual void UpdatePointClickInput()
        {
            // If it's building something, not allow point click movement
            if (CurrentBuildingEntity != null)
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
                targetEntity = null;
                targetPosition = null;
                Vector3? tempMapPosition = null;
                float tempHighestY = float.MinValue;
                BuildingMaterial tempBuildingMaterial;
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
                            tempMapPosition = null;
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
                        if (targetPlayer != null && !targetPlayer.IsDead())
                        {
                            SetTarget(targetPlayer);
                            tempMapPosition = null;
                            break;
                        }
                        else if (targetMonster != null && !targetMonster.IsDead())
                        {
                            SetTarget(targetMonster);
                            tempMapPosition = null;
                            break;
                        }
                        else if (targetNpc != null)
                        {
                            SetTarget(targetNpc);
                            tempMapPosition = null;
                            break;
                        }
                        else if (targetItemDrop != null)
                        {
                            SetTarget(targetItemDrop);
                            tempMapPosition = null;
                            break;
                        }
                        else if (targetHarvestable != null && !targetHarvestable.IsDead())
                        {
                            SetTarget(targetHarvestable);
                            tempMapPosition = null;
                            break;
                        }
                        else if (targetBuilding && !targetBuilding.IsDead() && targetBuilding.Activatable)
                        {
                            IsEditingBuilding = false;
                            SetTarget(targetBuilding);
                            tempMapPosition = null;
                            break;
                        }
                        else if (!GetRaycastIsTrigger(tempCounter))
                        {
                            // Set clicked map position, it will be used if no activating entity found
                            tempMapPosition = GetRaycastPoint(tempCounter);
                            if (tempMapPosition.Value.y > tempHighestY)
                                tempHighestY = tempMapPosition.Value.y;
                        }
                    }
                }
                // When clicked on map (Not touch any game entity)
                // - Clear selected target to hide selected entity UIs
                // - Set target position to position where mouse clicked
                if (tempMapPosition.HasValue)
                {
                    SelectedEntity = null;
                    targetPosition = tempMapPosition.Value;
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
                    queueUsingSkill = null;

                    // Move to target, will hide destination when target is object
                    if (targetEntity != null)
                        destination = null;
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

        protected virtual void UpdateWASDInput()
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
                queueUsingSkill = null;
                FindAndSetBuildingAreaFromCharacterDirection();
            }

            // For WASD mode, Using skill when player pressed hotkey
            if (queueUsingSkill.HasValue)
            {
                UsingSkillData queueUsingSkillValue = queueUsingSkill.Value;
                destination = null;
                PlayerCharacterEntity.StopMove();
                Skill skill = null;
                if (GameInstance.Skills.TryGetValue(queueUsingSkillValue.dataId, out skill) && skill != null)
                {
                    if (skill.IsAttack())
                    {
                        BaseCharacterEntity targetEntity;
                        if (TryGetSelectedTargetAsAttackingCharacter(out targetEntity))
                            SetTarget(targetEntity);
                        if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                        {
                            BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                            if (nearestTarget != null)
                            {
                                // Set target, then use skill later when moved nearby target
                                PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                            }
                            else
                            {
                                // No nearby target, so use skill immediately
                                if (RequestUsePendingSkill(isLeftHandAttacking, null))
                                    isLeftHandAttacking = !isLeftHandAttacking;
                            }
                        }
                        else if (!wasdLockAttackTarget)
                        {
                            // Not lock target, so not finding target and use skill immediately
                            if (RequestUsePendingSkill(isLeftHandAttacking, null))
                                isLeftHandAttacking = !isLeftHandAttacking;
                        }
                    }
                    else
                    {
                        // Not attack skill, so use skill immediately
                        RequestUsePendingSkill(isLeftHandAttacking, null);
                    }
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
                        PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                    }
                    else
                    {
                        // No nearby target, so attack immediately
                        if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking))
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
                    if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking))
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

        protected void UpdateBuilding()
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

        protected void UpdateFollowTarget()
        {
            // Temp variables
            if (TryGetAttackingCharacter(out targetEnemy))
            {
                if (targetEnemy.IsDead())
                {
                    queueUsingSkill = null;
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
                    targetLookDirection = (targetEnemy.CacheTransform.position - PlayerCharacterEntity.CacheTransform.position).normalized;
                    if (PlayerCharacterEntity.IsPositionInFov(attackFov, targetEnemy.CacheTransform.position))
                    {
                        // If has queue using skill, attack by the skill
                        if (queueUsingSkill.HasValue &&
                            RequestUsePendingSkill(isLeftHandAttacking, targetEnemy.OpponentAimTransform.position))
                            isLeftHandAttacking = !isLeftHandAttacking;
                        else if (PlayerCharacterEntity.RequestAttack(isLeftHandAttacking, targetEnemy.OpponentAimTransform.position))
                            isLeftHandAttacking = !isLeftHandAttacking;
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
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
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
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
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
                float actDistance = gameInstance.conversationDistance - StoppingDistance;
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
                float actDistance = gameInstance.pickUpItemDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
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
                if (Vector3.Distance(CharacterTransform.position, targetBuilding.CacheTransform.position) <= actDistance)
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
                    queueUsingSkill = null;
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
                    targetLookDirection = (targetHarvestable.CacheTransform.position - PlayerCharacterEntity.CacheTransform.position).normalized;
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
            targetLookDirection = (targetPosition - PlayerCharacterEntity.CacheTransform.position).normalized;
        }

        protected void UpdateLookAtTarget()
        {
            if (destination != null)
                targetLookDirection = (destination.Value - PlayerCharacterEntity.CacheTransform.position).normalized;
            if (Vector3.Angle(tempLookAt * Vector3.forward, targetLookDirection) > 0)
            {
                // Update rotation when angle difference more than 1
                tempLookAt = Quaternion.RotateTowards(tempLookAt, Quaternion.LookRotation(targetLookDirection), Time.deltaTime * angularSpeed);
                PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
            }
        }

        public override void UseHotkey(int hotkeyIndex)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= PlayerCharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            Skill skill = hotkey.GetSkill();
            if (skill != null)
            {
                short skillLevel;
                if (PlayerCharacterEntity.CacheSkills.TryGetValue(skill, out skillLevel))
                {
                    BaseCharacterEntity attackingCharacter;
                    if (TryGetAttackingCharacter(out attackingCharacter))
                    {
                        // If attacking any character, will use skill later
                        queueUsingSkill = new UsingSkillData(null, skill.DataId);
                    }
                    else if (skill.CanUse(PlayerCharacterEntity, skillLevel))
                    {
                        // If not attacking any character, use skill immediately
                        if (skill.IsAttack())
                        {
                            if (IsLockTarget())
                            {
                                // If attacking any character, will use skill later
                                queueUsingSkill = new UsingSkillData(null, skill.DataId);
                                if (SelectedEntity != null && SelectedEntity is BaseCharacterEntity)
                                {
                                    // Attacking selected target
                                    PlayerCharacterEntity.SetTargetEntity(SelectedEntity);
                                }
                                else
                                {
                                    // Attacking nearest target
                                    BaseCharacterEntity nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill, isLeftHandAttacking) + lockAttackTargetDistance, false, true, false);
                                    if (nearestTarget != null)
                                        PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                                }
                            }
                            else
                            {
                                // Not lock target, use it immediately
                                destination = null;
                                PlayerCharacterEntity.StopMove();
                                PlayerCharacterEntity.RequestUseSkill(skill.DataId, isLeftHandAttacking);
                                isLeftHandAttacking = !isLeftHandAttacking;
                            }
                        }
                        else
                        {
                            // This is not attack skill, use it immediately
                            destination = null;
                            PlayerCharacterEntity.StopMove();
                            PlayerCharacterEntity.RequestUseSkill(skill.DataId, isLeftHandAttacking);
                        }
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
                        RequestEquipItem((short)itemIndex);
                    else if (item.IsPotion() || item.IsPet())
                        RequestUseItem((short)itemIndex);
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
    }
}
