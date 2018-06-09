using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ServerCharacter : MonoBehaviour
{
    public Camera controlCamera;
    [Header("Movement Settings")]
    public float groundingDistance = 0.1f;
    public float jumpHeight = 2f;
    public float gravityRate = 1f;
    public float angularSpeed = 120f;
    public float moveSpeed = 8f;

    private Transform cacheTransform;
    public Transform CacheTransform
    {
        get
        {
            if (cacheTransform == null)
                cacheTransform = GetComponent<Transform>();
            return cacheTransform;
        }
    }

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

    private Vector3 moveDirection;
    private bool isJumping;
    private bool isGrounded;
    void Awake()
    {
        CacheRigidbody.useGravity = false;
    }

    void Update()
    {
        var horizontalInput = InputManager.GetAxis("Horizontal", false);
        var verticalInput = InputManager.GetAxis("Vertical", false);
        var isJump = InputManager.GetButtonDown("Jump");

        moveDirection = Vector3.zero;
        var cameraTransform = controlCamera.transform;
        if (cameraTransform != null)
        {
            moveDirection += cameraTransform.forward * verticalInput;
            moveDirection += cameraTransform.right * horizontalInput;
        }
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;

        if (moveDirection.magnitude == 0 && isGrounded)
            CacheRigidbody.velocity = new Vector3(0, CacheRigidbody.velocity.y, 0);
        if (!isJumping)
            isJumping = isGrounded && isJump;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isGrounded && collision.impulse.y > 0)
            isGrounded = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isGrounded && collision.impulse.y > 0)
            isGrounded = true;
    }

    private void FixedUpdate()
    {
        var gameInstance = GameInstance.Singleton;
        var velocity = CacheRigidbody.velocity;
        var moveDirectionMagnitude = moveDirection.magnitude;
        if (moveDirectionMagnitude != 0)
        {
            if (moveDirectionMagnitude > 1)
                moveDirection = moveDirection.normalized;

            var targetVelocity = moveDirection * moveSpeed;

            // Apply a force that attempts to reach our target velocity
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -moveSpeed, moveSpeed);
            velocityChange.y = 0;
            velocityChange.z = Mathf.Clamp(velocityChange.z, -moveSpeed, moveSpeed);
            CacheRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            // Calculate rotation on client only, will send update to server later
            CacheTransform.rotation = Quaternion.RotateTowards(CacheTransform.rotation, Quaternion.LookRotation(moveDirection), angularSpeed * Time.fixedDeltaTime);
        }

        // Jump
        if (isGrounded && isJumping)
        {
            CacheRigidbody.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
            isJumping = false;
        }

        if (Mathf.Abs(velocity.y) > groundingDistance)
            isGrounded = false;

        // We apply gravity manually for more tuning control
        CacheRigidbody.AddForce(new Vector3(0, Physics.gravity.y * CacheRigidbody.mass * gravityRate, 0));
    }

    private float CalculateJumpVerticalSpeed()
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        return Mathf.Sqrt(2f * jumpHeight * -Physics.gravity.y * gravityRate);
    }
}
