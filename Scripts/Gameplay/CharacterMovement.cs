using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    public Vector3 moveVelocity;
    public float moveVelocityMultiplier = 1f;

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

    protected virtual void FixedUpdate()
    {
        var oldVelocity = TempRigidbody.velocity;
        var velocityChange = (moveVelocity * moveVelocityMultiplier) - oldVelocity;
        velocityChange.y = 0;
        TempRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
