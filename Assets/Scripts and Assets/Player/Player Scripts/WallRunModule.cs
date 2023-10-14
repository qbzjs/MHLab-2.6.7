using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(WallRunModule))]
public class RequiredScriptsWallRun : Editor
{
    public override void OnInspectorGUI()
    {
        WallRunModule script = (WallRunModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module, Camera Module");
    }
}
#endif

public class WallRunModule : MonoBehaviour
{
    [Header("Customize")]
    [Tooltip("The movement speed that the player will reach when wall running. The recommended value is 175.")]
    [SerializeField] private float wallRunSpeed;

    [Tooltip("The max speed that the player will reach when wall running. The recommended value is 175.")]
    [SerializeField] private float wallRunMaxSpeed;
    
    [Tooltip("The layer used to tell the script what a wall is. Set this to the layer that your walls are on.")]
    [SerializeField] private LayerMask wallLayer;
    
    [Tooltip("The distance that the player can be from a wall to start wall running. The recommended value is 1.")]
    [SerializeField] private float wallDistance;
    
    [Tooltip("The minimum height that the player can be from the ground to start wall running. The recommended value is 1.5.")]
    [SerializeField] private float minimumHeight;
    
    [Tooltip("The force applied to the player when they jump off of a wall. The recommended value is 5.")]
    [SerializeField] private float wallRunJumpForce;
    
    [Tooltip("The amount of tilt applied to the camera when wall running. The recommended value is 20.")]
    [SerializeField] private float wallRunCameraTilt;
    
    [Tooltip("The amount of FOV applied to the camera when wall running. The recommended value is 60.")]
    [SerializeField] private float wallRunFov;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private MovementModule movementModule;
    [SerializeField] private CameraModule cameraModule;

    [Header("Private Variables")]
    private bool wallLeft;
    private bool wallRight;
    private bool jumped;
    private bool shoulduseEndWallRun = true;
    private bool useTilt = true;

    // Input action callback to start a wall jump
    public void StartWallJump(InputAction.CallbackContext context)
    {
        if (wallRight)
        {
            // Calculate the jump direction based on the wall the player is on
            Vector3 wallRunDirection = wallLeft ? -Vector3.right : Vector3.right;

            // Calculate the desired jump direction to the opposite side of the wall
            Vector3 jumpDirection = wallLeft ? Vector3.right : -Vector3.right; // Jump in the opposite direction of the wall

            // Reset the player's vertical velocity to achieve a clean jump
            movementManager.rb.velocity = new Vector3(movementManager.rb.velocity.x, 0, movementManager.rb.velocity.z);

            // Apply a force for the jump in the calculated direction
            movementManager.rb.AddForce(jumpDirection * wallRunJumpForce, ForceMode.Impulse);

            // Set the movementModule to not allow movement while in the air
            ResetJump();
            if (movementManager.useDebug) Debug.Log("StartWallJump executed for wallRight");
        }

        if (wallLeft)
        {
            // Calculate the jump direction based on the wall the player is on
            Vector3 wallRunDirection = wallLeft ? -Vector3.right : Vector3.right;

            // Calculate the desired jump direction to the opposite side of the wall
            Vector3 jumpDirection = wallRunDirection;

            // Reset the player's vertical velocity to achieve a clean jump
            movementManager.rb.velocity = new Vector3(movementManager.rb.velocity.x, 0, movementManager.rb.velocity.z);

            // Apply a force for the jump in the calculated direction
            movementManager.rb.AddForce(jumpDirection * wallRunJumpForce, ForceMode.Impulse);

            ResetJump();
            if (movementManager.useDebug) Debug.Log("StartWallJump executed for wallLeft");
        }
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumHeight);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, wallDistance, wallLayer);
        wallRight = Physics.Raycast(transform.position, transform.right, wallDistance, wallLayer);
    }
    
    private void Update()
    {
        CheckWall();

        if (CanWallRun() && movementManager.canWallRun)
        {
            if (wallLeft || wallRight)
            {
                if (!movementManager.isWallRunning)
                {
                    StartWallRun();
                    movementManager.canClimb = false; // Disable climbModule when starting wall run
                }
            }
            else
            {
                if (movementManager.isWallRunning)
                {
                    StopWallRun();
                    movementManager.canClimb = true; // Enable climbModule when stopping wall run
                }
            }
        }
        else
        {
            if (movementManager.isWallRunning)
            {
                StopWallRun();
                movementManager.canClimb = true; // Enable climbModule when stopping wall run
            }
        }

        // Check if the player is wall running and call DoTilt accordingly
        if (movementManager.isWallRunning)
        {
            // Calculate the tilt direction based on the wall side
            float tiltDirection = wallLeft ? -wallRunCameraTilt : wallRunCameraTilt;

            if (useTilt)
            {
                cameraModule.DoTilt(tiltDirection);
            }
        }

        if (jumped && movementManager.isGrounded)
        {
            movementManager.canWalk = true;
            jumped = false;
            shoulduseEndWallRun = true;

            movementManager.isWallRunning = false; // Set movementManager.isWallRunning to false when stopping wall run

            useTilt = true;

            movementManager.rb.useGravity = true;
        }
    }

    void StartWallRun()
    {
        movementManager.isWallRunning = true; // Set movementManager.isWallRunning to true when starting wall run

        movementManager.rb.useGravity = false;

        
        Debug.Log("Starting wall run");

        cameraModule.DoFov(wallRunFov);

        // Calculate the wall run direction and desired tilt direction
        Vector3 wallRunDirection = wallLeft ? -Vector3.right : Vector3.right;
        float tiltDirection = wallLeft ? -wallRunCameraTilt : wallRunCameraTilt;

        if (useTilt)
        {
            cameraModule.DoTilt(tiltDirection);
        }
        
        movementManager.canSprint = false;
        
        movementModule.moveSpeed = wallRunSpeed;
        movementModule.moveSpeed = wallRunMaxSpeed;
    }

    void StopWallRun()
    {
        if (shoulduseEndWallRun)
        {
            movementManager.isWallRunning = false; // Set movementManager.isWallRunning to false when stopping wall run

            movementManager.rb.useGravity = true;

            if (useTilt == true)
            {
                cameraModule.DoTilt(0f);
            }

            cameraModule.DoFov(cameraModule.defaultFov);

            // Re-enable player movement
            movementManager.canWalk = true;
            
            movementManager.canSprint = true;

            movementModule.moveSpeed = movementModule.normalMoveSpeed;
            movementModule.maxSpeed = movementModule.normalMaxSpeed;
        }
    }

    void ResetJump()
    {
        movementManager.canWalk = false;

        useTilt = false;

        shoulduseEndWallRun = false;

        jumped = true;

        cameraModule.DoTilt(0f);

        cameraModule.DoFov(cameraModule.defaultFov);

        movementManager.rb.useGravity = true;
        
        movementManager.canSprint = true;

        movementModule.moveSpeed = movementModule.normalMoveSpeed;
        movementModule.maxSpeed = movementModule.normalMaxSpeed;
    }
}
