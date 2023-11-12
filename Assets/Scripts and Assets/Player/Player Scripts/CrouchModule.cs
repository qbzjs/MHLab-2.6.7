using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CrouchModule))]
public class RequiredScriptsCrouch : Editor
{
    public override void OnInspectorGUI()
    {
        CrouchModule script = (CrouchModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module");
    }
}
#endif

public class CrouchModule : MonoBehaviour
{
    [Header("Customize")]
    
    [SerializeField] [Tooltip("Speed value for movement speed when crouching. The recommended value is 10.")]
    private float crouchMoveSpeed;
    
    [SerializeField] [Tooltip("The maximum speed that the player can move at when crouching. The recommended value is 10.")]
    private float crouchMaxSpeed;
    
    [SerializeField] [Tooltip("The scale of the player when crouching. The recommended value is 0.5.")]
    private float crouchScale;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private MovementModule movementModule;

    private void Crouch()
    {
        movementManager.isCrouching = true;
        
        movementModule.moveSpeed = crouchMoveSpeed;
        movementModule.maxSpeed = crouchMaxSpeed;

        movementManager.playerObject.localScale = new Vector3(crouchScale, crouchScale, crouchScale);
    }

    public void StartCrouch(InputAction.CallbackContext context)
    {
        if (movementManager.canCrouch && !movementManager.isSliding && !movementManager.isDashing && !movementManager.isSprinting)
        {
            Crouch();
        }
    }

    public void StopCrouch(InputAction.CallbackContext context)
    {
        if (movementManager.isCrouching)
        {
            movementManager.isCrouching = false;

            movementModule.moveSpeed = movementModule.normalMoveSpeed;
            movementModule.maxSpeed = movementModule.normalMaxSpeed;

            movementManager.playerObject.localScale = new Vector3(movementManager.playerScale,
                movementManager.playerScale,
                movementManager.playerScale);
        }
    }
}