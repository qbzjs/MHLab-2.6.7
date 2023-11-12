using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(MovementManager))]
public class RequiredScriptsMovementManager : Editor
{
    public override void OnInspectorGUI()
    {
        MovementManager script = (MovementManager)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("The Base Script Used To Control Everything");
    }
}
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class MovementManager : MonoBehaviour
{
    [Header("Input")] 
    [SerializeField] private PlayerMovementInput playerInput;
    [SerializeField] private InputAction crouch;
    [SerializeField] private InputAction sprint;
    [SerializeField] private InputAction slide;
    [SerializeField] private InputAction dash;

    [Header("Modules")]
    [SerializeField] private MovementModule movementModule;
    [SerializeField] private CrouchModule crouchModule;
    [SerializeField] private SprintModule sprintModule;
    [SerializeField] private SlideModule slideModule;
    [SerializeField] private CameraModule cameraModule;
    [SerializeField] private DashModule dashModule;
    [SerializeField] private RotateModule rotateModule;

    [Header("Refrences")]
    [Tooltip("The 2d rigidbody used to apply movement to the player.")]
    [HideInInspector] public Rigidbody2D rb;
    public Camera camera;

    [Tooltip("The transform used to set the position and rotation of the player.")]
    public Transform playerObject;
    
    [Tooltip("The game object with the rigidbody used to apply movement to the player.")]
    public GameObject rigidbodyObject;
    
    [Header("Customize")]
    public float playerScale;
    public bool useDebug;
    
    [Header("State Machine")]
    public bool isSliding = false;
    public bool isSprinting = false;
    public bool isCrouching = false;
    public bool isWalking = false;
    public bool isDashing = false;
    public bool isRotating = false;

    public bool canSlide = true;
    public bool canSprint = true;
    public bool canCrouch = true;
    public bool canWalk = true;
    public bool canDash = true;
    public bool canRotate = true;
    
    #region  Player Input

    private void Start()
    {
        rb = rigidbodyObject.GetComponent<Rigidbody2D>();

        canSlide = true;
        canSprint = true;
        canCrouch = true;
        canWalk = true;
        canDash = true;
        canRotate = true;

        isSliding = false;
        isSprinting = false;
        isCrouching = false;
        isWalking = false;
        isDashing = false;
        isRotating = false;
    }
    
    private void Awake()
    {
        playerInput = new PlayerMovementInput();
        
        transform.localScale = new Vector3(playerScale, playerScale, playerScale);
    }

    private void OnEnable()
    {
        crouch = playerInput.Player.Crouch;
        crouch.Enable();
        crouch.performed += crouchModule.StartCrouch;
        crouch.canceled += crouchModule.StopCrouch;
        
        sprint = playerInput.Player.Sprint;
        sprint.Enable();
        sprint.performed += sprintModule.StartSprint;
        sprint.canceled += sprintModule.StopSprint;
        
        slide = playerInput.Player.Slide;
        slide.Enable();
        slide.performed += slideModule.StartSlide;
        slide.canceled += slideModule.StopSlide;
        
        dash = playerInput.Player.Dash;
        dash.Enable();
        dash.performed += dashModule.StartDash;
    }

    private void OnDisable()
    {
        crouch.Disable();
        
        sprint.Disable();
        
        slide.Disable();
        
        dash.Disable();
    }

    #endregion
}
