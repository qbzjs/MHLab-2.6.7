using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [HideInInspector] public float projectileSpeed;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GetComponent<Rigidbody>().velocity = this.transform.forward * projectileSpeed;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        
 #if DEDICATED_SERVER
        
        NetworkObject networkObject = GetComponent<NetworkObject>();
        
        if (other.CompareTag("Bullet"))
        {
            networkObject.Despawn();
        }
        
#endif
        
    }    
}
 