using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        buildingObject = null;
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

        if (CharacterEntity.CurrentHp <= 0)
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
                StopAllMovement();
                return;
            }
        }

        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.updateRotation = InputManager.GetButton("CameraRotate");

        if (CharacterEntity.CurrentHp <= 0)
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
        // If it's building something, don't allow to activate NPC/Warp/Pickup Item
        if (buildingObject != null)
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
        UpdateBuildingObject();
    }

    protected void UpdatePointClickInput()
    {
        // If it's building something, not allow point click movement
        if (buildingObject != null)
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
            CharacterEntity.SetTargetEntity(null);
            LiteNetLibIdentity targetIdentity = null;
            Vector3? targetPosition = null;
            var layerMask = 0;
            if (gameInstance.nonTargetingLayers.Length > 0)
            {
                foreach (var nonTargetingLayer in gameInstance.nonTargetingLayers)
                {
                    layerMask = layerMask | ~(nonTargetingLayer.Mask);
                }
            }
            else
                layerMask = -1;
            RaycastHit[] hits = Physics.RaycastAll(targetCamera.ScreenPointToRay(Input.mousePosition), 100f, layerMask);
            foreach (var hit in hits)
            {
                var hitTransform = hit.transform;
                targetPosition = hit.point;
                var playerEntity = hitTransform.GetComponent<PlayerCharacterEntity>();
                var monsterEntity = hitTransform.GetComponent<MonsterCharacterEntity>();
                var npcEntity = hitTransform.GetComponent<NpcEntity>();
                var itemDropEntity = hitTransform.GetComponent<ItemDropEntity>();
                CharacterEntity.SetTargetEntity(null);
                if (playerEntity != null && playerEntity.CurrentHp > 0)
                {
                    targetPosition = playerEntity.CacheTransform.position;
                    targetIdentity = playerEntity.Identity;
                    CharacterEntity.SetTargetEntity(playerEntity);
                    break;
                }
                else if (monsterEntity != null && monsterEntity.CurrentHp > 0)
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
                    CharacterEntity.PointClickMovement(targetPosition.Value);
                }
            }
        }
    }

    protected void UpdateWASDInput()
    {
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

        if (queueUsingSkill.HasValue)
        {
            var queueUsingSkillValue = queueUsingSkill.Value;
            StopAllMovement();
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
        else if (InputManager.GetButton("Attack"))
        {
            StopAllMovement();
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

    protected void UpdateBuildingObject()
    {
        var uiBuilding = CacheUISceneGameplay.uiBuilding;
        if (uiBuilding != null)
        {
            if (uiBuilding.IsVisible() && buildingObject == null)
                uiBuilding.Hide();
            if (!uiBuilding.IsVisible() && buildingObject != null)
                uiBuilding.Show();
        }

        if (buildingObject == null)
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
        if (TryGetAttackingCharacter(out targetEnemy))
        {
            if (targetEnemy.CurrentHp <= 0)
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
            var attackDistance = CharacterEntity.GetAttackDistance();
            var attackFov = CharacterEntity.GetAttackFov();
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
                        return;
                    }
                }
                else
                    queueUsingSkill = null;
            }
            var actDistance = attackDistance;
            actDistance -= actDistance * 0.1f;
            actDistance -= StoppingDistance;
            actDistance += targetEnemy.CacheCapsuleCollider.radius;
            if (Vector3.Distance(CharacterTransform.position, targetEnemy.CacheTransform.position) <= actDistance)
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
            if (targetPlayer.CurrentHp <= 0)
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
                if (tempEntity == null || tempEntity.CurrentHp <= 0)
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

        buildingItemIndex = -1;
        if (buildingObject != null)
        {
            Destroy(buildingObject.gameObject);
            buildingObject = null;
        }

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
                else if ((controllerMode == PlayerCharacterControllerMode.WASD || controllerMode == PlayerCharacterControllerMode.Both) || skill.IsAttack())
                    queueUsingSkill = new UsingSkillData(CharacterTransform.position, skillIndex);
                else if (CharacterEntity.Skills[skillIndex].CanUse(CharacterEntity))
                {
                    StopAllMovement();
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
                    buildingItemIndex = itemIndex;
                    buildingObject = Instantiate(item.buildingObject);
                    buildingObject.SetupAsBuildMode();
                    buildingObject.CacheTransform.parent = null;
                    SetBuildingObjectByCharacterTransform();
                    StopAllMovement();
                }
            }
        }
    }

    private void SetBuildingObjectByCharacterTransform()
    {
        if (buildingObject != null)
        {
            var placePosition = CharacterEntity.CacheTransform.position + (CharacterEntity.CacheTransform.forward * buildingObject.characterForwardDistance);
            buildingObject.CacheTransform.eulerAngles = GetBuildingPlaceEulerAngles(CharacterEntity.CacheTransform.eulerAngles);
            buildingObject.buildingArea = null;
            if (!RaycastToSetBuildingArea(new Ray(placePosition + (Vector3.up * 2.5f), Vector3.down), 5f))
                buildingObject.CacheTransform.position = GetBuildingPlacePosition(placePosition);
        }
    }

    private int GetBuildRaycastLayerMask()
    {
        var layerMask = 0;
        if (gameInstance.nonTargetingLayers.Length > 0)
        {
            foreach (var nonTargetingLayer in gameInstance.nonTargetingLayers)
            {
                layerMask = layerMask | ~(nonTargetingLayer.Mask);
            }
        }
        else
            layerMask = -1;
        layerMask = layerMask | ~(gameInstance.characterLayer.Mask);
        layerMask = layerMask | ~(gameInstance.itemDropLayer.Mask);
        return layerMask;
    }

    private Vector3 GetBuildingPlacePosition(Vector3 position)
    {
        if (buildGridSnap)
            position = new Vector3(Mathf.Round(position.x / buildGridSize) * buildGridSize, position.y, Mathf.Round(position.z / buildGridSize) * buildGridSize);
        return position;
    }

    private Vector3 GetBuildingPlaceEulerAngles(Vector3 eulerAngles)
    {
        if (buildRotationSnap)
        {
            eulerAngles.x = 0;
            eulerAngles.z = 0;
            // Uncomment this to make Y rotation set to 0, 90, 180
            eulerAngles.x = Mathf.Round(eulerAngles.x / 90) * 90;
            eulerAngles.y = Mathf.Round(eulerAngles.y / 90) * 90;
            eulerAngles.z = Mathf.Round(eulerAngles.z / 90) * 90;
        }
        return eulerAngles;
    }

    private bool RaycastToSetBuildingArea(Ray ray, float dist = 5f)
    {
        var layerMask = GetBuildRaycastLayerMask();
        RaycastHit[] hits = Physics.RaycastAll(ray, dist, layerMask);
        foreach (var hit in hits)
        {
            if (Vector3.Distance(hit.point, CharacterTransform.position) > gameInstance.buildDistance)
                return false;

            var buildingArea = hit.collider.GetComponent<BuildingArea>();
            if (buildingArea == null)
                continue;

            if (buildingObject.buildingType.Equals(buildingArea.buildingType))
            {
                buildingObject.CacheTransform.position = GetBuildingPlacePosition(hit.point);
                buildingObject.buildingArea = buildingArea;
                return true;
            }
        }
        return false;
    }

    private void StopAllMovement()
    {
        destination = null;
        queueUsingSkill = null;
        CharacterEntity.StopMove();
        CancelBuild();
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
}
