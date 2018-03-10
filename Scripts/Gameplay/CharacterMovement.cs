using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    public Vector3 moveVelocity;
    public float moveVelocityMultiplier = 1f;

    private Rigidbody cacheRigidbody;
    public Rigidbody CacheRigidbody
    {
        get
        {
            if (cacheRigidbody == null)
                cacheRigidbody = GetComponent<Rigidbody>();
            return cacheRigidbody;
        }
    }

    protected virtual void FixedUpdate()
    {
        var oldVelocity = CacheRigidbody.velocity;
        var velocityChange = (moveVelocity * moveVelocityMultiplier) - oldVelocity;
        velocityChange.y = 0;
        CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
}
