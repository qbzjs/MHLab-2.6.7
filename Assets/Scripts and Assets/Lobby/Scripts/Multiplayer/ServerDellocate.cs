using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Services.Multiplay;

public class ServerDeallocate : NetworkBehaviour
{
        
    [SerializeField] private float targetMinutes;
    private float currentTime;
    private bool timerRunning = false;
    
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
    
    private void Update()
    {
        if (timerRunning)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= targetMinutes * 60.0f)
            {
                TimerComplete();
            }
        }
    }
    
    private void StartTimer()
    {
        currentTime = 0.0f;
        timerRunning = true;
    }

    private void TimerComplete()
    {
        OnTimerComplete();
        
        timerRunning = false;
    }

    private void OnTimerComplete()
    {
        Debug.Log("Timer Complete");

        Application.Quit();
    }
#endif
    
    private void OnClientDisconnectCallback(ulong clientId)
    {
        Loader.Load(Loader.Scene.LobbyScene);
    }    
} 