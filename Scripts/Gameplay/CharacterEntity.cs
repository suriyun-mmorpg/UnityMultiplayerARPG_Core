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
    private string currentMapName;
    private Vector3 currentPosition;
    private string respawnMapName;
    private Vector3 respawnPosition;
    private int lastUpdate;
    private bool isSetup;

    [Header("Sync Lists")]
    public SyncListCharacterAttributeLevel attributeLevels = new SyncListCharacterAttributeLevel();
    public SyncListCharacterSkillLevel skillLevels = new SyncListCharacterSkillLevel();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();

    protected CharacterModel model;

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
            if (isSetup && !currentMapName.Equals(value))
                Identity.RebuildSubscribers(false);
            currentMapName = value;
        }
    }
    public Vector3 CurrentPosition { get { return currentPosition; } set { currentPosition = value; } }
    public string RespawnMapName { get { return respawnMapName; } set { respawnMapName = value; } }
    public Vector3 RespawnPosition { get { return respawnPosition; } set { respawnPosition = value; } }
    public int LastUpdate { get { return lastUpdate; } set { lastUpdate = value; } }

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

    public CharacterMovementInput TempCharacterMovementInput { get; private set; }

    protected virtual void Awake()
    {
        TempCharacterMovementInput = GetComponent<CharacterMovementInput>();
        if (TempCharacterMovementInput != null)
            TempCharacterMovementInput.enabled = false;

        TempCharacterMovement.enabled = false;
        TempRigidbody.Sleep();

        prototypeId.onChange += OnPrototypeIdChange;
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
        {
            // Local player must have movement input
            if (TempCharacterMovementInput == null)
                TempCharacterMovementInput = gameObject.AddComponent<CharacterMovementInput>();
            TempCharacterMovementInput.enabled = true;
            TempCharacterMovement.enabled = true;
        }
        isSetup = true;
    }

    public override bool ShouldAddSubscriber(LiteNetLibPlayer subscriber)
    {
        var spawnedObjects = subscriber.SpawnedObjects.Values;
        foreach (var spawnedObject in spawnedObjects)
        {
            var characterEntity = spawnedObject.GetComponent<CharacterEntity>();
            // There are some characters that have same map?, if yes return true
            if (characterEntity != null && characterEntity.CurrentMapName.Equals(CurrentMapName))
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
                // There are some characters that have same map?, if yes return true
                if (characterEntity != null && characterEntity.CurrentMapName.Equals(CurrentMapName))
                    subscribers.Add(subscriber);
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
