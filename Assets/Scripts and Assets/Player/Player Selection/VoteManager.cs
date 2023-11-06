using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplay;
using UnityEngine;
using System.Linq;
using TMPro;

public class VoteManager : NetworkBehaviour {
    
    public static event EventHandler OnInstanceCreated;

    public static VoteManager Instance { get; private set; }
    
    private NetworkVariable<float> playerCount = new NetworkVariable<float>(0f);
    private NetworkVariable<float> maxPlayerCount = new NetworkVariable<float>(0f);
    
    [SerializeField] private TMP_Text playerCountText;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float countdownTime;
    
    [SerializeField] private float everyoneReadyTimerValue;
    private bool everyoneIsReadyRunning = true;

    [SerializeField] private Dictionary<string, int> voteDictionary = new Dictionary<string, int>();
   

    private NetworkVariable<float> timer = new NetworkVariable<float>(
        value: 60f,
        NetworkVariableReadPermission.Everyone);
        
        
    private bool isRunning = true;
    
    private bool everyoneReady = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        OnInstanceCreated?.Invoke(this, EventArgs.Empty);
    }

    private async void Start()
    {
#if DEDICATED_SERVER
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallBack;
#endif
        
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER CHARACTER SELECT");

        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();
        
        isRunning = true;
        
        everyoneReady = false;
#endif
        
    }
    
     public override void OnDestroy()
     {
#if DEDICATED_SERVER
         NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnnectedCallback;
         NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallBack;
#endif
     }    

    [ServerRpc(RequireOwnership = false)]
    public void SendVoteServerRpc(string map, string gameMode, ServerRpcParams rpcParams = default)
    {
        // Store the vote information on the server
        string mapKey = "map_" + map;
        string gameModeKey = "gameMode_" + gameMode;
    
        if (voteDictionary.ContainsKey(mapKey))
        {
            voteDictionary[mapKey]++;
        }
        else
        {
            voteDictionary.Add(mapKey, 1);
        }
    
        if (voteDictionary.ContainsKey(gameModeKey))
        {
            voteDictionary[gameModeKey]++;
        }
        else
        {
            voteDictionary.Add(gameModeKey, 1);
        }

        // Debug statement to show the received vote
        Debug.Log($"Received vote for map: {map}, game mode: {gameMode}");
        
#if DEDICATED_SERVER
        ReceiveVote();
#endif
    }

    public void StartGame()
    {
        Debug.Log("Starting Game");
    
        // Stop the timer
        isRunning = false;
    
        // Determine the most voted map
        List<string> highestVotedMaps = voteDictionary.Keys.Where(key => key.StartsWith("map_"))
                                                          .OrderByDescending(key => voteDictionary[key])
                                                          .ToList();
        if (highestVotedMaps.Count == 0)
        {
            Debug.Log("No map has been chosen.");
    
            Application.Quit();
            return;
        }
    
        string chosenMap = highestVotedMaps[UnityEngine.Random.Range(0, highestVotedMaps.Count)].Substring(4); // Remove the "map_" prefix
        Debug.Log("Chosen Map: " + chosenMap);
    
        // Determine the most voted game mode
        List<string> highestVotedGameModes = voteDictionary.Keys.Where(key => key.StartsWith("gameMode_"))
                                                               .OrderByDescending(key => voteDictionary[key])
                                                               .ToList();
        if (highestVotedGameModes.Count == 0)
        {
            Debug.Log("No game mode has been chosen.");
    
            Application.Quit();
            return;
        }
    
        string chosenGameMode = highestVotedGameModes[UnityEngine.Random.Range(0, highestVotedGameModes.Count)].Substring(9); // Remove the "gameMode_" prefix
        Debug.Log("Chosen Game Mode: " + chosenGameMode);
    
        // Load Map
         Loader.LoadNetwork(chosenMap + "_" + chosenGameMode);
    }

    private void Update()
    {
        
#if DEDICATED_SERVER
        
        if (isRunning)
        {
            timer.Value -= Time.deltaTime;

            if (timer.Value <= 0f)
            {
                StartGame();
            }
        }
        
        if (everyoneIsReadyRunning == true)
        {
            everyoneReadyTimerValue -= Time.deltaTime;

            if (everyoneReadyTimerValue <= 0f)
            {
                everyoneReady = true;

                if (playerCount.Value == maxPlayerCount.Value)
                {
                    Debug.Log("Everyone Is Ready");
                    
                    StartGame();
                }    
            }
            
        }

#else

        if (timer.Value != 0)
        {
            timerText.text = timer.Value.ToString("F0");
        }
        else
        {
            timerText.text = "Starting!";
        }
            
        playerCountText.text = playerCount.Value.ToString("F0") + "/" + maxPlayerCount.Value.ToString("F0");

#endif

    }
    
#if DEDICATED_SERVER 
    
    public void ReceiveVote()
    {
        playerCount.Value += 1;
    }
      
    private void OnClientConnnectedCallback(ulong clientId)
    {
        maxPlayerCount.Value += 1;
    }
    
    private void OnClientDisconnectCallBack(ulong clientID)
    {
        maxPlayerCount.Value -= 1;
    }
    
#endif
    
}