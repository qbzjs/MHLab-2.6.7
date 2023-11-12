 using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.UI;
using Michsky.MUIP;

public class MatchmakerUI : MonoBehaviour {


    public const string DEFAULT_QUEUE = "Queue-A";

    [SerializeField] private Transform lookingForMatchTransform;
    [SerializeField] private Transform connectingTransform;
    [SerializeField] private GameObject matchmakingWindow;
    [SerializeField] private NotificationManager notification;
    [SerializeField] private Button playButton;

    private CreateTicketResponse createTicketResponse;
    private float pollTicketTimer;
    private float pollTicketTimerMax = 1.1f;


    private void Awake() {
        lookingForMatchTransform.gameObject.SetActive(false);
        connectingTransform.gameObject.SetActive(false);
    }

    public async void FindMatch() {
        Debug.Log("FindMatch");
        
        playButton.interactable = false;

        lookingForMatchTransform.gameObject.SetActive(true);
        matchmakingWindow.SetActive(true);

        createTicketResponse = await MatchmakerService.Instance.CreateTicketAsync(new List<Unity.Services.Matchmaker.Models.Player> {
             new Unity.Services.Matchmaker.Models.Player(AuthenticationService.Instance.PlayerId, 
             new MatchmakingPlayerData {
             })
        }, new CreateTicketOptions { QueueName = DEFAULT_QUEUE });

        // Wait a bit, don't poll right away
        pollTicketTimer = pollTicketTimerMax;
    }

    [Serializable]
    public class MatchmakingPlayerData {
        public int Skill;
    }


    private void Update() {
        if (createTicketResponse != null) {
            // Has ticket
            pollTicketTimer -= Time.deltaTime;
            if (pollTicketTimer <= 0f) {
                pollTicketTimer = pollTicketTimerMax;

                PollMatchmakerTicket();
            }
        }
    }

    private async void PollMatchmakerTicket() {
        Debug.Log("PollMatchmakerTicket");
        TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(createTicketResponse.Id);

        if (ticketStatusResponse == null) {
            // Null means no updates to this ticket, keep waiting
            Debug.Log("Null means no updates to this ticket, keep waiting");
            return;
        }

        // Not null means there is an update to the ticket
        if (ticketStatusResponse.Type == typeof(MultiplayAssignment)) {
            // It's a Multiplay assignment
            MultiplayAssignment multiplayAssignment = ticketStatusResponse.Value as MultiplayAssignment;

            Debug.Log("multiplayAssignment.Status " + multiplayAssignment.Status);
            switch (multiplayAssignment.Status) {
                case MultiplayAssignment.StatusOptions.Found:
                    playButton.interactable = false;
                    CreateNotificationServerFound();
                    
                    createTicketResponse = null;

                    Debug.Log(multiplayAssignment.Ip + " " + multiplayAssignment.Port);

                    string ipv4Address = multiplayAssignment.Ip;
                    ushort port = (ushort)multiplayAssignment.Port;
                    NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipv4Address, port);

                    MultiplayerManager.Instance.StartClient();
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    // Still waiting...
                    playButton.interactable = false;
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    createTicketResponse = null;
                    playButton.interactable = true;
                    Debug.Log("Failed to create Multiplay server!");
                    CreateNotificationAllocationError();
                    lookingForMatchTransform.gameObject.SetActive(false);
                    matchmakingWindow.SetActive(false);
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    createTicketResponse = null;
                    Debug.Log("Multiplay Timeout!");
                    playButton.interactable = true;
                    CreateNotificationTimeout();
                    lookingForMatchTransform.gameObject.SetActive(false);
                    matchmakingWindow.SetActive(false);
                    break;
            }
        }

    }
    
    private void CreateNotificationAllocationError()
    {
        notification.title = "Server Allocation Error";
        notification.description = "Failed to allocate the server. Please try again.";
        notification.UpdateUI(); // Update UI
        notification.Open(); // Open notification
    }
    
    private void CreateNotificationTimeout()
    {
        notification.title = "Server Timeout";
        notification.description = "Matchmaking took too long to .complete Please try again.";
        notification.UpdateUI(); // Update UI
        notification.Open(); // Open notification
    }
    
    private void CreateNotificationServerFound()
    {
        notification.title = "Server Found";
        notification.description = "The server has been found. You will now be connected.";
        notification.UpdateUI(); // Update UI
        notification.Open(); // Open notification
    }
}