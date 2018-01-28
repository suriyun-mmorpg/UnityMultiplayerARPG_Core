using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : NetworkBehaviour
{
    [SyncVar]
    public string characterName;
    [SyncVar]
    public string characterClassId;
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

    public CharacterClass Class
    {
        get { return GameInstance.CharacterClasses[characterClassId]; }
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

    public int MaxHp
    {
        // TODO: Bring data from game instance, equipments
        get { return 0; }
    }

    public int MaxMp
    {
        // TODO: Bring data from game instance, equipments
        get { return 0; }
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
        TempRigidbody.WakeUp();
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
}
