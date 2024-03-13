using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    //Receives Welcome Packet from the Server
    public static void Welcome(Packet _packet)
    {
        
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        //Maybe check later if id matches
        Client.instance.player = new Player(UIManager.instance.usernameField.text);
        UIManager.instance.OnConnectionConfirmed();
        ClientSend.WelcomeReceived();
    }

    public static void LobbyCreated(Packet _packet)
    {
        
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();
        int lobbyid = _packet.ReadInt();
        string lobbyName = _packet.ReadString();
        int lobbyType = _packet.ReadInt();
        int lobbyStatus = _packet.ReadInt();
        Debug.Log($"Message from server: {_msg}");
        if(UIManager.instance.createLobbyScript.waitingPanel.activeSelf)
        {
            UIManager.instance.createLobbyScript.waitingPanel.SetActive(false);

            Client.instance.player.lobby = new Lobby(lobbyid, lobbyName, lobbyType, lobbyStatus);
            if(Client.instance.player.lobby.GetLobbyType().Equals("AIvAI"))
            {
                Client.instance.player.lobby.AddSpectator(Client.instance.player.playerName);
                Client.instance.player.currentStatus = PlayerStatus.Spectator;
            }
            else
            {
                Client.instance.player.lobby.AddPlayer(Client.instance.player.playerName);
                Client.instance.player.currentStatus = PlayerStatus.Player;
            }
            
            UIManager.instance.MoveToLobby();
        }
        else 
        {
            ClientSend.LeaveLobby();
        }
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }
    public static void ReceiveLobbies(Packet _packet)
    {
        int _myId = _packet.ReadInt();
        //Maybe check later if id matches
        
        int lobbyCount = _packet.ReadInt();
        UIManager.instance.joinLobbyManagerScript.lobbies.Clear();
        for(int i = 0; i < lobbyCount; i++)
        {
            int lobbyid = _packet.ReadInt();
            string lobbyName = _packet.ReadString();
            int lobbyType = _packet.ReadInt();
            int lobbyStatus = _packet.ReadInt();
            Lobby lobby = new Lobby(lobbyid, lobbyName, lobbyType, lobbyStatus);
            UIManager.instance.joinLobbyManagerScript.lobbies.Add(lobby);
        }
        Debug.Log($"Total Lobbies {UIManager.instance.joinLobbyManagerScript.lobbies.Count}");
        UIManager.instance.joinLobbyManagerScript.FetchAndDisplayLobbies();
    }

    public static void RejectJoin(Packet _packet)
    {
        int _myId = _packet.ReadInt();
        //Maybe check later if id matches

        int lobbyCount = _packet.ReadInt();
        UIManager.instance.joinLobbyManagerScript.lobbies.Clear();
        for(int i = 0; i < lobbyCount; i++)
        {
            int lobbyid = _packet.ReadInt();
            string lobbyName = _packet.ReadString();
            int lobbyType = _packet.ReadInt();
            int lobbyStatus = _packet.ReadInt();
            Lobby lobby = new Lobby(lobbyid, lobbyName, lobbyType, lobbyStatus);
            UIManager.instance.joinLobbyManagerScript.lobbies.Add(lobby);
        }
        if(UIManager.instance.joinLobbyManagerScript.waitingPanel.activeSelf)
        {
            UIManager.instance.joinLobbyManagerScript.waitingPanelText.text = "Position you tried to join is full!";
            UIManager.instance.joinLobbyManagerScript.FetchAndDisplayLobbies();
        }
        
    }

    public static void ConfirmJoin(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        int lobbyid = _packet.ReadInt();
        string lobbyName = _packet.ReadString();
        int lobbyType = _packet.ReadInt();
        int lobbyStatus = _packet.ReadInt();
        int playerStatus = _packet.ReadInt();
        int playerCount = _packet.ReadInt();
        List<string> playerNames = new List<string>();

        for(int i = 0; i< playerCount; i++)
        {
            playerNames.Add(_packet.ReadString());
        }
        int spectatorCount = _packet.ReadInt();
        List<string> spectatorNames = new List<string>();

        for(int i = 0; i< spectatorCount; i++)
        {
            spectatorNames.Add(_packet.ReadString());
        }
        if(UIManager.instance.joinLobbyManagerScript.waitingPanel.activeSelf)
        {
            Debug.Log($"Joined Lobby {lobbyid}");
            UIManager.instance.joinLobbyManagerScript.waitingPanel.SetActive(false);
            UIManager.instance.joinLobbyManagerScript.ClearButtons();
            Client.instance.player.lobby = new Lobby(lobbyid, lobbyName, lobbyType, lobbyStatus);

            for(int i = 0; i< playerCount; i++)
            {
                Client.instance.player.lobby.AddPlayer(playerNames[i]);
            }

            for(int i = 0; i< spectatorCount; i++)
            {
                Client.instance.player.lobby.AddSpectator(spectatorNames[i]);
            }
            if(playerStatus == 1) Client.instance.player.currentStatus = PlayerStatus.Player;
            else Client.instance.player.currentStatus = PlayerStatus.Spectator;
            UIManager.instance.MoveToLobby();
        }
        else 
        {
            ClientSend.LeaveLobby();
        }
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void UpdateLobby(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        int lobbyid = _packet.ReadInt();
        string lobbyName = _packet.ReadString();
        int lobbyType = _packet.ReadInt();
        int lobbyStatus = _packet.ReadInt();
        int playerCount = _packet.ReadInt();
        List<string> playerNames = new List<string>();

        for(int i = 0; i< playerCount; i++)
        {
            playerNames.Add(_packet.ReadString());
        }
        int spectatorCount = _packet.ReadInt();
        List<string> spectatorNames = new List<string>();

        for(int i = 0; i< spectatorCount; i++)
        {
            spectatorNames.Add(_packet.ReadString());
        }
        if(Client.instance.player != null && Client.instance.player.lobby != null)
        {
            Debug.Log($"Updated Lobby {lobbyid}");
            GameState state = Client.instance.player.lobby.GetState(); 
            Client.instance.player.lobby = new Lobby(lobbyid, lobbyName, lobbyType, lobbyStatus);
            Client.instance.player.lobby.SetStatus(state);
            for(int i = 0; i< playerCount; i++)
            {
                Client.instance.player.lobby.AddPlayer(playerNames[i]);
            }

            for(int i = 0; i< spectatorCount; i++)
            {
                Client.instance.player.lobby.AddSpectator(spectatorNames[i]);
            }
            if(Client.instance.player.lobby.GetState() == GameState.Menu)
            {
                UIManager.instance.UpdateLobbyUI();
            }
            
        }
        else 
        {
            ClientSend.LeaveLobby();
        }
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void ConfirmSwitch(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        if(Client.instance.player.currentStatus == PlayerStatus.Player)
        {
            Client.instance.player.currentStatus = PlayerStatus.Spectator;
        }
        else if(Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            Client.instance.player.currentStatus = PlayerStatus.Player;
        }
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }
    public static void DenySwitch(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        
        UIManager.instance.lobbyScript.switchFailPanel.SetActive(true);
        UIManager.instance.lobbyScript.startGame.interactable =false;
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void UpdateChat(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        string username = _packet.ReadString();
        string message = _packet.ReadString();
        bool player = _packet.ReadBool();
        UIManager.instance.lobbyScript.AddChatMessage(username, message, player);
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void AllowGame(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        bool _firstMove = _packet.ReadBool();
        int _dice1= _packet.ReadInt();
        int _dice2 = _packet.ReadInt();
        if(Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            string currentPlayerName = _packet.ReadString();
            Client.instance.player.currentPlayerName = currentPlayerName;
            
        }
        UIManager.instance.StartGame();
        Client.instance.player.lobby.SetStatus(GameState.InGame);
        Client.instance.player.turn = _firstMove;
        Client.instance.player.dice1 = _dice1;
        Client.instance.player.dice2 = _dice2;
        
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void DenyGame(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        UIManager.instance.lobbyScript.failPanel.SetActive(true);
        UIManager.instance.lobbyScript.startGame.interactable =false;
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void EndGame(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        string winnerName = _packet.ReadString();
        
        GameManager.Instance.FinishGame(winnerName);
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void UpdateGame(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        bool turn = _packet.ReadBool();
        int _dice1= _packet.ReadInt();
        int _dice2 = _packet.ReadInt();
        
        int moveCount = _packet.ReadInt();
        if(Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            string currentPlayerName = _packet.ReadString();
            Client.instance.player.currentPlayerName = currentPlayerName;
        }
        Client.instance.player.turn = turn;
        Client.instance.player.dice1 = _dice1;
        Client.instance.player.dice2 = _dice2;
        

        List<List<int>> list = new();
        if(turn || Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            
            for (int i = 0; i < moveCount; i++)
            {
                int start = _packet.ReadInt();
                int end = _packet.ReadInt();
                list.Add(new List<int>{start, end});
            }
            
            
            GameManager.Instance.UpdateBoard(list);
        }
        else if(!turn)
        {
            GameManager.Instance.DisableDice();
            GameManager.Instance.die1.Reset();
            GameManager.Instance.SetFinaleDice(Client.instance.player.dice1, Client.instance.player.dice2);
            GameManager.Instance.RollDice();
        }
        
        if(Client.instance.player.lobby.GetLobbyType().Equals("PvAI") && !turn && Client.instance.player.currentStatus == PlayerStatus.Player)
        {
            // Call this method to begin the process with a delay, e.g., 2 seconds
            GameManager.Instance.SwitchTurnText(false);
            for (int i = 0; i < moveCount; i++)
            {
                int start = _packet.ReadInt();
                int end = _packet.ReadInt();
                GameManager.Instance.agent.UpdateBoard(start, end);
            }
            GameManager.Instance.agent.SetDice(_dice1, _dice2);
            GameManager.Instance.WaitForAIToStartCalc(2f, GameManager.Instance.agent);
        }
       
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void InvalidTurn(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        
        GameManager.Instance.UndoTurn();
        GameManager.Instance.invalidTurnText.SetActive(true);
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }

    public static void HostLeft(Packet _packet)
    {
        
        int _myId = _packet.ReadInt();
        
        GameManager.Instance.FinishGame("Host Left", true);
        //Client.instance.myId = _myId;
        //Maybe check later if id matches
    }
    
}
