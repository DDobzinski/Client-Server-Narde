using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinLobbyManager : MonoBehaviour
{
    public GameObject lobbyButtonPrefab;
    public Transform lobbyListParent;
    public CanvasGroup lobbyListParentCanvas;
    public List<Lobby> lobbies = new List<Lobby>();
    public Button refresh;
    public GameObject waitingPanel;
    public TMP_Text waitingPanelText;
    public void FetchAndDisplayLobbies()
    {
        EnableButtons();
        // Example loop to simulate fetching lobbies. Replace with actual data fetching.
        foreach (Transform child in lobbyListParent) {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < lobbies.Count; i++)
        {
            GameObject buttonObj = Instantiate(lobbyButtonPrefab, lobbyListParent);
            buttonObj.name = "LobbyButton_" + lobbies[i].GetId();
            TMP_Text LobbyName = buttonObj.transform.Find("LobbyNameText").GetComponent<TMP_Text>();
            LobbyName.text = lobbies[i].GetName();

            TMP_Text LobbyType = buttonObj.transform.Find("LobbyTypeText").GetComponent<TMP_Text>();
            LobbyType.text = lobbies[i].GetLobbyType();

            Button joinPlayerButton = buttonObj.transform.Find("JoinLobbyButton").GetComponent<Button>();
            Button joinSpectatorButton = buttonObj.transform.Find("SpectateLobbyButton").GetComponent<Button>();

            int lobbyId = lobbies[i].GetId(); // Use actual lobby ID
            joinPlayerButton.onClick.AddListener(() => JoinLobby(lobbyId, false));
            joinSpectatorButton.onClick.AddListener(() => JoinLobby(lobbyId, true));
            LobbyStatus status = lobbies[i].GetStatus();
            if(status == LobbyStatus.PlayersOnly)
            {
                joinSpectatorButton.GetComponentInChildren<TMP_Text>().color = new Color32(128, 128, 128, 255);
                joinSpectatorButton.interactable = false;
                
            } 
            else if(status == LobbyStatus.SpectatorsOnly)
            {
                joinPlayerButton.GetComponentInChildren<TMP_Text>().color = new Color32(128, 128, 128, 255);
                joinPlayerButton.interactable = false;
                
            }
        }
        refresh.interactable = true;
    }

    void JoinLobby(int lobbyId, bool asSpectator)
    {
        waitingPanel.SetActive(false);
        StartCoroutine(JoinLobbyTimeout(10f));
        ClientSend.JoinLobby(lobbyId, asSpectator);
        
    }
    public void ClearButtons()
    {
        foreach (Transform child in lobbyListParent) {
            Destroy(child.gameObject);
        }
        lobbies.Clear();
    }

    public void Refresh()
    {
        ClearButtons();
        refresh.interactable = false;
        ClientSend.RequestLobbies();
    }

    public void DisableButtons()
    {
        lobbyListParentCanvas.interactable = false;
        refresh.interactable = false;
    }

    public void EnableButtons()
    {
        lobbyListParentCanvas.interactable = true;
        refresh.interactable = true;
    }

    IEnumerator JoinLobbyTimeout(float timeoutDuration)
    {
        waitingPanel.SetActive(true); // Show waiting popup
        DisableButtons();
        waitingPanelText.text = "Joining lobby...";
        yield return new WaitForSeconds(timeoutDuration); // Wait for the timeout duration

        // Check if still waiting for a response
        if (waitingPanel.activeSelf && waitingPanelText.text == "Joining lobby...")
        {
            waitingPanelText.text = "Joining lobby timed out. Try again";
        }
    }

    public void CancelJoin()
    {
        EnableButtons();
        waitingPanel.SetActive(false);
    }
}
