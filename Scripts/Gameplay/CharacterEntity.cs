using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibHighLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : LiteNetLibBehaviour, ICharacterData
{
    public const float UPDATE_CURRENT_MAP_INTERVAL = 1f;
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
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();
    
    #region Protected data
    // Entity data
    protected CharacterModel model;
    protected bool isSetup;
    protected float lastUpdateCurrentMapTime;
    protected GameMapEntity currentMapEntity;
    // Save data
    protected string currentMapName;
    #endregion

    public string Id { get { return id; } set { id.Value = value; } }
    public string CharacterName { get { return characterName; } set { characterName.Value = value; } }
    public string PrototypeId
    {
        get { return prototypeId; }
        set
        {
            prototypeId.Value = value;
            // Setup model
            if (model != null)
                Destroy(model.gameObject);
            model = this.InstantiateModel(transform);
            // Wake up rigidbody
            TempRigidbody.WakeUp();
        }
    }
    public int Level { get { return level.Value; } set { level.Value = value; } }
    public int Exp { get { return exp.Value; } set { exp.Value = value; } }
    public int CurrentHp { get { return (int)currentHp.Value; } set { currentHp.Value = value; } }
    public int CurrentMp { get { return (int)currentMp.Value; } set { currentMp.Value = value; } }
    public int StatPoint { get { return statPoint.Value; } set { statPoint.Value = value; } }
    public int SkillPoint { get { return skillPoint.Value; } set { skillPoint.Value = value; } }
    public int Gold { get { return gold.Value; } set { gold.Value = value; } }
    public string CurrentMapName
    {
        get { return currentMapName; }
        set
        {
            if (isSetup && IsServer && !currentMapName.Equals(value))
                Identity.RebuildSubscribers(false);
            currentMapName = value;
        }
    }
    public Vector3 CurrentPosition { get; set; }
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
            nonEquipItems.Clear();
            foreach (var entry in value)
                nonEquipItems.Add(entry);
        }
    }

    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
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

    private BaseRpgNetworkManager tempManager;
    public BaseRpgNetworkManager TempManager
    {
        get
        {
            if (tempManager == null)
                tempManager = Manager as BaseRpgNetworkManager;
            return tempManager;
        }
    }

    public virtual Vector3 WorldPosition
    {
        get { return TempTransform.position; }
        set { TempTransform.position = value; }
    }

    protected virtual void Awake()
    {
        TempCharacterMovement.enabled = false;
        TempRigidbody.Sleep();

        prototypeId.onChange += OnPrototypeIdChange;
    }

    protected virtual void Update()
    {
        if (!isSetup)
            return;

        if (IsServer)
        {
            if (Time.realtimeSinceStartup - lastUpdateCurrentMapTime >= UPDATE_CURRENT_MAP_INTERVAL)
            {
                ConvertWorldToSavePosition();
                lastUpdateCurrentMapTime = Time.realtimeSinceStartup;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        prototypeId.onChange -= OnPrototypeIdChange;
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
        if (IsLocalClient)
            TempCharacterMovement.enabled = true;

        if (IsServer)
        {
            ConvertSaveToWorldPosition();
            lastUpdateCurrentMapTime = Time.realtimeSinceStartup;
        }
        isSetup = true;
    }

    protected void ConvertWorldToSavePosition()
    {
        // Convert world position to save position
        if (!currentMapEntity.IsInMap(WorldPosition))
        {
            currentMapEntity = TempManager.TempLoadGameMaps.GetMapByWorldPosition(WorldPosition);
            CurrentMapName = currentMapEntity.SceneName;
        }
        CurrentPosition = WorldPosition - currentMapEntity.MapOffsets;
    }

    protected void ConvertSaveToWorldPosition()
    {
        // Convert save position to world position
        if (TempManager.TempLoadGameMaps.LoadedMap.ContainsKey(CurrentMapName))
        {
            currentMapEntity = TempManager.TempLoadGameMaps.LoadedMap[CurrentMapName];
            WorldPosition = CurrentPosition + currentMapEntity.MapOffsets;
        }
        else
        {
            Debug.LogWarning("Cannot find character's map [" + CurrentMapName + "]");
            CurrentMapName = TempManager.TempLoadGameMaps.gameMaps[0].sceneName;
            currentMapEntity = TempManager.TempLoadGameMaps.LoadedMap[CurrentMapName];
            RaycastHit rayHit;
            if (Physics.Raycast(currentMapEntity.MapOffsets + (Vector3.up * currentMapEntity.MapBounds.size.y / 2), Vector3.down, out rayHit, currentMapEntity.MapBounds.size.y))
            {
                WorldPosition = rayHit.point;
                CurrentPosition = WorldPosition - currentMapEntity.MapOffsets;
            }
        }
    }

    public override bool ShouldAddSubscriber(LiteNetLibPlayer subscriber)
    {
        var spawnedObjects = subscriber.SpawnedObjects.Values;
        foreach (var spawnedObject in spawnedObjects)
        {
            var characterEntity = spawnedObject.GetComponent<CharacterEntity>();
            if (characterEntity == null)
                continue;
            // There are some characters that have same map?, if yes return true
            if (characterEntity.CurrentMapName.Equals(CurrentMapName))
                return true;
        }
        return false;
    }

    public override bool OnRebuildSubscribers(HashSet<LiteNetLibPlayer> subscribers, bool initialize)
    {
        var players = Manager.Players.Values;
        foreach (var subscriber in players)
        {
            var spawnedObjects = subscriber.SpawnedObjects.Values;
            foreach (var spawnedObject in spawnedObjects)
            {
                var characterEntity = spawnedObject.GetComponent<CharacterEntity>();
                if (characterEntity == null)
                    continue;
                // There are some characters that have same map?, if yes add to new subscribers list
                if (characterEntity != null && characterEntity.CurrentMapName.Equals(CurrentMapName))
                {
                    subscribers.Add(subscriber);
                    break;
                }
            }
        }
        return true;
    }

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

    #region SyncVar Hooks
    protected virtual void OnPrototypeIdChange(string prototypeId)
    {
        PrototypeId = prototypeId;
    }
    #endregion
}
