using UnityEngine;
using DG.Tweening;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(CameraModule))]
public class RequiredScriptsCamera : Editor
{
    public override void OnInspectorGUI()
    {
        CameraModule script = (CameraModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager");
    }
}
#endif

public class CameraModule : MonoBehaviour
{
    [Header("Customize")]
    
    [Tooltip("The default field of view of the camera. The recommended value is 75.")]
    [SerializeField] private float defaultFov;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    private void Start()
    {
        DoFov(defaultFov);
    }

    public void DoFov(float endValue)
    {
        movementManager.camera.DOFieldOfView(endValue, 0.25f);
    }
}