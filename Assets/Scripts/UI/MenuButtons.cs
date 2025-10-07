using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public Button singleplayer;
    public Button profile;
    public Button settings;
    public Button exit;

    private void Start()
    {
        singleplayer.onClick.AddListener(onSingleplayerClicked);
        profile.onClick.AddListener(onProfileClicked);
        settings.onClick.AddListener(onSettingsClicked);
        exit.onClick.AddListener(onExitClicked);


    }

    void onSingleplayerClicked()
    {
        Debug.Log("Singleplayer clicked");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SingleplayerMenu");
        profile.gameObject.SetActive(false);


    }
    void onProfileClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Profile");
    }
    void onSettingsClicked()
    {
        Debug.Log("Settings clicked");
    }
    void onExitClicked()
    {
        Debug.Log("Exit clicked");
        UserSession.EndSession();
        Application.Quit();
    }
}
