using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.MUIP;

public class ConnectingUI : MonoBehaviour {

    [SerializeField] private GameObject MatchMakerLooingForUI;
    [SerializeField] private GameObject matchmakingWindow;
    [SerializeField] private GameObject connectingUI;
    
    [SerializeField] private NotificationManager notification;

    private void Start() {
        MultiplayerManager.Instance.OnTryingToJoinGame += MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame += MultiplayerManager_OnFailedToJoinGame;
    }

    private void MultiplayerManager_OnFailedToJoinGame(object sender, System.EventArgs e) {
        Hide();
    }

    private void MultiplayerManager_OnTryingToJoinGame(object sender, System.EventArgs e) {
        Show();
    }

    private void Show() {
        connectingUI.SetActive(true);
        MatchMakerLooingForUI.SetActive(false);
    }

    private void Hide() {
        connectingUI.SetActive(false);
        matchmakingWindow.SetActive(false);
        MatchMakerLooingForUI.SetActive(false);
        matchmakingWindow.SetActive(false);
        CreateNotification();
    }

    private void OnDestroy() {
        MultiplayerManager.Instance.OnTryingToJoinGame -= MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame -= MultiplayerManager_OnFailedToJoinGame;
    }
    
    private void CreateNotification()
    {
        notification.title = "Connection Error";
        notification.description = "Failed to connect to the server. Please try again.";
        notification.UpdateUI(); // Update UI
        notification.Open(); // Open notification
    }    

}