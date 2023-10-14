using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Services.Multiplay;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public Transform initialSpawnPoint;
    
    private MultiplayEventCallbacks m_MultiplayEventCallbacks;
    private IServerEvents m_ServerEvents;
    
    private async void Start()
    {
#if DEDICATED_SERVER
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

        await MultiplayService.Instance.UnreadyServerAsync();
            
        Debug.Log("Unreadied Server");
        
        // We must first prepare our callbacks like so:
        m_MultiplayEventCallbacks = new MultiplayEventCallbacks();
       // m_MultiplayEventCallbacks.Allocate += OnAllocate;
        m_MultiplayEventCallbacks.Deallocate += OnDeallocate;
        //m_MultiplayEventCallbacks.Error += OnError;
        m_MultiplayEventCallbacks.SubscriptionStateChanged += OnSubscriptionStateChanged;
            		
        // We must then subscribe.
        m_ServerEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(m_MultiplayEventCallbacks);
#endif
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject player = Instantiate(playerPrefab, initialSpawnPoint.position, initialSpawnPoint.rotation);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            Debug.Log("Spawned Player");
        }
    }
    
    private void OnDeallocate(MultiplayDeallocation deallocation)
    {
        Loader.Load(Loader.Scene.MainMenuScene);
    }
    
    private void OnSubscriptionStateChanged(MultiplayServerSubscriptionState state)
    {
    	switch (state)
    	{
    		case MultiplayServerSubscriptionState.Unsubscribed: /* The Server Events subscription has been unsubscribed from. */ break;
    		case MultiplayServerSubscriptionState.Synced: /* The Server Events subscription is up to date and active. */ break;
    		case MultiplayServerSubscriptionState.Unsynced: /* The Server Events subscription has fallen out of sync, the subscription tries to automatically recover. */ break;
    		case MultiplayServerSubscriptionState.Error: /* The Server Events subscription has fallen into an errored state and won't recover automatically. */ break;
    		case MultiplayServerSubscriptionState.Subscribing: /* The Server Events subscription is trying to sync. */ break;
    	}
    }
} 