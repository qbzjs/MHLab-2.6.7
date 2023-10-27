using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;

public class LobbyMessageUI : MonoBehaviour {

    [SerializeField] private GameObject connectingUI;
    [SerializeField] private GameObject matchmakingWindow;
    
    [SerializeField] private NotificationManager notification;

    private void Start() {
        MultiplayerManager.Instance.OnFailedToJoinGame += MultiplayerManager_OnFailedToJoinGame;

        Hide();
    }

    private void MultiplayerManager_OnFailedToJoinGame(object sender, System.EventArgs e) {
        if (NetworkManager.Singleton.DisconnectReason == "") {
            CreateNotification();
        } else {
            CreateNotification();
        }
    }

    private void Show() {
        connectingUI.SetActive(true);
        matchmakingWindow.SetActive(true);
    }

    private void Hide() {
        connectingUI.SetActive(false);
        matchmakingWindow.SetActive(false);
    }

    private void OnDestroy() {
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