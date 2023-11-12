using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(SprintModule))]
public class RequiredScriptsSprint : Editor
{
    public override void OnInspectorGUI()
    {
        SprintModule script = (SprintModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module");
    }
}
#endif

public class SprintModule : MonoBehaviour
{
    [Header("Customize")]
    
    [Tooltip("The value used to set how fast you move while sprinting. The recommended value is 25.")]
    [SerializeField] private float sprintMoveSpeed;
    
    [Tooltip("The value used to set the max speed you can move while sprinting. The recommended value is 25.")]
    [SerializeField] private float sprintMaxSpeed;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private MovementModule movementModule;

    public void StartSprint(InputAction.CallbackContext context)
    {
        if (!movementManager.isCrouching && !movementManager.isSliding && !movementManager.isDashing && movementManager.canSprint)
        {
            Sprint();
        }
    }

    public void StopSprint(InputAction.CallbackContext context)
    {
        StopSprint();
    }

    private void Sprint()
    {
        movementManager.isSprinting = true;
        
        movementModule.moveSpeed = sprintMoveSpeed;
        movementModule.maxSpeed = sprintMaxSpeed;
    }

    private void StopSprint()
    {
        movementModule.moveSpeed = movementModule.normalMoveSpeed;
        movementModule.maxSpeed = movementModule.normalMaxSpeed;

        movementManager.isSprinting = false;
    }
}