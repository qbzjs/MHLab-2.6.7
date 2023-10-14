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
        // Get the player's orientation (Quaternion) from movementManager
        Quaternion playerOrientation = movementManager.orientation.rotation;

        // Calculate the movement direction based on input and player's orientation
        Vector3 moveDirection = playerOrientation * new Vector3(movementInput.x, 0f, movementInput.y);
        Vector3 desiredVelocity = moveDirection * moveSpeed;

        // Calculate the X and Z velocities separately to allow free horizontal movement
        Vector3 currentVelocity = movementManager.rb.velocity;
        Vector2 xzVelocity = new Vector2(currentVelocity.x, currentVelocity.z);

        // Apply acceleration to the X and Z velocities
        Vector2 accelerationVector = (new Vector2(desiredVelocity.x, desiredVelocity.z) - xzVelocity) * acceleration;
        movementManager.rb.AddForce(new Vector3(accelerationVector.x, 0, accelerationVector.y), ForceMode.Force);

        // Limit the maximum speed for X and Z axes only
        Vector3 clampedVelocity = movementManager.rb.velocity;
        clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxSpeed, maxSpeed);
        clampedVelocity.z = Mathf.Clamp(clampedVelocity.z, -maxSpeed, maxSpeed);
        movementManager.rb.velocity = clampedVelocity;
    }

    void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }
}
