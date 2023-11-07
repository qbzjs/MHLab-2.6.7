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
    public float fireRate;
    public bool canHoldFire;
    public float maxRotation;
    public float minRotation;
    public string maxAmmoConfigKey;
    public string reloadTimeConfigKey;
    public string projectileSpeedConfigKey;
    public string damageConfigKey;

    [Header("References")]
    public Animator animator;
    public GameObject projectile;
    public Transform firePoint;
    public Camera camera;
    public struct userAttributes { }
    public struct appAttributes { }

    [Header("Private Variables")] 
    private bool isFiring;
    private float currentAmmo;
    private Vector3 destination;
    private bool isReloading;

    [Header("Input")]
    public PlayerMovementInput playerInput;
    private InputAction fire1;
    private InputAction reload;

    private void Awake()
    {
        playerInput = new PlayerMovementInput();
        FetchRemoteConfiguration();
    }

    private void FetchRemoteConfiguration()
    {
        if (gameObject.active)
        {
            ConfigManager.FetchCompleted += ApplyRemoteConfiguration;
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
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

    void Update()
    {
#if !DEDICATED_SERVER
        Vector3 cursorScreenPosition = Input.mousePosition;
        Vector3 cursorWorldPosition = camera.ScreenToWorldPoint(new Vector3(cursorScreenPosition.x,
            cursorScreenPosition.y, transform.position.z - camera.transform.position.z));
        
        Vector3 direction = cursorWorldPosition - transform.position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (angle >= maxRotation && angle <= minRotation)
        {
            if (angle > -107.5f)
            {
                angle = minRotation;
            }
            else
            {
                angle = maxRotation;
            }
        }

        if (angle > maxRotation && angle < 90f)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
            firePoint.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // Keep the Y-axis fixed at 0 while changing the Z-axis
            float newZRotation = 180f - angle;
            transform.rotation = Quaternion.Euler(180, 0, newZRotation + 180);
            firePoint.transform.rotation = Quaternion.Euler(180, 0, angle);
        }
#endif

        if (currentAmmo > 0 && (canHoldFire || isFiring))
        {
            if (fire1.ReadValue<float>() > 0)
            {
                if (!isFiring)
                {
                    isFiring = true;
                    StartCoroutine(FireAtRate());
                }
            }
            else
            {
                isFiring = false;
            }
        }
    }

    void OnEnable()
    {
        isReloading = false;
        animator.SetBool("Reloading", false);

        fire1 = playerInput.Player.Fire1;
        fire1.Enable();
        fire1.performed += Fire1;
        fire1.canceled += CancelFire1; // Called when the player releases the fire1 button

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
        yield return new WaitForSeconds(reloadTime - 0.25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(1f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    public void Fire1(InputAction.CallbackContext context)
    {
        if (currentAmmo > 0 && (canHoldFire || !isFiring))
        {
            if (!isFiring)
            {
                // Start firing when the button is initially pressed
                isFiring = true;
                StartCoroutine(FireAtRate());
            }
        }
    }

    void CancelFire1(InputAction.CallbackContext context)
    {
        isFiring = false;
    }

    IEnumerator FireAtRate()
    {
        while (isFiring)
        {
            Shoot();
            yield return new WaitForSeconds(1 / fireRate);
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
#if !DEDICATED_SERVER
        instantiatedBullet.GetComponent<Projectile>().projectileSpeed.Value = projectileSpeed;
        instantiatedBullet.GetComponent<Projectile>().damage.Value = damage;
 #endif
    }
}