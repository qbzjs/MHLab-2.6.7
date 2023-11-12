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
    [Tooltip("The value used to set how fast you move regularly. The recommended value is 15.")]
    public float normalMoveSpeed;

    [Tooltip("The value used to set the max speed you can move regularly. The recommended value is 15.")]
    public float normalMaxSpeed;

    [Tooltip("The value used to set how you go from zero to your max speed. The recommended value is 55.")]
    [SerializeField] private float acceleration;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    [Header("Private Variables")]
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float maxSpeed;
    private Vector2 movementInput;

    void FixedUpdate()
    {
        if (movementManager.canWalk && !movementManager.isSliding && !movementManager.isDashing)
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
        Vector2 moveDirection = movementInput.normalized;
    
        Vector2 moveDirectionWorld = movementManager.playerObject.transform.TransformDirection(moveDirection);
    
        moveDirectionWorld = Quaternion.Euler(0, 0, -90) * moveDirectionWorld;
    
        Vector2 desiredVelocity = moveDirectionWorld * moveSpeed;
    
        Vector2 accelerationVector = (desiredVelocity - movementManager.rb.velocity) * acceleration;
        movementManager.rb.AddForce(accelerationVector, ForceMode2D.Force);
    
        movementManager.rb.velocity = Vector2.ClampMagnitude(movementManager.rb.velocity, maxSpeed);
    }

    void OnMovement(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }
}
