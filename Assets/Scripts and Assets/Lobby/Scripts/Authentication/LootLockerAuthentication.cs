using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using Michsky.MUIP;

public class LootLockerAuthentication : MonoBehaviour
{   
    public TMP_InputField logInEmailInputField;
    public TMP_InputField logInPasswordInputField;

    public TMP_InputField signUpEmailInputField;
    public TMP_InputField signUpPasswordInputField;

    public TMP_InputField playerNameInputField;

    public TMP_InputField resetEmailInputField;

    public GameObject Authentication;
    public GameObject LogInPage;
    public GameObject SignUpPage;
    public GameObject ResetPasswordPage;
    public GameObject VerifyPage;
    [SerializeField] private ModalWindowManager errorWindow;
    public GameObject LogInButton;
    public GameObject SignUpButton;
    public GameObject Wait;
    public GameObject setPlayerName;

    public GameObject signUpPage;
    public GameObject logInPage;

    public GameObject signUpButton;
    public GameObject logInButton;

    public GameObject buttonsUI;

    bool rememberMe = true;

    private string logInEmail;
    private string logInPassword;

    private string signUpEmail;
    private string signUpPassword;

    private string playerName;

    private string resetEmail;

    private bool canStart = true;

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    private void Start()
    {

#if DEDICATED_SERVER
        Authentication.SetActive(false);

        canStart = false;
#endif

        if (canStart == true)
        {
            StartFunctions();
        }
    }

    public void StartFunctions()
    {
        Debug.developerConsoleVisible = true;

        // Load saved email and password from PlayerPrefs
        if (PlayerPrefs.HasKey("SavedEmail") && PlayerPrefs.HasKey("SavedPassword"))
        {
            logInEmail = PlayerPrefs.GetString("SavedEmail");
            logInPassword = PlayerPrefs.GetString("SavedPassword");
        }

        // Set the input fields' text based on the saved data
        if (logInEmailInputField != null)
        {
            logInEmailInputField.text = logInEmail;
        }

        if (logInPasswordInputField != null)
        {
            logInPasswordInputField.text = logInPassword;
        }

        LogIn();
    }

    private void UpdateEmailFromInputField()
    {
        if (logInEmailInputField != null)
        {
            logInEmail = logInEmailInputField.text;
        }

        if (signUpEmailInputField != null)
        {
            signUpEmail = signUpEmailInputField.text;
        }

        if (resetEmailInputField != null)
        {
            resetEmail = resetEmailInputField.text;
        }
    }

    private void UpdatePasswordFromInputField()
    {
        logInPassword = logInPasswordInputField.text;
        
        signUpPassword = signUpPasswordInputField.text;

        if (signUpPassword.Length <= 8)
        {
            signUpButton.SetActive(false);
        }
        else
        {
            signUpButton.SetActive(true);
        }
    }

    private void SetPlayerNameFromInputField()
    {
        if (playerNameInputField != null)
        {
            playerName = playerNameInputField.text;
        }
    }

    public void Update()
    {
        UpdateEmailFromInputField();
        UpdatePasswordFromInputField();
        SetPlayerNameFromInputField();
    }

    public void SignUp()
    {
        LootLockerSDKManager.WhiteLabelSignUp(signUpEmail, signUpPassword, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("Error while creating user");

                Error("Create User Error", "Error while creating user. Please try again");

                return;
            }

            Debug.Log("User created successfully");

            VerifyPage.SetActive(true);
            SignUpPage.SetActive(false);

        });
    }

    public void LogIn()
    {
        LootLockerSDKManager.WhiteLabelLoginAndStartSession(logInEmail, logInPassword, rememberMe, response =>
        {
            if (!response.success)
            {
                if (!response.LoginResponse.success)
                {
                    Debug.Log("error while logging in");

                    Error("Login Error", "Error while logging in. Please try again");

                    LogInButton.SetActive(true);
                    SignUpButton.SetActive(true);

                    signUpPage.SetActive(false);
                    logInPage.SetActive(false);

                    Wait.SetActive(false);      
                }
                else if (!response.SessionResponse.success)
                {
                    Debug.Log("error while starting session");

                    Error("Session Start Error", "Error while starting the user sesion. Please try again");

                    LogInButton.SetActive(true);
                    SignUpButton.SetActive(true);

                    signUpPage.SetActive(false);
                    logInPage.SetActive(false);

                    Wait.SetActive(false);
                }
                return;
            }

            LootLockerSDKManager.CheckWhiteLabelSession(response =>
            {
                if (response)
                {
                    LootLockerSDKManager.StartWhiteLabelSession((response) =>
                    {
                        if (!response.success)
                        {
                            Debug.Log("error starting LootLocker session");

                            Error("Session Start Error", "Error while starting the user sesion. Please try again");

                            return;
                        }
                        
                        PlayerPrefs.SetString("PlayerULID", response.player_ulid);

                        Debug.Log("session started successfully");

                        LootLockerSDKManager.GetPlayerName((response) =>
                        {
                            if (response.success)
                            {
                                Debug.Log("Successfully retrieved player name: " + response.name);

                                if (response.name == "")
                                {
                                    setPlayerName.SetActive(true);
                                } else
                                {
                                    Authentication.SetActive(false);

                                    PlayerPrefs.SetString("SavedEmail", logInEmail);
                                    PlayerPrefs.SetString("SavedPassword", logInPassword);
                                    PlayerPrefs.Save();

                                    buttonsUI.SetActive(true);
                                }
                            }
                            else
                            {
                                Debug.Log("Error getting player name");
                            }
                        });
                    });
                }
                else
                {
                    Debug.Log("session is NOT valid, we should show the login form");

                    LogInButton.SetActive(true);
                    SignUpButton.SetActive(true);
                    Wait.SetActive(false);
                }
            });

        });
    }

    public void ResetPassword()
    {
        LootLockerSDKManager.WhiteLabelRequestPassword(resetEmail, (response) =>
        {
            if (!response.success)
            {
                Debug.Log("error requesting password reset");

                Error("Password Reset Error", "Error while requesting a password reset. Please try again");

                return;
            }

            Debug.Log("requested password reset successfully");

            LogInButton.SetActive(true);
            SignUpButton.SetActive(true);
            ResetPasswordPage.SetActive(false);

        });
    }

    public void SetPlayerName()
    {
        LootLockerSDKManager.SetPlayerName(playerName, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name");

                Authentication.SetActive(false);

                PlayerPrefs.SetString("SavedEmail", logInEmail);
                PlayerPrefs.SetString("SavedPassword", logInPassword);
                PlayerPrefs.Save();

                buttonsUI.SetActive(true);
            }
            else
            {
                Debug.Log("Error setting player name");
            }
        });
    }
    
    void Error(string title, string description)
    {
        errorWindow.titleText = title;
        errorWindow.descriptionText = description;
        errorWindow.UpdateUI();
        errorWindow.Open();
    }
}