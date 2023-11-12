using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
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

    [Tooltip("The value used to set how fast you move while sliding. The recommended value is 50.")]
    [SerializeField] private float slideSpeed;

    [Tooltip("The value used to set the scale of the player is while sliding. The recommended value is 0.5")]
    [SerializeField] private float slideScale;

    [Tooltip("The value used to set the minimum speed required to slide. The recommended value is 2.5")]
    [SerializeField] private float minSlideSpeed;

    [Tooltip("The cooldown time between slides in seconds. The recommended value is 5.")]
    [SerializeField] private float slideCooldown;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    
    private void Slide()
    {
        movementManager.isSliding = true;
        movementManager.canSlide = false;

        movementManager.playerObject.localScale = new Vector3(slideScale, slideScale, slideScale);

        Vector2 slideDirection = Quaternion.Euler(0f, 0f, -90f) * transform.up; // Subtract 90 degrees

        // Set Rigidbody2D velocity directly for sliding
        movementManager.rb.velocity = slideDirection * slideSpeed;

        movementManager.canWalk = false;

        // Start cooldown coroutine
        StartCoroutine(SlideCooldown());
    }

    private IEnumerator SlideCooldown()
    {
        yield return new WaitForSeconds(slideCooldown);
        movementManager.canSlide = true;
    }

    public void StopSlide(InputAction.CallbackContext context)
    {
        if (movementManager.isSliding)
        {
            CancelSlide();
        }    
    }

    public void CancelSlide()
    {
        if (movementManager.isSliding)
        {
            movementManager.isSliding = false;

            movementManager.playerObject.localScale = new Vector3(movementManager.playerScale,
                movementManager.playerScale,
                movementManager.playerScale);

            // Set Rigidbody2D velocity directly to zero to stop sliding
            movementManager.rb.velocity = Vector2.zero;

            movementManager.canWalk = true;
        }    
    }

    private void Update()
    {
        if (movementManager.rb.velocity.magnitude < minSlideSpeed && movementManager.isSliding)
        {
            CancelSlide();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CancelSlide();
    }

    public void StartSlide(InputAction.CallbackContext context)
    {
        if (movementManager.isSprinting && movementManager.isWalking && movementManager.canSlide && !movementManager.isSliding && !movementManager.isDashing)
        {
            Slide();
        }
    }
}
