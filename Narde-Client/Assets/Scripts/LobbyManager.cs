using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LobbyManager : MonoBehaviour
{
    public TMP_Text lobbyName; // Assign in Inspector
    public GameObject playerPanel; // Assign in Inspector
    public GameObject spectatorPanel; // Assign in Inspector
    public GameObject playerTextPrefab; // Assign a prefab with Text component
    public GameObject spectatorTextPrefab; // Assign a prefab with Text component

    public GameObject failPanel; // Assign a prefab with Text component
    public Button startGame; // Assign a prefab with Text component
    public TMP_Text startGameText; // Assign a prefab with Text component
    // Start is called before the first frame update

    public void UpdatePlayerListUI()
    {
        ClearPanel(playerPanel);
        List<string> playerNames = Client.instance.player.lobby.GetPlayers();
        foreach (string name in playerNames)
        {
            Debug.Log(name);
            AddTextToPanel(name, playerPanel, playerTextPrefab);
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
    
    public void StartGame()
    {
        ClientSend.StartGame();
    }

    public void RemoveFailPanel()
    {
        failPanel.SetActive(false);
        startGame.interactable = true;
    }
}
