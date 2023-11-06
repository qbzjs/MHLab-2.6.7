using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(HealthModule))]
public class RequiredScriptsMealth : Editor
{
    public override void OnInspectorGUI()
    {
        HealthModule script = (HealthModule)target;

        // Display standard script fields
        DrawDefaultInspector();

        // Display information about required scripts
        EditorGUILayout.LabelField("Required Modules:", "Movement Manager");
    }
}
#endif

public class HealthModule : NetworkBehaviour
{
    
    [Header("Customize")]
    
    public float maxHealth = 120;
    [SerializeField] private float positionRange = 70f;
    
    [Header("References")]
    
    [SerializeField] private MovementManager movementManager;
    
    [Header("Private Variables")]
   
    [HideInInspector]
    public NetworkVariable<float> health = new NetworkVariable<float>(
    value: 100,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    
    public override void OnNetworkSpawn()
    {
        health.Value = maxHealth;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(IsServer)
        {
            if (collision.gameObject.tag == "Projectiles")
            {
                Projectile projectileScript = collision.gameObject.GetComponent<Projectile>();
            
                health.Value -= projectileScript.damage.Value;

                if (health.Value <= 0)
                {
                    Die();
                }

                Debug.Log("Player Hit By Projectile From " + collision.gameObject.name + " For " + projectileScript.damage + " Damage");
                
                NetworkObject networkObject = collision.gameObject.GetComponent<NetworkObject>();
                
                networkObject.Despawn();
            }
            
        }    
    }

    void Die()
    {
        transform.position = new Vector3(Random.Range(positionRange, -positionRange), Random.Range(positionRange, -positionRange), 0);
        Debug.Log("Player Died");
        health.Value = maxHealth;
    }  
}
