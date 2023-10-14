using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(SlideModule))]
public class RequiredScriptsSlide : Editor
{
    public override void OnInspectorGUI()
    {
        SlideModule script = (SlideModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager, Movement Module, Sprint Module");
    }
}
#endif

public class SlideModule : MonoBehaviour
{
    [Header("Customize")]
    [Tooltip("The value used to set how fast you move while sliding. The recommended value is 18")]
    [SerializeField] private float slideSpeed;
    
    [Tooltip("The value used to set how high the player is while sliding. The recommended value is 0.5")]
    [SerializeField] private float slideHeight;
    
    [Tooltip("The value used to set the minimum speed required to slide. The recommended value is 2.5")]
    [SerializeField] private float minSlideSpeed;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    private void Slide()
    {
        if (movementManager.useDebug)
        {
            Debug.Log("Slide method called.");
        }

        movementManager.isSliding = true;

        movementManager.orientation.localScale = new Vector3(1, slideHeight, 1);

        Vector3 slideDirection = transform.forward;

        movementManager.rb.AddForce(slideDirection * slideSpeed, ForceMode.VelocityChange);

        movementManager.canWalk = false;

        movementManager.rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void StopSlide()
    {
        if (movementManager.useDebug)
        {
            Debug.Log("StopSlide method called.");
        }

        movementManager.isSliding = false;

        movementManager.orientation.localScale = new Vector3(1, movementManager.playerHeight, 1);

        movementManager.canWalk = true;
    }

    private void Update()
    {
        if (movementManager.rb.velocity.magnitude < minSlideSpeed && movementManager.isSliding)
        {
            if (movementManager.useDebug)
            {
                Debug.Log("Update: Stopping slide due to low velocity.");
            }
            StopSlide();
        }
    }

    public void StartSlide(InputAction.CallbackContext context)
    {
        if (movementManager.isSprinting && movementManager.canSlide)
        {
            if (movementManager.useDebug)
            {
                Debug.Log("StartSlide method called.");
            }
            Slide();
        }
    }

    public void StopSlide(InputAction.CallbackContext context)
    {
        if (movementManager.useDebug)
        {
            Debug.Log("StopSlide (InputAction) method called.");
        }
        StopSlide();
    }
}