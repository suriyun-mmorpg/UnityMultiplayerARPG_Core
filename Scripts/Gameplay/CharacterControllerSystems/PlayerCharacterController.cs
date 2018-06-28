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
        public bool wasdLockAttackTarget;
        public float lockAttackTargetDistance = 10f;
        public FollowCameraControls gameplayCameraPrefab;
        public GameObject targetObjectPrefab;
        [Header("Building Settings")]
        public bool buildGridSnap;
        public float buildGridSize = 4f;
        public bool buildRotationSnap;
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
        public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
        public GameObject CacheTargetObject { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            buildingItemIndex = -1;
            currentBuildingObject = null;
        }

        protected override void Start()
        {
            base.Start();

            // Set parent transform to root for the best performance
            if (gameplayCameraPrefab != null)
            {
                CacheGameplayCameraControls = Instantiate(gameplayCameraPrefab);
                CacheGameplayCameraControls.target = CharacterTransform;
            }
            // Set parent transform to root for the best performance
            if (targetObjectPrefab != null)
            {
                CacheTargetObject = Instantiate(targetObjectPrefab);
                CacheTargetObject.SetActive(false);
            }
        }

        protected override void Update()
        {
            if (CharacterEntity == null || !CharacterEntity.IsOwnerClient)
                return;

            base.Update();

            if (CacheTargetObject != null)
                CacheTargetObject.gameObject.SetActive(destination.HasValue);

            if (CharacterEntity.IsDead())
            {
                queueUsingSkill = null;
                destination = null;
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetCharacter(null);
                CancelBuild();
            }
            else
            {
                BaseCharacterEntity targetCharacter = null;
                CharacterEntity.TryGetTargetEntity(out targetCharacter);
                if (CacheUISceneGameplay != null)
                    CacheUISceneGameplay.SetTargetCharacter(targetCharacter);
            }

            if (destination.HasValue)
            {
                var destinationValue = destination.Value;
                if (CacheTargetObject != null)
                    CacheTargetObject.transform.position = destinationValue;
                if (Vector3.Distance(destinationValue, CharacterTransform.position) < StoppingDistance + 0.5f)
                    destination = null;
            }

            UpdateInput();
            UpdateFollowTarget();
        }

        protected virtual void UpdateInput()
        {
            var fields = FindObjectsOfType<InputField>();
            foreach (var field in fields)
            {
                if (field.isFocused)
                {
                    destination = null;
                    CharacterEntity.StopMove();
                    return;
                }
            }

            if (CacheGameplayCameraControls != null)
                CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");

            if (CharacterEntity.IsDead())
                return;

            // If it's building something, don't allow to activate NPC/Warp/Pickup Item
            if (currentBuildingObject != null)
            {
                // Activate nearby npcs
                if (InputManager.GetButtonDown("Activate"))
                {
                    var foundEntities = Physics.OverlapSphere(CharacterTransform.position, gameInstance.conversationDistance, gameInstance.characterLayer.Mask);
                    foreach (var foundEntity in foundEntities)
                    {
                        var npcEntity = foundEntity.GetComponent<NpcEntity>();
                        if (npcEntity != null)
                        {
                            CharacterEntity.RequestNpcActivate(npcEntity.ObjectId);
                            break;
                        }
                    }
                    if (foundEntities.Length == 0)
                        CharacterEntity.RequestEnterWarp();
                }
                // Pick up nearby items
                if (InputManager.GetButtonDown("PickUpItem"))
                {
                    var foundEntities = Physics.OverlapSphere(CharacterTransform.position, gameInstance.pickUpItemDistance, gameInstance.itemDropLayer.Mask);
                    foreach (var foundEntity in foundEntities)
                    {
                        var itemDropEntity = foundEntity.GetComponent<ItemDropEntity>();
                        if (itemDropEntity != null)
                        {
                            CharacterEntity.RequestPickupItem(itemDropEntity.ObjectId);
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
            if (currentBuildingObject != null)
                return;
            var getMouseDown = Input.GetMouseButtonDown(0);
            var getMouseUp = Input.GetMouseButtonUp(0);
            var getMouse = Input.GetMouseButton(0);
            if (getMouseDown)
            {
                isMouseDragOrHoldOrOverUI = false;
                mouseDownTime = Time.unscaledTime;
                mouseDownPosition = Input.mousePosition;
            }
            var isPointerOverUI = CacheUISceneGameplay != null && CacheUISceneGameplay.IsPointerOverUIObject();
            var isMouseDragDetected = (Input.mousePosition - mouseDownPosition).magnitude > DETECT_MOUSE_DRAG_DISTANCE;
            var isMouseHoldDetected = Time.unscaledTime - mouseDownTime > DETECT_MOUSE_HOLD_DURATION;
            if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
                isMouseDragOrHoldOrOverUI = true;
            if (!isPointerOverUI && (getMouse || getMouseUp))
            {
                var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
                CharacterEntity.SetTargetEntity(null);
                LiteNetLibIdentity targetIdentity = null;
                Vector3? targetPosition = null;
                var layerMask = gameInstance.GetTargetLayerMask();
                var hits = Physics.RaycastAll(targetCamera.ScreenPointToRay(Input.mousePosition), 100f, layerMask);
                foreach (var hit in hits)
                {
                    var hitTransform = hit.transform;
                    // When clicking on target
                    if (getMouseUp &&
                        !isMouseDragOrHoldOrOverUI &&
                        (controllerMode == PlayerCharacterControllerMode.PointClick || controllerMode == PlayerCharacterControllerMode.Both))
                    {
                        var playerEntity = hitTransform.GetComponent<PlayerCharacterEntity>();
                        var monsterEntity = hitTransform.GetComponent<MonsterCharacterEntity>();
                        var npcEntity = hitTransform.GetComponent<NpcEntity>();
                        var itemDropEntity = hitTransform.GetComponent<ItemDropEntity>();
                        var harvestableEntity = hitTransform.GetComponent<HarvestableEntity>();
                        targetPosition = hit.point;
                        CharacterEntity.SetTargetEntity(null);
                        if (playerEntity != null && !playerEntity.IsDead())
                        {
                            targetPosition = playerEntity.CacheTransform.position;
                            targetIdentity = playerEntity.Identity;
                            CharacterEntity.SetTargetEntity(playerEntity);
                            break;
                        }
                        else if (monsterEntity != null && !monsterEntity.IsDead())
                        {
                            targetPosition = monsterEntity.CacheTransform.position;
                            targetIdentity = monsterEntity.Identity;
                            CharacterEntity.SetTargetEntity(monsterEntity);
                            break;
                        }
                        else if (npcEntity != null)
                        {
                            targetPosition = npcEntity.CacheTransform.position;
                            targetIdentity = npcEntity.Identity;
                            CharacterEntity.SetTargetEntity(npcEntity);
                            break;
                        }
                        else if (itemDropEntity != null)
                        {
                            targetPosition = itemDropEntity.CacheTransform.position;
                            targetIdentity = itemDropEntity.Identity;
                            CharacterEntity.SetTargetEntity(itemDropEntity);
                            break;
                        }
                        else if (harvestableEntity != null && !harvestableEntity.IsDead())
                        {
                            targetPosition = harvestableEntity.CacheTransform.position;
                            targetIdentity = harvestableEntity.Identity;
                            CharacterEntity.SetTargetEntity(harvestableEntity);
                            break;
                        }
                    }
                    // When holding on target
                    else if (!isMouseDragDetected && isMouseHoldDetected)
                    {
                        var buildingMaterial = hitTransform.GetComponent<BuildingMaterial>();
                        CharacterEntity.SetTargetEntity(null);
                        if (buildingMaterial != null && buildingMaterial.buildingEntity != null && !buildingMaterial.buildingEntity.IsDead())
                        {
                            targetPosition = buildingMaterial.buildingEntity.CacheTransform.position;
                            targetIdentity = buildingMaterial.buildingEntity.Identity;
                            CharacterEntity.SetTargetEntity(buildingMaterial.buildingEntity);
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
                        CharacterEntity.PointClickMovement(targetPosition.Value);
                    }
                }
            }
        }

        protected void UpdateWASDInput()
        {
            if (controllerMode != PlayerCharacterControllerMode.WASD &&
                controllerMode != PlayerCharacterControllerMode.Both)
                return;

            if (CharacterEntity.IsPlayingActionAnimation())
            {
                CharacterEntity.StopMove();
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
                CharacterEntity.StopMove();
                var characterSkill = CharacterEntity.Skills[queueUsingSkillValue.skillIndex];
                var skill = characterSkill.GetSkill();
                if (skill != null)
                {
                    if (skill.IsAttack())
                    {
                        BaseCharacterEntity targetEntity;
                        if (wasdLockAttackTarget && !CharacterEntity.TryGetTargetEntity(out targetEntity))
                        {
                            var nearestTarget = FindNearestAliveCharacter<MonsterCharacterEntity>(CharacterEntity.GetSkillAttackDistance(skill) + lockAttackTargetDistance);
                            if (nearestTarget != null)
                                CharacterEntity.SetTargetEntity(nearestTarget);
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
                CharacterEntity.StopMove();
                BaseCharacterEntity targetEntity;
                if (wasdLockAttackTarget && !CharacterEntity.TryGetTargetEntity(out targetEntity))
                {
                    var nearestTarget = FindNearestAliveCharacter<MonsterCharacterEntity>(CharacterEntity.GetAttackDistance() + lockAttackTargetDistance);
                    if (nearestTarget != null)
                        CharacterEntity.SetTargetEntity(nearestTarget);
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
                    if (CharacterEntity.HasNavPaths)
                        CharacterEntity.StopMove();
                    destination = null;
                    CharacterEntity.SetTargetEntity(null);
                }
                CharacterEntity.KeyMovement(moveDirection, jumpInput);
            }
        }

        protected void UpdateBuilding()
        {
            // Current building UI
            BuildingEntity currentBuilding;
            var uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
            if (uiCurrentBuilding != null)
            {
                if (uiCurrentBuilding.IsVisible() && !CharacterEntity.TryGetTargetEntity(out currentBuilding))
                    uiCurrentBuilding.Hide();
            }

            // Construct building UI
            var uiConstructBuilding = CacheUISceneGameplay.uiConstructBuilding;
            if (uiConstructBuilding != null)
            {
                if (uiConstructBuilding.IsVisible() && currentBuildingObject == null)
                    uiConstructBuilding.Hide();
                if (!uiConstructBuilding.IsVisible() && currentBuildingObject != null)
                    uiConstructBuilding.Show();
            }

            if (currentBuildingObject == null)
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
            BaseCharacterEntity targetEnemy;
            PlayerCharacterEntity targetPlayer;
            NpcEntity targetNpc;
            ItemDropEntity targetItemDrop;
            BuildingEntity targetBuilding;
            HarvestableEntity targetHarvestable;
            if (TryGetAttackingCharacter(out targetEnemy))
            {
                if (targetEnemy.IsDead())
                {
                    queueUsingSkill = null;
                    CharacterEntity.SetTargetEntity(null);
                    CharacterEntity.StopMove();
                    return;
                }

                if (CharacterEntity.IsPlayingActionAnimation())
                {
                    CharacterEntity.StopMove();
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
                if (RaycastToTarget(targetEnemy.CacheTransform, actDistance, gameInstance.characterLayer.Mask))
                {
                    // Stop movement to attack
                    CharacterEntity.StopMove();
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
            else if (CharacterEntity.TryGetTargetEntity(out targetPlayer))
            {
                if (targetPlayer.IsDead())
                {
                    queueUsingSkill = null;
                    CharacterEntity.SetTargetEntity(null);
                    CharacterEntity.StopMove();
                    return;
                }
                var actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetPlayer.CacheTransform.position) <= actDistance)
                {
                    CharacterEntity.StopMove();
                    // TODO: do something
                }
                else
                    UpdateTargetEntityPosition(targetPlayer);
            }
            else if (CharacterEntity.TryGetTargetEntity(out targetNpc))
            {
                var actDistance = gameInstance.conversationDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetNpc.CacheTransform.position) <= actDistance)
                {
                    CharacterEntity.RequestNpcActivate(targetNpc.ObjectId);
                    CharacterEntity.StopMove();
                    CharacterEntity.SetTargetEntity(null);
                }
                else
                    UpdateTargetEntityPosition(targetNpc);
            }
            else if (CharacterEntity.TryGetTargetEntity(out targetItemDrop))
            {
                var actDistance = gameInstance.pickUpItemDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetItemDrop.CacheTransform.position) <= actDistance)
                {
                    CharacterEntity.RequestPickupItem(targetItemDrop.ObjectId);
                    CharacterEntity.StopMove();
                    CharacterEntity.SetTargetEntity(null);
                }
                else
                    UpdateTargetEntityPosition(targetItemDrop);
            }
            else if (CharacterEntity.TryGetTargetEntity(out targetBuilding))
            {
                var uiCurrentBuilding = CacheUISceneGameplay.uiCurrentBuilding;
                var actDistance = gameInstance.buildDistance - StoppingDistance;
                if (Vector3.Distance(CharacterTransform.position, targetBuilding.CacheTransform.position) <= actDistance)
                {
                    if (uiCurrentBuilding != null && !uiCurrentBuilding.IsVisible())
                        uiCurrentBuilding.Show();
                    CharacterEntity.StopMove();
                }
                else
                {
                    UpdateTargetEntityPosition(targetBuilding);
                    if (uiCurrentBuilding != null && uiCurrentBuilding.IsVisible())
                        uiCurrentBuilding.Hide();
                }
            }
            else if (CharacterEntity.TryGetTargetEntity(out targetHarvestable))
            {
                if (targetHarvestable.IsDead())
                {
                    queueUsingSkill = null;
                    CharacterEntity.SetTargetEntity(null);
                    CharacterEntity.StopMove();
                    return;
                }

                var attackDistance = 0f;
                var attackFov = 0f;
                if (!GetAttackDistanceAndFov(out attackDistance, out attackFov))
                    return;
                var actDistance = attackDistance;
                actDistance -= actDistance * 0.1f;
                actDistance -= StoppingDistance;
                if (RaycastToTarget(targetHarvestable.CacheTransform, actDistance, gameInstance.harvestableLayer.Mask))
                {
                    // Stop movement to attack
                    CharacterEntity.StopMove();
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
            CharacterEntity.PointClickMovement(targetPosition);
        }

        protected T FindNearestAliveCharacter<T>(float distance) where T : BaseCharacterEntity
        {
            T result = null;
            var colliders = Physics.OverlapSphere(CharacterTransform.position, distance, gameInstance.characterLayer.Mask);
            if (colliders != null && colliders.Length > 0)
            {
                float tempDistance;
                T tempEntity;
                float nearestDistance = float.MaxValue;
                T nearestEntity = null;
                foreach (var collider in colliders)
                {
                    tempEntity = collider.GetComponent<T>();
                    if (tempEntity == null || tempEntity.IsDead())
                        continue;

                    tempDistance = Vector3.Distance(CharacterTransform.position, tempEntity.CacheTransform.position);
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
            CharacterEntity.RequestAttack();
        }

        public void RequestUseSkill(Vector3 position, int skillIndex)
        {
            CharacterEntity.RequestUseSkill(position, skillIndex);
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

        public void RequestEquipItem(int itemIndex)
        {
            CharacterEntity.RequestEquipItem(itemIndex);
        }

        public void RequestUseItem(int itemIndex)
        {
            CharacterEntity.RequestUseItem(itemIndex);
        }

        public override void UseHotkey(int hotkeyIndex)
        {
            if (hotkeyIndex < 0 || hotkeyIndex >= CharacterEntity.Hotkeys.Count)
                return;

            CancelBuild();
            buildingItemIndex = -1;
            currentBuildingObject = null;
            
            var hotkey = CharacterEntity.Hotkeys[hotkeyIndex];
            var skill = hotkey.GetSkill();
            if (skill != null)
            {
                var skillIndex = CharacterEntity.IndexOfSkill(skill.DataId);
                if (skillIndex >= 0)
                {
                    BaseCharacterEntity attackingCharacter;
                    if (TryGetAttackingCharacter(out attackingCharacter))
                        queueUsingSkill = new UsingSkillData(CharacterTransform.position, skillIndex);
                    else if (CharacterEntity.Skills[skillIndex].CanUse(CharacterEntity))
                    {
                        destination = null;
                        CharacterEntity.StopMove();
                        RequestUseSkill(CharacterTransform.position, skillIndex);
                    }
                }
            }
            var item = hotkey.GetItem();
            if (item != null)
            {
                var itemIndex = CharacterEntity.IndexOfNonEquipItem(item.DataId);
                if (itemIndex >= 0)
                {
                    if (item.IsEquipment())
                        RequestEquipItem(itemIndex);
                    else if (item.IsPotion())
                        RequestUseItem(itemIndex);
                    else if (item.IsBuilding())
                    {
                        destination = null;
                        CharacterEntity.StopMove();
                        buildingItemIndex = itemIndex;
                        currentBuildingObject = Instantiate(item.buildingObject);
                        currentBuildingObject.SetupAsBuildMode();
                        currentBuildingObject.CacheTransform.parent = null;
                        SetBuildingObjectByCharacterTransform();
                    }
                }
            }
        }

        private void SetBuildingObjectByCharacterTransform()
        {
            if (currentBuildingObject != null)
            {
                var placePosition = CharacterEntity.CacheTransform.position + (CharacterEntity.CacheTransform.forward * currentBuildingObject.characterForwardDistance);
                currentBuildingObject.CacheTransform.eulerAngles = GetBuildingPlaceEulerAngles(CharacterEntity.CacheTransform.eulerAngles);
                currentBuildingObject.buildingArea = null;
                if (!RaycastToSetBuildingArea(new Ray(placePosition + (Vector3.up * 2.5f), Vector3.down), 5f))
                    currentBuildingObject.CacheTransform.position = GetBuildingPlacePosition(placePosition);
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
            var layerMask = gameInstance.GetBuildLayerMask();
            BuildingArea nonSnapBuildingArea = null;
            RaycastHit[] hits = Physics.RaycastAll(ray, dist, layerMask);
            foreach (var hit in hits)
            {
                if (Vector3.Distance(hit.point, CharacterTransform.position) > gameInstance.buildDistance)
                    return false;

                var buildingArea = hit.collider.GetComponent<BuildingArea>();
                if (buildingArea == null || (buildingArea.buildingObject != null && buildingArea.buildingObject == currentBuildingObject))
                    continue;

                if (currentBuildingObject.buildingType.Equals(buildingArea.buildingType))
                {
                    currentBuildingObject.CacheTransform.position = GetBuildingPlacePosition(hit.point);
                    currentBuildingObject.buildingArea = buildingArea;
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
            if (CharacterEntity.TryGetTargetEntity(out character))
            {
                // TODO: Returning Pvp characters
                if (character is MonsterCharacterEntity)
                    return true;
                else
                    character = null;
            }
            return false;
        }

        public bool GetAttackDistanceAndFov(out float attackDistance, out float attackFov)
        {
            attackDistance = CharacterEntity.GetAttackDistance();
            attackFov = CharacterEntity.GetAttackFov();
            if (queueUsingSkill.HasValue)
            {
                var queueUsingSkillValue = queueUsingSkill.Value;
                var characterSkill = CharacterEntity.Skills[queueUsingSkillValue.skillIndex];
                var skill = characterSkill.GetSkill();
                if (skill != null)
                {
                    if (skill.IsAttack())
                    {
                        attackDistance = CharacterEntity.GetSkillAttackDistance(skill);
                        attackFov = CharacterEntity.GetSkillAttackFov(skill);
                    }
                    else
                    {
                        // Stop movement to use non attack skill
                        CharacterEntity.StopMove();
                        RequestUsePendingSkill();
                        return false;
                    }
                }
                else
                    queueUsingSkill = null;
            }
            return true;
        }

        public bool RaycastToTarget(Transform target, float actDistance, int layerMask)
        {
            var targetPosition = target.position;
            var characterPosition = CharacterEntity.CacheTransform.position;
            var heading = targetPosition - characterPosition;
            var distance = heading.magnitude;
            var direction = heading / distance;
            RaycastHit hitInfo;
            return Physics.Raycast(characterPosition, direction, out hitInfo, actDistance, layerMask) && hitInfo.transform == target;
        }
    }
}
