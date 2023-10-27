#if !UNITY_WEBGL

using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using TMPro;

public class VersionChecker : MonoBehaviour
{
    public string githubVersionURL;
    public string localVersion;
    public GameObject updatePrompt;
    public TMP_Text updateText;
    
    private string onlineVersion;

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
            onlineVersion = client.DownloadString(githubVersionURL).Trim();
        }

        // Compare local and online versions.
        if (localVersion != onlineVersion)
        { 
            // The versions do not match.
            // Enable the updatePrompt GameObject and set the updateText.
            updatePrompt.SetActive(true);
            updateText.text = "Your game is outdated. The launcer should have automatically updated your game but has failed. Try redownloading the launcher on the karmaa.tech website. If you see this message again, contact an admin on discord.\n\nPlease update your game to " + onlineVersion;
        }
        else
        {
            // The versions match.
            // Disable the updatePrompt GameObject (assuming it's initially enabled).
            updatePrompt.SetActive(false);
        }
    }
}

#endif