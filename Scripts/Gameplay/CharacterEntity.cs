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
            TempCapsuleCollider.center = model.center;
            TempCapsuleCollider.radius = model.radius;
            TempCapsuleCollider.height = model.height;
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
            if (IsServer && (string.IsNullOrEmpty(currentMapName) || !currentMapName.Equals(value)))
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

    public virtual Vector3 WorldPosition
    {
        get { return TempTransform.position; }
        set { TempTransform.position = value; }
    }

    protected virtual void Awake()
    {
        TempCharacterMovement.enabled = false;
    }

    protected virtual void Start()
    {
        if (IsServer)
        {
            var loadedMaps = TempManager.TempLoadGameMaps.LoadedMaps;
            // Setup current map and position
            if (loadedMaps.ContainsKey(CurrentMapName))
            {
                currentMapEntity = loadedMaps[CurrentMapName];
                WorldPosition = currentMapEntity.ConvertLocalPositionToWorld(CurrentPosition);
            }
            else
            {
                Debug.LogWarning("Cannot find character's map [" + CurrentMapName + "]");
                // If no map found try to spawn character at any maps
                var gameMapValues = new List<GameMapEntity>(loadedMaps.Values);
                CurrentMapName = gameMapValues[Random.Range(0, gameMapValues.Count - 1)].MapName;
                currentMapEntity = loadedMaps[CurrentMapName];
                RaycastHit rayHit;
                if (Physics.Raycast(currentMapEntity.MapOffsets + (Vector3.up * currentMapEntity.MapExtents.y), Vector3.down, out rayHit, currentMapEntity.MapExtents.y * 2))
                {
                    WorldPosition = rayHit.point;
                    CurrentPosition = currentMapEntity.ConvertWorldPositionToLocal(WorldPosition);
                }
            }
        }

        if (IsLocalClient)
            TempCharacterMovement.enabled = true;
    }

    protected virtual void Update()
    {
        // Use this to update animations
    }

    protected virtual void FixedUpdate()
    {
        if (IsServer)
            CurrentPosition = currentMapEntity.ConvertWorldPositionToLocal(WorldPosition);
    }

    protected virtual void OnDestroy()
    {
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
    }

    public void Warp(string mapName, Vector3 position)
    {
        if (!IsServer)
            return;

        var loadedMaps = TempManager.TempLoadGameMaps.LoadedMaps;
        if (!loadedMaps.ContainsKey(mapName))
        {
            Debug.LogWarning("Cannot warp character to " + mapName + ", map not found.");
            return;
        }

        // If warping to same map player does not have to reload new map data
        if (mapName.Equals(CurrentMapName))
        {
            if (!currentMapEntity.IsLocalPositionInMap(position))
            {
                Debug.LogWarning("Cannot warp character to " + mapName + " at " + position + ", position out of bound.");
                return;
            }
            CurrentPosition = position;
            WorldPosition = currentMapEntity.ConvertLocalPositionToWorld(position);
        }
        else
        {
            var newMap = loadedMaps[mapName];
            if (!newMap.IsLocalPositionInMap(position))
            {
                Debug.LogWarning("Cannot warp character to " + mapName + " at " + position + ", position out of bound.");
                return;
            }
            currentMapEntity = newMap;
            CurrentMapName = mapName;
            CurrentPosition = position;
            WorldPosition = currentMapEntity.ConvertLocalPositionToWorld(position);
            // If this is player, warp with messages that tell player to load new map
            var player = Identity.Player;
            if (player != null)
            {
                // Keep character data to pending character list, we'll use it after warped
                TempManager.AddPendingCharacter(player.Peer, this);
                // Set player to non ready to remove objects/subscribing to not receive objects data
                TempManager.SetPlayerReady(player.Peer, false);
                // Send new map data to client
                TempManager.SendMapResultToPeer(player.Peer, mapName);
            }
        }
    }

    #region Interest Management
    public override bool ShouldAddSubscriber(LiteNetLibPlayer subscriber)
    {
        var spawnedObjects = subscriber.SpawnedObjects.Values;
        foreach (var spawnedObject in spawnedObjects)
        {
            var characterEntity = spawnedObject.GetComponent<CharacterEntity>();
            if (characterEntity == null)
                continue;
            // There are some characters that have same map?, if yes return true
            if (!string.IsNullOrEmpty(characterEntity.CurrentMapName) && 
                characterEntity.CurrentMapName.Equals(CurrentMapName))
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
                if (characterEntity != null && 
                    !string.IsNullOrEmpty(characterEntity.CurrentMapName) && 
                    characterEntity.CurrentMapName.Equals(CurrentMapName))
                {
                    subscribers.Add(subscriber);
                    break;
                }
            }
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
}
