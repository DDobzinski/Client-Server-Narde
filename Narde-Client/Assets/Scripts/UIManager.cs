using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public GameObject mainMenu;
    public GameObject mainLayout;
    public GameObject lobbyListLayout;
    public GameObject createLayout;
    public GameObject lobbyLayout;
    public GameObject optionsLayout;
    public TMP_InputField usernameField;
    public TMP_Text menuHeader;
    public Button connectButton;
    public GameObject connectionPanel;
    public CreateLobby createLobbyScript;
    public LobbyManager lobbyScript;
    public JoinLobbyManager joinLobbyManagerScript;
    public string playerName;
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

    private void Start()
    {
        if(Client.instance.player != null)
        {
            instance.startMenu.SetActive(false);
            instance.connectionPanel.SetActive(false);
            instance.mainMenu.SetActive(true);
            if(Client.instance.player != null)
            {
                mainLayout.SetActive(false);
                createLayout.SetActive(false);
                lobbyListLayout.SetActive(false);
                lobbyLayout.SetActive(true);
                menuHeader.text = "Lobby";
                lobbyScript.lobbyName.text = Client.instance.player.lobby.GetName();
                lobbyScript.UpdatePlayerListUI();
                lobbyScript.UpdateSpectatorListUI();
            }
        }
    }

    public void ConnectToServer()
    {
        connectionPanel.SetActive(true);
        usernameField.interactable = false;
        connectButton.interactable = false;
        Client.instance.ConnectToServer();
    }
    public void OnConnectionConfirmed()
    {
        playerName = usernameField.text;
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
        lobbyLayout.SetActive(false);
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
        joinLobbyManagerScript.ClearButtons();
        ClientSend.RequestLobbies();
    }
    public void CreateOpen()
    {
        mainLayout.SetActive(false);
        menuHeader.text = "Create";
        createLobbyScript.lobbyNameInput.interactable = true;
        createLobbyScript.SpectatorSlider.interactable = true;
        createLobbyScript.TypeDropDown.interactable = true;
        createLobbyScript.CreateButton.interactable = true;
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
    public void MoveToLobby()
    {
        createLayout.SetActive(false);
        lobbyListLayout.SetActive(false);
        lobbyLayout.SetActive(true);
        menuHeader.text = "Lobby";
        lobbyScript.lobbyName.text = Client.instance.player.lobby.GetName();
        lobbyScript.UpdatePlayerListUI();
        lobbyScript.UpdateSpectatorListUI();
    }

    public void LeaveLobby()
    {
        ClientSend.LeaveLobby();
        Client.instance.player.lobby = null;
        Client.instance.player.currentStatus = PlayerStatus.Menu;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
