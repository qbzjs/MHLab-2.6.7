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
    
    [Tooltip("The value used to set how fast you move while sprinting. The recommended value is 25")]
    [SerializeField] private float sprintMoveSpeed;
    
    [Tooltip("The value used to set the max speed you can move while sprinting. The recommended value is 25")]
    [SerializeField] private float sprintMaxSpeed;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private MovementModule movementModule;

    public void StartSprint(InputAction.CallbackContext context)
    {
        movementManager.isSprinting = true;
        if (movementManager.useDebug) Debug.Log("StartSprint called.");
    }

    public void StopSprint(InputAction.CallbackContext context)
    {
        movementManager.isSprinting = false;
        if (movementManager.useDebug) Debug.Log("StopSprint called.");
    }

    private void Update()
    {
        if (movementManager.isCrouching == false)
        {
            if (movementManager.isSprinting && movementManager.canSprint)
            {
                Sprint();
            }
            else if (movementManager.isSprinting == false)
            {
                StopSprint();
            }
        }
        if (movementManager.useDebug) Debug.Log("Update called.");
    }

    private void Sprint()
    {
        movementModule.moveSpeed = sprintMoveSpeed;
        movementModule.maxSpeed = sprintMaxSpeed;
        if (movementManager.useDebug) Debug.Log("Sprint called.");
    }

    private void StopSprint()
    {
        movementModule.moveSpeed = movementModule.normalMoveSpeed;
        movementModule.maxSpeed = movementModule.normalMaxSpeed;
        if (movementManager.useDebug) Debug.Log("StopSprint called.");
    }
}