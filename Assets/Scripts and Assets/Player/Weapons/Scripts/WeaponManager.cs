using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.RemoteConfig;

public class WeaponManager : NetworkBehaviour
{
    [Header("Customize")]
    [Tooltip("The value used to set how fast the bullet moves. The recommended value is 100.")]
    [SerializeField] private float projectileSpeed;
    
    [Tooltip("The value used to set the maximum amount of ammo the weapon can have. The recommended value is 31.")]
    [SerializeField] private float maxAmmo;
    
    [Tooltip("The value used to set how long it takes the gun to reload in seconds. The recommended value is 2.")]
    [SerializeField] private float reloadTime;
    
    [Tooltip("The value used to set how much damage the gun applies to the player on contact. The recommended value is 10.")]
    [SerializeField] private float damage;
    
    [Tooltip("The value used to set how fast the gun shoots bullets. The recommended value is 5.")]
    [SerializeField] private float fireRate;
    
    [Tooltip("The value used to set how many bullets get shot per time the shoot method gets called. The recommended value is 1.")]
    [SerializeField] private int projectilesPerShot;
    
    [Tooltip("The value used to set how many projectiles get spawned initially. The recommended value is 40.")]
    [SerializeField] private int poolSize;
    
    [Tooltip("The value used to set how much recoil the player experiences when shooting. The recommended value is 2.")]
    [SerializeField] private float recoilAmount;

    [Tooltip("The value used to set how much knockback the player experiences when shooting. The recommended value is 2.")]
    [SerializeField] private float knockbackForce;

    [Tooltip("The value used to set how much spread the bullets have when shooting. The recommended value is 1.")]
    [SerializeField] private float spreadAngle;

    [Tooltip("The audio clip played when the player shoots.")]
    [SerializeField] private AudioClip shootSound;
    
    [Tooltip("The boolean used to determine if the player is allowed to hold down the shoot button. The recommended value is true.")]
    [SerializeField] private bool canHoldFire;
    
    [Tooltip("The boolean used to determine if a laser sight should be displayed. The recommended value is true.")]
    [SerializeField] private bool useLaser;
    
    [Header("Cloud Config")]
    [Tooltip("The cloud key value that is used to set the max ammo variable. The recommended value is [weaponName]MaxAmmo.")]
    [SerializeField] private string maxAmmoConfigKey;
    
    [Tooltip("The cloud key value that is used to set the reload time variable. The recommended value is [weaponName]ReloadTime.")]
    [SerializeField] private string reloadTimeConfigKey;
    
    [Tooltip("The cloud key value that is used to set the projectile speed variable. The recommended value is [weaponName]Speed.")]
    [SerializeField] private string projectileSpeedConfigKey;
    
    [Tooltip("The cloud key value that is used to set the weapon damage variable. The recommended value is [weaponName]damage.")]
    [SerializeField] private string damageConfigKey;

    [Header("References")]
    [SerializeField] private MovementManager movementManager;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform laserPoint;
    [SerializeField] private struct userAttributes { }
    [SerializeField] private struct appAttributes { }
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Private Variables")]
    private bool isFiring;
    private bool canFire;
    private bool canShoot = true;
    private bool canShootWhileReloading = true;
    private float currentAmmo;
    private Vector3 destination;
    private bool isReloading;
    private float timeBetweenShots;
    private float lastShotTime;
    private const float SingleClickCooldown = 1f;
    private Quaternion initialRotation;
    private float totalRecoilRotation;
    private bool isRecoiling;

    [Header("Input")]
    [SerializeField] private PlayerMovementInput playerInput;
    private InputAction fire1;
    private InputAction reload;

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SpawnProjectileServerRPC(firePoint.position, firePoint.rotation);
        }
        
        playerInput = new PlayerMovementInput();
        FetchRemoteConfiguration();
        
        currentAmmo = maxAmmo;

        isReloading = false;
        animator.SetBool("Reloading", false);

        fire1 = playerInput.Player.Fire1;
        fire1.Enable();
        fire1.performed += Fire1;
        fire1.canceled += CancelFire1;

        reload = playerInput.Player.Reload;
        reload.Enable();
        reload.performed += StartReload;
        
        lineRenderer = laserPoint.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        
        if (useLaser)
        {
            laserPoint.gameObject.SetActive(true);
        }
        else
        {
            laserPoint.gameObject.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        fire1.performed -= Fire1;
        fire1.canceled -= CancelFire1;
        fire1.Disable();
        
        reload.performed -= StartReload;
        reload.Disable();
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
                break;
            case ConfigOrigin.Cached:
                break;
            case ConfigOrigin.Remote:
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
    }
    
    private void Update()
    {
        if (currentAmmo > 0 && (canHoldFire || isFiring))
        {
            if ((fire1.ReadValue<float>() > 0 || isFiring) && canShootWhileReloading)
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
        
        if (useLaser)
        {
            DrawLaser();
        }
    }
    
    public void DrawLaser()
    {
        int projectileLayer = LayerMask.NameToLayer("Projectile");
        int layerMask = ~(1 << projectileLayer);

        RaycastHit2D hit = Physics2D.Raycast(laserPoint.position, laserPoint.up, Mathf.Infinity, layerMask);

        lineRenderer.SetPosition(0, laserPoint.position);

        if (hit.collider != null)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, laserPoint.position + laserPoint.up * 100f);
        }
    }    
    
    public void StartReload(InputAction.CallbackContext context)
    {
        if (currentAmmo < maxAmmo && !isReloading)
        {
            StartCoroutine(Reload());
        }
        else
        {
            canFire = true;
        }
    }

    IEnumerator Reload()
    {
        animator.SetTrigger("Reload");
        canShootWhileReloading = false;
        isReloading = true;
        animator.SetBool("Reloading", true);
        isFiring = false;
        yield return new WaitForSeconds(reloadTime - 0.25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(1f);
        currentAmmo = maxAmmo;
        isReloading = false;
        canShootWhileReloading = true;
        canFire = true;
    }
    
    void Fire1(InputAction.CallbackContext context)
    {
        if (currentAmmo > 0 && (canHoldFire || isFiring))
        {
            if (context.started)
            {
                float timeSinceLastShot = Time.time - lastShotTime;
    
                if (!isFiring && (timeSinceLastShot >= SingleClickCooldown) && !isReloading)
                {
                    isFiring = true;
                    StartCoroutine(FireAtRate());
                }
            }
            else if (context.canceled)
            {
                isFiring = false;
            }
        }
        else if (!canHoldFire)
        {
            isFiring = context.started;
    
            if (currentAmmo > 0 && context.started && !isReloading)
            {
                Shoot();
            }
        }
    }

    void CancelFire1(InputAction.CallbackContext context)
    {
        isFiring = false;
    }

    IEnumerator FireAtRate()
    {
        while (isFiring && currentAmmo > 0)
        {
            if (canShoot)
            {
                Shoot();
    
                timeBetweenShots = 1 / fireRate;
    
                lastShotTime = Time.time;
    
                canShoot = false;
    
                yield return new WaitForSeconds(timeBetweenShots);
    
                canShoot = true;
            }
            else
            {
                yield return null;
            }
        }
    }
    
    void Shoot()
    {
        for (int i = 0; i < projectilesPerShot; i++)
        {
            currentAmmo--;
            
            SpawnProjectileServerRPC(firePoint.position, firePoint.rotation);
            
            // Recoil
            animator.SetTrigger("Shoot");
    
            // Knockback
            if (knockbackForce > 0)
            {
                Rigidbody2D rb = movementManager.rb;
                if (rb != null)
                {
                    Vector2 knockbackDirection = -transform.up;
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }
            }
    
            // Audio
            if (shootSound != null)
            {
                GetComponent<AudioSource>().PlayOneShot(shootSound);
            }
        }
    }
    
    [ServerRpc]
    private void InitialSpawnProjectileServerRPC(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject instantiatedProjectile = ObjectPoolManager.SpawnObject(projectile, position, rotation);
        
        NetworkObject networkObject = instantiatedProjectile.GetComponent<NetworkObject>();
        
        networkObject.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

#if !DEDICATED_SERVER
        Projectile projectileScript = instantiatedProjectile.GetComponent<Projectile>();
        
        projectileScript.projectileSpeed.Value = projectileSpeed;
        projectileScript.GetComponent<Projectile>().damage.Value = damage;
#endif
        
        ObjectPoolManager.ReturnObjectToPool(instantiatedProjectile);
    }

    [ServerRpc]
    private void SpawnProjectileServerRPC(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject instantiatedProjectile = ObjectPoolManager.SpawnObject(projectile, position, rotation);
    }
}