using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        public const float MIN_START_MOVE_DISTANCE = 0.01f;

        public int FindClickObjects(out Vector3 worldPosition2D)
        {
            return physicFunctions.RaycastPickObjects(CacheGameplayCameraController.Camera, InputManager.MousePosition(), CurrentGameInstance.GetTargetLayerMask(), 100f, out worldPosition2D);
        }

        public virtual void UpdateInput()
        {
            bool isFocusInputField = GenericUtils.IsFocusInputField();
            bool isPointerOverUIObject = UISceneGameplay.IsPointerOverUIObject();
            bool isBlockController = UISceneGameplay.IsBlockController();
            CacheGameplayCameraController.UpdateRotationX = false;
            CacheGameplayCameraController.UpdateRotationY = false;
            CacheGameplayCameraController.UpdateRotation = !isFocusInputField && !isBlockController && !isPointerOverUIObject && InputManager.GetButton("CameraRotate");
            CacheGameplayCameraController.UpdateZoom = !isFocusInputField && !isBlockController && !isPointerOverUIObject;

            if (isFocusInputField || isBlockController || PlayingCharacterEntity.IsDead())
            {
                PlayingCharacterEntity.KeyMovement(Vector3.zero, MovementState.None);
                return;
            }

            // If it's building something, don't allow to activate NPC/Warp/Pickup Item
            if (ConstructingBuildingEntity == null)
            {
                // Activate nearby npcs / players / activable buildings
                if (activateInput.IsHold)
                {
                    if (ActivatableEntityDetector.holdActivatableEntities.Count > 0)
                    {
                        IHoldActivatableEntity holdActivatable;
                        for (int i = 0; i < ActivatableEntityDetector.holdActivatableEntities.Count; ++i)
                        {
                            holdActivatable = ActivatableEntityDetector.holdActivatableEntities[i];
                            if (holdActivatable.CanHoldActivate())
                            {
                                holdActivatable.OnHoldActivate();
                                break;
                            }
                        }
                    }
                }
                else if (activateInput.IsRelease)
                {
                    if (ActivatableEntityDetector.activatableEntities.Count > 0)
                    {
                        IActivatableEntity activatable;
                        for (int i = 0; i < ActivatableEntityDetector.activatableEntities.Count; ++i)
                        {
                            activatable = ActivatableEntityDetector.activatableEntities[i];
                            if (activatable.CanActivate())
                            {
                                activatable.OnActivate();
                                break;
                            }
                        }
                    }
                }
                // Pick up nearby items
                if (pickupItemInput.IsPress)
                {
                    if (ItemDropEntityDetector.pickupActivatableEntities.Count > 0)
                    {
                        IPickupActivatableEntity activatable;
                        for (int i = 0; i < ItemDropEntityDetector.pickupActivatableEntities.Count; ++i)
                        {
                            activatable = ItemDropEntityDetector.pickupActivatableEntities[i];
                            if (activatable.CanPickupActivate())
                            {
                                activatable.OnPickupActivate();
                                break;
                            }
                        }
                    }
                }
                // Reload
                if (reloadInput.IsPress)
                {
                    // Reload ammo when press the button
                    ReloadAmmo();
                }
                // Find target to attack
                if (findEnemyInput.IsPress)
                {
                    ++findingEnemyIndex;
                    if (findingEnemyIndex < 0 || findingEnemyIndex >= EnemyEntityDetector.characters.Count)
                        findingEnemyIndex = 0;
                    if (EnemyEntityDetector.characters.Count > 0)
                    {
                        SetTarget(null, TargetActionType.Attack);
                        if (!EnemyEntityDetector.characters[findingEnemyIndex].IsHideOrDead())
                        {
                            SetTarget(EnemyEntityDetector.characters[findingEnemyIndex], TargetActionType.Attack);
                            if (SelectedGameEntity != null)
                            {
                                // Turn character to enemy but does not move or attack yet.
                                TurnCharacterToEntity(SelectedGameEntity);
                            }
                        }
                    }
                }
                if (exitVehicleInput.IsPress)
                {
                    // Exit vehicle
                    PlayingCharacterEntity.CallServerExitVehicle();
                }
                if (switchEquipWeaponSetInput.IsPress)
                {
                    // Switch equip weapon set
                    GameInstance.ClientInventoryHandlers.RequestSwitchEquipWeaponSet(new RequestSwitchEquipWeaponSetMessage()
                    {
                        equipWeaponSet = (byte)(PlayingCharacterEntity.EquipWeaponSet + 1),
                    }, ClientInventoryActions.ResponseSwitchEquipWeaponSet);
                }
                if (InputManager.GetButtonDown("Sprint"))
                {
                    // Toggles sprint state
                    isSprinting = !isSprinting;
                    isWalking = false;
                }
                else if (InputManager.GetButtonDown("Walk"))
                {
                    // Toggles sprint state
                    isWalking = !isWalking;
                    isSprinting = false;
                }
                // Auto reload
                if (PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                    PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
                {
                    // Reload ammo when empty and not press any keys
                    ReloadAmmo();
                }
            }
            // Update enemy detecting radius to attack distance
            EnemyEntityDetector.detectingRadius = Mathf.Max(PlayingCharacterEntity.GetAttackDistance(false), lockAttackTargetDistance);
            // Update inputs
            UpdateQueuedSkill();
            UpdatePointClickInput();
            UpdateWASDInput();
            // Set extra movement state
            if (isSprinting)
                PlayingCharacterEntity.SetExtraMovementState(ExtraMovementState.IsSprinting);
            else if (isWalking)
                PlayingCharacterEntity.SetExtraMovementState(ExtraMovementState.IsWalking);
            else
                PlayingCharacterEntity.SetExtraMovementState(ExtraMovementState.None);
        }

        protected void ReloadAmmo()
        {
            // Reload ammo at server
            if (!PlayingCharacterEntity.EquipWeapons.rightHand.IsAmmoFull())
                PlayingCharacterEntity.Reload(false);
            else if (!PlayingCharacterEntity.EquipWeapons.leftHand.IsAmmoFull())
                PlayingCharacterEntity.Reload(true);
        }

        public virtual void UpdatePointClickInput()
        {
            if (controllerMode == PlayerCharacterControllerMode.WASD)
                return;

            // If it's building something, not allow point click movement
            if (ConstructingBuildingEntity != null)
                return;

            // If it's aiming skills, not allow point click movement
            if (UICharacterHotkeys.UsingHotkey != null)
                return;

            getMouseDown = InputManager.GetMouseButtonDown(0);
            getMouseUp = InputManager.GetMouseButtonUp(0);
            getMouse = InputManager.GetMouseButton(0);

            if (getMouseDown)
            {
                isMouseDragOrHoldOrOverUI = false;
                mouseDownTime = Time.unscaledTime;
                mouseDownPosition = InputManager.MousePosition();
            }
            // Read inputs
            isPointerOverUI = UISceneGameplay.IsPointerOverUIObject();
            isMouseDragDetected = (InputManager.MousePosition() - mouseDownPosition).sqrMagnitude > DETECT_MOUSE_DRAG_DISTANCE_SQUARED;
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
                didActionOnTarget = false;
                // Prepare temp variables
                Transform tempTransform;
                Vector3 tempVector3;
                bool tempHasMapPosition = false;
                Vector3 tempMapPosition = Vector3.zero;
                // If mouse up while cursor point to target (character, item, npc and so on)
                bool mouseUpOnTarget = getMouseUp && !isMouseDragOrHoldOrOverUI;
                int tempCount = FindClickObjects(out tempVector3);
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempTransform = physicFunctions.GetRaycastTransform(tempCounter);
                    // When holding on target, or already enter edit building mode
                    if (isMouseHoldAndNotDrag)
                    {
                        IHoldActivatableEntity activatable = tempTransform.GetComponent<IHoldActivatableEntity>();
                        if (!activatable.IsNull() && activatable.CanHoldActivate())
                        {
                            SetTarget(activatable, TargetActionType.HoldClickActivate);
                            isFollowingTarget = true;
                            tempHasMapPosition = false;
                            break;
                        }
                    }
                    else if (mouseUpOnTarget)
                    {
                        ITargetableEntity targetable = tempTransform.GetComponent<ITargetableEntity>();
                        IActivatableEntity activatable = targetable as IActivatableEntity;
                        IPickupActivatableEntity pickupActivatable = targetable as IPickupActivatableEntity;
                        IDamageableEntity damageable = targetable as IDamageableEntity;
                        if (!targetable.IsNull() && !targetable.NotBeingSelectedOnClick())
                        {
                            if (!activatable.IsNull() && activatable.CanActivate())
                            {
                                if (activatable.ShouldBeAttackTarget())
                                    SetTarget(activatable, TargetActionType.Attack);
                                else
                                    SetTarget(activatable, TargetActionType.ClickActivate);
                                isFollowingTarget = true;
                                tempHasMapPosition = false;
                                break;
                            }
                            else if (!pickupActivatable.IsNull() && pickupActivatable.CanPickupActivate())
                            {
                                SetTarget(pickupActivatable, TargetActionType.ClickActivate);
                                isFollowingTarget = true;
                                tempHasMapPosition = false;
                                break;
                            }
                            else if (damageable != null && !damageable.IsHideOrDead())
                            {
                                SetTarget(damageable, TargetActionType.Attack);
                                isFollowingTarget = true;
                                tempHasMapPosition = false;
                                break;
                            }
                        }
                        if (!physicFunctions.GetRaycastIsTrigger(tempCounter))
                        {
                            // Set clicked map position, it will be used if no activating entity found
                            tempHasMapPosition = true;
                            tempMapPosition = physicFunctions.GetRaycastPoint(tempCounter);
                            break;
                        }
                    } // End mouseUpOnTarget
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
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D && mouseUpOnTarget && tempCount == 0)
                {
                    ClearTarget();
                    tempVector3.z = 0;
                    targetPosition = tempVector3;
                }

                // Found ground position
                if (targetPosition.HasValue)
                {
                    // Close NPC dialog, when target changes
                    HideNpcDialog();
                    ClearQueueUsingSkill();
                    isFollowingTarget = false;
                    if (PlayingCharacterEntity.IsPlayingActionAnimation())
                    {
                        if (pointClickInterruptCastingSkill)
                            PlayingCharacterEntity.InterruptCastingSkill();
                    }
                    else
                    {
                        OnPointClickOnGround(targetPosition.Value);
                    }
                }
            }
        }

        /// <summary>
        /// When point click on ground, move target to the position
        /// </summary>
        /// <param name="targetPosition"></param>
        protected virtual void OnPointClickOnGround(Vector3 targetPosition)
        {
            if (Vector3.Distance(MovementTransform.position, targetPosition) > MIN_START_MOVE_DISTANCE)
            {
                destination = targetPosition;
                PlayingCharacterEntity.PointClickMovement(targetPosition);
            }
        }

        protected virtual void SetTarget(ITargetableEntity entity, TargetActionType targetActionType, bool checkControllerMode = true)
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
                (entity != null && SelectedEntity != null && entity.EntityGameObject == SelectedEntity.EntityGameObject) ||
                (entity != null && entity.SetAsTargetInOneClick()))
            {
                this.targetActionType = targetActionType;
                destination = null;
                TargetEntity = entity;
                if (entity is IGameEntity gameEntity)
                    PlayingCharacterEntity.SetTargetEntity(gameEntity.Entity);
            }
            SelectedEntity = entity;
        }

        protected virtual void ClearTarget(bool exceptSelectedTarget = false)
        {
            if (!exceptSelectedTarget)
                SelectedEntity = null;
            TargetEntity = null;
            PlayingCharacterEntity.SetTargetEntity(null);
            targetPosition = null;
            targetActionType = TargetActionType.ClickActivate;
        }

        public override void DeselectBuilding()
        {
            base.DeselectBuilding();
            ClearTarget();
        }

        public virtual void UpdateWASDInput()
        {
            if (controllerMode == PlayerCharacterControllerMode.PointClick)
                return;

            // If mobile platforms, don't receive input raw to make it smooth
            bool raw = !GameInstance.Singleton.IsMobileTestInEditor() && !Application.isMobilePlatform && !GameInstance.Singleton.IsConsoleTestInEditor() && !Application.isConsolePlatform;
            Vector3 moveDirection = GetMoveDirection(InputManager.GetAxis("Horizontal", raw), InputManager.GetAxis("Vertical", raw));
            moveDirection.Normalize();

            // Move
            if (moveDirection.sqrMagnitude > 0f)
            {
                HideNpcDialog();
                ClearQueueUsingSkill();
                destination = null;
                isFollowingTarget = false;
                if (TargetGameEntity != null && Vector3.Distance(EntityTransform.position, TargetGameEntity.EntityTransform.position) >= wasdClearTargetDistance)
                {
                    // Clear target when character moved far from target
                    ClearTarget();
                }
                if (!PlayingCharacterEntity.IsPlayingActionAnimation())
                    PlayingCharacterEntity.SetLookRotation(Quaternion.LookRotation(moveDirection));
            }

            // Attack when player pressed attack button
            if (InputManager.GetButton("Attack"))
                UpdateWASDAttack();

            // Always forward
            MovementState movementState = MovementState.Forward;
            if (InputManager.GetButtonDown("Jump"))
                movementState |= MovementState.IsJump;
            PlayingCharacterEntity.KeyMovement(moveDirection, movementState);
        }

        protected void UpdateWASDAttack()
        {
            destination = null;
            BaseCharacterEntity targetEntity;

            if (TryGetSelectedTargetAsAttackingEntity(out targetEntity))
                SetTarget(targetEntity, TargetActionType.Attack, false);

            if (wasdLockAttackTarget)
            {
                if (!TryGetAttackingEntity(out targetEntity) || targetEntity.IsHideOrDead())
                {
                    // Find nearest target and move to the target
                    targetEntity = PlayingCharacterEntity
                        .FindNearestAliveCharacter<BaseCharacterEntity>(
                        Mathf.Max(PlayingCharacterEntity.GetAttackDistance(isLeftHandAttacking), lockAttackTargetDistance),
                        false,
                        true,
                        false);
                }
                if (targetEntity != null && !targetEntity.IsHideOrDead())
                {
                    // Set target, then attack later when moved nearby target
                    SelectedEntity = targetEntity;
                    SetTarget(targetEntity, TargetActionType.Attack, false);
                    isFollowingTarget = true;
                }
                else
                {
                    // No nearby target, so attack immediately
                    RequestAttack();
                    isFollowingTarget = false;
                }
            }
            else if (!wasdLockAttackTarget)
            {
                // Find nearest target and set selected target to show character hp/mp UIs
                SelectedEntity = PlayingCharacterEntity
                    .FindNearestAliveCharacter<BaseCharacterEntity>(
                    PlayingCharacterEntity.GetAttackDistance(isLeftHandAttacking),
                    false,
                    true,
                    false);
                if (SelectedGameEntity != null)
                {
                    // Look at target and attack
                    TurnCharacterToEntity(SelectedGameEntity);
                }
                // Not lock target, so not finding target and attack immediately
                RequestAttack();
                isFollowingTarget = false;
            }
        }

        protected void UpdateQueuedSkill()
        {
            if (PlayingCharacterEntity.IsDead())
            {
                ClearQueueUsingSkill();
                return;
            }
            if (queueUsingSkill.skill == null || queueUsingSkill.level <= 0)
                return;
            if (PlayingCharacterEntity.IsPlayingActionAnimation())
                return;
            destination = null;
            BaseSkill skill = queueUsingSkill.skill;
            int skillLevel = queueUsingSkill.level;
            BaseCharacterEntity targetEntity;
            // Point click mode always lock on target
            bool wasdLockAttackTarget = this.wasdLockAttackTarget || controllerMode == PlayerCharacterControllerMode.PointClick;

            if (skill.HasCustomAimControls() && queueUsingSkill.aimPosition.type == AimPositionType.Position)
            {
                // Target not required, use skill immediately
                TurnCharacterToPosition(queueUsingSkill.aimPosition.position);
                RequestUsePendingSkill();
                isFollowingTarget = false;
                return;
            }

            if (skill.IsAttack)
            {
                if (wasdLockAttackTarget)
                {
                    if (!TryGetSelectedTargetAsAttackingEntity(out targetEntity) || targetEntity.IsHideOrDead())
                    {
                        // Try find nearby enemy if no selected target or selected taget is not enemy or target is hide or dead
                        targetEntity = PlayingCharacterEntity
                            .FindNearestAliveCharacter<BaseCharacterEntity>(
                            Mathf.Max(skill.GetCastDistance(PlayingCharacterEntity, skillLevel, isLeftHandAttacking), lockAttackTargetDistance),
                            false,
                            true,
                            false);
                    }
                    if (targetEntity != null && !targetEntity.IsHideOrDead())
                    {
                        // Set target, then use skill later when moved nearby target
                        SelectedEntity = targetEntity;
                        SetTarget(targetEntity, TargetActionType.UseSkill, false);
                        isFollowingTarget = true;
                    }
                    else
                    {
                        // No target, so use skill immediately
                        RequestUsePendingSkill();
                        isFollowingTarget = false;
                    }
                }
                else
                {
                    // Find nearest target and set selected target to show character hp/mp UIs
                    SelectedEntity = PlayingCharacterEntity
                        .FindNearestAliveCharacter<BaseCharacterEntity>(
                        skill.GetCastDistance(PlayingCharacterEntity, skillLevel, isLeftHandAttacking),
                        false,
                        true,
                        false);
                    if (SelectedGameEntity != null)
                    {
                        // Look at target and attack
                        TurnCharacterToEntity(SelectedGameEntity);
                    }
                    // Not lock target, so not finding target and use skill immediately
                    RequestUsePendingSkill();
                    isFollowingTarget = false;
                }
            }
            else
            {
                // Not attack skill, so use skill immediately
                if (skill.RequiredTarget)
                {
                    if (SelectedGameEntity == null)
                    {
                        RequestUsePendingSkill();
                        isFollowingTarget = false;
                        return;
                    }
                    if (wasdLockAttackTarget)
                    {
                        // Set target, then use skill later when moved nearby target
                        if (SelectedGameEntity is BaseCharacterEntity)
                        {
                            SetTarget(SelectedGameEntity, TargetActionType.UseSkill, false);
                            isFollowingTarget = true;
                        }
                        else
                        {
                            ClearQueueUsingSkill();
                            isFollowingTarget = false;
                        }
                    }
                    else
                    {
                        // Try apply skill to selected entity immediately, it will fail if selected entity is far from the character
                        if (SelectedGameEntity is BaseCharacterEntity)
                        {
                            if (SelectedGameEntity != PlayingCharacterEntity)
                            {
                                // Look at target and use skill
                                TurnCharacterToEntity(SelectedGameEntity);
                            }
                            RequestUsePendingSkill();
                            isFollowingTarget = false;
                        }
                        else
                        {
                            ClearQueueUsingSkill();
                            isFollowingTarget = false;
                        }
                    }
                }
                else
                {
                    // Target not required, use skill immediately
                    RequestUsePendingSkill();
                    isFollowingTarget = false;
                }
            }
        }

        public void UpdateFollowTarget()
        {
            if (!isFollowingTarget)
                return;

            IDamageableEntity targetDamageable;
            IActivatableEntity activatableEntity;
            IHoldActivatableEntity holdActivatableEntity;
            IPickupActivatableEntity pickupActivatableEntity;
            if (TryGetAttackingEntity(out targetDamageable))
            {
                if (targetDamageable.IsHideOrDead())
                {
                    ClearQueueUsingSkill();
                    PlayingCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }
                float attackDistance;
                float attackFov;
                GetAttackDistanceAndFov(isLeftHandAttacking, out attackDistance, out attackFov);
                AttackOrMoveToEntity(targetDamageable, attackDistance, CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.monsterLayer.Mask);
            }
            else if (TryGetUsingSkillEntity(out targetDamageable))
            {
                if (queueUsingSkill.skill.IsAttack && targetDamageable.IsHideOrDead())
                {
                    ClearQueueUsingSkill();
                    PlayingCharacterEntity.StopMove();
                    ClearTarget();
                    return;
                }
                float castDistance;
                float castFov;
                GetUseSkillDistanceAndFov(isLeftHandAttacking, out castDistance, out castFov);
                UseSkillOrMoveToEntity(targetDamageable, castDistance);
            }
            else if (TryGetDoActionEntity(out activatableEntity, TargetActionType.ClickActivate))
            {
                DoActionOrMoveToEntity(activatableEntity, activatableEntity.GetActivatableDistance(), () =>
                {
                    if (activatableEntity.ShouldNotActivateAfterFollowed())
                        return;
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        if (activatableEntity.CanActivate())
                            activatableEntity.OnActivate();
                        if (activatableEntity.ShouldClearTargetAfterActivated())
                            ClearTarget();
                    }
                });
            }
            else if (TryGetDoActionEntity(out holdActivatableEntity, TargetActionType.HoldClickActivate))
            {
                DoActionOrMoveToEntity(holdActivatableEntity, holdActivatableEntity.GetActivatableDistance(), () =>
                {
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        if (holdActivatableEntity.CanHoldActivate())
                            holdActivatableEntity.OnHoldActivate();
                        if (holdActivatableEntity.ShouldClearTargetAfterActivated())
                            ClearTarget();
                    }
                });
            }
            else if (TryGetDoActionEntity(out pickupActivatableEntity, TargetActionType.ClickActivate))
            {
                DoActionOrMoveToEntity(pickupActivatableEntity, pickupActivatableEntity.GetActivatableDistance(), () =>
                {
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        if (pickupActivatableEntity.CanPickupActivate())
                            pickupActivatableEntity.OnPickupActivate();
                        if (pickupActivatableEntity.ShouldClearTargetAfterActivated())
                            ClearTarget();
                    }
                });
            }
        }

        protected virtual bool OverlappedEntity(ITargetableEntity entity, Vector3 sourcePosition, Vector3 targetPosition, float distance)
        {
            if (Vector3.Distance(sourcePosition, targetPosition) <= distance)
                return true;
            // Target is far from controlling entity, try overlap the entity
            if (entity == null)
                return false;
            return physicFunctions.IsGameEntityInDistance(entity, sourcePosition, distance, false);
        }

        protected virtual bool OverlappedEntityHitBox<T>(T entity, Vector3 sourcePosition, Vector3 targetPosition, float distance)
            where T : BaseGameEntity
        {
            if (Vector3.Distance(sourcePosition, targetPosition) <= distance)
                return true;
            // Target is far from controlling entity, try overlap the entity
            if (entity == null)
                return false;
            return physicFunctions.IsGameEntityHitBoxInDistance(entity, sourcePosition, distance, false);
        }

        protected virtual void DoActionOrMoveToEntity(ITargetableEntity entity, float distance, System.Action action)
        {
            if (entity.IsNull())
                return;
            Vector3 sourcePosition = EntityTransform.position;
            Vector3 targetPosition = entity.EntityTransform.position;
            if (OverlappedEntity(entity, sourcePosition, targetPosition, distance))
            {
                // Stop movement to do action
                PlayingCharacterEntity.StopMove();
                // Do action
                action.Invoke();
                // This function may be used by extending classes
                OnDoActionOnEntity();
            }
            else
            {
                // Move to target entity
                UpdateTargetEntityPosition(sourcePosition, targetPosition, distance);
            }
        }

        protected virtual void OnDoActionOnEntity()
        {

        }

        protected virtual void AttackOrMoveToEntity(IDamageableEntity entity, float distance, int layerMask)
        {
            Transform damageTransform = PlayingCharacterEntity.GetWeaponDamageInfo(ref isLeftHandAttacking).GetDamageTransform(PlayingCharacterEntity, isLeftHandAttacking);
            Vector3 sourcePosition = damageTransform.position;
            Vector3 targetPosition = entity.OpponentAimTransform.position;
            if (OverlappedEntityHitBox(entity.Entity, sourcePosition, targetPosition, distance))
            {
                // Stop movement to attack
                PlayingCharacterEntity.StopMove();
                // Turn character to attacking target
                TurnCharacterToEntity(entity.Entity);
                // Do action
                RequestAttack();
                // This function may be used by extending classes
                OnAttackOnEntity();
            }
            else
            {
                // Move to target entity
                UpdateTargetEntityPosition(sourcePosition, targetPosition, distance);
            }
        }

        protected virtual void OnAttackOnEntity()
        {

        }

        protected virtual void UseSkillOrMoveToEntity(IDamageableEntity entity, float distance)
        {
            if (queueUsingSkill.skill != null)
            {
                Transform applyTransform = queueUsingSkill.skill.GetApplyTransform(PlayingCharacterEntity, isLeftHandAttacking);
                Vector3 sourcePosition = applyTransform.position;
                Vector3 targetPosition = entity.OpponentAimTransform.position;
                if (entity.GetObjectId() == PlayingCharacterEntity.GetObjectId() /* Applying skill to user? */ ||
                    OverlappedEntityHitBox(entity.Entity, sourcePosition, targetPosition, distance))
                {
                    // Set next frame target action type
                    targetActionType = queueUsingSkill.skill.IsAttack ? TargetActionType.Attack : TargetActionType.ClickActivate;
                    // Stop movement to use skill
                    PlayingCharacterEntity.StopMove();
                    // Turn character to attacking target
                    TurnCharacterToEntity(entity.Entity);
                    // Use the skill
                    RequestUsePendingSkill();
                    // This function may be used by extending classes
                    OnUseSkillOnEntity();
                }
                else
                {
                    // Move to target entity
                    UpdateTargetEntityPosition(sourcePosition, targetPosition, distance);
                }
            }
            else
            {
                // Can't use skill
                targetActionType = TargetActionType.ClickActivate;
                ClearQueueUsingSkill();
                return;
            }
        }

        protected virtual void OnUseSkillOnEntity()
        {

        }

        protected virtual void UpdateTargetEntityPosition(Vector3 sourcePosition, Vector3 targetPosition, float distance)
        {
            if (PlayingCharacterEntity.IsPlayingActionAnimation())
                return;

            Vector3 direction = (targetPosition - sourcePosition).normalized;
            Vector3 position = targetPosition - (direction * (distance - StoppingDistance));
            if (Vector3.Distance(MovementTransform.position, position) > MIN_START_MOVE_DISTANCE &&
                Vector3.Distance(previousPointClickPosition, position) > MIN_START_MOVE_DISTANCE)
            {
                PlayingCharacterEntity.PointClickMovement(position);
                previousPointClickPosition = position;
            }
        }

        protected void TurnCharacterToEntity(BaseGameEntity entity)
        {
            if (entity == null)
                return;
            TurnCharacterToPosition(entity.EntityTransform.position);
        }

        protected void TurnCharacterToPosition(Vector3 position)
        {
            Vector3 lookAtDirection = (position - EntityTransform.position).normalized;
            if (lookAtDirection.sqrMagnitude > 0)
                PlayingCharacterEntity.SetLookRotation(Quaternion.LookRotation(lookAtDirection));
        }

        public override void UseHotkey(HotkeyType type, string relateId, AimPosition aimPosition)
        {
            ClearQueueUsingSkill();
            switch (type)
            {
                case HotkeyType.Skill:
                    if (onBeforeUseSkillHotkey != null)
                        onBeforeUseSkillHotkey.Invoke(relateId, aimPosition);
                    UseSkill(relateId, aimPosition);
                    if (onAfterUseSkillHotkey != null)
                        onAfterUseSkillHotkey.Invoke(relateId, aimPosition);
                    break;
                case HotkeyType.Item:
                    HotkeyEquipWeaponSet = PlayingCharacterEntity.EquipWeaponSet;
                    if (onBeforeUseItemHotkey != null)
                        onBeforeUseItemHotkey.Invoke(relateId, aimPosition);
                    UseItem(relateId, aimPosition);
                    if (onAfterUseItemHotkey != null)
                        onAfterUseItemHotkey.Invoke(relateId, aimPosition);
                    break;
            }
        }

        protected void UseSkill(string id, AimPosition aimPosition)
        {
            BaseSkill skill;
            int skillLevel;
            if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out skill) || skill == null ||
                !PlayingCharacterEntity.GetCaches().Skills.TryGetValue(skill, out skillLevel))
                return;
            SetQueueUsingSkill(aimPosition, skill, skillLevel);
        }

        protected void UseItem(string id, AimPosition aimPosition)
        {
            int itemIndex;
            BaseItem item;
            int dataId = BaseGameData.MakeDataId(id);
            if (GameInstance.Items.ContainsKey(dataId))
            {
                item = GameInstance.Items[dataId];
                itemIndex = OwningCharacter.IndexOfNonEquipItem(dataId);
            }
            else
            {
                InventoryType inventoryType;
                byte equipWeaponSet;
                CharacterItem characterItem;
                if (PlayingCharacterEntity.IsEquipped(
                    id,
                    out inventoryType,
                    out itemIndex,
                    out equipWeaponSet,
                    out characterItem))
                {
                    GameInstance.ClientInventoryHandlers.RequestUnEquipItem(
                        inventoryType,
                        itemIndex,
                        equipWeaponSet,
                        -1,
                        ClientInventoryActions.ResponseUnEquipArmor,
                        ClientInventoryActions.ResponseUnEquipWeapon);
                    return;
                }
                item = characterItem.GetItem();
            }

            if (itemIndex < 0)
                return;

            if (item == null)
                return;

            if (item.IsEquipment())
            {
                GameInstance.ClientInventoryHandlers.RequestEquipItem(
                        PlayingCharacterEntity,
                        itemIndex,
                        HotkeyEquipWeaponSet,
                        ClientInventoryActions.ResponseEquipArmor,
                        ClientInventoryActions.ResponseEquipWeapon);
            }
            else if (item.IsSkill())
            {
                SetQueueUsingSkill(aimPosition, (item as ISkillItem).UsingSkill, (item as ISkillItem).UsingSkillLevel, itemIndex);
            }
            else if (item.IsBuilding())
            {
                destination = null;
                PlayingCharacterEntity.StopMove();
                buildingItemIndex = itemIndex;
                ShowConstructBuildingDialog();
            }
            else if (item.IsUsable())
            {
                PlayingCharacterEntity.CallServerUseItem(itemIndex);
            }
        }
    }
}
