using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using Unity.Networking.Transport;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Services.Multiplay;

public class ServerDeallocate : NetworkBehaviour
{
        
    [SerializeField] private float countdownTime;
    private bool isCounting = true;
    
    private MultiplayEventCallbacks m_MultiplayEventCallbacks;
    private IServerEvents m_ServerEvents; 
    
    private async void Start()
    {
        
#if DEDICATED_SERVER
        StartTimer();
#endif
        
        SubscribeToEvents();
    }   
    
    private async void SubscribeToEvents()
    {
        
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        
 #if DEDICATED_SERVER
        // We must first prepare our callbacks like so:
         m_MultiplayEventCallbacks = new MultiplayEventCallbacks();
        // m_MultiplayEventCallbacks.Allocate += OnAllocate;
        m_MultiplayEventCallbacks.Deallocate += OnDeallocate;
        //m_MultiplayEventCallbacks.Error += OnError;
                
        // We must then subscribe.
        m_ServerEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(m_MultiplayEventCallbacks);
#endif
     }  
    
#if DEDICATED_SERVER
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        SubscribeToEvents();
    }
    
    private void OnDeallocate(MultiplayDeallocation deallocation)
    {
        Debug.Log("Deallocated");
        
        Application.Quit();
    }
    
#if DEDICATED_SERVER    
    
    private void StartTimer()
    {
        isCounting = true;
    }
    
#endif
    
    private void Update()
    {
        if (isCounting)
        {
            countdownTime -= Time.deltaTime;
            
            if (countdownTime <= 0)
            {
                countdownTime = 0;
                isCounting = false;
                TimerFinished();
            }
        }
    }

    private void TimerFinished()
    {
        Debug.Log("Timer Complete");

        Application.Quit();
    }
    
#endif

    public override void OnDestroy()
    {
         NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
         SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }    
    
    private void OnClientDisconnectCallback(ulong clientId)
    {
        Loader.Load(Loader.Scene.LobbyScene);
    }
    
    private void OnSceneUnloaded(Scene scene)
   {
       NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
       SceneManager.sceneUnloaded -= OnSceneUnloaded; 
   }
} 