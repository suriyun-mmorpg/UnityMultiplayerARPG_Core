using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterMovementInput : MonoBehaviour
{
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

    protected virtual void Update()
    {
        var inputX = Input.GetAxis("Horizontal");
        var inputZ = Input.GetAxis("Vertical");
        var moveVelocity = new Vector3(inputX, 0, inputZ);
        if (moveVelocity.magnitude > 1)
            moveVelocity.Normalize();
        TempCharacterMovement.moveVelocity = moveVelocity;
    }
}
