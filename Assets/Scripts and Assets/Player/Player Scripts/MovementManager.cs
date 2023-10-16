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

public class MovementManager : MonoBehaviour
{
    [Header("Input")] 
    public PlayerMovementInput playerInput;
    private InputAction crouch;
    private InputAction jump;
    private InputAction sprint;
    private InputAction slide;
    private InputAction climb;

    [Header("Modules")]
    public MovementModule movementModule;
    public CrouchModule crouchModule;
    public JumpModule jumpModule;
    public SprintModule sprintModule;
    public SlideModule slideModule;
    public ClimbModule climbModule;
    public CameraModule cameraModule;

    [Header("Refrences")]
    [Tooltip("The 2d rigidbody used to apply movement to the player.")]
    [HideInInspector] public Rigidbody2D rb;
    
    [Tooltip("The game object with the rigidbody used to apply movement to the player.")]
    public GameObject rigidbodyObject;
    
    [Tooltip("The transform used to rotate the player and get the rotation for the player.")]
    public Transform orientation;
    
    [Tooltip("The layer used to tell the script what the ground is for performing raycasts. Set this to the layer that your ground is on.")]
    public LayerMask groundLayer;
    
    [Header("Info")]
    public bool isGrounded;
    public float playerHeight;
    public bool useDebug;
    
    [Header("Customize")]
    public float groundRaycastLength;
    
    [Header("State Machine")]
    public bool isClimbing;
    public bool isSliding;
    public bool isSprinting;
    public bool isCrouching;
    public bool isWalking;

    public bool canClimb = true;
    public bool canSlide = true;
    public bool canSprint = true;
    public bool canCrouch = true;
    public bool canJump = true;
    public bool canWalk = true;


    #region  Player Input

    private void Start()
    {
        rb = rigidbodyObject.GetComponent<Rigidbody2D>();

        canClimb = true;
        canSlide = true;
        canSprint = true;
        canCrouch = true;
        canJump = true;
        canWalk = true;
    }
    
    private void Awake()
    {
        playerInput = new PlayerMovementInput();
    }

    private void OnEnable()
    {
        crouch = playerInput.Player.Crouch;
        crouch.Enable();
        crouch.performed += crouchModule.StartCrouch;
        crouch.canceled += crouchModule.StopCrouch;
        
        jump = playerInput.Player.Jump;
        jump.Enable();
        jump.performed += jumpModule.StartJump;
        jump.performed += climbModule.StartClimbJump;
        
        sprint = playerInput.Player.Sprint;
        sprint.Enable();
        sprint.performed += sprintModule.StartSprint;
        sprint.canceled += sprintModule.StopSprint;
        
        slide = playerInput.Player.Slide;
        slide.Enable();
        slide.performed += slideModule.StartSlide;
        slide.canceled += slideModule.StopSlide;
        
        climb = playerInput.Player.Climb;
        climb.Enable();
        climb.performed += climbModule.StartClimb;
        climb.canceled += climbModule.StopClimb;
    }

    private void OnDisable()
    {
        crouch.Disable();
        
        jump.Disable();
        
        sprint.Disable();
        
        slide.Disable();
        
        climb.Disable();
    }

    #endregion

    #region Ground Check
    
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, groundRaycastLength, groundLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    #endregion
}
