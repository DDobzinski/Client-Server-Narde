using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public GameObject mainMenu;
    public GameObject mainLayout;
    public GameObject lobbyListLayout;
    public GameObject createLayout;
    public GameObject optionsLayout;
    public TMP_InputField usernameField;
    public TMP_Text menuHeader;
    public Button connectButton;
    public GameObject connectionPanel;

    private void Awake()
    {
        if(instance == null)//ensures only one instance of the client class exists
        {
            instance = this;
            
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        connectionPanel.SetActive(true);
        usernameField.interactable = false;
        connectButton.interactable = false;
        Client.instance.ConnectToServer();
    }

    public static void OnConnectionConfirmed()
    {
        instance.startMenu.SetActive(false);
        instance.connectionPanel.SetActive(false);
        instance.mainMenu.SetActive(true);
    }
    public void CancelConnect()
    {
        connectionPanel.SetActive(false);
        mainLayout.SetActive(true);
        lobbyListLayout.SetActive(false);
        createLayout.SetActive(false);
        optionsLayout.SetActive(false);
        menuHeader.text = "Narde";
        startMenu.SetActive(true);
        mainMenu.SetActive(false);
        usernameField.interactable = true;
        connectButton.interactable = true;
        Client.instance.Disconnect();
    }
    public void Quit()
    {
        CancelConnect();
        Application.Quit();
    }
    public void LobbyListOpen()
    {
        mainLayout.SetActive(false);
        menuHeader.text = "Lobbies";
        lobbyListLayout.SetActive(true);
    }
    public void CreateOpen()
    {
        mainLayout.SetActive(false);
        menuHeader.text = "Create";
        createLayout.SetActive(true);
    }
    public void OptionsOpen()
    {
        mainLayout.SetActive(false);
        menuHeader.text = "Options";
        optionsLayout.SetActive(true);
    }
    public void Return(GameObject layout)
    {
        mainLayout.SetActive(true);
        menuHeader.text = "Narde";
        layout.SetActive(false);
    }
}
