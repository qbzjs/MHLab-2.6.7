using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.InputSystem;

public class WeaponManager : NetworkBehaviour
{
    [Header("Customize")]
    public float fireRate;
    public GameObject projectile;
    public float projectileSpeed;
    public Transform InistialTransform;
    public int maxAmmo;
    public float reloadTime;
    public Camera fpsCam;
    
    [Header("References")]
    public GameObject impactEffect;
    public Animator animator;
    
    [Header("Private Variables")]
    private int currentAmmo;
    private Vector3 destination;
    private bool isReloading = false;
    
    [Header("Input")]
    public PlayerMovementInput playerInput;
    private InputAction fire1;
    private InputAction reload;

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
     
    void Update()
    {
        if (isReloading)
        {
            return;
        }
    }
    
    public void StartReload(InputAction.CallbackContext context)
    {
        if (currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
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
        if (currentAmmo >= 0)
        {
            Shoot();
        }
    }    

    void Shoot()
    {
        currentAmmo--;
        
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGO, 2f);
            
            if (Physics.Raycast(ray, out hit))
            {
                destination = hit.point;
            } 
            else
            {
                destination = ray.GetPoint(1000);
            }
            
            SpawnBulletServerRPC(InistialTransform.position, InistialTransform.rotation);
        }
    }

    [ServerRpc]
    private void SpawnBulletServerRPC (Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject InstantiatedBullet = Instantiate(projectile, position, rotation);

        InstantiatedBullet.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
        
        InstantiatedBullet.GetComponent<Projectile>().projectileSpeed = projectileSpeed;
    }
}