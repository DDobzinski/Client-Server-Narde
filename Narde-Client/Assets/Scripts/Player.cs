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
    public bool turn = false;
    public int dice1 = 1;
    public int dice2 = 1;
    public string currentPlayerName = null;
    public int point1Colour = 0;
    public int point2Colour = 1;
    public int playerCheckerColour = 2;
    public int enemyCheckerColour = 3;
    public int boardColour = 4;
    public int sideColour = 5;
    public Player(string _playerName)
    {
        playerName = _playerName;
    }
    public void SaveSettings(int _point1Colour, int _point2Colour, int _playerCheckerColour, int _enemyCheckerColour, int _boardColour, int _sideColour)
    {
        point1Colour = _point1Colour;
        point2Colour = _point2Colour;
        playerCheckerColour = _playerCheckerColour;
        enemyCheckerColour = _enemyCheckerColour;
        boardColour = _boardColour;
        sideColour = _sideColour;
    }
}
