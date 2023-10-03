using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour {

    public GameObject MatchMakerLooingForUI;

    private void Start() {
        MultiplayerManager.Instance.OnTryingToJoinGame += MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame += MultiplayerManager_OnFailedToJoinGame;

        Hide();
    }

    private void MultiplayerManager_OnFailedToJoinGame(object sender, System.EventArgs e) {
        Hide();
        MatchMakerLooingForUI.SetActive(false);
    }

    private void MultiplayerManager_OnTryingToJoinGame(object sender, System.EventArgs e) {
        Show();
        MatchMakerLooingForUI.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void OnDestroy() {
        MultiplayerManager.Instance.OnTryingToJoinGame -= MultiplayerManager_OnTryingToJoinGame;
        MultiplayerManager.Instance.OnFailedToJoinGame -= MultiplayerManager_OnFailedToJoinGame;
    }

}