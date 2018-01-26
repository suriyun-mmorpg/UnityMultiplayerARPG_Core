using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterEntity : NetworkBehaviour
{
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
