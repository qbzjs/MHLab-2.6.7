using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Services.Multiplay;

public class GameManager : NetworkBehaviour
{
    
    [SerializeField] private GameObject playerPrefab;
    
    private MultiplayEventCallbacks m_MultiplayEventCallbacks;
    private IServerEvents m_ServerEvents;
    
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;
    [SerializeField] private Transform spawnPoint4;
    [SerializeField] private Transform spawnPoint5;
    [SerializeField] private Transform spawnPoint6;
    [SerializeField] private Transform spawnPoint7;
    [SerializeField] private Transform spawnPoint8;
    [SerializeField] private Transform spawnPoint9;
    [SerializeField] private Transform spawnPoint10;
    
    [HideInInspector] public int playerCount;
    
    [SerializeField] private float targetMinutes;
    private float currentTime;
    private bool timerRunning = false;
    
#if DEDICATED_SERVER
    
    private async void Start()
    {
        playerCount = 0;
        
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;

        await MultiplayService.Instance.UnreadyServerAsync();
            
        Debug.Log("Unreadied Server");
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        SubscribeToEvents();
    }
    
    private async void SubscribeToEvents()
    {
         NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        
        // We must first prepare our callbacks like so:
         m_MultiplayEventCallbacks = new MultiplayEventCallbacks();
        // m_MultiplayEventCallbacks.Allocate += OnAllocate;
        m_MultiplayEventCallbacks.Deallocate += OnDeallocate;
        //m_MultiplayEventCallbacks.Error += OnError;
                
        // We must then subscribe.
        m_ServerEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(m_MultiplayEventCallbacks);
    }
#endif    
    
    private void OnClientDisconnectCallback(ulong clientId)
    {
        
#if !DEDICATED_SERVER
        Loader.Load(Loader.Scene.LobbyScene);
#endif
        
#if DEDICATED_SERVER
        Debug.Log("Client disconnected" + clientId);
    
        if (playerCount <= 1)
        {
            Application.Quit();
        }
    
        playerCount--;
#endif
    }
    
#if DEDICATED_SERVER

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Transform[] spawnPoints = new Transform[] { spawnPoint1, spawnPoint2, spawnPoint3, spawnPoint4, spawnPoint5, spawnPoint6, spawnPoint7, spawnPoint8, spawnPoint9, spawnPoint10 };
    
        for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsIds.Count; i++)
        {
            ulong clientId = NetworkManager.Singleton.ConnectedClientsIds[i];
    
            if (i < spawnPoints.Length)
            {
                Transform spawnPoint = spawnPoints[i];
                GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    
                playerCount ++;
    
                Debug.Log("Spawned Player at spawn point " + (i + 1));
            }
            else
            {
                Debug.LogWarning("Not enough spawn points for player " + (i + 1));
            }
        }
    }
    
    private void OnDeallocate(MultiplayDeallocation deallocation)
    {
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
        Application.Quit();
    }
#endif
} 