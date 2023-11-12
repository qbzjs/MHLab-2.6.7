using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();
    
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == objectToSpawn.name);
        
        // If the pool does not exist, create it
        
        if (pool == null)
        {
            pool = new PooledObjectInfo() {
                LookupString = objectToSpawn.name
            };
            
            ObjectPools.Add(pool);
        }
        
        // Chheck if there are any inactive objects in the pool
        
        GameObject spawnableObj = pool.inactiveObjects.FirstOrDefault();
        
        if (spawnableObj == null)
        {
            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);
        }else
        {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.inactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }    
        
        return spawnableObj;
    }
    
    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7);
        
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == goName);
        
        if (pool == null)
        {
            Debug.LogError("No pool exists for " + obj.name);
            return;
        }
        
        obj.SetActive(false);
        pool.inactiveObjects.Add(obj);
    } 
}

public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> inactiveObjects = new List<GameObject>();
}    