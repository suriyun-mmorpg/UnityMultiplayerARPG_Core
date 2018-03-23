using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using LiteNetLib;
using LiteNetLibHighLevel;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class PlayerCharacterEntity : CharacterEntity, IPlayerCharacterData
{
    public static PlayerCharacterEntity OwningCharacter { get; private set; }

    #region Sync data
    public SyncFieldString id = new SyncFieldString();
    public SyncFieldInt statPoint = new SyncFieldInt();
    public SyncFieldInt skillPoint = new SyncFieldInt();
    public SyncFieldInt gold = new SyncFieldInt();
    #endregion

    #region Net Functions
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncSwapOrMergeItem;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddAttribute;
    protected LiteNetLibFunction<NetFieldInt> netFuncAddSkill;
    protected LiteNetLibFunction<NetFieldVector3, NetFieldUInt> netFuncPointClickMovement;
    #endregion

    #region Interface implementation
    public string Id { get { return id; } set { id.Value = value; } }
    public int StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
    public int SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
    public int Gold { get { return gold.Value; } set { gold.Value = value; } }
    public string CurrentMapName { get; set; }
    public Vector3 CurrentPosition { get { return CacheTransform.position; } set { CacheTransform.position = value; } }
    public string RespawnMapName { get; set; }
    public Vector3 RespawnPosition { get; set; }
    public int LastUpdate { get; set; }
    #endregion

    #region Settings
    [Header("Movement AI")]
    [Range(0.01f, 1f)]
    public float stoppingDistance = 0.1f;
    [Header("Movement Settings")]
    public float groundingDistance = 0.1f;
    public float jumpHeight = 2f;
    public float gravityRate = 1f;
    public float angularSpeed = 120f;
    #endregion

    #region Protected data
    protected Queue<Vector3> navPaths;
    protected Vector3 moveDirection;
    protected bool isJumping;
    protected bool isGrounded;
    protected Vector3? destination;
    #endregion

    #region Cache components
    private Rigidbody cacheRigidbody;
    public Rigidbody CacheRigidbody
    {
        get
        {
            if (cacheRigidbody == null)
                cacheRigidbody = GetComponent<Rigidbody>();
            return cacheRigidbody;
        }
    }

    private LiteNetLibTransform cacheNetTransform;
    public LiteNetLibTransform CacheNetTransform
    {
        get
        {
            if (cacheNetTransform == null)
                cacheNetTransform = GetComponent<LiteNetLibTransform>();
            return cacheNetTransform;
        }
    }

    public FollowCameraControls CacheMinimapCameraControls { get; protected set; }
    public FollowCameraControls CacheGameplayCameraControls { get; protected set; }
    public GameObject CacheTargetObject { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }
    #endregion

    protected virtual void Awake()
    {
        CacheRigidbody.useGravity = false;
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.playerTag;
        gameObject.layer = gameInstance.playerLayer;
    }

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (IsLocalClient)
        {
            OwningCharacter = this;
            CacheMinimapCameraControls = Instantiate(gameInstance.minimapCameraPrefab);
            CacheMinimapCameraControls.target = CacheTransform;
            CacheGameplayCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            CacheGameplayCameraControls.target = CacheTransform;
            CacheTargetObject = Instantiate(gameInstance.targetObject);
            CacheTargetObject.gameObject.SetActive(false);
            CacheUISceneGameplay = Instantiate(gameInstance.uiSceneGameplayPrefab);
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateBuffs();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected override void Update()
    {
        base.Update();
        UpdateInput();

        if (destination.HasValue)
        {
            var destinationValue = destination.Value;
            CacheTargetObject.transform.position = destinationValue;
            if (Vector3.Distance(destinationValue, CacheTransform.position) < stoppingDistance + 0.5f)
                destination = null;
        }
        CacheTargetObject.gameObject.SetActive(destination.HasValue);
    }


    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!isGrounded && collision.impulse.y > 0)
            isGrounded = true;
    }

    protected virtual void OnCollisionStay(Collision collision)
    {
        if (!isGrounded && collision.impulse.y > 0)
            isGrounded = true;
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer)
            return;
        
        if (isGrounded)
        {
            Vector3 velocity = CacheRigidbody.velocity;

            if (moveDirection.magnitude != 0)
            {
                if (moveDirection.magnitude > 1)
                    moveDirection = moveDirection.normalized;

                var moveSpeed = this.GetStatsWithBuffs().moveSpeed;
                var targetVelocity = moveDirection * moveSpeed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -moveSpeed, moveSpeed);
                velocityChange.y = 0;
                velocityChange.z = Mathf.Clamp(velocityChange.z, -moveSpeed, moveSpeed);
                CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                // slerp to the desired rotation over time
                CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.deltaTime);
            }

            // Jump
            if (isJumping)
                CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);

            if (Mathf.Abs(velocity.y) > groundingDistance)
                isGrounded = false;
        }

        // We apply gravity manually for more tuning control
        CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
    }

    protected void LateUpdate()
    {
        if (!IsServer)
            return;

        if (navPaths != null)
        {
            if (navPaths.Count > 0)
            {
                var target = navPaths.Peek();
                target = new Vector3(target.x, 0, target.z);
                var currentPosition = CacheTransform.position;
                currentPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
                moveDirection = (target - currentPosition).normalized;
                if (Vector3.Distance(target, currentPosition) < stoppingDistance)
                    navPaths.Dequeue();
            }
            else
                ClearPaths();
        }
    }

    protected virtual void ClearPaths()
    {
        navPaths = null;
        moveDirection = Vector3.zero;
        CacheRigidbody.velocity = Vector3.zero;
    }

    protected virtual void UpdateInput()
    {
        if (!IsLocalClient)
            return;

        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.updateRotation = Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var gameInstance = GameInstance.Singleton;
            var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
            RaycastHit hit;
            if (Physics.Raycast(targetCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f))
            {
                var hitTransform = hit.transform;
                var hitLayer = hitTransform.gameObject.layer;
                var hitPoint = hit.point;
                if (hitLayer == gameInstance.playerLayer ||
                    hitLayer == gameInstance.npcLayer ||
                    hitLayer == gameInstance.itemDropLayer)
                {
                    var networkEntity = hitTransform.GetComponent<RpgNetworkEntity>();
                    destination = null;
                    PointClickMovement(hitPoint, networkEntity.ObjectId);
                }
                else
                {
                    destination = hitPoint;
                    PointClickMovement(hitPoint, 0);
                }
            }
        }
    }
    
    protected float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
    }

    protected override Vector3 GetMovementVelocity()
    {
        return CacheRigidbody.velocity;
    }

    protected override CharacterAction GetCharacterAction(CharacterEntity characterEntity)
    {
        return CharacterAction.None;
    }

    protected override bool IsAlly(CharacterEntity characterEntity)
    {
        // TOOD: Will be implement it later with party/guild system
        return false;
    }

    protected override bool IsEnemy(CharacterEntity characterEntity)
    {
        return true;
    }

    #region Setup functions
    protected override void SetupNetElements()
    {
        base.SetupNetElements();
        id.sendOptions = SendOptions.ReliableOrdered;
        id.forOwnerOnly = false;
        statPoint.sendOptions = SendOptions.ReliableOrdered;
        statPoint.forOwnerOnly = true;
        skillPoint.sendOptions = SendOptions.ReliableOrdered;
        skillPoint.forOwnerOnly = true;
        gold.sendOptions = SendOptions.ReliableOrdered;
        gold.forOwnerOnly = false;
    }

    public override void OnSetup()
    {
        base.OnSetup();

        netFuncSwapOrMergeItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncSwapOrMergeItemCallback);
        netFuncAddAttribute = new LiteNetLibFunction<NetFieldInt>(NetFuncAddAttributeCallback);
        netFuncAddSkill = new LiteNetLibFunction<NetFieldInt>(NetFuncAddSkillCallback);
        netFuncPointClickMovement = new LiteNetLibFunction<NetFieldVector3, NetFieldUInt>(NetFuncPointClickMovementCallback);

        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
        RegisterNetFunction("AddAttribute", netFuncAddAttribute);
        RegisterNetFunction("AddSkill", netFuncAddSkill);
        RegisterNetFunction("PointClickMovement", netFuncPointClickMovement);
    }
    #endregion

    #region Net functions callbacks
    protected void NetFuncSwapOrMergeItemCallback(NetFieldInt fromIndex, NetFieldInt toIndex)
    {
        NetFuncSwapOrMergeItem(fromIndex, toIndex);
    }

    protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0 || doingAction ||
            fromIndex < 0 || fromIndex > nonEquipItems.Count ||
            toIndex < 0 || toIndex > nonEquipItems.Count)
            return;

        var fromItem = nonEquipItems[fromIndex];
        var toItem = nonEquipItems[toIndex];
        if (!fromItem.IsValid() || !toItem.IsValid())
            return;

        if (fromItem.itemId.Equals(toItem.itemId) && !fromItem.IsFull() && !toItem.IsFull())
        {
            // Merge if same id and not full
            var maxStack = toItem.GetMaxStack();
            if (toItem.amount + fromItem.amount <= maxStack)
            {
                toItem.amount += fromItem.amount;
                nonEquipItems[fromIndex] = CharacterItem.Empty;
                nonEquipItems[toIndex] = toItem;
            }
            else
            {
                var remains = toItem.amount + fromItem.amount - maxStack;
                toItem.amount = maxStack;
                fromItem.amount = remains;
                nonEquipItems[fromIndex] = fromItem;
                nonEquipItems[toIndex] = toItem;
            }
        }
        else
        {
            // Swap
            nonEquipItems[fromIndex] = toItem;
            nonEquipItems[toIndex] = fromItem;
        }
    }

    protected void NetFuncAddAttributeCallback(NetFieldInt attributeIndex)
    {
        NetFuncAddAttribute(attributeIndex);
    }

    protected void NetFuncAddAttribute(int attributeIndex)
    {
        if (CurrentHp <= 0 || attributeIndex < 0 || attributeIndex >= attributes.Count)
            return;

        var attribute = attributes[attributeIndex];
        if (!attribute.CanIncrease(this))
            return;

        attribute.Increase(1);
        attributes[attributeIndex] = attribute;
    }

    protected void NetFuncAddSkillCallback(NetFieldInt skillIndex)
    {
        NetFuncAddSkill(skillIndex);
    }

    protected void NetFuncAddSkill(int skillIndex)
    {
        if (CurrentHp <= 0 || skillIndex < 0 || skillIndex >= skills.Count)
            return;

        var skill = skills[skillIndex];
        if (!skill.CanLevelUp(this))
            return;

        skill.LevelUp(1);
        skills[skillIndex] = skill;
    }

    protected void NetFuncPointClickMovementCallback(NetFieldVector3 position, NetFieldUInt entityId)
    {
        NetFuncPointClickMovement(position, entityId);
    }

    protected void NetFuncPointClickMovement(Vector3 position, uint entityId)
    {
        var navPath = new NavMeshPath();
        LiteNetLibIdentity identity;
        if (entityId > 0 && Manager.Assets.SpawnedObjects.TryGetValue(entityId, out identity))
        {
            var entity = identity.GetComponent<RpgNetworkEntity>();
            if (entity != null)
            {
                position = entity.CacheTransform.position;
                var gameInstance = GameInstance.Singleton;
                var layer = entity.gameObject.layer;
                if (layer == gameInstance.playerLayer)
                {
                    var playerEntity = entity as PlayerCharacterEntity;
                    if (playerEntity != null && playerEntity.CurrentHp > 0)
                        SetTargetEntity(playerEntity);
                }
                else if (layer == gameInstance.npcLayer)
                {
                    var npcEntity = entity as NonPlayerCharacterEntity;
                    if (npcEntity != null && npcEntity.CurrentHp > 0)
                        SetTargetEntity(npcEntity);
                }
                else if (layer == gameInstance.itemDropLayer)
                {
                    var itemDropEntity = entity as ItemDropEntity;
                    if (itemDropEntity != null)
                        SetTargetEntity(itemDropEntity);
                }
            }
        }
        if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
        {
            navPaths = new Queue<Vector3>(navPath.corners);
            // Dequeue first path it's not require for future movement
            navPaths.Dequeue();
        }
    }
    #endregion

    #region Net functions callers
    public void SwapOrMergeItem(int fromIndex, int toIndex)
    {
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }

    public void AddAttribute(int attributeIndex)
    {
        CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex);
    }

    public void AddSkill(int skillIndex)
    {
        CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex);
    }

    public void PointClickMovement(Vector3 position, uint entityId)
    {
        CallNetFunction("PointClickMovement", FunctionReceivers.Server, position, entityId);
    }
    #endregion

    #region Sync data changes callback
    protected override void OnClassIdChange(string classId)
    {
        base.OnClassIdChange(classId);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected override void OnChangeEquipWeapons(EquipWeapons equipWeapons)
    {
        base.OnChangeEquipWeapons(equipWeapons);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected override void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnAttributesOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected override void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnSkillsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
        }
    }

    protected override void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnBuffsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateBuffs();
        }
    }

    protected override void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnEquipItemsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected override void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnNonEquipItemsOperation(operation, index);

        if (IsLocalClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }
    #endregion

    public void Warp(string mapName, Vector3 position)
    {
        if (!IsServer)
            return;

        // If warping to same map player does not have to reload new map data
        if (string.IsNullOrEmpty(mapName) || mapName.Equals(CurrentMapName))
        {
            CurrentPosition = position;
            return;
        }
    }
}
