using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuDedicatedServer : MonoBehaviour {

#if DEDICATED_SERVER
    private void Start() 
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount  = 0;
        

        Debug.Log("DEDICATED_SERVER");
        Loader.Load(Loader.Scene.LobbyScene);
    }
#endif

}