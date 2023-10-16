using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(ClimbModule))]
public class RequiredScriptsClimb : Editor
{
    public override void OnInspectorGUI()
    {
        ClimbModule script = (ClimbModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module");
    }
}
#endif

public class ClimbModule : MonoBehaviour
{
    [Header("Customize")]
    [Tooltip("The layer used to tell the script what a wall is. Set this to the layer that your walls are on.")]
    [SerializeField] private LayerMask wallLayer;

    [Tooltip("Speed value for climbing. The recommended value is 5.")]
    [SerializeField] private float climbingSpeed;

    [Tooltip("The maximum speed that the player can climb at. The recommended value is 0.04.")]
    [SerializeField] private float maxClimbingSpeed;

    [Tooltip("The radius of the sphere used to detect walls. The recommended value is 0.75.")]
    [SerializeField] private float wallDetectionRadius;

    [Tooltip("The force applied to the player when they jump off of a wall. The recommended value is 2000.")]
    [SerializeField] private float jumpForce;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    [Header("Private Variables")]
    private bool isHoldingClimb;
    private bool canJump;

    private void Update()
    {
        if (movementManager.useDebug)
        {
            Debug.Log("Update method is called.");
        }
    }

    private void FixedUpdate()
    {
        if (movementManager.useDebug)
        {
            Debug.Log("FixedUpdate method is called.");
        }

        // Check for walls continuously
        bool wallDetected = CheckForWall();

        if (wallDetected && isHoldingClimb && !movementManager.isClimbing && movementManager.canClimb)
        {
            // Start climbing if a wall is detected and climb button is held
            movementManager.isClimbing = true;

            movementManager.canWalk = false;
            movementManager.canSlide = false;
            movementManager.canJump = false;
            movementManager.canJump = true;

            if (movementManager.useDebug)
            {
                Debug.Log("Start climbing.");
            }
        }
        else if (!wallDetected && movementManager.isClimbing)
        {
            // Stop climbing if no wall is detected
            movementManager.isClimbing = false;

            movementManager.canWalk = true;
            movementManager.canSlide = true;
            movementManager.canJump = true;
            movementManager.canJump = false;

            if (movementManager.useDebug)
            {
                Debug.Log("Stop climbing.");
            }
        }
        else if (!wallDetected)
        {
            movementManager.canJump = false;
            movementManager.canSlide = true;
            movementManager.canJump = true;
        }

        if (movementManager.isClimbing)
        {
            // Apply climbing force with max speed
            Vector3 climbingForce = Vector3.up * climbingSpeed;
            climbingForce = Vector3.ClampMagnitude(climbingForce, maxClimbingSpeed);
            movementManager.rb.AddForce(climbingForce, ForceMode2D.Impulse);
        }
    }

    private bool CheckForWall()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, wallDetectionRadius, wallLayer);
        return hitColliders.Length > 0;
    }

    public void StartClimb(InputAction.CallbackContext context)
    {
        if (!movementManager.isClimbing)
        {
            isHoldingClimb = true;
        }
    }

    public void StopClimb(InputAction.CallbackContext context)
    {
        isHoldingClimb = false;

        if (movementManager.isClimbing)
        {
            movementManager.isClimbing = false;
            movementManager.canWalk = true;

            if (movementManager.useDebug)
            {
                Debug.Log("Stop climbing.");
            }
        }
    }

    public void StartClimbJump(InputAction.CallbackContext context)
    {
        if (canJump)
        {
            Vector3 backwardDirection = -movementManager.orientation.forward;
            movementManager.rb.AddForce(backwardDirection * jumpForce, ForceMode2D.Impulse);

            if (movementManager.useDebug)
            {
                Debug.Log("Start climb jump.");
            }
        }
    }
}