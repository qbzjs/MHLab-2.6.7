using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [HideInInspector] public NetworkVariable<float> projectileSpeed = new NetworkVariable<float>(value: 0f, NetworkVariableReadPermission.Everyone);
    [HideInInspector] public NetworkVariable<float> damage = new NetworkVariable<float>(value: 0f, NetworkVariableReadPermission.Everyone);
    
    private float bulletLife = 10f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 moveDirection = new Vector2(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
        rb.velocity = moveDirection.normalized * projectileSpeed.Value;
        
        StartCoroutine(DisableAfterDelay(bulletLife));
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        NetworkObject networkObject = GetComponent<NetworkObject>();
        
        if (collision.gameObject.tag != "Players" || collision.gameObject.tag != "Projectiles")
        {
            
            gameObject.SetActive(false);
            
#if DEDICATED_SERVER
            
            networkObject.Despawn();
#endif
            
        }
        
    }
    
    private IEnumerator DisableAfterDelay(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        
        
#if DEDICATED_SERVER
        NetworkObject networkObject = GetComponent<NetworkObject>();
        
        networkObject.Despawn();
#endif
        
    }
  
}
 