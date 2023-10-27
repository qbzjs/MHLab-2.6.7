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
    
    [Tooltip("The transform that will be rotated by the camera. The recommended value is the GameObject holding the camera.")]
    [SerializeField] private Transform cameraHolder;
    
    [Tooltip("The default field of view of the camera. The recommended value is 75.")]
    public float defaultFov = 75f;
    public float defaultTilt;

    [Header("References")]
    [SerializeField] private MovementManager MovementManager;

    private void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        
        DoFov(defaultFov);
        DoTilt(defaultTilt);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}