using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLib;
using LiteNetLibHighLevel;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class PlayerCharacterEntity : CharacterEntity, IPlayerCharacterData
{
    #region Sync data
    public SyncFieldInt statPoint = new SyncFieldInt();
    public SyncFieldInt skillPoint = new SyncFieldInt();
    public SyncFieldInt gold = new SyncFieldInt();
    // List
    public SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
    #endregion

    #region Sync data actions
    public System.Action<int> onStatPointChange;
    public System.Action<int> onSkillPointChange;
    public System.Action<int> onGoldChange;
    // List
    public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
    #endregion

    #region Interface implementation
    public int StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
    public int SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
    public int Gold { get { return gold.Value; } set { gold.Value = value; } }
    public string CurrentMapName { get; set; }
    public Vector3 CurrentPosition { get { return CacheTransform.position; } set { CacheTransform.position = value; } }
    public string RespawnMapName { get; set; }
    public Vector3 RespawnPosition { get; set; }
    public int LastUpdate { get; set; }

    public IList<CharacterHotkey> Hotkeys
    {
        get { return hotkeys; }
        set
        {
            hotkeys.Clear();
            foreach (var entry in value)
                hotkeys.Add(entry);
        }
    }
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
    #endregion

    protected override void Awake()
    {
        base.Awake();
        CacheRigidbody.useGravity = false;
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.playerTag;
        StopMove();
    }

    protected override void Update()
    {
        base.Update();

        if (CurrentHp <= 0)
        {
            StopMove();
            SetTargetEntity(null);
            return;
        }
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
            if (CurrentHp > 0)
            {
                var moveDirectionMagnitude = moveDirection.sqrMagnitude;
                if (!isDoingAction.Value && moveDirectionMagnitude != 0)
                {
                    if (moveDirectionMagnitude > 1)
                        moveDirection = moveDirection.normalized;

                    var moveSpeed = this.GetStatsWithBuffs().moveSpeed;
                    var targetVelocity = moveDirection * moveSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -moveSpeed, moveSpeed);
                    velocityChange.y = 0;
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -moveSpeed, moveSpeed);
                    CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
                    // Calculate rotation on client only, will send update to server later
                    CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.fixedDeltaTime);
                }

                CharacterEntity tempCharacterEntity;
                if (moveDirectionMagnitude == 0 && TryGetTargetEntity(out tempCharacterEntity))
                {
                    var targetDirection = (tempCharacterEntity.CacheTransform.position - CacheTransform.position).normalized;
                    if (targetDirection.sqrMagnitude != 0f)
                    {
                        var fromRotation = CacheTransform.rotation.eulerAngles;
                        var lookAtRotation = Quaternion.LookRotation(targetDirection).eulerAngles;
                        lookAtRotation = new Vector3(fromRotation.x, lookAtRotation.y, fromRotation.z);
                        CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.Euler(lookAtRotation), angularSpeed * Time.fixedDeltaTime);
                    }
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

    protected void LateUpdate()
    {
        if (!IsServer && !IsOwnerClient)
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
                StopMove();
        }
    }

    protected virtual void StopMove()
    {
        navPaths = null;
        moveDirection = Vector3.zero;
        CacheRigidbody.velocity = Vector3.zero;
    }

    protected float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
    }

    protected override void Respawn()
    {
        if (!IsServer || CurrentHp > 0)
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
        statPoint.sendOptions = SendOptions.ReliableOrdered;
        statPoint.forOwnerOnly = true;
        skillPoint.sendOptions = SendOptions.ReliableOrdered;
        skillPoint.forOwnerOnly = true;
        gold.sendOptions = SendOptions.ReliableOrdered;
        gold.forOwnerOnly = false;

        hotkeys.forOwnerOnly = true;
    }

    public override void OnSetup()
    {
        base.OnSetup();
        // Setup network components
        CacheNetTransform.ownerClientCanSendTransform = false;
        CacheNetTransform.ownerClientNotInterpolate = true;
        // On data changes events
        statPoint.onChange += OnStatPointChange;
        skillPoint.onChange += OnSkillPointChange;
        gold.onChange += OnGoldChange;
        // On list changes events
        hotkeys.onOperation += OnHotkeysOperation;
        // Register Network functions
        RegisterNetFunction("SwapOrMergeItem", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((fromIndex, toIndex) => NetFuncSwapOrMergeItem(fromIndex, toIndex)));
        RegisterNetFunction("AddAttribute", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((attributeIndex, amount) => NetFuncAddAttribute(attributeIndex, amount)));
        RegisterNetFunction("AddSkill", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((skillIndex, amount) => NetFuncAddSkill(skillIndex, amount)));
        RegisterNetFunction("PointClickMovement", new LiteNetLibFunction<NetFieldVector3>((position) => NetFuncPointClickMovement(position)));
        RegisterNetFunction("Respawn", new LiteNetLibFunction(NetFuncRespawn));
        RegisterNetFunction("AssignHotkey", new LiteNetLibFunction<NetFieldString, NetFieldByte, NetFieldString>((hotkeyId, type, dataId) => NetFuncAssignHotkey(hotkeyId, type, dataId)));
    }
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // On data changes events
        statPoint.onChange -= OnStatPointChange;
        skillPoint.onChange -= OnSkillPointChange;
        gold.onChange += OnGoldChange;
        // On list changes events
        hotkeys.onOperation -= OnHotkeysOperation;
    }

    #region Net functions callbacks
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

    protected void NetFuncAddAttribute(int attributeIndex, int amount)
    {
        if (CurrentHp <= 0 || attributeIndex < 0 || attributeIndex >= attributes.Count || amount <= 0 || amount > StatPoint)
            return;

        var attribute = attributes[attributeIndex];
        if (!attribute.CanIncrease(this))
            return;

        attribute.Increase(amount);
        attributes[attributeIndex] = attribute;

        StatPoint -= amount;
    }

    protected void NetFuncAddSkill(int skillIndex, int amount)
    {
        if (CurrentHp <= 0 || skillIndex < 0 || skillIndex >= skills.Count || amount <= 0 || amount > SkillPoint)
            return;

        var skill = skills[skillIndex];
        if (!skill.CanLevelUp(this))
            return;

        skill.LevelUp(amount);
        skills[skillIndex] = skill;

        SkillPoint -= amount;
    }

    protected void NetFuncPointClickMovement(Vector3 position)
    {
        if (CurrentHp <= 0)
            return;
        SetMovePaths(position);
    }

    protected void NetFuncRespawn()
    {
        Respawn();
    }

    protected void NetFuncAssignHotkey(string hotkeyId, byte type, string dataId)
    {
        var characterHotkey = new CharacterHotkey();
        characterHotkey.hotkeyId = hotkeyId;
        characterHotkey.type = (HotkeyTypes)type;
        characterHotkey.dataId = dataId;
        var hotkeyIndex = hotkeys.IndexOf(hotkeyId);
        if (hotkeyIndex >= 0)
            hotkeys[hotkeyIndex] = characterHotkey;
        else
            hotkeys.Add(characterHotkey);
    }
    #endregion

    #region Net functions callers
    public void RequestSwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }

    public void RequestAddAttribute(int attributeIndex, int amount)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("AddAttribute", FunctionReceivers.Server, attributeIndex, amount);
    }

    public void RequestAddSkill(int skillIndex, int amount)
    {
        if (CurrentHp <= 0)
            return;
        CallNetFunction("AddSkill", FunctionReceivers.Server, skillIndex, amount);
    }

    public void RequestPointClickMovement(Vector3 position)
    {
        if (CurrentHp <= 0)
            return;
        if (!IsServer && CacheNetTransform.ownerClientNotInterpolate)
            SetMovePaths(position);
        CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
    }

    public void RequestRespawn()
    {
        CallNetFunction("Respawn", FunctionReceivers.Server);
    }

    public void RequestAssignHotkey(string hotkeyId, HotkeyTypes type, string dataId)
    {
        CallNetFunction("AssignHotkey", FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
    }
    #endregion

    #region Sync data changes callback
    protected virtual void OnStatPointChange(int statPoint)
    {
        if (onStatPointChange != null)
            onStatPointChange.Invoke(statPoint);
    }

    protected virtual void OnSkillPointChange(int skillPoint)
    {
        if (onSkillPointChange != null)
            onSkillPointChange.Invoke(skillPoint);
    }

    protected virtual void OnGoldChange(int gold)
    {
        if (onGoldChange != null)
            onGoldChange.Invoke(gold);
    }

    protected virtual void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (onHotkeysOperation != null)
            onHotkeysOperation.Invoke(operation, index);
    }
    #endregion

    protected void SetMovePaths(Vector3 position)
    {
        var navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(CacheTransform.position, position, NavMesh.AllAreas, navPath))
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

    internal virtual void IncreaseGold(int gold)
    {
        if (!IsServer)
            return;
        Gold += gold;
    }
}
