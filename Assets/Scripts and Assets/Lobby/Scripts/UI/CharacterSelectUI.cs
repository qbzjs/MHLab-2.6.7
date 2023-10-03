using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour {


    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;


    private void Awake() {
        mainMenuButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        readyButton.onClick.AddListener(() => {
            CharacterSelectReady.Instance.SetPlayerReady();
        });
    }
}