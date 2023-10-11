using System.Collections;
using System.Collections.Generic;
using LootLocker.Requests;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    
    // Reference to my player script that contains data about the player.
    private PlayerMovementAdvanced pm;

    [Header("Sliding")]
    public float speedThreshold = 0;
    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    
    // Input used for averaging out input.
    private float horizontalInput;
    private float verticalInput;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();

        float speed = rb.velocity.magnitude;
        
        if (speed <= speedThreshold)
        {
            StopSlide(); 
        }

        if (!pm.grounded)
            StopSlide();
    }

    private void StartSlide()
    {
        if (pm.wallrunning) return;

        pm.sliding = true;

        pm.moveSpeed = 30;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        pm.groundDrag = 0f;

        rb.AddForce(pm.moveDirection.normalized * pm.moveSpeed * 15f, ForceMode.Force);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        pm.sliding = true;
    }

    private void StopSlide()
    {
        pm.sliding = false;
        
        pm.groundDrag = 4f;
        
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

        pm.moveSpeed = pm.walkSpeed;
    }
}
