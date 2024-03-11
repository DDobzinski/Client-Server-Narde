using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class CreateLobby : MonoBehaviour
{
    public TMP_InputField lobbyNameInput; // Reference to the input field
    public GameObject waitingPanel; // Reference to a waiting popup
    public GameObject wrongNameText;
    public TMP_Text SpectatorNr;
    public TMP_Text waitingPanelText;
    public Slider SpectatorSlider;
    public TMP_Dropdown TypeDropDown;
    public Button CreateButton;
    void Start()
    {
        //waitingPopup.SetActive(false); // Hide waiting popup initially
        wrongNameText.SetActive(false); // Hide waiting popup initially
        lobbyNameInput.interactable = true;
        SpectatorSlider.interactable = true;
        TypeDropDown.interactable = true;
        CreateButton.interactable = true;
    }

    public void CreateLobbyRequest()
    {
        string lobbyName = lobbyNameInput.text.Trim();
        if (string.IsNullOrEmpty(lobbyName) || lobbyName.Length > 25)
        {
            Debug.Log("Lobby name is required.");
            wrongNameText.SetActive(true);
            // Show an error message to the user
            return;
        }
        wrongNameText.SetActive(false);
        lobbyNameInput.interactable = false;
        SpectatorSlider.interactable =false;
        TypeDropDown.interactable = false;
        
        CreateButton.interactable =false;
        StartCoroutine(CreateLobbyTimeout(10f));  // Show waiting popup
        // Send lobby creation request to the server with lobbyName
        // On server response success, proceed to enter lobby UI
        // On failure, hide waiting popup and show error
        ClientSend.CreateLobbyRequestToServer();
    }

    IEnumerator CreateLobbyTimeout(float timeoutDuration)
    {
        waitingPanel.SetActive(true); // Show waiting popup
        waitingPanelText.text = "Creating lobby...";
        yield return new WaitForSeconds(timeoutDuration); // Wait for the timeout duration

        // Check if still waiting for a response
        if (waitingPanel.activeSelf)
        {
            waitingPanelText.text = "Lobby creation timed out.";
        }
    }

    public void UpdateSpectatorNr(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        SpectatorNr.text = intValue.ToString(); // Format as needed
    }

    public void CancelCreate()
    {
        lobbyNameInput.interactable = true;
        SpectatorSlider.interactable = true;
        TypeDropDown.interactable = true;
        CreateButton.interactable = true;
        waitingPanel.SetActive(false);
    }

    
    public void UpdateSpectatorSlider()
    {
        if (TypeDropDown.value == 2) // index of AI v AI option
        {
            SpectatorSlider.minValue = 1;
            if (SpectatorSlider.value < 1) SpectatorSlider.value = 1;
        }
        else
        {
            SpectatorSlider.minValue = 0;
        }
    }
}
