using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using System.Collections;

#if UNITY_EDITOR
[CustomEditor(typeof(DashModule))]
public class RequiredScriptsDash : Editor
{
    public override void OnInspectorGUI()
    {
        DashModule script = (DashModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager");
    }
}
#endif

[RequireComponent(typeof(CharacterController))]
public class DashModule : MonoBehaviour
{
    [Header("Customize")]
    [Tooltip("The value used to set how far the player gets pushed when dashing. The recommended value is 15.")]
    [SerializeField] private float dashDistance;

    [Tooltip("The value used for the amount of time in seconds that it takes to get the player to the dash distance. The recommended value is 0.5.")]
    [SerializeField] private float dashDuration;

    [Tooltip("The cooldown time between dashes in seconds. The recommended value is 5.")]
    [SerializeField] private float dashCooldown;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;

    [Header("Private Variables")]
    private bool isDashing = false;
    private float dashCooldownTimer;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check for obstacles during the dash
        if (isDashing)
        {
            // Handle collision with an obstacle (you can adjust this part based on your game's logic)
            StopDash();
        }
    }

    IEnumerator Dash()
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            // Perform the dash using Translate
            transform.Translate(Vector2.right * (dashDistance / dashDuration) * Time.deltaTime, Space.Self);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset after the dash is complete
        StopDash();
    }

    void StopDash()
    {
        // Reset after the dash is complete or interrupted
        isDashing = false;
        movementManager.isDashing = false;
        movementManager.canDash = true;
        dashCooldownTimer = dashCooldown;
    }

    public void StartDash(InputAction.CallbackContext context)
    {
        if (!isDashing && movementManager.canDash && movementManager.isWalking && dashCooldownTimer <= 0)
        {
            isDashing = true;
            movementManager.canDash = false;
            movementManager.isDashing = true;

            StartCoroutine(Dash());
        }
    }

    void Update()
    {
        // Update the dash cooldown timer
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
}