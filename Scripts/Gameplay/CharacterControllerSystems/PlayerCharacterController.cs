using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace MultiplayerARPG
{
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
        [Tooltip("Set this to TRUE to find nearby enemy and look to it while attacking when `Controller Mode` is `WASD`")]
        public bool wasdLockAttackTarget;
        [Tooltip("This will be used to find nearby enemy when `Controller Mode` is `Point Click` or when `Wasd Lock Attack Target` is `TRUE`")]
        public float lockAttackTargetDistance = 10f;
        public FollowCameraControls gameplayCameraPrefab;
        public GameObject targetObjectPrefab;
        [Header("Building Settings")]
        public bool buildGridSnap;
        public float buildGridSize = 4f;
        public bool buildRotationSnap;
        public struct UsingSkillData
        {
            public Vector3? position;
            public int skillIndex;
            public UsingSkillData(Vector3? position, int skillIndex)
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
        protected uint lastNpcObjectId;
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        public GameObject CacheTargetObject { get; protected set; }

        // Optimizing garbage collection
        protected bool getMouseUp;
        protected bool getMouseDown;
        protected bool getMouse;
        protected bool isPointerOverUI;
        protected bool isMouseDragDetected;
        protected bool isMouseHoldDetected;
        protected RaycastHit[] foundRaycastAll;
        protected Collider[] foundOverlapSphere;
        protected BaseCharacterEntity targetCharacter;
        protected BaseCharacterEntity targetEnemy;
        protected BasePlayerCharacterEntity targetPlayer;
        protected BaseMonsterCharacterEntity targetMonster;
        protected NpcEntity targetNpc;
        protected ItemDropEntity targetItemDrop;
        protected BuildingEntity targetBuilding;
        protected HarvestableEntity targetHarvestable;

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
            if (targetObjectPrefab != null)
            {
                // Set parent transform to root for the best performance
                CacheTargetObject = Instantiate(targetObjectPrefab);
                CacheTargetObject.SetActive(false);
            }
        }

        protected override void Setup(BasePlayerCharacterEntity characterEntity)
        {
            base.Setup(characterEntity);

            if (characterEntity == null)
                return;

            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.target = characterEntity.CacheTransform;
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
            if (CacheTargetObject != null)
                Destroy(CacheTargetObject.gameObject);
        }

        protected override void Update()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            base.Update();

            if (CacheTargetObject != null)
                CacheTargetObject.gameObject.SetActive(destination.HasValue);

            if (PlayerCharacterEntity.IsDead())
            {
                queueUsingSkill = null;
                destination = null;
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetCharacter(null);
                CancelBuild();
            }
            else
            {
                targetCharacter = null;
                PlayerCharacterEntity.TryGetTargetEntity(out targetCharacter);
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetCharacter(targetCharacter);
            }

            if (destination.HasValue)
            {
                if (CacheTargetObject != null)
                    CacheTargetObject.transform.position = destination.Value;
                if (Vector3.Distance(destination.Value, CharacterTransform.position) < StoppingDistance + 0.5f)
                    destination = null;
            }

            UpdateInput();
            UpdateFollowTarget();
        }

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
                    foundOverlapSphere = Physics.OverlapSphere(CharacterTransform.position, gameInstance.conversationDistance, gameInstance.characterLayer.Mask);
                    targetPlayer = null;
                    targetNpc = null;
                    foreach (var foundEntity in foundOverlapSphere)
                    {
                        if (targetPlayer == null)
                        {
                            targetPlayer = foundEntity.GetComponent<BasePlayerCharacterEntity>();
                            if (targetPlayer == PlayerCharacterEntity)
                                targetPlayer = null;
                        }
                        if (targetNpc == null)
                            targetNpc = foundEntity.GetComponent<NpcEntity>();
                    }
                    // Priority Player -> Npc -> Buildings
                    if (targetPlayer != null && CacheUISceneGameplay != null)
                        CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
                    else if (targetNpc != null)
                        PlayerCharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                    if (foundOverlapSphere.Length == 0)
                        PlayerCharacterEntity.RequestEnterWarp();
                }
                // Pick up nearby items
                if (InputManager.GetButtonDown("PickUpItem"))
                {
                    foundOverlapSphere = Physics.OverlapSphere(CharacterTransform.position, gameInstance.pickUpItemDistance, gameInstance.itemDropLayer.Mask);
                    foreach (var foundEntity in foundOverlapSphere)
                    {
                        targetItemDrop = foundEntity.GetComponent<ItemDropEntity>();
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
            if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
                isMouseDragOrHoldOrOverUI = true;
            if (!isPointerOverUI && (getMouse || getMouseUp))
            {
                var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
                PlayerCharacterEntity.SetTargetEntity(null);
                LiteNetLibIdentity targetIdentity = null;
                Vector3? targetPosition = null;
                foundRaycastAll = Physics.RaycastAll(targetCamera.ScreenPointToRay(Input.mousePosition), 100f, gameInstance.GetTargetLayerMask());
                foreach (var hit in foundRaycastAll)
                {
                    var hitTransform = hit.transform;
                    // When clicking on target
                    if (getMouseUp &&
                        !isMouseDragOrHoldOrOverUI &&
                        (controllerMode == PlayerCharacterControllerMode.PointClick || controllerMode == PlayerCharacterControllerMode.Both))
                    {
                        targetPlayer = hitTransform.GetComponent<BasePlayerCharacterEntity>();
                        targetMonster = hitTransform.GetComponent<BaseMonsterCharacterEntity>();
                        targetNpc = hitTransform.GetComponent<NpcEntity>();
                        targetItemDrop = hitTransform.GetComponent<ItemDropEntity>();
                        targetHarvestable = hitTransform.GetComponent<HarvestableEntity>();
                        targetPosition = hit.point;
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
                    else if (!isMouseDragDetected && isMouseHoldDetected)
                    {
                        var buildingMaterial = hitTransform.GetComponent<BuildingMaterial>();
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

            if (moveDirection.magnitude > 0.1f)
            {
                if (CacheUISceneGameplay != null && CacheUISceneGameplay.uiNpcDialog != null)
                    CacheUISceneGameplay.uiNpcDialog.Hide();
                SetBuildingObjectByCharacterTransform();
            }

            // For WASD mode, Using skill when player pressed hotkey
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                destination = null;
                PlayerCharacterEntity.StopMove();
                var characterSkill = PlayerCharacterEntity.Skills[queueUsingSkillValue.skillIndex];
                var skill = characterSkill.GetSkill();
                if (skill != null)
                {
                    if (skill.IsAttack())
                    {
                        BaseCharacterEntity targetEntity;
                        if (wasdLockAttackTarget && !TryGetAttackingCharacter(out targetEntity))
                        {
                            var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance, false, true);
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
                    var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetAttackDistance() + lockAttackTargetDistance, false, true);
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
                if (moveDirection.magnitude > 0)
                {
                    if (PlayerCharacterEntity.IsMoving())
                        PlayerCharacterEntity.StopMove();
                    destination = null;
                    PlayerCharacterEntity.SetTargetEntity(null);
                }
                PlayerCharacterEntity.KeyMovement(moveDirection, jumpInput);
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
            {
                var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
                RaycastToSetBuildingArea(targetCamera.ScreenPointToRay(Input.mousePosition), 100f);
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
                if (FindTarget(targetEnemy.CacheTransform, actDistance, gameInstance.characterLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    var halfFov = attackFov * 0.5f;
                    var targetDir = (CharacterTransform.position - targetEnemy.CacheTransform.position).normalized;
                    var angle = Vector3.Angle(targetDir, CharacterTransform.forward);
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
                if (FindTarget(targetHarvestable.CacheTransform, actDistance, gameInstance.harvestableLayer.Mask))
                {
                    // Stop movement to attack
                    PlayerCharacterEntity.StopMove();
                    var halfFov = attackFov * 0.5f;
                    var targetDir = (CharacterTransform.position - targetHarvestable.CacheTransform.position).normalized;
                    var angle = Vector3.Angle(targetDir, CharacterTransform.forward);
                    if (angle < 180 + halfFov && angle > 180 - halfFov)
                        RequestAttack();
                }
                else
                    UpdateTargetEntityPosition(targetHarvestable);
            }
        }

        protected void UpdateTargetEntityPosition(RpgNetworkEntity entity)
        {
            if (entity == null)
                return;

            var targetPosition = entity.CacheTransform.position;
            PlayerCharacterEntity.PointClickMovement(targetPosition);
        }

        public void RequestAttack()
        {
            PlayerCharacterEntity.RequestAttack();
        }

        public void RequestUseSkill(Vector3 position, int skillIndex)
        {
            PlayerCharacterEntity.RequestUseSkill(position, skillIndex);
        }

        public void RequestUsePendingSkill()
        {
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                var position = queueUsingSkillValue.position.HasValue ? queueUsingSkillValue.position.Value : CharacterTransform.position;
                RequestUseSkill(position, queueUsingSkillValue.skillIndex);
                queueUsingSkill = null;
            }
        }

        public void RequestEquipItem(int itemIndex)
        {
            PlayerCharacterEntity.RequestEquipItem(itemIndex);
        }

        public void RequestUseItem(int itemIndex)
        {
            PlayerCharacterEntity.RequestUseItem(itemIndex);
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
                        queueUsingSkill = new UsingSkillData(null, skillIndex);
                    }
                    else if (PlayerCharacterEntity.Skills[skillIndex].CanUse(PlayerCharacterEntity))
                    {
                        // If not attacking any character, use skill immediately
                        if (skill.IsAttack() && IsLockTarget())
                        {
                            // If attacking any character, will use skill later
                            queueUsingSkill = new UsingSkillData(null, skillIndex);
                            var nearestTarget = PlayerCharacterEntity.FindNearestAliveCharacter<BaseCharacterEntity>(PlayerCharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance, false, true);                            if (nearestTarget != null)
                                PlayerCharacterEntity.SetTargetEntity(nearestTarget);
                        }
                        else
                        {
                            destination = null;
                            PlayerCharacterEntity.StopMove();
                            RequestUseSkill(CharacterTransform.position, skillIndex);
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
                        RequestEquipItem(itemIndex);
                    else if (item.IsPotion())
                        RequestUseItem(itemIndex);
                    else if (item.IsBuilding())
                    {
                        destination = null;
                        PlayerCharacterEntity.StopMove();
                        buildingItemIndex = itemIndex;
                        currentBuildingEntity = Instantiate(item.buildingEntity);
                        currentBuildingEntity.SetupAsBuildMode();
                        currentBuildingEntity.CacheTransform.parent = null;
                        SetBuildingObjectByCharacterTransform();
                    }
                }
            }
        }

        private void SetBuildingObjectByCharacterTransform()
        {
            if (currentBuildingEntity != null)
            {
                var placePosition = CharacterTransform.position + (CharacterTransform.forward * currentBuildingEntity.characterForwardDistance);
                currentBuildingEntity.CacheTransform.eulerAngles = GetBuildingPlaceEulerAngles(CharacterTransform.eulerAngles);
                currentBuildingEntity.buildingArea = null;
                if (!RaycastToSetBuildingArea(new Ray(placePosition + (Vector3.up * 2.5f), Vector3.down), 5f))
                    currentBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(placePosition);
            }
        }

        private Vector3 GetBuildingPlacePosition(Vector3 position)
        {
            if (buildGridSnap)
                position = new Vector3(Mathf.Round(position.x / buildGridSize) * buildGridSize, position.y, Mathf.Round(position.z / buildGridSize) * buildGridSize);
            return position;
        }

        private Vector3 GetBuildingPlaceEulerAngles(Vector3 eulerAngles)
        {
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            // Make Y rotation set to 0, 90, 180
            if (buildRotationSnap)
                eulerAngles.y = Mathf.Round(eulerAngles.y / 90) * 90;
            return eulerAngles;
        }

        private bool RaycastToSetBuildingArea(Ray ray, float dist = 5f)
        {
            BuildingArea nonSnapBuildingArea = null;
            foundRaycastAll = Physics.RaycastAll(ray, dist, gameInstance.GetBuildLayerMask());
            foreach (var hit in foundRaycastAll)
            {
                if (Vector3.Distance(hit.point, CharacterTransform.position) > gameInstance.buildDistance)
                    return false;

                var buildingArea = hit.collider.GetComponent<BuildingArea>();
                if (buildingArea == null || (buildingArea.buildingEntity != null && buildingArea.buildingEntity == currentBuildingEntity))
                    continue;

                if (currentBuildingEntity.buildingType.Equals(buildingArea.buildingType))
                {
                    currentBuildingEntity.CacheTransform.position = GetBuildingPlacePosition(hit.point);
                    currentBuildingEntity.buildingArea = buildingArea;
                    if (buildingArea.snapBuildingObject)
                        return true;
                    nonSnapBuildingArea = buildingArea;
                }
            }
            if (nonSnapBuildingArea != null)
                return true;
            return false;
        }

        public bool TryGetAttackingCharacter(out BaseCharacterEntity character)
        {
            character = null;
            if (PlayerCharacterEntity.TryGetTargetEntity(out character))
            {
                if (character.CanReceiveDamageFrom(PlayerCharacterEntity))
                    return true;
                else
                    character = null;
            }
            return false;
        }

        public bool GetAttackDistanceAndFov(out float attackDistance, out float attackFov)
        {
            attackDistance = PlayerCharacterEntity.GetAttackDistance();
            attackFov = PlayerCharacterEntity.GetAttackFov();
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                var characterSkill = PlayerCharacterEntity.Skills[queueUsingSkillValue.skillIndex];
                var skill = characterSkill.GetSkill();
                if (skill != null)
                {
                    if (skill.IsAttack())
                    {
                        attackDistance = PlayerCharacterEntity.GetSkillAttackDistance(skill);
                        attackFov = PlayerCharacterEntity.GetSkillAttackFov(skill);
                    }
                    else
                    {
                        // Stop movement to use non attack skill
                        PlayerCharacterEntity.StopMove();
                        RequestUsePendingSkill();
                        return false;
                    }
                }
                else
                    queueUsingSkill = null;
            }
            return true;
        }

        public bool FindTarget(Transform target, float actDistance, int layerMask)
        {
            foundOverlapSphere = Physics.OverlapSphere(CharacterTransform.position, actDistance, layerMask);
            foreach (var collider in foundOverlapSphere)
            {
                if (collider.transform == target)
                    return true;
            }
            return false;
        }

        public bool IsLockTarget()
        {
            return controllerMode == PlayerCharacterControllerMode.Both ||
                controllerMode == PlayerCharacterControllerMode.PointClick ||
                (controllerMode == PlayerCharacterControllerMode.WASD && wasdLockAttackTarget);
        }
    }
}
