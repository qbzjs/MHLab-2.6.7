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
    
    private float playerCount;
    private float maxPlayerCount;
    
    [SerializeField] private TMP_Text playerCountText;

    [SerializeField] private TMP_Text timerText;

    [SerializeField] private Dictionary<string, int> voteDictionary = new Dictionary<string, int>();
    
    public event EventHandler OnGameStarting;

    private NetworkVariable<float> timer = new NetworkVariable<float>(0f);
    private bool isRunning = false;

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

        Debug.Log("On Character Select Screen");

        timer.Value = 60f;
        
        playerCount = 0;
    }

    private async void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnnectedCallback;
        
#if DEDICATED_SERVER
        Debug.Log("DEDICATED_SERVER CHARACTER SELECT");

        Debug.Log("ReadyServerForPlayersAsync");
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        Camera.main.enabled = false;
#endif
    }

    [ServerRpc]
    public void SendVoteServerRpc(string map, string gameMode, ServerRpcParams rpcParams = default)
    {
        // Store the vote information on the server
        if (voteDictionary.ContainsKey(map))
        {
            voteDictionary[map]++;
        }
        else
        {
            voteDictionary.Add(map, 1);
        }

        if (voteDictionary.ContainsKey(gameMode))
        {
            voteDictionary[gameMode]++;
        }
        else
        {
            voteDictionary.Add(gameMode, 1);
        }

        // Debug statement to show the received vote
        Debug.Log($"Received vote for map: {map}, game mode: {gameMode}");

        ReceiveVoteClientRpc();
    }

    [ServerRpc]
    public void StartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        // Determine the most voted map and game mode
        string mostVotedMap = voteDictionary.OrderByDescending(x => x.Value).First().Key;
        int maxVotes = voteDictionary[mostVotedMap];
        List<string> highestVotedMaps = voteDictionary.Where(x => x.Value == maxVotes).Select(x => x.Key).ToList();
        string chosenMap = highestVotedMaps[UnityEngine.Random.Range(0, highestVotedMaps.Count)];

        // Split the chosen map and game mode from the final map choice
        string[] mapAndGameMode = chosenMap.Split();
        string finalMapChoice = mapAndGameMode[0];
        string finalGameModeChoice = mapAndGameMode[1];

        // Debug statement to show the chosen map and game mode
        Debug.Log($"Chosen map: {finalMapChoice}, Chosen game mode: {finalGameModeChoice}");

        // Store the final vote information on the server
        string finalVote = finalMapChoice + finalGameModeChoice;
        Debug.Log($"Final vote received: {finalVote}");

        // Load Map
        Loader.LoadNetwork(finalVote);
    }

    private void Update()
    {
        if (!IsServer)
        {
            timerText.text = timer.Value.ToString("F0");
            
            playerCountText.text = playerCount.ToString("F0") + "/" + maxPlayerCount.ToString("F0");
        }    
        
        if (IsServer)
        {
            if (isRunning)
            {
                timer.Value -= Time.deltaTime;

                if (timer.Value <= 0f)
                {
                    StartGameServerRpc();
                }
            }
        }
    }
    
    [ClientRpc]
    public void ReceiveVoteClientRpc()
    {
        playerCount++;
    }
    
    private void OnClientConnnectedCallback(ulong clientId)
    {
        maxPlayerCount++;
    }
}