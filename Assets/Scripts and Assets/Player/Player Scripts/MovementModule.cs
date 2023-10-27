using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MovementModule))]
public class RequiredScriptsMovement : Editor
{
    public override void OnInspectorGUI()
    {
        MovementModule script = (MovementModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager");
    }
}
#endif

public class MovementModule : MonoBehaviour
{
    [Header("Customize")]
    [Tooltip("The value used to set how fast you move regularly. The recommended value is 15")]
    public float normalMoveSpeed;

    [Tooltip("The value used to set the max speed you can move regularly. The recommended value is 15")]
    public float normalMaxSpeed;

    [Tooltip("The value used to set how you go from zero to your max speed. The recommended value is 55")]
    public float acceleration;

    [Header("References")]
    public MovementManager movementManager;

    [Header("Private Variables")]
    public float moveSpeed;
    public float maxSpeed;
    private Vector2 movementInput;

    public float flipSpeed = 0.5f; // Adjust the speed in the Inspector
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (movementManager.canWalk)
        {
            ApplyMovement();

            movementManager.isWalking = true;
        }
        else
        {
            movementManager.isWalking = false;
        }
    }

    void ApplyMovement()
    {
        // Flip the sprite horizontally based on movement input
        if (movementInput.x > 0)
        {
            spriteRenderer.flipX = false; // Not flipped
        }
        else if (movementInput.x < 0)
        {
            spriteRenderer.flipX = true; // Flipped
        }

        // Calculate the movement direction based on input (not player's orientation)
        Vector3 moveDirection = new Vector3(movementInput.x, 0f, 0f);

        // Calculate the desired velocity based on the movement direction
        Vector3 desiredVelocity = moveDirection * moveSpeed;

        // Calculate the X velocity separately to allow free horizontal movement
        Vector3 currentVelocity = movementManager.rb.velocity;
        currentVelocity.x = desiredVelocity.x;
        currentVelocity.z = 0f; // Set Z velocity to 0 to restrict Z movement

        // Apply acceleration to the X velocity
        float accelerationX = (currentVelocity.x - desiredVelocity.x) * acceleration;
        currentVelocity.x -= accelerationX;

        // Limit the maximum speed for the X axis only
        currentVelocity.x = Mathf.Clamp(currentVelocity.x, -maxSpeed, maxSpeed);

        // Apply the new velocity to the Rigidbody
        movementManager.rb.velocity = currentVelocity;
    }

    void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }
}