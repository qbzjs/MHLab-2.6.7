using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(RotateModule))]
public class RequiredScriptsRotate : Editor
{
    public override void OnInspectorGUI()
    {
        RotateModule script = (RotateModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager");
    }
}
#endif

public class RotateModule : MonoBehaviour
{
    [Header("Customize")]
    [SerializeField] private float rotationSpeed; // Adjust the speed as needed

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    private void FixedUpdate()
    {
#if !DEDICATED_SERVER

        if (movementManager.canRotate)
        {
            Rotate();

            movementManager.isRotating = true;
        }
        else
        {
            movementManager.isRotating = false;
        }

#endif
    }

    private void Rotate()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 playerPosition = movementManager.camera.WorldToScreenPoint(movementManager.playerObject.transform.position);

        Vector2 direction = mousePosition - playerPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        movementManager.playerObject.transform.rotation = Quaternion.Slerp(
            movementManager.playerObject.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}