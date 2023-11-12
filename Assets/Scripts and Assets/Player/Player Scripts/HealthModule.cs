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
    
    [Tooltip("The max amount of health the player can have. The recomended value is 100.")]
    [SerializeField] private float maxHealth;

    [Tooltip("The range that the player can be teleported when dying. The recommended value is 70")]
    [SerializeField] private float positionRange;
    
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
        if (IsOwner)
        {
            if (collision.gameObject.CompareTag("Projectiles"))
            {
                Projectile projectileScript = collision.gameObject.GetComponent<Projectile>();

#if !DEDICATED_SERVER
                health.Value -= projectileScript.damage.Value;
#endif

                if (health.Value <= 0)
                {
                    Die();
                }
            }
        }
    }

    void Die()
    {
        transform.position = new Vector3(Random.Range(positionRange, -positionRange), Random.Range(positionRange, -positionRange), 0);
        health.Value = maxHealth;
    }  
}
