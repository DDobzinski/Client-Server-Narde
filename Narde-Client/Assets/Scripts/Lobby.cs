using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LobbyStatus
    {
        Open, // Lobby has open slots for players
        PlayersOnly, // Lobby has no spots for spectators
        SpectatorsOnly, // Lobby is full, but spectator slots are available
        Full
    }

public enum GameState
{
    Menu, // Lobby has open slots for players
    InGame // Lobby is in a game, no slots available
}

public class Lobby
{
    private readonly int id;
    private readonly string name;
    private readonly string type;
    private LobbyStatus status;
    private GameState gameState = GameState.Menu;
    private readonly List<string> playerNames = new List<string>(); // Example player list
    private readonly List<string> spectatorNames = new List<string>(); // Example spectator list
    public Lobby(int _id, string _name, int _type, int _status)
    {
        id = _id;
        name = _name;
        type = _type switch
        {
            1 => "PvP",
            2 => "PvAI",
            _ => "AIvAI",
        };
        status = _status switch
        {
            1 => LobbyStatus.Open,
            2 => LobbyStatus.PlayersOnly,
            3 => LobbyStatus.SpectatorsOnly,
            _ => LobbyStatus.Full,
        };
    }

    public int GetId()
    {
        return id;
    }
    public string GetName()
    {
        return name;
    }
    public string GetLobbyType()
    {
        return type;
    }
    public LobbyStatus GetStatus()
    {
        return status;
    }

    public GameState GetState()
    {
        return gameState;
    }

    public void AddPlayer(string name)
    {
        playerNames.Add(name);
    }

    public void AddSpectator(string name)
    {
        spectatorNames.Add(name);
    }

    public List<string> GetPlayers()
    {
        return playerNames;
    }

    public List<string> GetSpectators()
    {
        return spectatorNames;
    }
    public void SetStatus(GameState _state)
    {
        gameState = _state;
    }
}

