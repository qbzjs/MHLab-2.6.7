using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Networking.Transport;

public class EscapeMenuController : MonoBehaviour
{
    [SerializeField] private GameObject escapeMenu;
    [SerializeField] private Button leaveButton;

    private bool isEscapeMenuActive;

    private void Start()
    {
        isEscapeMenuActive = false;
        escapeMenu.SetActive(false);
        
        leaveButton.onClick.AddListener(Leave);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle the escape menu on or off
            isEscapeMenuActive = !isEscapeMenuActive;

            // Set the active state of the escape menu GameObject based on the toggle
            escapeMenu.SetActive(isEscapeMenuActive);
        }
    }
    
    private void Leave()
    {
        NetworkManager.Singleton.Shutdown();
        Loader.Load(Loader.Scene.LobbyScene);
    }    
}
