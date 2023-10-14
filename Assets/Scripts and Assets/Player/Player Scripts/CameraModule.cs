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
    [Tooltip("Sensitivity value for the x-axis only. The recommended value is 125.")]
    [SerializeField] private float sensitivityX = 125f;
    
    [Tooltip("Sensitivity value for the Y-axis only. The recommended value is 125.")]
    [SerializeField] private float sensitivityY = 125f;
    
    [Tooltip("The transform that will be rotated by the camera. The recommended value is the GameObject holding the camera.")]
    [SerializeField] private Transform cameraHolder;
    
    [Tooltip("The default field of view of the camera. The recommended value is 75.")]
    public float defaultFov = 75f;

    [Header("References")]
    [SerializeField] private MovementManager MovementManager;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivityX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivityY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate the camera and orientation
        cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        MovementManager.orientation.rotation = Quaternion.Euler(0, yRotation, 0);
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