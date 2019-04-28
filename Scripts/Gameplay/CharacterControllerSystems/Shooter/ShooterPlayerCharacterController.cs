using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public bool showConfirmConstructionUI;
        public RectTransform crosshairRect;
        public bool IsBlockController { get; protected set; }
        public float DefaultGameplayCameraFOV { get; protected set; }
        public Vector3 DefaultGameplayCameraOffset { get; protected set; }

        // Temp data
        BuildingMaterial tempBuildingMaterial;
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
        bool isLeftHandAttacking;
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
        // Crosshair
        Vector2 currentCrosshairSize;
        CrosshairSetting currentCrosshairSetting;
        // Controlling states
        bool isDoingAction;
        bool mustReleaseFireKey;
        Item rightHandWeapon;
        Item leftHandWeapon;
        MovementFlag movementState;
        BaseWeaponAbility weaponAbility;
        WeaponAbilityState weaponAbilityState;

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

            tempLookAt = characterEntity.CacheTransform.rotation;
            
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
            mustReleaseFireKey = false;
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
            else if (movementState.HasFlag(MovementFlag.Forward) ||
                movementState.HasFlag(MovementFlag.Backward) ||
                movementState.HasFlag(MovementFlag.Left) ||
                movementState.HasFlag(MovementFlag.Right) ||
                movementState.HasFlag(MovementFlag.IsJump))
            {
                UpdateCrosshair(currentCrosshairSetting, currentCrosshairSetting.expandPerFrameWhileMoving);
            }
            else
            {
                UpdateCrosshair(currentCrosshairSetting, -currentCrosshairSetting.shrinkPerFrame);
            }

            // Clear controlling states from last update
            isDoingAction = false;
            movementState = MovementFlag.None;
            UpdateActivatedWeaponAbility(tempDeltaTime);

            if (IsBlockController || GenericUtils.IsFocusInputField())
            {
                mustReleaseFireKey = false;
                PlayerCharacterEntity.KeyMovement(Vector3.zero, MovementFlag.None);
                DeactivateWeaponAbility();
                return;
            }

            // Find target character
            Ray ray = CacheGameplayCameraControls.CacheCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 forward = CacheGameplayCameraControls.CacheCameraTransform.forward;
            Vector3 right = CacheGameplayCameraControls.CacheCameraTransform.right;
            float distanceFromOrigin = Vector3.Distance(ray.origin, PlayerCharacterEntity.CacheTransform.position);
            float aimDistance = distanceFromOrigin;
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
                    aimDistance += PlayerCharacterEntity.GetSkillAttackDistance(queueSkill, isLeftHandAttacking);
                }
                else
                {
                    // Increase aim distance by attack distance
                    aimDistance += PlayerCharacterEntity.GetAttackDistance(isLeftHandAttacking);
                }
            }
            actionLookDirection = aimPosition = ray.origin + ray.direction * aimDistance;
            actionLookDirection.y = PlayerCharacterEntity.CacheTransform.position.y;
            actionLookDirection = actionLookDirection - PlayerCharacterEntity.CacheTransform.position;
            actionLookDirection.Normalize();
            // Prepare variables to find nearest raycasted hit point
            float tempDistance;
            float tempNearestDistance = float.MaxValue;
            // Find for enemy character
            if (CurrentBuildingEntity == null)
            {
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, aimDistance);
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
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
                    if (tempEntity == null || tempEntity == PlayerCharacterEntity)
                        continue;
                    // Target must be alive
                    if (tempEntity is IDamageableEntity && (tempEntity as IDamageableEntity).IsDead())
                        continue;
                    // Target must be in front of player character
                    if (!PlayerCharacterEntity.IsPositionInFov(60f, tempEntity.CacheTransform.position, forward))
                        continue;
                    // Set aim position and found target
                    tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                    if (tempDistance < tempNearestDistance)
                    {
                        tempNearestDistance = tempDistance;
                        aimPosition = tempHitInfo.point;
                        if (tempEntity != null)
                            SelectedEntity = tempEntity;
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
                int tempCount = Physics.RaycastNonAlloc(ray, raycasts, gameInstance.buildDistance);
                BuildingArea buildingArea = null;
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempHitInfo = raycasts[tempCounter];
                    tempEntity = tempHitInfo.collider.GetComponentInParent<BuildingEntity>();
                    if (tempEntity == null || tempEntity == CurrentBuildingEntity)
                        continue;

                    buildingArea = tempHitInfo.transform.GetComponent<BuildingArea>();
                    if (buildingArea == null ||
                        (buildingArea.buildingEntity != null && buildingArea.buildingEntity == CurrentBuildingEntity) ||
                        !CurrentBuildingEntity.buildingType.Equals(buildingArea.buildingType))
                        continue;

                    // Set aim position
                    tempDistance = Vector3.Distance(CacheGameplayCameraControls.CacheCameraTransform.position, tempHitInfo.point);
                    if (tempDistance < tempNearestDistance)
                    {
                        aimPosition = tempHitInfo.point;
                        CurrentBuildingEntity.buildingArea = buildingArea;
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
                if (queueSkill != null || tempPressAttackRight || tempPressAttackLeft || tempPressActivate || PlayerCharacterEntity.IsPlayingActionAnimation())
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
                    tempCalculateAngle = Vector3.Angle(PlayerCharacterEntity.CacheTransform.forward, actionLookDirection);
                    if (tempCalculateAngle > 15f)
                    {
                        if (queueSkill != null && queueSkill.IsAttack())
                            turningState = TurningState.UseSkill;
                        else if (tempPressAttackRight || tempPressAttackLeft)
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
                            UseSkill(isLeftHandAttacking, aimPosition);
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
                        UseSkill(false);
                    queueSkill = null;
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
                    if (SelectedEntity != null)
                        PlayerCharacterEntity.RequestPickupItem((SelectedEntity as ItemDropEntity).ObjectId);
                }
                else if (tempPressReload)
                {
                    // Reload ammo when press the button
                    ReloadAmmo();
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
                movementState |= MovementFlag.IsJump;

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
                    PlayerCharacterEntity.UpdateYRotation(tempLookAt.eulerAngles.y);
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
            CurrentBuildingEntity = null;

            CharacterHotkey hotkey = PlayerCharacterEntity.Hotkeys[hotkeyIndex];
            Skill skill = hotkey.GetSkill();
            if (skill != null)
            {
                short skillLevel;
                if (PlayerCharacterEntity.CacheSkills.TryGetValue(skill, out skillLevel))
                {
                    if (skill.CanUse(PlayerCharacterEntity, skillLevel))
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
                        CurrentBuildingEntity = Instantiate(item.buildingEntity);
                        CurrentBuildingEntity.SetupAsBuildMode();
                        CurrentBuildingEntity.CacheTransform.parent = null;
                    }
                }
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
        }

        public void UseSkill(bool isLeftHand, Vector3 aimPosition)
        {
            if (queueSkill == null)
                return;
            PlayerCharacterEntity.RequestUseSkill(queueSkill.DataId, isLeftHand, aimPosition);
        }

        public int OverlapObjects(Vector3 position, float distance, int layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, distance, overlapColliders, layerMask);
        }

        public bool FindTarget(GameObject target, float actDistance, int layerMask)
        {
            int tempCount = OverlapObjects(CharacterTransform.position, actDistance, layerMask);
            for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
            {
                tempGameObject = overlapColliders[tempCounter].gameObject;
                if (tempGameObject == target)
                    return true;
            }
            return false;
        }
    }
}
