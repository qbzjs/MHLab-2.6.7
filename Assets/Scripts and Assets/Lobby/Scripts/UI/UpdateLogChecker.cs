#if !UNITY_WEBGL

using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using TMPro;

public class UpdateLogChecker: MonoBehaviour
{
    public string githubVersionURL;
    public TMP_Text updateText;
    
    private string updateLogText;

    void Awake()
    {
        // Start the version checking coroutine.
        StartCoroutine(CheckVersion());
    }

    IEnumerator CheckVersion()
    {
        // Download the online version from GitHub.
        using (WebClient client = new WebClient())
        {
            yield return null;
            updateLogText = client.DownloadString(githubVersionURL).Trim();
        }

        updateText.text = updateLogText;
    }
}

#endif