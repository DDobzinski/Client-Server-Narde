using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentMoveOption {
    public AgentPoint TargetPoint;
    public AgentPoint StartingPoint;
    public List<int> DiceUsed; // Contains 1, 2, or both for the dice roll(s) enabling this move
    public bool usesAllDice;
    public int MoveNumber;
    public AgentMoveOption PrevMove;
    public int totalCheckerCount;
    //Class for possible moves of dice
    public AgentMoveOption(AgentPoint startingPoint, AgentPoint targetPoint, List<int> diceUsed, int moveNumber, AgentMoveOption prevMove, bool UsesAllDice = false, int _totalCheckerCount = 15) 
        {
            StartingPoint = startingPoint;
            TargetPoint = targetPoint;
            DiceUsed = diceUsed;
            MoveNumber = moveNumber;
            usesAllDice = UsesAllDice;
            PrevMove = prevMove;
            totalCheckerCount = _totalCheckerCount;
        }
    }
public class AgentPoint
{
    public int id;
    public int checkerCount;
    public Checker.CheckerColor color;
    public List<AgentMoveOption> possibleMoves = new();
    public bool removePoint;
    public AgentPoint(int _id , int _amount = 0, bool _removePoint = false, Checker.CheckerColor _color = Checker.CheckerColor.None)
    {
        id = _id;
        checkerCount = _amount;
        removePoint = _removePoint;
        color = _color;
    }
    public bool HasCheckers()
    {
        return checkerCount > 0;
    }

    public void AddChecker(Checker.CheckerColor _color)
    {
        checkerCount += 1;
        color = _color;
    }


   public void RemoveTopChecker()
    {
        checkerCount -= 1;
        if(checkerCount == 0)
        {
            color = Checker.CheckerColor.None;
        }
    }


    public bool ContainsCheckerColor(Checker.CheckerColor _color)
    {
        if (checkerCount > 0)
        {
            return color == _color;
        }
        return false;
    }


    public bool CanRemoveChecker()
    {
        return removePoint;
    }
}
