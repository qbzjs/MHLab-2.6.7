using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.RemoteConfig;

public class WeaponManager : NetworkBehaviour
{
    [Header("Customize")]
    public float projectileSpeed;
    public float maxAmmo;
    public float reloadTime;
    public float damage;
    public float maxRotation;
    public string maxAmmoConfigKey;
    public string reloadTimeConfigKey;
    public string projectileSpeedConfigKey;
    public string damageConfigKey;
    
    [Header("References")]
    public Animator animator;
    public GameObject projectile;
    public Transform firePoint;
    public Camera camera;
    public Transform playerTransform;
    public struct userAttributes {}
    public struct appAttributes {}
    
    [Header("Private Variables")]
    private float currentAmmo;
    private Vector3 destination;
    private bool isReloading;
    
    [Header("Input")]
    public PlayerMovementInput playerInput;
    private InputAction fire1;
    private InputAction reload;
    
    void Update()
    {
 #if !DEDICATED_SERVER
        
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
        
#endif
    }

    private void Awake()
    {
        playerInput = new PlayerMovementInput();
        
        FetchRemoteConfiguration();
    }
    
    private void FetchRemoteConfiguration()
    {
        if(gameObject.active)
        {
            ConfigManager.FetchCompleted += ApplyRemoteConfiguration;
            ConfigManager.FetchConfigs<userAttributes, appAttributes> (new userAttributes(), new appAttributes());
        }    
    }
    
    private void ApplyRemoteConfiguration(ConfigResponse response)
    {
        switch (response.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                
                SetRemoteConfigurationValues();
                
                break;
        }    
    }
    
    private void SetRemoteConfigurationValues()
    {
        maxAmmo = ConfigManager.appConfig.GetFloat(maxAmmoConfigKey);
        reloadTime = ConfigManager.appConfig.GetFloat(reloadTimeConfigKey);
        projectileSpeed = ConfigManager.appConfig.GetFloat(projectileSpeedConfigKey);
        damage = ConfigManager.appConfig.GetFloat(damageConfigKey);
        
        Debug.Log("maxAmmo: " + maxAmmo + " reloadTime: " + reloadTime + " projectileSpeed: " + projectileSpeed + " damage: " + damage);
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
        
        instantiatedBullet.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

        instantiatedBullet.GetComponent<Projectile>().projectileSpeed.Value = projectileSpeed;
        instantiatedBullet.GetComponent<Projectile>().damage.Value = damage;
    }
}