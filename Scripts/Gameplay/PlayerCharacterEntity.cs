using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLib;
using LiteNetLibManager;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LiteNetLibTransform))]
public class PlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
{
    #region Sync data
    public SyncFieldInt statPoint = new SyncFieldInt();
    public SyncFieldInt skillPoint = new SyncFieldInt();
    public SyncFieldInt gold = new SyncFieldInt();
    // List
    public SyncListCharacterHotkey hotkeys = new SyncListCharacterHotkey();
    public SyncListCharacterQuest quests = new SyncListCharacterQuest();
    #endregion

    #region Sync data actions
    public System.Action<string> onShowNpcDialog;
    public System.Action<int> onStatPointChange;
    public System.Action<int> onSkillPointChange;
    public System.Action<int> onGoldChange;
    // List
    public System.Action<LiteNetLibSyncList.Operation, int> onHotkeysOperation;
    public System.Action<LiteNetLibSyncList.Operation, int> onQuestsOperation;
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

    public IList<CharacterQuest> Quests
    {
        get { return quests; }
        set
        {
            quests.Clear();
            foreach (var entry in value)
                quests.Add(entry);
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
    protected NpcDialog currentNpcDialog;
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

        var gameInstance = GameInstance.Singleton;
        if (isGrounded)
        {
            Vector3 velocity = CacheRigidbody.velocity;
            if (CurrentHp > 0)
            {
                var moveDirectionMagnitude = moveDirection.sqrMagnitude;
                if (!IsPlayingActionAnimation() && moveDirectionMagnitude != 0)
                {
                    if (moveDirectionMagnitude > 1)
                        moveDirection = moveDirection.normalized;

                    var moveSpeed = MoveSpeed * gameInstance.moveSpeedMultiplier;
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

                BaseCharacterEntity tempCharacterEntity;
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

                // Jump
                if (isJumping)
                {
                    CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                    isJumping = false;
                }
            }

            if (Mathf.Abs(velocity.y) > groundingDistance)
                isGrounded = false;
        }

        // We apply gravity manually for more tuning control
        CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

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

    public virtual void StopMove()
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

    protected override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
    {
        // TODO: May implement this for party/guild battle purposes
        return characterEntity != null && characterEntity is MonsterCharacterEntity;
    }

    protected override bool IsAlly(BaseCharacterEntity characterEntity)
    {
        // TODO: May implement this for party/guild battle purposes
        return false;
    }

    protected override bool IsEnemy(BaseCharacterEntity characterEntity)
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
        quests.forOwnerOnly = true;
    }

    public override void OnSetup()
    {
        base.OnSetup();
        // Setup network components
        CacheNetTransform.ownerClientCanSendTransform = true;
        CacheNetTransform.ownerClientNotInterpolate = false;
        // On data changes events
        statPoint.onChange += OnStatPointChange;
        skillPoint.onChange += OnSkillPointChange;
        gold.onChange += OnGoldChange;
        // On list changes events
        hotkeys.onOperation += OnHotkeysOperation;
        quests.onOperation += OnQuestsOperation;
        // Register Network functions
        RegisterNetFunction("SwapOrMergeItem", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((fromIndex, toIndex) => NetFuncSwapOrMergeItem(fromIndex, toIndex)));
        RegisterNetFunction("AddAttribute", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((attributeIndex, amount) => NetFuncAddAttribute(attributeIndex, amount)));
        RegisterNetFunction("AddSkill", new LiteNetLibFunction<NetFieldInt, NetFieldInt>((skillIndex, amount) => NetFuncAddSkill(skillIndex, amount)));
        RegisterNetFunction("Respawn", new LiteNetLibFunction(NetFuncRespawn));
        RegisterNetFunction("AssignHotkey", new LiteNetLibFunction<NetFieldString, NetFieldByte, NetFieldString>((hotkeyId, type, dataId) => NetFuncAssignHotkey(hotkeyId, type, dataId)));
        RegisterNetFunction("NpcActivate", new LiteNetLibFunction<NetFieldUInt>((objectId) => NetFuncNpcActivate(objectId)));
        RegisterNetFunction("ShowNpcDialog", new LiteNetLibFunction<NetFieldString>((npcDialogId) => NetFuncShowNpcDialog(npcDialogId)));
        RegisterNetFunction("SelectNpcDialogMenu", new LiteNetLibFunction<NetFieldInt>((menuIndex) => NetFuncSelectNpcDialogMenu(menuIndex)));
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
        quests.onOperation -= OnQuestsOperation;
    }

    #region Net functions callbacks
    protected void NetFuncSwapOrMergeItem(int fromIndex, int toIndex)
    {
        if (CurrentHp <= 0 ||
            IsPlayingActionAnimation() ||
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

    protected void NetFuncRespawn()
    {
        Respawn();
    }

    protected void NetFuncAssignHotkey(string hotkeyId, byte type, string dataId)
    {
        var characterHotkey = new CharacterHotkey();
        characterHotkey.hotkeyId = hotkeyId;
        characterHotkey.type = (HotkeyType)type;
        characterHotkey.dataId = dataId;
        var hotkeyIndex = this.IndexOfHotkey(hotkeyId);
        if (hotkeyIndex >= 0)
            hotkeys[hotkeyIndex] = characterHotkey;
        else
            hotkeys.Add(characterHotkey);
    }

    protected void NetFuncNpcActivate(uint objectId)
    {
        NpcEntity entity;
        if (!Manager.Assets.TryGetSpawnedObject(objectId, out entity))
            return;
        currentNpcDialog = entity.startDialog;
        if (currentNpcDialog != null)
            RequestShowNpcDialog(currentNpcDialog.Id);
    }

    protected void NetFuncShowNpcDialog(string npcDialogId)
    {
        if (onShowNpcDialog != null)
            onShowNpcDialog(npcDialogId);
    }

    protected void NetFuncSelectNpcDialogMenu(int menuIndex)
    {
        if (currentNpcDialog == null)
            return;
        var menus = currentNpcDialog.menus;
        NpcDialogMenu selectedMenu;
        switch (currentNpcDialog.type)
        {
            case NpcDialogType.Normal:
                if (menuIndex < 0 || menuIndex >= menus.Length)
                    return;
                selectedMenu = menus[menuIndex];
                if (!selectedMenu.IsPassConditions(this) || selectedMenu.dialog == null || selectedMenu.isCloseMenu)
                {
                    currentNpcDialog = null;
                    RequestShowNpcDialog("");
                    return;
                }
                currentNpcDialog = selectedMenu.dialog;
                RequestShowNpcDialog(currentNpcDialog.Id);
                break;
            case NpcDialogType.Quest:
                NetFuncSelectNpcDialogQuestMenu(menuIndex);
                break;
        }
    }

    protected void NetFuncSelectNpcDialogQuestMenu(int menuIndex)
    {
        if (currentNpcDialog == null || currentNpcDialog.type != NpcDialogType.Quest || currentNpcDialog.quest == null)
        {
            currentNpcDialog = null;
            RequestShowNpcDialog("");
            return;
        }
        switch (menuIndex)
        {
            case NpcDialog.QUEST_ACCEPT_MENU_INDEX:
                NetFuncAcceptQuest(currentNpcDialog.quest.Id);
                currentNpcDialog = currentNpcDialog.questAcceptedDialog;
                break;
            case NpcDialog.QUEST_DECLINE_MENU_INDEX:
                currentNpcDialog = currentNpcDialog.questDeclinedDialog;
                break;
            case NpcDialog.QUEST_ABANDON_MENU_INDEX:
                NetFuncAbandonQuest(currentNpcDialog.quest.Id);
                currentNpcDialog = currentNpcDialog.questAbandonedDialog;
                break;
            case NpcDialog.QUEST_COMPLETE_MENU_INDEX:
                NetFuncCompleteQuest(currentNpcDialog.quest.Id);
                currentNpcDialog = currentNpcDialog.questCompletedDailog;
                break;
        }
        if (currentNpcDialog == null)
            RequestShowNpcDialog("");
        else
            RequestShowNpcDialog(currentNpcDialog.Id);
    }

    protected void NetFuncAcceptQuest(string questId)
    {
        var indexOfQuest = this.IndexOfQuest(questId);
        Quest quest;
        if (indexOfQuest >= 0 || !GameInstance.Quests.TryGetValue(questId, out quest))
            return;
        var characterQuest = CharacterQuest.Create(quest);
        quests.Add(characterQuest);
    }

    protected void NetFuncAbandonQuest(string questId)
    {
        var indexOfQuest = this.IndexOfQuest(questId);
        Quest quest;
        if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questId, out quest))
            return;
        var characterQuest = quests[indexOfQuest];
        if (characterQuest.isComplete)
            return;
        quests.RemoveAt(indexOfQuest);
    }

    protected void NetFuncCompleteQuest(string questId)
    {
        var indexOfQuest = this.IndexOfQuest(questId);
        Quest quest;
        if (indexOfQuest < 0 || !GameInstance.Quests.TryGetValue(questId, out quest))
            return;
        var characterQuest = quests[indexOfQuest];
        if (!characterQuest.IsAllTasksDone(this))
            return;
        if (characterQuest.isComplete)
            return;
        IncreaseExp(quest.rewardExp);
        IncreaseGold(quest.rewardGold);
        var rewardItems = quest.rewardItems;
        if (rewardItems != null && rewardItems.Length > 0)
        {
            foreach (var rewardItem in rewardItems)
            {
                if (rewardItem.item != null && rewardItem.amount > 0)
                    this.IncreaseItems(rewardItem.item.Id, 1, rewardItem.amount);
            }
        }
        characterQuest.isComplete = true;
        if (!quest.canRepeat)
            quests[indexOfQuest] = characterQuest;
        else
            quests.RemoveAt(indexOfQuest);
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

    public void RequestRespawn()
    {
        CallNetFunction("Respawn", FunctionReceivers.Server);
    }

    public void RequestAssignHotkey(string hotkeyId, HotkeyType type, string dataId)
    {
        CallNetFunction("AssignHotkey", FunctionReceivers.Server, hotkeyId, (byte)type, dataId);
    }

    public void RequestNpcActivate(uint objectId)
    {
        CallNetFunction("NpcActivate", FunctionReceivers.Server, objectId);
    }

    public void RequestShowNpcDialog(string npcDialogId)
    {
        CallNetFunction("ShowNpcDialog", ConnectId, npcDialogId);
    }

    public void RequestSelectNpcDialogMenu(int menuIndex)
    {
        CallNetFunction("SelectNpcDialogMenu", FunctionReceivers.Server, menuIndex);
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

    protected virtual void OnQuestsOperation(LiteNetLibSyncList.Operation operation, int index)
    {
        if (onQuestsOperation != null)
            onQuestsOperation.Invoke(operation, index);
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

    public void KeyMovement(Vector3 direction, bool isJump)
    {
        if (CurrentHp <= 0)
            return;
        moveDirection = direction;
        if (!isJumping)
            isJumping = isGrounded && isJump;
    }

    public void PointClickMovement(Vector3 position)
    {
        if (CurrentHp <= 0)
            return;
        SetMovePaths(position);
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

    protected override void Killed(BaseCharacterEntity lastAttacker)
    {
        base.Killed(lastAttacker);
        currentNpcDialog = null;
    }

    public virtual void IncreaseGold(int gold)
    {
        if (!IsServer)
            return;
        Gold += gold;
    }

    public virtual void OnKillMonster(MonsterCharacterEntity monsterCharacterEntity)
    {
        if (!IsServer || monsterCharacterEntity == null)
            return;

        for (var i = 0; i < Quests.Count; ++i)
        {
            var quest = Quests[i];
            if (quest.AddKillMonster(monsterCharacterEntity, 1))
                quests[i] = quest;
        }
    }
}
