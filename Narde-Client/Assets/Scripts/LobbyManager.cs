using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
public class LobbyManager : MonoBehaviour
{
    public TMP_Text lobbyName;
    public TMP_Text lobbyType;
    public GameObject playerPanel;
    public GameObject spectatorPanel;
    public GameObject playerTextPrefab;
    public GameObject spectatorTextPrefab; 
    public GameObject chatMessagePrefab;
    public Transform chatListParent; 
    public GameObject failPanel;
    public GameObject switchFailPanel;
    public Button startGame;
    public TMP_Text startGameText;
    public Button switchButton; 
    public TMP_Text switchButtonText; 
    public void UpdatePlayerListUI()
    {
        ClearPanel(playerPanel);
        List<string> playerNames = Client.instance.player.lobby.GetPlayers();
        foreach (string name in playerNames)
        {
            AddTextToPanel(name, playerPanel, playerTextPrefab);
        }

        if(Client.instance.player.lobby.GetLobbyType().Equals("AIvAI"))
        {
            AddTextToPanel("AdvancedAI", playerPanel, playerTextPrefab);
            AddTextToPanel("RandomAI", playerPanel, playerTextPrefab);
        }
        else if(Client.instance.player.lobby.GetLobbyType().Equals("PvAI"))
        {
            AddTextToPanel("AdvancedAI", playerPanel, playerTextPrefab);
        }

        LobbyStatus status = Client.instance.player.lobby.GetStatus();
        
        if(status == LobbyStatus.SpectatorsOnly || status == LobbyStatus.Full)
        {
            startGame.interactable = true;
            startGameText.color = new Color32(204, 157, 92, 255);
        }else
        {
            startGameText.color = new Color32(128, 128, 128, 255);
            startGame.interactable = false;
        }
        
    }

    // Call this method to update the spectator list UI
    public void UpdateSpectatorListUI()
    {
        ClearPanel(spectatorPanel);
        List<string> spectatorNames = Client.instance.player.lobby.GetSpectators();
        foreach (string name in spectatorNames)
        {
            AddTextToPanel(name, spectatorPanel, spectatorTextPrefab);
        }
    }

     public void SetSwitchButton()
    {
        PlayerStatus playerStatus = Client.instance.player.currentStatus;
        LobbyStatus lobbyStatus = Client.instance.player.lobby.GetStatus();
        if(playerStatus == PlayerStatus.Player)
        {
            if(lobbyStatus == LobbyStatus.Open || lobbyStatus == LobbyStatus.SpectatorsOnly)
            {
                switchButton.interactable = true;
                switchButtonText.color = new Color32(255, 242, 192, 255);
            }
            else
            {
                switchButton.interactable = false;
                switchButtonText.color = new Color32(128, 128, 128, 255);
            }
        }
        else if(playerStatus == PlayerStatus.Spectator)
        {
            if(lobbyStatus == LobbyStatus.Open || lobbyStatus == LobbyStatus.PlayersOnly)
            {
                switchButton.interactable = true;
                switchButtonText.color = new Color32(255, 242, 192, 255);
            }
            else
            {
                switchButton.interactable = false;
                switchButtonText.color = new Color32(128, 128, 128, 255);
            }
        }
    }

    public void ClearPanel(GameObject panel)
    {
        foreach (Transform child in panel.transform)
        {
            Destroy(child.gameObject);
        }
    }
    void AddTextToPanel(string text, GameObject panel, GameObject textPrefab)
    {
        GameObject newText = Instantiate(textPrefab, panel.transform);
        newText.GetComponent<TMP_Text>().text = text; // Or TextMeshProUGUI if using TMP
    }
    
    public void Switch()
    {
        ClientSend.SwitchStatus();
    }

    public void StartGame()
    {
        ClientSend.StartGame();
    }

    public void RemoveFailPanel()
    {
        failPanel.SetActive(false);
        startGame.interactable = true;
    }
    public void RemoveSwitchFailPanel()
    {
        switchFailPanel.SetActive(false);
        startGame.interactable = true;
    }
    public void SendChatMessage(InputField input)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            string message = input.text;
            input.text = "";
            ClientSend.SendChatMessage(message);
        }
    }
    public void AddChatMessage(string username, string message, bool player)
    {
        GameObject chatMessage = Instantiate(chatMessagePrefab, chatListParent);
        chatMessage.name = "ChatMessage";
        TMP_Text SenderName = chatMessage.transform.Find("PlayerName").GetComponent<TMP_Text>();
        SenderName.text = username;
        SenderName.color = player? new Color32(255, 240, 71, 255): new Color32(195, 195, 195, 255);

        TMP_Text MessageText = chatMessage.transform.Find("PlayerMessage").GetComponent<TMP_Text>();
        MessageText.text = message;
    }

    public void ClearChat()
    {
        foreach (Transform child in chatListParent) {
            Destroy(child.gameObject);
        }
    }
}
