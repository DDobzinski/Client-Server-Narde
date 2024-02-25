using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerStatus
    {
        Menu, // Player vs Player
        Player, // Player vs AI
        Spectator // AI vs AI
    }
public class Player
{
    public string playerName;
    public Lobby lobby = null;
    public PlayerStatus currentStatus = PlayerStatus.Menu; 
    public Player(string _playerName)
    {
        playerName = _playerName;
    }
}
