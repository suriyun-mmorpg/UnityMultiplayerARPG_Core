using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
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
    protected LiteNetLibFunction<NetFieldVector3> netFuncPointClickMovement;
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
    #endregion

    #region Protected data
    protected Queue<Vector3> navPaths;
    protected Vector3 moveDirection;
    #endregion

    #region Cache components
    private CapsuleCollider cacheCapsuleCollider;
    public CapsuleCollider CacheCapsuleCollider
    {
        get
        {
            if (cacheCapsuleCollider == null)
                cacheCapsuleCollider = GetComponent<CapsuleCollider>();
            return cacheCapsuleCollider;
        }
    }

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

    public FollowCameraControls CacheFollowCameraControls { get; protected set; }
    public UISceneGameplay CacheUISceneGameplay { get; protected set; }
    #endregion

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (IsLocalClient)
        {
            OwningCharacter = this;
            CacheFollowCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            CacheFollowCameraControls.target = CacheTransform;
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
    }

    protected void FixedUpdate()
    {
        if (!IsServer)
            return;
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
                var currentPosition = transform.position;
                currentPosition = new Vector3(currentPosition.x, 0, currentPosition.z);
                moveDirection = (target - currentPosition).normalized;
                if (Vector3.Distance(target, currentPosition) < stoppingDistance)
                    navPaths.Dequeue();
            }
            else
            {
                navPaths = null;
                moveDirection = Vector3.zero;
            }
        }
    }

    protected virtual void UpdateInput()
    {
        if (!IsLocalClient)
            return;

        if (CacheFollowCameraControls != null)
            CacheFollowCameraControls.updateRotation = Input.GetMouseButton(1);

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
                PointClickMovement(hit.point);
        }
    }

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
        netFuncPointClickMovement = new LiteNetLibFunction<NetFieldVector3>(NetFuncPointClickMovementCallback);

        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
        RegisterNetFunction("AddAttribute", netFuncAddAttribute);
        RegisterNetFunction("AddSkill", netFuncAddSkill);
        RegisterNetFunction("PointClickMovement", netFuncPointClickMovement);
    }

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

    protected void NetFuncPointClickMovementCallback(NetFieldVector3 position)
    {
        NetFuncPointClickMovement(position);
    }

    protected void NetFuncPointClickMovement(Vector3 position)
    {
        var navPath = new NavMeshPath();
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

    public void PointClickMovement(Vector3 position)
    {
        CallNetFunction("PointClickMovement", FunctionReceivers.Server, position);
    }
    #endregion

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

    protected override void SetupModel(CharacterModel characterModel)
    {
        CacheCapsuleCollider.center = characterModel.center;
        CacheCapsuleCollider.radius = characterModel.radius;
        CacheCapsuleCollider.height = characterModel.height;
    }

    protected override Vector3 GetMovementVelocity()
    {
        return CacheRigidbody.velocity;
    }
}
