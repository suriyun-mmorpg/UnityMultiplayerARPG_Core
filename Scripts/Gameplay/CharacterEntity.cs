using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : RpgNetworkEntity, ICharacterData
{
    public const float UPDATE_SKILL_BUFF_INTERVAL = 1f;
    // Use id as primary key
    [Header("Sync Fields")]
    public SyncFieldString id = new SyncFieldString();
    public SyncFieldString characterName = new SyncFieldString();
    public SyncFieldString prototypeId = new SyncFieldString();
    public SyncFieldInt level = new SyncFieldInt();
    public SyncFieldInt exp = new SyncFieldInt();
    public SyncFieldFloat currentHp = new SyncFieldFloat();
    public SyncFieldFloat currentMp = new SyncFieldFloat();
    public SyncFieldInt statPoint = new SyncFieldInt();
    public SyncFieldInt skillPoint = new SyncFieldInt();
    public SyncFieldInt gold = new SyncFieldInt();

    [Header("Sync Lists")]
    public SyncListCharacterAttributeLevel attributeLevels = new SyncListCharacterAttributeLevel();
    public SyncListCharacterSkillLevel skillLevels = new SyncListCharacterSkillLevel();
    public SyncListCharacterBuff buffs = new SyncListCharacterBuff();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
    
    #region Protected data
    // Entity data
    protected CharacterModel model;
    protected float lastUpdateSkillAndBuffTime = 0f;
    // Net Functions
    protected LiteNetLibFunction<NetFieldUInt> netFuncPickupItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncDropItem;
    protected LiteNetLibFunction<NetFieldInt, NetFieldInt> netFuncSwapOrMergeItem;
    #endregion

    public string Id { get { return id; } set { id.Value = value; } }
    public string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public string PrototypeId { get { return prototypeId; } set { prototypeId.Value = value; } }
    public int Level { get { return level.Value; } set { level.Value = value; } }
    public int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public int CurrentHp { get { return (int)currentHp.Value; } set { currentHp.Value = value; } }
    public int CurrentMp { get { return (int)currentMp.Value; } set { currentMp.Value = value; } }
    public int StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
    public int SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
    public int Gold { get { return gold.Value; } set { gold.Value = value; } }
    public string CurrentMapName { get; set; }
    public Vector3 CurrentPosition { get { return TempTransform.position; } set { TempTransform.position = value; } }
    public string RespawnMapName { get; set; }
    public Vector3 RespawnPosition { get; set; }
    public int LastUpdate { get; set; }

    public IList<CharacterAttributeLevel> AttributeLevels
    {
        get { return attributeLevels; }
        set
        {
            attributeLevels.Clear();
            foreach (var entry in value)
                attributeLevels.Add(entry);
        }
    }
    public IList<CharacterSkillLevel> SkillLevels
    {
        get { return skillLevels; }
        set
        {
            skillLevels.Clear();
            foreach (var entry in value)
                skillLevels.Add(entry);
        }
    }
    public IList<CharacterBuff> Buffs
    {
        get { return buffs; }
        set
        {
            buffs.Clear();
            foreach (var entry in value)
                buffs.Add(entry);
        }
    }
    public IList<CharacterItem> EquipItems
    {
        get { return equipItems; }
        set
        {
            equipItems.Clear();
            foreach (var entry in value)
                equipItems.Add(entry);
        }
    }
    public IList<CharacterItem> NonEquipItems
    {
        get { return nonEquipItems; }
        set
        {
            var gameInstance = GameInstance.Singleton;
            nonEquipItems.Clear();
            // Adjust inventory size
            var countItem = 0;
            foreach (var entry in value)
            {
                if (countItem < gameInstance.inventorySize)
                    nonEquipItems.Add(entry);
                ++countItem;
            }
            for (var i = countItem; i < gameInstance.inventorySize; ++i)
            {
                nonEquipItems.Add(new CharacterItem());
            }
        }
    }

    #region Temp components
    private CapsuleCollider tempCapsuleCollider;
    public CapsuleCollider TempCapsuleCollider
    {
        get
        {
            if (tempCapsuleCollider == null)
                tempCapsuleCollider = GetComponent<CapsuleCollider>();
            return tempCapsuleCollider;
        }
    }

    private Rigidbody tempRigidbody;
    public Rigidbody TempRigidbody
    {
        get
        {
            if (tempRigidbody == null)
                tempRigidbody = GetComponent<Rigidbody>();
            return tempRigidbody;
        }
    }

    private CharacterMovement tempCharacterMovement;
    public CharacterMovement TempCharacterMovement
    {
        get
        {
            if (tempCharacterMovement == null)
                tempCharacterMovement = GetComponent<CharacterMovement>();
            return tempCharacterMovement;
        }
    }

    public FollowCameraControls TempFollowCameraControls { get; protected set; }
    #endregion

    protected virtual void Awake()
    {
        TempCharacterMovement.enabled = false;
    }

    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        if (IsLocalClient)
        {
            TempCharacterMovement.enabled = true;
            TempFollowCameraControls = Instantiate(gameInstance.gameplayCameraPrefab);
            TempFollowCameraControls.target = TempTransform;
        }
    }

    protected virtual void Update()
    {
        // Use this to update animations
        if (CurrentHp > 0)
            UpdateSkillAndBuff();
    }

    protected void UpdateSkillAndBuff()
    {
        var timeDiff = Time.realtimeSinceStartup - lastUpdateSkillAndBuffTime;
        var count = skillLevels.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var skillLevel = skillLevels[i];
            if (skillLevel.ShouldUpdate())
            {
                skillLevel.Update(Time.unscaledDeltaTime);
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    skillLevels.Dirty(i);
            }
        }
        count = buffs.Count;
        for (var i = count - 1; i >= 0; --i)
        {
            var buff = buffs[i];
            if (buff.ShouldRemove())
                buffs.RemoveAt(i);
            else
            {
                buff.Update(Time.unscaledDeltaTime);
                if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
                    buffs.Dirty(i);
            }
        }
        if (timeDiff > UPDATE_SKILL_BUFF_INTERVAL)
            lastUpdateSkillAndBuffTime = Time.realtimeSinceStartup;
    }

    public override void OnBehaviourValidate()
    {
#if UNITY_EDITOR
        SetupNetElements();
        EditorUtility.SetDirty(this);
#endif
    }

    public override void OnSetup()
    {
        SetupNetElements();
        prototypeId.onChange += OnPrototypeIdChange;
        netFuncPickupItem = new LiteNetLibFunction<NetFieldUInt>(NetFuncPickupItemCallback);
        netFuncDropItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncDropItemCallback);
        netFuncSwapOrMergeItem = new LiteNetLibFunction<NetFieldInt, NetFieldInt>(NetFuncSwapOrMergeItemCallback);
        RegisterNetFunction("PickupItem", netFuncPickupItem);
        RegisterNetFunction("DropItem", netFuncDropItem);
        RegisterNetFunction("SwapOrMergeItem", netFuncSwapOrMergeItem);
    }

    #region Net functions callbacks
    protected void NetFuncPickupItemCallback(NetFieldUInt objectId)
    {
        var gameInstance = GameInstance.Singleton;
        var spawnedObjects = Manager.Assets.SpawnedObjects;
        // Find object by objectId, if not found don't continue
        if (!Manager.Assets.SpawnedObjects.ContainsKey(objectId))
            return;
        var spawnedObject = spawnedObjects[objectId];
        // Don't pickup item if it's too far
        if (Vector3.Distance(TempTransform.position, spawnedObject.transform.position) >= gameInstance.pickUpItemDistance)
            return;
        var itemDropEntity = spawnedObject.GetComponent<ItemDropEntity>();
        var itemDropData = itemDropEntity.dropData;
        if (!itemDropData.IsValid)
        {
            // Destroy item drop entity without item add because this is not valid
            Manager.Assets.NetworkDestroy(objectId);
            return;
        }
        var itemId = itemDropData.itemId;
        var level = itemDropData.level;
        var amount = itemDropData.amount;
        if (IncreaseItems(itemId, level, amount))
            Manager.Assets.NetworkDestroy(objectId);
    }

    protected void NetFuncDropItemCallback(NetFieldInt index, NetFieldInt amount)
    {
        var gameInstance = GameInstance.Singleton;
        if (index < 0 || index > nonEquipItems.Count)
            return;
        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid || amount > nonEquipItem.amount)
            return;
        var itemId = nonEquipItem.itemId;
        var level = nonEquipItem.level;
        if (DecreaseItems(index, amount))
        {
            var dropPosition = TempTransform.position + new Vector3(Random.value * gameInstance.dropDistance, 0, Random.value * gameInstance.dropDistance);
            var identity = Manager.Assets.NetworkSpawn(gameInstance.itemDropEntityPrefab.gameObject, dropPosition);
            var itemDropEntity = identity.GetComponent<ItemDropEntity>();
            var dropData = new CharacterItem();
            dropData.itemId = itemId;
            dropData.level = level;
            dropData.amount = amount;
            itemDropEntity.dropData = dropData;
        }
    }

    protected void NetFuncSwapOrMergeItemCallback(NetFieldInt fromIndex, NetFieldInt toIndex)
    {
        if (fromIndex < 0 || fromIndex > nonEquipItems.Count ||
            toIndex < 0 || toIndex > nonEquipItems.Count)
            return;
        var fromItem = nonEquipItems[fromIndex];
        var toItem = nonEquipItems[toIndex];
        if (!fromItem.IsValid || !toItem.IsValid)
            return;
        if (fromItem.itemId.Equals(toItem.itemId) && !fromItem.IsFull && !toItem.IsFull)
        {
            // Merge if same id and not full
            var maxStack = toItem.MaxStack;
            if (toItem.amount + fromItem.amount <= maxStack)
            {
                toItem.amount += fromItem.amount;
                fromItem.Empty();
                nonEquipItems[fromIndex] = fromItem;
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
            nonEquipItems.Dirty(fromIndex);
            nonEquipItems.Dirty(toIndex);
        }
    }
    #endregion

    #region Net functions callers
    public void PickupItem(uint objectId)
    {
        CallNetFunction("PickupItem", FunctionReceivers.Server, objectId);
    }

    public void DropItem(int index, int amount)
    {
        CallNetFunction("DropItem", FunctionReceivers.Server, index, amount);
    }

    public void SwapOrMergeItem(int fromIndex, int toIndex)
    {
        CallNetFunction("SwapOrMergeItem", FunctionReceivers.Server, fromIndex, toIndex);
    }
    #endregion

    #region Inventory helpers
    public bool IncreaseItems(string itemId, int level, int amount)
    {
        // If item not valid
        if (string.IsNullOrEmpty(itemId) || amount <= 0 || !GameInstance.Items.ContainsKey(itemId))
            return false;
        var maxStack = GameInstance.Items[itemId].maxStack;
        var emptySlots = new Dictionary<int, CharacterItem>();
        var changes = new Dictionary<int, CharacterItem>();
        // Loop to all slots to add amount to any slots that item amount not max in stack
        for (var i = 0; i < nonEquipItems.Count; ++i)
        {
            var nonEquipItem = nonEquipItems[i];
            if (!nonEquipItem.IsValid)
                emptySlots[i] = nonEquipItem;
            else if (nonEquipItem.itemId.Equals(itemId))
            {
                if (nonEquipItem.amount + amount <= maxStack)
                {
                    nonEquipItem.amount += amount;
                    changes[i] = nonEquipItem;
                    amount = 0;
                    break;
                }
                else if (maxStack - nonEquipItem.amount > 0)
                {
                    amount = maxStack - nonEquipItem.amount;
                    nonEquipItem.amount = amount;
                    changes[i] = nonEquipItem;
                }
            }
        }
        if (changes.Count == 0 && emptySlots.Count > 0)
        {
            foreach (var emptySlot in emptySlots)
            {
                var value = emptySlot.Value;
                var newItem = new CharacterItem();
                newItem.id = System.Guid.NewGuid().ToString();
                newItem.itemId = itemId;
                var addAmount = 0;
                if (amount - maxStack >= 0)
                {
                    addAmount = maxStack;
                    amount -= maxStack;
                }
                else
                {
                    addAmount = amount;
                    amount = 0;
                }
                newItem.amount = addAmount;
                changes[emptySlot.Key] = newItem;
            }
        }
        // Cannot add all items
        if (amount > 0)
            return false;
        // Apply all changes
        foreach (var change in changes)
        {
            nonEquipItems[change.Key] = change.Value;
            nonEquipItems.Dirty(change.Key);
        }
        return true;
    }
    
    public bool DecreaseItems(int index, int amount)
    {
        if (index < 0 || index > nonEquipItems.Count)
            return false;
        var nonEquipItem = nonEquipItems[index];
        if (!nonEquipItem.IsValid || amount > nonEquipItem.amount)
            return false;
        if (nonEquipItem.amount - amount == 0)
            nonEquipItems.RemoveAt(index);
        else
        {
            nonEquipItem.amount -= amount;
            nonEquipItems[index] = nonEquipItem;
            nonEquipItems.Dirty(index);
        }
        return true;
    }
    #endregion

    private void SetupNetElements()
    {
        id.sendOptions = SendOptions.ReliableOrdered;
        characterName.sendOptions = SendOptions.ReliableOrdered;
        prototypeId.sendOptions = SendOptions.ReliableOrdered;
        level.sendOptions = SendOptions.ReliableOrdered;
        exp.sendOptions = SendOptions.ReliableOrdered;
        currentHp.sendOptions = SendOptions.ReliableOrdered;
        currentMp.sendOptions = SendOptions.ReliableOrdered;
        statPoint.sendOptions = SendOptions.ReliableOrdered;
        statPoint.forOwnerOnly = true;
        skillPoint.sendOptions = SendOptions.ReliableOrdered;
        skillPoint.forOwnerOnly = true;
        gold.sendOptions = SendOptions.ReliableOrdered;
        skillLevels.forOwnerOnly = true;
        nonEquipItems.forOwnerOnly = true;
    }

    protected void OnPrototypeIdChange(string prototypeId)
    {
        // Setup model
        if (model != null)
            Destroy(model.gameObject);
        model = this.InstantiateModel(transform);
        if (model != null)
        {
            TempCapsuleCollider.center = model.center;
            TempCapsuleCollider.radius = model.radius;
            TempCapsuleCollider.height = model.height;
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

    protected virtual void OnDestroy()
    {
        prototypeId.onChange -= OnPrototypeIdChange;
    }
}
