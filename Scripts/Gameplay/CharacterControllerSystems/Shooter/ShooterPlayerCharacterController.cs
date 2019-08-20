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
            UseSkillItem,
        }

        public Mode mode;
        public float angularSpeed = 800f;
        [Range(0, 1f)]
        public float turnToTargetDuration = 0.1f;
        public float findTargetRaycastDistance = 512f;
        public bool showConfirmConstructionUI;
        public RectTransform crosshairRect;
        public bool IsBlockController { get; protected set; }
        public float DefaultGameplayCameraFOV { get; protected set; }
        public Vector3 DefaultGameplayCameraOffset { get; protected set; }

        // Temp data
        BuildingMaterial tempBuildingMaterial;
        IDamageableEntity tempDamageableEntity;
        BaseGameEntity tempEntity;
        Vector3 moveLookDirection;
        Vector3 targetLookDirection;
        Quaternion tempLookAt;
        TurningState turningState;
        float tempDeltaTime;
        float turnTimeCounter;
        float tempCalculateAngle;
        bool tempPressAttackRight;
        bool tempPressAttackLeft;
        bool tempPressWeaponAbility;
        bool tempPressActivate;
        bool tempPressPickupItem;
        bool tempPressReload;
        bool tempPressExitVehicle;
        bool isLeftHandAttacking;
        GameObject tempGameObject;
        BasePlayerCharacterEntity targetPlayer;
        NpcEntity targetNpc;
        BuildingEntity targetBuilding;
        RaycastHit[] raycasts = new RaycastHit[RAYCAST_COLLIDER_SIZE];
        Collider[] overlapColliders = new Collider[OVERLAP_COLLIDER_SIZE];
        RaycastHit tempHitInfo;
        Skill queueSkill;
        Skill queueSkillByItem;
        int queueSkillItemIndex;
        Vector3 aimPosition;
        Vector3 actionLookDirection;
        // Crosshair
        Vector2 currentCrosshairSize;
        CrosshairSetting currentCrosshairSetting;
        // Controlling states
        bool isDoingAction;
        bool mustReleaseFireKey;
        Item rightHandWeapon;
        Item leftHandWeapon;
        MovementState movementState;
        public BaseWeaponAbility weaponAbility { get; private set; }
        public WeaponAbilityState weaponAbilityState { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;

            if (CacheGameplayCameraControls != null)
            {
                DefaultGameplayCameraFOV = CacheGameplayCameraControls.CacheCamera.fieldOfView;
                DefaultGameplayCameraOffset = CacheGameplayCameraControls.targetOffset;
            }
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);

            if (characterEntity == null)
                return;

            tempLookAt = MovementTransform.rotation;

            SetupEquipWeapons(characterEntity.EquipWeapons);

            characterEntity.onEquipWeaponsChange += SetupEquipWeapons;
        }

        protected override void Desetup(BasePlayerCharacterEntity characterEntity)
        {
            base.Desetup(characterEntity);

            if (characterEntity == null)
                return;

            characterEntity.onEquipWeaponsChange -= SetupEquipWeapons;
        }

        protected void SetupEquipWeapons(EquipWeapons equipWeapons)
        {
            currentCrosshairSetting = PlayerCharacterEntity.GetCrosshairSetting();

            rightHandWeapon = equipWeapons.rightHand.GetWeaponItem();
            leftHandWeapon = equipWeapons.leftHand.GetWeaponItem();
            // Weapon ability will be able to use when equip weapon at main-hand only
            if (rightHandWeapon != null && leftHandWeapon == null)
            {
                if (rightHandWeapon.weaponAbility != weaponAbility)
                {
                    if (weaponAbility != null)
                        weaponAbility.ForceDeactivated();
                    weaponAbility = rightHandWeapon.weaponAbility;
                    weaponAbility.Setup(this);
                    weaponAbilityState = WeaponAbilityState.Deactivated;
                }
            }
            else
            {
                if (weaponAbility != null)
                    weaponAbility.ForceDeactivated();
                weaponAbility = null;
                weaponAbilityState = WeaponAbilityState.Deactivated;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();
            UpdateLookAtTarget();
            tempDeltaTime = Time.deltaTime;
            turnTimeCounter += tempDeltaTime;

            // Hide construction UI
            if (CurrentBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiConstructBuilding.IsVisible())
                    CacheUISceneGameplay.uiConstructBuilding.Hide();
            }
            if (ActiveBuildingEntity == null)
            {
                if (CacheUISceneGameplay.uiCurrentBuilding.IsVisible())
                    CacheUISceneGameplay.uiCurrentBuilding.Hide();
            }

            IsBlockController = CacheUISceneGameplay.IsBlockController();
            // Lock cursor when not show UIs
            Cursor.lockState = !IsBlockController ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = IsBlockController;

            CacheGameplayCameraControls.updateRotation = !IsBlockController;
            // Clear selected entity
            SelectedEntity = null;

            // Update crosshair (with states from last update)
            if (isDoingAction)
            {
                UpdateCrosshair(currentCrosshairSetting, currentCrosshairSetting.expandPerFrameWhileAttacking);
            }
            else if (movementState.HasFlag(MovementState.Forward) ||
                movementState.HasFlag(MovementState.Backward) ||
                movementState.HasFlag(MovementState.Left) ||
                movementState.HasFlag(MovementState.Right) ||
                movementState.HasFlag(MovementState.IsJump))
            {
                UpdateCrosshair(currentCrosshairSetting, currentCrosshairSetting.expandPerFrameWhileMoving);
            }
            else
            {
                UpdateCrosshair(currentCrosshairSetting, -currentCrosshairSetting.shrinkPerFrame);
            }

            // Clear controlling states from last update
            isDoingAction = false;
            movementState = MovementState.None;
            UpdateActivatedWeaponAbility(tempDeltaTime);

            if (IsBlockController || GenericUtils.IsFocusInputField())
            {
                mustReleaseFireKey = false;
                PlayerCharacterEntity.KeyMovement(Vector3.zero, MovementState.None);
                DeactivateWeaponAbility();
                return;
            }

            // Find target character
            Ray ray = CacheGameplayCameraControls.CacheCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 forward = CacheGameplayCameraControls.CacheCameraTransform.forward;
            Vector3 right = CacheGameplayCameraControls.CacheCameraTransform.right;
            float distanceFromOrigin = Vector3.Distance(ray.origin, MovementTransform.position);

            // Prepare variables to find nearest raycasted hit point
            float tempDistance;
            float tempNearestDistance = float.MaxValue;

            if (CurrentBuildingEntity == null)
            {
                // Prepare raycast distance / fov
                float aimDistance = distanceFromOrigin;
                float attackDistance = 0f;
                float attackFov = 90f;
                // Calculating aim distance, also read attack inputs here
                // Attack inputs will be used to calculate attack distance
                if (CurrentBuildingEntity == null)
                {
                    // Attack with right hand weapon
                    tempPressAttackRight = InputManager.GetButton("Fire1");
                    if (weaponAbility == null && leftHandWeapon != null)
                    {
                        // Attack with left hand weapon if left hand weapon not empty
                        tempPressAttackLeft = InputManager.GetButton("Fire2");
                    }
                    else if (weaponAbility != null)
                    {
                        // Use weapon ability if it can
                        tempPressWeaponAbility = InputManager.GetButtonDown("Fire2");
                    }
                    // Is left hand attack when not attacking with right hand
                    // So priority is right > left
                    isLeftHandAttacking = !tempPressAttackRight && tempPressAttackLeft;

                    // Calculate aim distance by skill or weapon
                    if (queueSkill != null && queueSkill.IsAttack())
                    {
                        // Increase aim distance by skill attack distance
                        attackDistance = PlayerCharacterEntity.GetSkillAttackDistance(queueSkill, isLeftHandAttacking);
                        attackFov = PlayerCharacterEntity.GetSkillAttackFov(queueSkill, isLeftHandAttacking);
                    }
                    else if (queueSkillByItem != null && queueSkillByItem.IsAttack())
                    {
                        // Increase aim distance by skill attack distance
                        attackDistance = PlayerCharacterEntity.GetSkillAttackDistance(queueSkillByItem, isLeftHandAttacking);
                        attackFov = PlayerCharacterEntity.GetSkillAttackFov(queueSkillByItem, isLeftHandAttacking);
                    }
                    else
                    {
                        // Increase aim distance by attack distance
                        attackDistance = PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking);
                        attackFov = PlayerCharacterEntity.GetAttackFov(isLeftHandAttacking);
                    }
                    aimDistance += attackDistance;
                }
                actionLookDirection = aimPosition = ray.origin + ray.direction * aimDistance;
                actionLookDirection.y = MovementTransform.position.y;
                actionLookDirection = actionLookDirection - MovementTransform.position;
                actionLookDirection.Normalize();
                // Find for enemy character
                bool foundDamageableEntity = false;
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, findTargetRaycastDistance);
                int tempCounter = 0;
                for (; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];

                    // Get distance between character and raycast hit point
                    tempDistance = Vector3.Distance(MovementTransform.position, tempHitInfo.point);
                    // If this is damageable entity
                    tempDamageableEntity = tempHitInfo.collider.GetComponent<IDamageableEntity>();
                    if (tempDamageableEntity != null && tempDistance <= attackDistance)
                    {
                        tempEntity = tempDamageableEntity.Entity;

                        // Target must be damageable, not player character entity, within aim distance and alive
                        if (tempDamageableEntity.ObjectId == PlayerCharacterEntity.ObjectId ||
                            tempDamageableEntity.IsDead())
                            continue;

                        // Set aim position and found target
                        if (tempDistance < tempNearestDistance)
                        {
                            tempNearestDistance = tempDistance;
                            aimPosition = tempHitInfo.point;
                            if (tempEntity != null)
                            {
                                SelectedEntity = tempEntity;
                                foundDamageableEntity = true;
                            }
                        }
                    }
                    // If already found damageable entity don't find for npc / item
                    if (foundDamageableEntity)
                        continue;
                    // Find item drop entity
                    tempEntity = tempHitInfo.collider.GetComponent<ItemDropEntity>();
                    if (tempEntity != null && tempDistance <= gameInstance.pickUpItemDistance)
                    {
                        // Set aim position and found target
                        if (tempDistance < tempNearestDistance)
                        {
                            tempNearestDistance = tempDistance;
                            aimPosition = tempHitInfo.point;
                            if (tempEntity != null)
                                SelectedEntity = tempEntity;
                        }
                    }
                    // Find activatable entity (NPC)
                    tempEntity = tempHitInfo.collider.GetComponent<BaseGameEntity>();
                    if (tempEntity != null && tempDistance <= gameInstance.conversationDistance)
                    {
                        // Set aim position and found target
                        if (tempDistance < tempNearestDistance)
                        {
                            tempNearestDistance = tempDistance;
                            aimPosition = tempHitInfo.point;
                            if (tempEntity != null)
                                SelectedEntity = tempEntity;
                        }
                    }
                }
                // Show target hp/mp
                CacheUISceneGameplay.SetTargetEntity(SelectedEntity);
            }
            else
            {
                // Clear area before next find
                CurrentBuildingEntity.buildingArea = null;
                // Find for position to construction building
                bool foundSnapBuildPosition = false;
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, findTargetRaycastDistance);
                int tempCounter = 0;
                BuildingArea buildingArea = null;
                for (; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];
                    tempEntity = tempHitInfo.collider.GetComponentInParent<BuildingEntity>();
                    if (tempEntity != null && tempEntity == CurrentBuildingEntity)
                    {
                        // Skip because it's raycast to the building that you are going to build
                        continue;
                    }

                    // Set aim position
                    tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                    if (tempDistance < tempNearestDistance)
                    {
                        aimPosition = tempHitInfo.point;
                        buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                        if (buildingArea == null ||
                            (buildingArea.buildingEntity != null && buildingArea.buildingEntity == CurrentBuildingEntity) ||
                            !CurrentBuildingEntity.buildingType.Equals(buildingArea.buildingType))
                        {
                            // Skip because this area is not allowed to build the building that you are going to build
                            continue;
                        }

                        CurrentBuildingEntity.buildingArea = buildingArea;
                        if (buildingArea.snapBuildingObject)
                        {
                            foundSnapBuildPosition = true;
                            break;
                        }
                    }
                }

                if (tempCount <= 0)
                {
                    // Not hit anything (player may look at the sky)
                    aimPosition = ray.origin + ray.direction * (distanceFromOrigin + gameInstance.buildDistance);
                }

                if (Vector3.Distance(PlayerCharacterEntity.CacheTransform.position, aimPosition) > gameInstance.buildDistance)
                {
                    // Mark as unable to build when the building is far from character
                    CurrentBuildingEntity.buildingArea = null;
                }

                if (!foundSnapBuildPosition)
                {
                    // Update building position when the building is not snapping to build area
                    CurrentBuildingEntity.CacheTransform.position = aimPosition;
                    // Rotate to camera
                    Vector3 direction = (aimPosition - CacheGameplayCameraControls.CacheCameraTransform.position).normalized;
                    direction.y = 0;
                    CurrentBuildingEntity.transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            // If mobile platforms, don't receive input raw to make it smooth
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
                    if (inputV > 0.5f || inputV < -0.5f || inputH > 0.5f || inputH < -0.5f)
                        movementState = MovementState.Forward;
                    moveLookDirection = moveDirection;
                    break;
                case Mode.Combat:
                    moveDirection += forward * inputV;
                    moveDirection += right * inputH;
                    if (inputV > 0.5f)
                        movementState |= MovementState.Forward;
                    else if (inputV < -0.5f)
                        movementState |= MovementState.Backward;
                    if (inputH > 0.5f)
                        movementState |= MovementState.Right;
                    else if (inputH < -0.5f)
                        movementState |= MovementState.Left;
                    moveLookDirection = actionLookDirection;
                    break;
            }

            // normalize input if it exceeds 1 in combined length:
            if (moveDirection.sqrMagnitude > 1)
                moveDirection.Normalize();

            if (CurrentBuildingEntity != null)
            {
                mustReleaseFireKey = false;
                // Building
                tempPressAttackRight = InputManager.GetButtonUp("Fire1");
                if (tempPressAttackRight)
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
                // Have to release fire key, then check press fire key later on next frame
                if (mustReleaseFireKey)
                {
                    tempPressAttackRight = false;
                    tempPressAttackLeft = false;
                    if (!isLeftHandAttacking &&
                        (InputManager.GetButtonUp("Fire1") ||
                        !InputManager.GetButton("Fire1")))
                        mustReleaseFireKey = false;
                    if (isLeftHandAttacking &&
                        (InputManager.GetButtonUp("Fire2") ||
                        !InputManager.GetButton("Fire2")))
                        mustReleaseFireKey = false;
                }
                // Not building so it is attacking
                tempPressActivate = InputManager.GetButtonDown("Activate");
                tempPressPickupItem = InputManager.GetButtonDown("PickUpItem");
                tempPressReload = InputManager.GetButtonDown("Reload");
                tempPressExitVehicle = InputManager.GetButtonDown("ExitVehicle");
                if (queueSkill != null || queueSkillByItem != null || tempPressAttackRight || tempPressAttackLeft || tempPressActivate || PlayerCharacterEntity.IsPlayingActionAnimation())
                {
                    // Find forward character / npc / building / warp entity from camera center
                    targetPlayer = null;
                    targetNpc = null;
                    targetBuilding = null;
                    if (tempPressActivate && !tempPressAttackRight && !tempPressAttackLeft)
                    {
                        if (SelectedEntity is BasePlayerCharacterEntity)
                            targetPlayer = SelectedEntity as BasePlayerCharacterEntity;
                        if (SelectedEntity is NpcEntity)
                            targetNpc = SelectedEntity as NpcEntity;
                        if (SelectedEntity is BuildingEntity)
                            targetBuilding = SelectedEntity as BuildingEntity;
                    }
                    // While attacking turn to camera forward
                    tempCalculateAngle = Vector3.Angle(MovementTransform.forward, actionLookDirection);
                    if (tempCalculateAngle > 15f)
                    {
                        if (queueSkill != null && queueSkill.IsAttack())
                        {
                            turningState = TurningState.UseSkill;
                        }
                        else if (queueSkillByItem != null && queueSkillByItem.IsAttack())
                        {
                            turningState = TurningState.UseSkillItem;
                        }
                        else if (tempPressAttackRight || tempPressAttackLeft)
                        {
                            turningState = TurningState.Attack;
                        }
                        else if (tempPressActivate)
                        {
                            turningState = TurningState.Activate;
                        }

                        turnTimeCounter = ((180f - tempCalculateAngle) / 180f) * turnToTargetDuration;
                        targetLookDirection = actionLookDirection;
                        // Set movement state by inputs
                        if (inputV > 0.5f)
                            movementState |= MovementState.Forward;
                        else if (inputV < -0.5f)
                            movementState |= MovementState.Backward;
                        if (inputH > 0.5f)
                            movementState |= MovementState.Right;
                        else if (inputH < -0.5f)
                            movementState |= MovementState.Left;
                    }
                    else
                    {
                        // Attack immediately if character already look at target
                        if (queueSkill != null && queueSkill.IsAttack())
                        {
                            UseSkill(isLeftHandAttacking, aimPosition);
                            isDoingAction = true;
                        }
                        else if (queueSkillByItem != null && queueSkillByItem.IsAttack())
                        {
                            UseSkillItem(isLeftHandAttacking, aimPosition);
                            isDoingAction = true;
                        }
                        else if (tempPressAttackRight || tempPressAttackLeft)
                        {
                            Attack(isLeftHandAttacking);
                            isDoingAction = true;
                        }
                        else if (tempPressActivate)
                        {
                            Activate();
                        }
                    }

                    // If skill is not attack skill, use it immediately
                    if (queueSkill != null && !queueSkill.IsAttack())
                    {
                        UseSkill(false);
                    }
                    else if (queueSkillByItem != null && queueSkillByItem.IsAttack())
                    {
                        UseSkillItem(false);
                    }
                }
                else if (tempPressWeaponAbility)
                {
                    switch (weaponAbilityState)
                    {
                        case WeaponAbilityState.Activated:
                        case WeaponAbilityState.Activating:
                            DeactivateWeaponAbility();
                            break;
                        case WeaponAbilityState.Deactivated:
                        case WeaponAbilityState.Deactivating:
                            ActivateWeaponAbility();
                            break;
                    }
                }
                else if (tempPressPickupItem)
                {
                    // Find for item to pick up
                    if (SelectedEntity != null && SelectedEntity is ItemDropEntity)
                        PlayerCharacterEntity.RequestPickupItem((SelectedEntity as ItemDropEntity).ObjectId);
                }
                else if (tempPressReload)
                {
                    // Reload ammo when press the button
                    ReloadAmmo();
                }
                else if (tempPressExitVehicle)
                {
                    // Exit vehicle
                    PlayerCharacterEntity.RequestExitVehicle();
                }
                else
                {
                    // Update move direction
                    if (moveDirection.magnitude != 0f)
                        targetLookDirection = moveLookDirection;
                }
                // Setup releasing state
                if (tempPressAttackRight && rightHandWeapon != null && rightHandWeapon.fireType == FireType.SingleFire)
                {
                    // The weapon's fire mode is single fire, so player have to release fire key for next fire
                    mustReleaseFireKey = true;
                }
                else if (tempPressAttackLeft && leftHandWeapon != null && leftHandWeapon.fireType == FireType.SingleFire)
                {
                    // The weapon's fire mode is single fire, so player have to release fire key for next fire
                    mustReleaseFireKey = true;
                }
                // Auto reload
                if (!tempPressAttackRight && !tempPressAttackLeft && !tempPressReload &&
                    (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                    PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty()))
                {
                    // Reload ammo when empty and not press any keys
                    ReloadAmmo();
                }
            }
            SetAimPosition(aimPosition);

            // Hide Npc UIs when move
            if (moveDirection.magnitude != 0f)
            {
                HideNpcDialogs();
                PlayerCharacterEntity.StopMove();
                PlayerCharacterEntity.SetTargetEntity(null);
            }
            // If jumping add jump state
            if (InputManager.GetButtonDown("Jump"))
                movementState |= MovementState.IsJump;

            PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
        }

        private void ReloadAmmo()
        {
            // Reload ammo at server
            if (!PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(false);
            else if (!PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoFull())
                PlayerCharacterEntity.RequestReload(true);
        }

        private void UpdateCrosshair(CrosshairSetting setting, float power)
        {
            if (crosshairRect == null)
                return;

            crosshairRect.gameObject.SetActive(!setting.hidden);
            currentCrosshairSize = crosshairRect.sizeDelta;
            // Change crosshair size by power
            currentCrosshairSize.x += power;
            currentCrosshairSize.y += power;
            // Set crosshair size
            crosshairRect.sizeDelta = new Vector2(Mathf.Clamp(currentCrosshairSize.x, setting.minSpread, setting.maxSpread), Mathf.Clamp(currentCrosshairSize.y, setting.minSpread, setting.maxSpread));
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
                    PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
                }
                else
                {
                    switch (turningState)
                    {
                        case TurningState.Attack:
                            Attack(isLeftHandAttacking);
                            break;
                        case TurningState.Activate:
                            Activate();
                            break;
                        case TurningState.UseSkill:
                            UseSkill(isLeftHandAttacking, aimPosition);
                            break;
                        case TurningState.UseSkillItem:
                            UseSkillItem(isLeftHandAttacking, aimPosition);
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
                    PlayerCharacterEntity.SetLookRotation(tempLookAt.eulerAngles);
                }
            }
        }

        public override void UseHotkey(int hotkeyIndex)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= PlayerCharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            CurrentBuildingEntity = null;
            ClearQueueSkill();

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            switch (hotkey.type)
            {
                case HotkeyType.Skill:
                    UseSkill(hotkey.relateId);
                    break;
                case HotkeyType.Item:
                    UseItem(hotkey.relateId);
                    break;
            }
        }

        protected void UseSkill(string id)
        {
            Skill skill = null;
            if (GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(id), out skill) &&
                skill != null && PlayerCharacterEntity.CacheSkills.ContainsKey(skill))
            {
                PlayerCharacterEntity.StopMove();
                queueSkill = skill;
            }
        }

        protected void UseItem(string id)
        {
            int itemIndex = -1;
            CharacterItem characterItem;
            InventoryType inventoryType;
            if (PlayerCharacterEntity.IsEquipped(id, out itemIndex, out characterItem, out inventoryType))
            {
                PlayerCharacterEntity.RequestUnEquipItem((byte)inventoryType, (short)itemIndex);
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
                    PlayerCharacterEntity.StopMove();
                    queueSkillByItem = item.skillLevel.skill;
                    queueSkillItemIndex = itemIndex;
                }
                else
                {
                    PlayerCharacterEntity.RequestUseItem((short)itemIndex);
                }
            }
            else if (item.IsBuilding())
            {
                PlayerCharacterEntity.StopMove();
                buildingItemIndex = itemIndex;
                CurrentBuildingEntity = Instantiate(item.buildingEntity);
                CurrentBuildingEntity.SetupAsBuildMode();
                CurrentBuildingEntity.CacheTransform.parent = null;
            }
        }

        public void SetAimPosition(Vector3 aimPosition)
        {
            PlayerCharacterEntity.RequestSetAimPosition(aimPosition);
        }

        public void Attack(bool isLeftHand)
        {
            PlayerCharacterEntity.RequestAttack(isLeftHand);
        }

        public void ActivateWeaponAbility()
        {
            if (weaponAbility == null)
                return;

            if (weaponAbilityState == WeaponAbilityState.Activated ||
                weaponAbilityState == WeaponAbilityState.Activating)
                return;

            weaponAbilityState = WeaponAbilityState.Activating;
        }

        private void UpdateActivatedWeaponAbility(float deltaTime)
        {
            if (weaponAbility == null)
                return;

            if (weaponAbilityState == WeaponAbilityState.Activated ||
                weaponAbilityState == WeaponAbilityState.Deactivated)
                return;

            weaponAbilityState = weaponAbility.UpdateActivation(weaponAbilityState, deltaTime);
        }

        private void DeactivateWeaponAbility()
        {
            if (weaponAbility == null)
                return;

            if (weaponAbilityState == WeaponAbilityState.Deactivated ||
                weaponAbilityState == WeaponAbilityState.Deactivating)
                return;

            weaponAbilityState = WeaponAbilityState.Deactivating;
        }

        public void Activate()
        {
            // Priority Player -> Npc -> Buildings
            if (targetPlayer != null && CacheUISceneGameplay != null)
                CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
            else if (targetNpc != null)
                PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
            else if (targetBuilding != null)
                ActivateBuilding(targetBuilding);
        }

        public void UseSkill(bool isLeftHand)
        {
            if (queueSkill == null)
                return;
            PlayerCharacterEntity.RequestUseSkill(queueSkill.DataId, isLeftHand);
            ClearQueueSkill();
        }

        public void UseSkill(bool isLeftHand, Vector3 aimPosition)
        {
            if (queueSkill == null)
                return;
            PlayerCharacterEntity.RequestUseSkill(queueSkill.DataId, isLeftHand, aimPosition);
            ClearQueueSkill();
        }

        public void UseSkillItem(bool isLeftHand)
        {
            if (queueSkillItemIndex < 0)
                return;
            PlayerCharacterEntity.RequestUseSkillItem((short)queueSkillItemIndex, isLeftHand);
            ClearQueueSkill();
        }

        public void UseSkillItem(bool isLeftHand, Vector3 aimPosition)
        {
            if (queueSkillItemIndex < 0)
                return;
            PlayerCharacterEntity.RequestUseSkillItem((short)queueSkillItemIndex, isLeftHand, aimPosition);
            ClearQueueSkill();
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(MovementTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempGameObject = overlapColliders[tempCounter].gameObject;
                if (tempGameObject == target)
                    return true;
            }
            return false;
        }

        protected void ClearQueueSkill()
        {
            queueSkill = null;
            queueSkillByItem = null;
            queueSkillItemIndex = -1;
        }
    }
}
