using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : NetworkBehaviour
{
    // Use id as primary key
    [SyncVar]
    public string id;
    [SyncVar]
    public string characterName;
    [SyncVar(hook = "OnCharacterClassIdChange")]
    public string classId;
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
    [Header("Sync Lists")]
    public SyncListCharacterAttributeLevel attributeLevels = new SyncListCharacterAttributeLevel();
    public SyncListCharacterSkillLevel skillLevels = new SyncListCharacterSkillLevel();
    public SyncListCharacterItem equipments = new SyncListCharacterItem();
    public SyncListCharacterItem inventory = new SyncListCharacterItem();

    public CharacterClass Class
    {
        get { return GameInstance.CharacterClasses[classId]; }
    }

    public int NextLevelExp
    {
        get
        {
            var expTree = GameInstance.Singleton.expTree;
            if (level > expTree.Length)
                return 0;
            return expTree[level - 1];
        }
    }

    #region Stats calculation, make saperate stats for buffs calculation
    public CharacterStats Stats
    {
        get
        {
            var result = Class.baseStats;
            result += Class.statsIncreaseEachLevel * level;
            foreach (var attributeLevel in attributeLevels)
            {
                if (attributeLevel.Attribute == null)
                {
                    Debug.LogError("Attribute: " + attributeLevel.attributeId + " owned by " + id + " is invalid data");
                    continue;
                }
                result += attributeLevel.Attribute.statsIncreaseEachLevel * level;
            }
            foreach (var equipment in equipments)
            {
                if (equipment.EquipmentItem == null) {
                    Debug.LogError("Item: " + equipment.id + " owned by "+ id + " is not equipment");
                    continue;
                }
                result += equipment.EquipmentItem.baseStats;
                result += equipment.EquipmentItem.statsIncreaseEachLevel * level;
            }
            return result;
        }
    }

    public CharacterStatsPercentage StatsPercentage
    {
        get
        {
            var result = Class.statsPercentageIncreaseEachLevel * level;
            foreach (var attributeLevel in attributeLevels)
            {
                if (attributeLevel.Attribute == null)
                {
                    Debug.LogError("Attribute: " + attributeLevel.attributeId + " owned by " + id + " is invalid data");
                    continue;
                }
                result += attributeLevel.Attribute.statsPercentageIncreaseEachLevel * level;
            }
            foreach (var equipment in equipments)
            {
                if (equipment.EquipmentItem == null)
                {
                    Debug.LogError("Item: " + equipment.id + " owned by " + id + " is not equipment");
                    continue;
                }
                result += equipment.EquipmentItem.statsPercentageIncreaseEachLevel * level;
            }
            return result;
        }
    }

    public CharacterStats StatsWithoutBuffs
    {
        get { return Stats + StatsPercentage; }
    }
    #endregion

    public int MaxHp
    {
        get { return (int)StatsWithoutBuffs.hp; }
    }

    public int MaxMp
    {
        get { return (int)StatsWithoutBuffs.mp; }
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
            OnCharacterClassIdChange(classId);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        OnCharacterClassIdChange(classId);
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
    protected virtual void OnCharacterClassIdChange(string classId)
    { 
        // TODO: I will use this hook to change character model
        // Setup model then wake up rigidbody
        TempRigidbody.WakeUp();
    }
    #endregion

    /// <summary>
    /// Use this function to make an character entity, useful to make temporary data in character create/selection UIs
    /// </summary>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static CharacterEntity CreateNewCharacter(string characterName, string classId)
    {
        var character = Instantiate(GameInstance.Singleton.characterEntityPrefab);
        character.characterName = characterName;
        character.classId = classId;
        foreach (var baseAttribute in character.Class.baseAttributes)
        {
            var attributeLevel = new CharacterAttributeLevel();
            attributeLevel.attributeId = baseAttribute.attribute.Id;
            attributeLevel.amount = baseAttribute.amount;
            character.attributeLevels.Add(attributeLevel);
        }
        character.currentHp = character.MaxHp;
        character.currentMp = character.MaxMp;
        return character;
    }

    public void SaveToPlayerPrefs()
    {

    }
}
