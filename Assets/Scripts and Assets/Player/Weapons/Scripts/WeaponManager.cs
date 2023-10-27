using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class WeaponManager : NetworkBehaviour
{
    [Header("Customize")]
    public GameObject projectile;
    public float projectileSpeed;
    public Transform firePoint;
    public int maxAmmo;
    public float reloadTime;
    public Camera camera;
    public float damage;
    public float maxRotation;
    public Transform playerTransform;
    
    [Header("References")]
    public Animator animator;
    
    [Header("Private Variables")]
    private int currentAmmo;
    private Vector3 destination;
    private bool isReloading;
    
    [Header("Input")]
    public PlayerMovementInput playerInput;
    private InputAction fire1;
    private InputAction reload;
    
    void Update()
    {
        Vector3 cursorScreenPosition = Input.mousePosition;
        Vector3 cursorWorldPosition = camera.ScreenToWorldPoint(new Vector3(cursorScreenPosition.x, cursorScreenPosition.y, transform.position.z - camera.transform.position.z));
    
        // Calculate the direction from the gun to the cursor
        Vector3 direction = cursorWorldPosition - transform.position;
    
        // Calculate the rotation angle based on the direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    
        // Ensure the rotation angle is outside the range of -65 to -150 degrees
        if (angle >= -150f && angle <= -65f)
        {
            if (angle > -107.5f)  // Adjust this threshold for smooth transition
            {
                angle = -65f; // Set it to the minimum angle (-65 degrees) if it's within the range
            }
            else
            {
                angle = -150f; // Set it to the maximum angle (-150 degrees) if it's within the range
            }
        }
    
        // Apply the new Z rotation while preserving the original Y and X rotations
        Quaternion originalRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(originalRotation.eulerAngles.x, originalRotation.eulerAngles.y, angle);
    }

    private void Awake()
    {
        playerInput = new PlayerMovementInput();
    }
    
    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);
        
        fire1 = playerInput.Player.Fire1;
        fire1.Enable();
        fire1.performed += Fire1;
        
        reload = playerInput.Player.Reload;
        reload.Enable();
        reload.performed += StartReload;
    }
    
    void OnDisable()
    {
        fire1.Disable();
        
        reload.Disable();
    }
    
    public void StartReload(InputAction.CallbackContext context)
    {
        if (currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }    

    IEnumerator Reload()
    {
        isReloading = true;

        animator.SetBool("Reloading", true);

        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(1f);


        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    public void Fire1(InputAction.CallbackContext context)
    {
        if (currentAmmo > 0)
        {
            Shoot();
        }
    }    

    void Shoot()
    {
        currentAmmo--;
    
        SpawnBulletServerRPC(firePoint.position, firePoint.rotation);
    }

    [ServerRpc]
    private void SpawnBulletServerRPC(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject instantiatedBullet = Instantiate(projectile, position, rotation);

        instantiatedBullet.GetComponent<Projectile>().projectileSpeed = projectileSpeed;
        instantiatedBullet.GetComponent<Projectile>().damage = damage;

        instantiatedBullet.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
    }
}