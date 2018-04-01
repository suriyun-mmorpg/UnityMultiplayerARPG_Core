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
    protected LiteNetLibFunction netFuncRespawn;
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
    protected bool pointClickMoveStopped;
    protected bool lookAtTargetUpdated;
    protected Vector3 oldFollowTargetPosition;
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

    protected override void Awake()
    {
        base.Awake();
        CacheRigidbody.useGravity = false;
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.playerTag;
        ClearDestination();
    }

    protected override void Start()
    {
        base.Start();
        var gameInstance = GameInstance.Singleton;
        if (IsOwnerClient)
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
            CacheUISceneGameplay.UpdateEquipItems();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentHp <= 0)
        {
            ClearDestination();
            SetTargetEntity(null);
            return;
        }

        if (destination.HasValue)
        {
            var destinationValue = destination.Value;
            if (CacheTargetObject != null)
                CacheTargetObject.transform.position = destinationValue;
            if (Vector3.Distance(destinationValue, CurrentPosition) < stoppingDistance + 0.5f)
                destination = null;
        }
        
        if (CacheTargetObject != null)
            CacheTargetObject.gameObject.SetActive(destination.HasValue);

        UpdateInput();
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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!IsServer && !IsOwnerClient)
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
                CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.fixedDeltaTime);
            }
            else if (navPaths == null)
            {
                CharacterEntity tempCharacterEntity;
                if (TryGetTargetEntity(out tempCharacterEntity))
                {
                    var targetDirection = (tempCharacterEntity.CacheTransform.position - CurrentPosition).normalized;
                    if (targetDirection.magnitude != 0f)
                        CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(targetDirection), angularSpeed * Time.fixedDeltaTime);
                }
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

    protected  void LateUpdate()
    {
        if (!IsServer && !IsOwnerClient)
            return;

        if (navPaths != null)
        {
            if (navPaths.Count > 0)
            {
                var target = navPaths.Peek();
                target = new Vector3(target.x, 0, target.z);
                var currentPosition = CurrentPosition;
                currentPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
                moveDirection = (target - currentPosition).normalized;
                if (Vector3.Distance(target, currentPosition) < stoppingDistance)
                    navPaths.Dequeue();
            }
            else
                ClearDestination();
        }
    }

    protected virtual void ClearDestination()
    {
        navPaths = null;
        moveDirection = Vector3.zero;
        CacheRigidbody.velocity = Vector3.zero;
        pointClickMoveStopped = true;
        lookAtTargetUpdated = false;
        destination = null;
    }

    protected virtual void UpdateInput()
    {
        if (!IsOwnerClient)
            return;

        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.updateRotation = Input.GetMouseButton(1);

        if (CurrentHp <= 0)
            return;

        var gameInstance = GameInstance.Singleton;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var targetCamera = CacheGameplayCameraControls != null ? CacheGameplayCameraControls.targetCamera : Camera.main;
            RaycastHit hit;
            if (Physics.Raycast(targetCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f))
            {
                SetTargetEntity(null);
                LiteNetLibIdentity targetIdentity = null;
                var hitTransform = hit.transform;
                var hitTag = hitTransform.gameObject.tag;
                var hitPoint = hit.point;
                if (hitTag.Equals(gameInstance.playerTag) ||
                    hitTag.Equals(gameInstance.monsterTag) ||
                    hitTag.Equals(gameInstance.npcTag) ||
                    hitTag.Equals(gameInstance.itemDropTag))
                {
                    destination = null;
                    var entity = hitTransform.GetComponent<RpgNetworkEntity>();
                    if (entity != null)
                    {
                        hitPoint = entity.CacheTransform.position;
                        if (hitTag.Equals(gameInstance.playerTag))
                        {
                            var playerEntity = entity as PlayerCharacterEntity;
                            if (playerEntity != null && playerEntity.CurrentHp > 0)
                            {
                                targetIdentity = playerEntity.Identity;
                                SetTargetEntity(playerEntity);
                            }
                        }
                        else if (hitTag.Equals(gameInstance.monsterTag))
                        {
                            var monsterEntity = entity as MonsterCharacterEntity;
                            if (monsterEntity != null && monsterEntity.CurrentHp > 0)
                            {
                                targetIdentity = monsterEntity.Identity;
                                SetTargetEntity(monsterEntity);
                            }
                        }
                        else if (hitTag.Equals(gameInstance.npcTag))
                        {
                            var npcEntity = entity as NpcEntity;
                            if (npcEntity != null)
                            {
                                targetIdentity = npcEntity.Identity;
                                SetTargetEntity(npcEntity);
                            }
                        }
                        else if (hitTag.Equals(gameInstance.itemDropTag))
                        {
                            var itemDropEntity = entity as ItemDropEntity;
                            if (itemDropEntity != null)
                            {
                                targetIdentity = itemDropEntity.Identity;
                                SetTargetEntity(itemDropEntity);
                            }
                        }
                    }
                }
                else
                    destination = hitPoint;
                lookAtTargetUpdated = false;
                PointClickMovement(hitPoint, targetIdentity);
            }
        }

        // Temp variables
        PlayerCharacterEntity targetPlayer;
        MonsterCharacterEntity targetMonster;
        NpcEntity targetNpc;
        ItemDropEntity targetItemDrop;
        if (TryGetTargetEntity(out targetPlayer))
        {
            if (targetPlayer.CurrentHp <= 0)
            {
                SetTargetEntity(null);
                StopPointClickMove(null);
                return;
            }
            var conversationDistance = gameInstance.conversationDistance;
            if (Vector3.Distance(CurrentPosition, targetPlayer.CacheTransform.position) <= conversationDistance)
            {
                UpdateLookAtTargetEntityPosition(targetPlayer);
                // TODO: do something
            }
            else
                UpdateTargetEntityPosition(targetPlayer);
        }
        else if (TryGetTargetEntity(out targetMonster))
        {
            if (targetMonster.CurrentHp <= 0)
            {
                SetTargetEntity(null);
                StopPointClickMove(null);
                return;
            }
            var attackDistance = GetAttackDistance();
            attackDistance -= attackDistance * 0.1f;
            attackDistance -= stoppingDistance;
            attackDistance += targetMonster.CacheCapsuleCollider.radius;
            if (Vector3.Distance(CurrentPosition, targetMonster.CacheTransform.position) <= attackDistance)
            {
                UpdateLookAtTargetEntityPosition(targetMonster);
                Attack();
            }
            else
                UpdateTargetEntityPosition(targetMonster);
        }
        else if (TryGetTargetEntity(out targetNpc))
        {
            var conversationDistance = gameInstance.conversationDistance;
            if (Vector3.Distance(CurrentPosition, targetNpc.CacheTransform.position) <= conversationDistance)
            {
                UpdateLookAtTargetEntityPosition(targetNpc);
                // TODO: implement npc conversation
            }
            else
                UpdateTargetEntityPosition(targetNpc);
        }
        else if (TryGetTargetEntity(out targetItemDrop))
        {
            var pickUpItemDistance = gameInstance.pickUpItemDistance;
            if (Vector3.Distance(CurrentPosition, targetItemDrop.CacheTransform.position) <= pickUpItemDistance)
            {
                UpdateLookAtTargetEntityPosition(targetItemDrop);
                PickupItem(targetItemDrop.ObjectId);
            }
            else
                UpdateTargetEntityPosition(targetItemDrop);
        }
    }

    protected void UpdateLookAtTargetEntityPosition(RpgNetworkEntity entity)
    {
        if (!lookAtTargetUpdated)
        {
            StopPointClickMove(entity == null ? null : entity.Identity);
            lookAtTargetUpdated = true;
        }
    }

    protected void UpdateTargetEntityPosition(RpgNetworkEntity entity)
    {
        if (entity == null)
            return;

        var targetPosition = entity.CacheTransform.position;
        if (oldFollowTargetPosition != targetPosition)
        {
            PointClickMovement(targetPosition, entity.Identity);
            oldFollowTargetPosition = targetPosition;
            lookAtTargetUpdated = false;
        }
    }
    
    protected float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
    }

    public override void Respawn()
    {
        if (CurrentHp > 0)
            return;
        base.Respawn();
        Warp(RespawnMapName, RespawnPosition);
    }

    protected override bool CanReceiveDamageFrom(CharacterEntity characterEntity)
    {
        // TODO: May implement this for party/guild battle purposes
        return characterEntity != null && characterEntity is MonsterCharacterEntity;
    }

    protected override bool IsAlly(CharacterEntity characterEntity)
    {
        // TODO: May implement this for party/guild battle purposes
        return false;
    }

    protected override bool IsEnemy(CharacterEntity characterEntity)
    {
        // TODO: May implement this for party/guild battle purposes
        return characterEntity != null && characterEntity is MonsterCharacterEntity;
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

        CacheNetTransform.ownerClientCanSendTransform = false;
        CacheNetTransform.ownerClientNotInterpolate = true;

        netFuncSwapOrMergeItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncSwapOrMergeItemCallback);
        netFuncAddAttribute = new LiteNetLibFunction<NetFieldInt>(NetFuncAddAttributeCallback);
        netFuncAddSkill = new LiteNetLibFunction<NetFieldInt>(NetFuncAddSkillCallback);
        netFuncPointClickMovement = new LiteNetLibFunction<NetFieldVector3, NetFieldUInt>(NetFuncPointClickMovementCallback);
        netFuncRespawn = new LiteNetLibFunction(NetFuncRespawnCallback);

        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
        RegisterNetFunction("AddAttribute", netFuncAddAttribute);
        RegisterNetFunction("AddSkill", netFuncAddSkill);
        RegisterNetFunction("PointClickMovement", netFuncPointClickMovement);
        RegisterNetFunction("Respawn", netFuncRespawn);
    }
    #endregion

    #region Net functions callbacks
    protected void NetFuncSwapOrMergeItemCallback(NetFieldInt fromIndex, NetFieldInt toIndex)
    {
        NetFuncSwapOrMergeItem(fromIndex, toIndex);
    }

    protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0 || 
            isDoingAction.Value ||
            fromIndex < 0 || 
            fromIndex > nonEquipItems.Count ||
            toIndex < 0 || 
            toIndex > nonEquipItems.Count)
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
        SetTargetEntity(null);
        LiteNetLibIdentity identity;
        if (Manager.Assets.SpawnedObjects.TryGetValue(entityId, out identity))
        {
            var entity = identity.GetComponent<RpgNetworkEntity>();
            SetTargetEntity(entity);
        }
        SetMovePaths(position);
    }

    protected void NetFuncRespawnCallback()
    {
        Respawn();
    }
    #endregion

    #region Net functions callers
    public void SwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }

    public void AddAttribute(int attributeIndex)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex);
    }

    public void AddSkill(int skillIndex)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex);
    }

    public void PointClickMovement(Vector3 position, LiteNetLibIdentity identity)
    {
        if (CurrentHp <= 0)
            return;
        if (!IsServer && CacheNetTransform.ownerClientNotInterpolate)
            SetMovePaths(position);
        pointClickMoveStopped = false;
        uint entityId = 0;
        if (identity != null)
            entityId = identity.ObjectId;
        CallNetFunction("PointClickMovement", FunctionReceivers.Server, position, entityId);
    }

    public void StopPointClickMove(LiteNetLibIdentity entity)
    {
        if (!pointClickMoveStopped)
            PointClickMovement(CurrentPosition, entity);
        pointClickMoveStopped = true;
    }

    public void RequestRespawn()
    {
        CallNetFunction("Respawn", FunctionReceivers.Server);
    }
    #endregion

    #region Sync data changes callback
    protected override void OnDatabaseIdChange(string databaseId)
    {
        base.OnDatabaseIdChange(databaseId);

        if (IsOwnerClient && CacheUISceneGameplay != null)
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

        if (IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected override void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnAttributesOperation(operation, index);

        if (IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected override void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnSkillsOperation(operation, index);

        if (IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateSkills();
        }
    }

    protected override void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnBuffsOperation(operation, index);

        if (IsOwnerClient && CacheUISceneGameplay != null)
            CacheUISceneGameplay.UpdateCharacter();
    }

    protected override void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnEquipItemsOperation(operation, index);

        if (IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateEquipItems();
        }
    }

    protected override void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        base.OnNonEquipItemsOperation(operation, index);

        if (IsOwnerClient && CacheUISceneGameplay != null)
        {
            CacheUISceneGameplay.UpdateCharacter();
            CacheUISceneGameplay.UpdateNonEquipItems();
        }
    }
    #endregion

    protected void SetMovePaths(Vector3 position)
    {
        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(CurrentPosition, position, NavMesh.AllAreas, navPath))
        {
            navPaths = new Queue<Vector3>(navPath.corners);
            // Dequeue first path it's not require for future movement
            navPaths.Dequeue();
        }
    }

    public void Warp(string mapName, Vector3 position)
    {
        if (!IsServer)
            return;

        // If warping to same map player does not have to reload new map data
        if (string.IsNullOrEmpty(mapName) || mapName.Equals(CurrentMapName))
        {
            CacheNetTransform.Teleport(position, Quaternion.identity);
            return;
        }
    }
}
