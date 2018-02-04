using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : NetworkBehaviour, ICharacterData
{
    // Use id as primary key
    [Header("SyncVars")]
    [SyncVar]
    public string id;
    [SyncVar]
    public string characterName;
    [SyncVar(hook = "OnPrototypeIdChange")]
    public string prototypeId;
    [SyncVar]
    public int level;
    [SyncVar]
    public int exp;
    [SyncVar]
    public int currentHp;
    [SyncVar]
    public int currentMp;
    [SyncVar]
    public int statPoint;
    [SyncVar]
    public int skillPoint;
    [SyncVar]
    public int gold;
    protected int lastUpdate;

    [Header("Sync Lists")]
    public SyncListCharacterAttributeLevel attributeLevels = new SyncListCharacterAttributeLevel();
    public SyncListCharacterSkillLevel skillLevels = new SyncListCharacterSkillLevel();
    public SyncListCharacterItem equipItems = new SyncListCharacterItem();
    public SyncListCharacterItem nonEquipItems = new SyncListCharacterItem();

    protected CharacterModel model;

    public string Id { get { return id; } set { id = value; } }
    public string CharacterName { get { return characterName; } set { characterName = value; } }
    public string PrototypeId
    {
        get { return prototypeId; }
        set
        {
            prototypeId = value;
            // Setup model
            if (model != null)
                Destroy(model.gameObject);
            model = this.InstantiateModel(transform);
            // Wake up rigidbody
            TempRigidbody.WakeUp();
        }
    }
    public int Level { get { return level; } set { level = value; } }
    public int Exp { get { return exp; } set { exp = value; } }
    public int CurrentHp { get { return currentHp; } set { currentHp = value; } }
    public int CurrentMp { get { return currentMp; } set { currentMp = value; } }
    public int StatPoint { get { return statPoint; } set { statPoint = value; } }
    public int SkillPoint { get { return skillPoint; } set { skillPoint = value; } }
    public int Gold { get { return gold; } set { gold = value; } }
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
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
            OnPrototypeIdChange(prototypeId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        OnPrototypeIdChange(prototypeId);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Local player must have movement input
        if (TempCharacterMovementInput == null)
            TempCharacterMovementInput = gameObject.AddComponent<CharacterMovementInput>();
        TempCharacterMovementInput.enabled = true;
        TempCharacterMovement.enabled = true;
    }

    #region SyncVar Hooks
    protected virtual void OnPrototypeIdChange(string prototypeId)
    {
        PrototypeId = prototypeId;
    }
    #endregion
}
