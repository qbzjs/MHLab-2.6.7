using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float projectileSpeed;
    public float damage;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 moveDirection = new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        rb.velocity = moveDirection.normalized * projectileSpeed;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        #if DEDICATED_SERVER
        NetworkObject networkObject = GetComponent<NetworkObject>();
        
        if (other.gameObject.tag != "Players")
        {
            networkObject.Despawn();
        }
        #endif
    }
  
}
 