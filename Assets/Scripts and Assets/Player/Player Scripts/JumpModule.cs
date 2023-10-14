using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(JumpModule))]
public class RequiredScriptsJump : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        JumpModule script = (JumpModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module");
    }
}
#endif

public class JumpModule : MonoBehaviour
{
    // Serialized private variables
    [Header("Customize")]
    [Tooltip("The force applied to the player when they jump. The recommended value is 5.")]
    [SerializeField] private float jumpForce;
    
    [Tooltip("The maximum number of jumps that the player can perform. The recommended value is 2.")]
    [SerializeField] private int maxJumps;
    
    [Tooltip("The amount of time that the player has to wait before they can jump again. The recommended value is 0.125.")]
    [SerializeField] private float jumpCooldown;
    
    [Tooltip("The layer used to tell the script what the ground is. Set this to the layer that your ground is on.")]
    [SerializeField] private LayerMask groundLayer;

    // Serialized private variables
    [Header("Private Variables")]
    [SerializeField] private int jumpsRemaining;
    [SerializeField] private float lastJumpTime;

    // Serialized private reference
    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    private void Start()
    {
        ResetJumps();
    }

    // Resets the number of jumps remaining to the maximum number of jumps.
    private void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }

    // Checks if the player has touched the ground, if they have it resets their jumps.
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the specified layer
        if ((groundLayer.value & 1 << collision.gameObject.layer) != 0)
        {
            // Reset jumps remaining
            ResetJumps();
        }
    }

    // Debug helper method
    private void DebugLog(string message)
    {
        if (movementManager.useDebug)
        {
            Debug.Log(message);
        }
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        if (jumpsRemaining > 0 && Time.time - lastJumpTime >= jumpCooldown && movementManager.canJump)
        {
            Jump();
        }

        // Debug
        DebugLog("StartJump method called.");
    }

    public void Jump()
    {
        if (jumpsRemaining > 0 && movementManager.canJump)
        {
            movementManager.rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpsRemaining--;

            // Debug
            DebugLog("Jump method called.");
        }

        lastJumpTime = Time.time;
    }
}
