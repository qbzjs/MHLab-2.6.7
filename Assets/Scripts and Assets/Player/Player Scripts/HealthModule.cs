using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    value: 120,
    NetworkVariableReadPermission.Everyone);
    
    public override void OnNetworkSpawn()
    {
        health.Value = maxHealth;
    }
    
    private void OnCollisionEnter(Collision co)
    {
        if (co.gameObject.tag == "Bullet")
        {
            health.Value -= 15f;
        }
    }

    void Update()
    {
        if (health.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        transform.position = new Vector3(Random.Range(positionRange, -positionRange), 0, Random.Range(positionRange, -positionRange));
        Debug.Log("Player Died");
        health.Value = maxHealth;
    }  
}
