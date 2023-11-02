using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ClientVoter : MonoBehaviour {

    [HideInInspector] public string map;
    [HideInInspector] public string gameMode;

    private bool hasVoted;
    
    public void GoToMainMenu()
    {
        LobbyManager.Instance.LeaveLobby();
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    public void SetMap(string mapName)
    {
        map = mapName;
    }
    
    public void SetGameMode(string gameModeName)
    {
        gameMode = gameModeName;
    }

    public void Vote()
    {
        if (!hasVoted)
        {
            // Call the method to send the vote information to the server
            VoteManager.Instance.SendVoteServerRpc(map, gameMode);
            
            Debug.Log(map + gameMode);
            
            hasVoted = true;
        }
    }
}