using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEditor; // At the top of your file
using UnityEngine.SceneManagement;
public class MoveOption {
    public Point TargetPoint;
    public Point StartingPoint;
    public List<int> DiceUsed; // Contains 1, 2, or both for the dice roll(s) enabling this move
    public bool usesAllDice;
    public int MoveNumber;
    public MoveOption PrevMove;
    //Class for possible moves of dice
    public MoveOption(Point startingPoint, Point targetPoint, List<int> diceUsed, int moveNumber, MoveOption prevMove, bool UsesAllDice = false) 
        {
            StartingPoint = startingPoint;
            TargetPoint = targetPoint;
            DiceUsed = diceUsed;
            MoveNumber = moveNumber;
            usesAllDice = UsesAllDice;
            PrevMove = prevMove;
        }
    }
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Checker.CheckerColor playerColor = Checker.CheckerColor.Player;
    private Point selectedPoint;
    public GameObject playerCheckerPrefab;
    public GameObject enemyCheckerPrefab;
    public Dice die1;
    public Dice die2;
    public Remover remover;
    public Button endTurnButton;
    public Button surrenderButton;
    public Button stopSpectatingButton;
    public TMP_Text endturnButtonText;
    public CanvasGroup board;
    public CanvasGroup UI;
    public GameObject EndPanel;
    public TMP_Text WinnerText;
    public TMP_Text TurnText;
    public List<Point> Points; // Assign the transform of each Point point in the editor
    public List<int> startingCheckersCountPlayer; // Number of Player 1 checkers at each Point point at the start
    public List<int> startingCheckersCountEnemy; // Number of Player 2 checkers at each Point point at the start
    private bool diceRolled = false;
    private int diceRollResult1 = 0;
    private int diceRollResult2 = 0;
    public List<MoveOption> MovesDone = new();
    private int diceUseCount1 = 0;
    private int diceUseCount2 = 0;
    private bool firstTurnOfTheGame = true;
    private int moveOfTheHeadCount = 0;
    private int moveOfTheHeadLimit = 1;
    private bool highestDiePossible = false;
    private int moveNr = 1;
    private int maxMovesPossible = 0;
    private MoveOption lastMoveDone;
    public Agent agent;
    bool removalStage = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupBoard();
        if(Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            endTurnButton.gameObject.SetActive(false);
            surrenderButton.gameObject.SetActive(false);
            stopSpectatingButton.gameObject.SetActive(true);
            die1.DisableDice();
        }
        else
        {
            SwitchTurnText(Client.instance.player.turn);
        }
        if(Client.instance.player.lobby.GetLobbyType().Equals("PvAI"))
        {
            agent = new();
            if(!Client.instance.player.turn)
            {
                agent.SetDice(Client.instance.player.dice1, Client.instance.player.dice2);
                WaitForAIToStartCalc();
            }
        }
    }

     void SetupBoard()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Point PointScript = Points[i].GetComponent<Point>();
            if (PointScript == null) continue;

            // Instantiate Player 1 checkers
            for (int j = 0; j < startingCheckersCountPlayer[i]; j++)
            {
                GameObject newChecker = Instantiate(playerCheckerPrefab, Points[i].transform);
                PositionChecker(newChecker, j, PointScript.pointType); // Offset by Player 1 checkers
                Points[i].AddChecker(newChecker);
            }

            // Instantiate Player 2 checkers
            for (int j = 0; j < startingCheckersCountEnemy[i]; j++)
            {
                GameObject newChecker = Instantiate(enemyCheckerPrefab, Points[i].transform);
                PositionChecker(newChecker, j, PointScript.pointType); // Offset by Player 1 checkers
                Points[i].AddChecker(newChecker);
            }
        }
    }

    void PositionChecker(GameObject checker, int index, PointType pointType)
    {
        Checker checkerScript = checker.GetComponent<Checker>();
        if (checkerScript != null)
        {
            checkerScript.PositionCheckerInStack(index, pointType);
        }
    }

    public void SelectTriangle(Point point)
    {
        if (!diceRolled) return; // Exit if no dice roll has occurred
        if (selectedPoint != null)
        {
            if(point.isHighlighted)
            {
                if(selectedPoint!= point)
                {
                    MoveChecker(selectedPoint, point);
                    MovesDone.Add(FindMoveForPoint(selectedPoint, point));
                    lastMoveDone = MovesDone[^1];
                    List<int> UsedDice = lastMoveDone.DiceUsed;
                    moveNr += UsedDice.Count;
                    
                    if(point.CanRemoveChecker() && !removalStage)
                    {
                        removalStage = AreAllCheckersInLastSix(Points, playerColor);
                    }

                    for(int i = 0; i < UsedDice.Count; i++)
                    {
                        if(UsedDice[i] == 1)
                        {
                            diceUseCount1 -= 1;
                        } 
                        else 
                        {
                            diceUseCount2-= 1;
                        }
                    }

                    
                    if(selectedPoint == Points[0])
                    {
                        moveOfTheHeadCount += 1;
                    }
                    if(firstTurnOfTheGame && diceRollResult1 == diceRollResult2)
                    {
                        if((diceRollResult1 == 4 && point == Points[8]) || ( diceRollResult1 == 3 && point == Points[9]) || ( diceRollResult1 == 6 && point == Points[6]))
                        {
                            moveOfTheHeadLimit = 2;
                        }
                    }
                }

                if(moveNr == maxMovesPossible + 1) MakeButtonInteractable(true);
                selectedPoint = null;
                
                
                UnhighlightAllPoints();
                
            }
            
        }
        else if (point.HasCheckers() && point.ContainsCheckerColor(playerColor) && (diceUseCount1 > 0 || diceUseCount2 > 0))
        {
            if(point == Points[0] && moveOfTheHeadCount == moveOfTheHeadLimit) return;
            selectedPoint = point;
            
            point.SetHighlight(true, true);
            // Optional: Highlight other triangles where a move is possible
            HighlightPossibleMoves(point);            
        }
    }

    void MoveChecker(Point fromPoint, Point toPoint)
    {
        GameObject checkerToMove = fromPoint.RemoveTopChecker();
        if (checkerToMove != null)
        {
            // Update the parent of the checker to the new triangle
            checkerToMove.transform.SetParent(toPoint.transform, false);

            // Add the checker to the new triangle's stack
            toPoint.AddChecker(checkerToMove);

            // Sort checkers in both triangles
            fromPoint.SortCheckers();
            toPoint.SortCheckers();
        }
    }

    void SimulateMoveChecker(Point fromPoint, Point toPoint)
    {
        GameObject checkerToMove = fromPoint.SimulateRemoveTopChecker();
        if (checkerToMove != null)
        {
            // Add the checker to the new triangle's stack
            toPoint.SimulateAddChecker(checkerToMove);
        }
    }

    void HighlightPossibleMoves(Point currentPoint)
    {
        if (!diceRolled) return; // Exit if no dice roll has occurred

        // Highlight based on each dice result
        
        foreach(var move in currentPoint.possibleMoves)
        {

            if(move.PrevMove != lastMoveDone) continue;
            if((diceUseCount1 > 0 && move.DiceUsed.Count == 1 && move.DiceUsed[0] == 1) || (diceUseCount2 > 0 && move.DiceUsed.Count == 1 && move.DiceUsed[0] == 2))
            {
                if(move.MoveNumber != moveNr) continue;
                if(move.TargetPoint != null) move.TargetPoint.SetHighlight(true);
                else if (removalStage) remover.SetRemovalAllowed(true);
            }
            
            else if(diceUseCount1 > 0 && diceUseCount2 > 0 && move.DiceUsed.Count == 2 && move.TargetPoint != null)
            {
                move.TargetPoint.SetHighlight(true);
            }
            else if(diceRollResult1 == diceRollResult2 && diceUseCount1 >= move.DiceUsed.Count && move.TargetPoint != null)
            {
                move.TargetPoint.SetHighlight(true);
            }         
        }
        
    }
    private void CalculateAllowedMoves(List<Point> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor, int dice1uses, int dice2uses, bool simRemovalStage, int CalculateHeadMoveCount = 0, int CalculateHeadMoveLimit = 1, MoveOption moveCalculated = null)
    {
        List<MoveOption> possibleMoves = new();
        bool allMovesPossible = false;
        if(firstTurnOfTheGame && diceRollResult1 == diceRollResult2 && (diceRollResult1 == 3 || diceRollResult1 == 4 || diceRollResult1 == 6)) CalculateHeadMoveLimit = 2;
        if(dice1uses == 0 && dice2uses == 0)
        {
            return;
        }
        foreach (var point in copiedPoints)
        {
            //Debug.Log(copiedPoints.IndexOf(point));
            if(point.HasCheckers() && point.ContainsCheckerColor(_playerColor))
            {   
                
                //Debug.Log(copiedPoints.IndexOf(point));
                if(point == copiedPoints[0] && CalculateHeadMoveCount == CalculateHeadMoveLimit) continue;   
                    bool dieMove1Possible = false;
                    bool dieMove2Possible = false;
                    if(dice1uses > 0) dieMove1Possible = CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, diceRollResult1, new List<int> { 1 }, dice1uses, dice2uses, simRemovalStage, moveCalculated);
                    if(dice2uses > 0) dieMove2Possible = CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, diceRollResult2, new List<int> { 2 }, dice1uses, dice2uses, simRemovalStage, moveCalculated);
                    if(diceRollResult1 >= diceRollResult2 && dieMove1Possible && !highestDiePossible) highestDiePossible = true;
                    else if(diceRollResult2 >= diceRollResult1  && dieMove2Possible && !highestDiePossible) highestDiePossible = true;
                    
                    if(dieMove1Possible || dieMove2Possible)
                    {
                        if(dice1uses > 0 && dice2uses > 0)
                        {
                            if(!dieMove1Possible && dieMove2Possible)
                            {
                                CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, diceRollResult1+diceRollResult2, new List<int> { 2,1 }, dice1uses, dice2uses, simRemovalStage,moveCalculated);
                            }
                            else
                            {
                                CalculatetMoveOptions(copiedPoints,_playerColor, _enemyColor, possibleMoves, point, diceRollResult1+diceRollResult2, new List<int> { 1,2 }, dice1uses, dice2uses, simRemovalStage,moveCalculated);
                            }
                            
                        }
                        if(diceRollResult1 == diceRollResult2)
                        {
                            
                            List<int> list;
                            for(int i = 2; i <= dice1uses; i++)
                            {
                                
                                if(!dieMove1Possible) break;
                                list = Enumerable.Repeat(1, i).ToList();
                                
                                dieMove1Possible = CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, diceRollResult1 * i, list, dice1uses, dice2uses, simRemovalStage, moveCalculated);
                            }
                        }
                    } 
            }
        }
        int tempDice1Uses;
        int tempDice2Uses;
        int calculateHeadMoveCountTemp;
        bool removalStageTemp;
        foreach (var move in possibleMoves)
        {
            //Debug.Log("Target" + Points.IndexOf(move.TargetPoint));
            tempDice1Uses = dice1uses;
            tempDice2Uses = dice2uses;
            calculateHeadMoveCountTemp = CalculateHeadMoveCount;
            removalStageTemp = simRemovalStage;
            if(move.DiceUsed.Count == 2 && diceRollResult1 != diceRollResult2)
            {   
                tempDice1Uses -= 1;
                tempDice2Uses -= 1;
            }
            else if(move.DiceUsed.Count >= 2)
            {
                tempDice1Uses -= move.DiceUsed.Count;
            }
            else
            {
                if(move.DiceUsed[0] == 1) tempDice1Uses -= 1;
                else tempDice2Uses -= 1;
            }
            if(tempDice1Uses == 0 && tempDice2Uses == 0)
            {
                move.usesAllDice = true;
                allMovesPossible = true;
                if(moveCalculated != null)
                {
                    moveCalculated.usesAllDice = true;
                }
            }
            else if(move.TargetPoint == null)
            {
                GameObject checker = move.StartingPoint.SimulateRemoveTopChecker();
                CalculateAllowedMoves(copiedPoints, _playerColor, _enemyColor, tempDice1Uses, tempDice2Uses, removalStageTemp, calculateHeadMoveCountTemp, CalculateHeadMoveLimit, move);
                
                if (move.usesAllDice)
                {
                    if(moveCalculated != null) moveCalculated.usesAllDice = true;
                    allMovesPossible = true;
                }
                move.StartingPoint.SimulateAddChecker(checker);
            }
            else
            {
                SimulateMoveChecker(move.StartingPoint ,move.TargetPoint);
                if(move.StartingPoint == copiedPoints[0])
                {
                    calculateHeadMoveCountTemp += 1;
                }
                if(move.TargetPoint.CanRemoveChecker() && !removalStageTemp)
                    {
                        removalStageTemp = AreAllCheckersInLastSix(copiedPoints, _playerColor);
                    }

                CalculateAllowedMoves(copiedPoints, _playerColor, _enemyColor, tempDice1Uses, tempDice2Uses, removalStageTemp,calculateHeadMoveCountTemp, CalculateHeadMoveLimit, move);
                
                if (move.usesAllDice)
                {
                    if(moveCalculated != null) moveCalculated.usesAllDice = true;
                    allMovesPossible = true;
                }
                SimulateMoveChecker(move.TargetPoint, move.StartingPoint); 
            }
            
            int startIndex = copiedPoints.IndexOf(move.StartingPoint);
            
            
            copiedPoints[startIndex].possibleMoves.Add(move);
        }
        
        if(moveCalculated == null)
        {
            int currentMaxMoves = 0;
            foreach(var point in copiedPoints)
            {   

                if(allMovesPossible)
                {
                    point.possibleMoves.RemoveAll(move => !move.usesAllDice);
                }
                else
                {   
                    if(diceRollResult1 >= diceRollResult2 && highestDiePossible)
                    {
                        point.possibleMoves.RemoveAll(move => move.DiceUsed[0] != 1);
                    }
                    else if(highestDiePossible)
                    {
                        point.possibleMoves.RemoveAll(move => move.DiceUsed[0] != 2);
                    }
                }
                if(point.possibleMoves.Count > 0)
                {
                    int maxMoveInPoint = point.possibleMoves.Max(move => move.MoveNumber);
                    currentMaxMoves = Math.Max(currentMaxMoves, maxMoveInPoint);
                }
            }
            maxMovesPossible = currentMaxMoves;
            if(maxMovesPossible == 0 && Client.instance.player.turn) MakeButtonInteractable(true);
        }
    
        
    }

    private bool CalculatetMoveOptions(List<Point> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor, List<MoveOption> possibleMoves, Point startPoint, int moveDistance, List<int> diceUsed,  int dice1uses, int dice2uses, bool simRemovalStage, MoveOption prevMove = null)
    {
        
        int startIndex = copiedPoints.IndexOf(startPoint);

        if(simRemovalStage && startPoint.CanRemoveChecker() && diceUsed.Count == 1)
        {
            if(24 - startIndex == moveDistance)
            {
                //Debug.Log("Remove move added for startIndex:" + startIndex);
                MoveOption newMoveOption = new(startPoint, null, diceUsed, diceUseCount1 + diceUseCount2 - dice1uses - dice2uses + 1, prevMove);
                possibleMoves.Add(newMoveOption);
            }
            else if( 24 - startIndex < moveDistance)
            {
                bool previousAreEmpty = true;
                for(int i = startIndex-1; i >= copiedPoints.Count - 6; i--)
                {
                    if(copiedPoints[i].HasCheckers() && copiedPoints[i].ContainsCheckerColor(_playerColor)) 
                    {
                        previousAreEmpty = false;
                        break;
                    } 
                }
                if(previousAreEmpty)
                {
                    MoveOption newMoveOption = new(startPoint, null, diceUsed, diceUseCount1 + diceUseCount2 - dice1uses - dice2uses + 1, prevMove);
                    possibleMoves.Add(newMoveOption);
                }
            }
        }
        
        int targetIndex = startIndex + moveDistance; // Assuming a circular board
        if(targetIndex > copiedPoints.Count -1) return false;
        Point targetPoint = copiedPoints[targetIndex];
        
        // Check if the target point is open and intermediate points are open if needed
        if (IsMoveValid(startPoint, targetPoint, copiedPoints, _playerColor, _enemyColor))
        {   
            AddMoveOption(possibleMoves, startPoint, targetPoint, diceUsed, diceUseCount1 + diceUseCount2 - dice1uses - dice2uses + 1, prevMove);
            //MoveOption newMoveOption = new MoveOption(startPoint, targetPoint, diceUsed, 1);
            //startPoint.possibleMoves.Add(newMoveOption);
            return true;
        }
        
        return false;
    }

    void AddMoveOption(List<MoveOption> possibleMoves, Point startPoint, Point targetPoint, List<int> diceUsed, int turnNumber, MoveOption prevMove = null) {
        
        MoveOption newMoveOption = new(startPoint, targetPoint, diceUsed, turnNumber, prevMove);
        possibleMoves.Add(newMoveOption);
    }

    private MoveOption FindMoveForPoint(Point currentPoint, Point targetPoint) {
        return currentPoint.possibleMoves.FirstOrDefault(move => move.TargetPoint == targetPoint && move.PrevMove == lastMoveDone);
    }

    bool IsMoveValid(Point startPoint, Point targetPoint, List<Point> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor)
    {
        // Implement logic to check if a move is valid based on Narde rules
        // This includes checking if the target point is open and if intermediate points are open

        if(targetPoint.ContainsCheckerColor(_enemyColor))
        {
            return false;
        }
        if (CreatesRowOfSix(copiedPoints, startPoint, targetPoint, _playerColor, _enemyColor)) {
            // Check if there's an opponent's checker past the target point
            
            if (!HasOpponentsCheckerPast(copiedPoints, targetPoint, _playerColor, _enemyColor)) {
                return false; // Invalidate move if no opponent's checker is past the row of 6
            }
        }
        return true; // Placeholder return
    }

    bool CreatesRowOfSix(List<Point> copiedPoints, Point startPoint, Point targetPoint, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor) 
    {
        // logic to check if placing a checker on targetPoint would create a row of 6
        int rowLengthCount = 1;
        int startIndex = copiedPoints.IndexOf(startPoint);
        bool checkRight = true;
        bool checkLeft = true;
        int leftindex = copiedPoints.IndexOf(targetPoint);
        int rightindex = leftindex;
        while((checkRight || checkLeft) && rowLengthCount < 6)
        {
            if(checkRight)
            {
                rightindex -= 1;
                if(rightindex < 0)
                {
                    rightindex = 23;
                }
                if(copiedPoints[rightindex].HasCheckers() && copiedPoints[rightindex].ContainsCheckerColor(_playerColor))
                {   
                    if(rightindex != startIndex ||(rightindex == startIndex && startPoint.checkersStack.Count > 1))
                    {
                        rowLengthCount += 1;
                    }
                    else
                    {
                        checkRight = false;
                    }
                    
                }
                else checkRight = false;   
            }
            if(checkLeft)
            {
                leftindex += 1;
                if(leftindex > 23)
                {
                    leftindex = 0;
                }
                if(copiedPoints[leftindex].HasCheckers() && copiedPoints[leftindex].ContainsCheckerColor(_playerColor))
                {
                    if(copiedPoints[leftindex] != startPoint || (leftindex == startIndex && startPoint.checkersStack.Count > 1))
                    {
                        rowLengthCount += 1;
                    }
                    else
                    {
                        checkLeft = false;
                    }
                }
                else checkLeft = false;   
            }
        }
        if(rowLengthCount >= 6) return true;
        return false;
    }

    bool HasOpponentsCheckerPast(List<Point> copiedPoints, Point targetPoint, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor) 
    {
        // logic to check for an opponent's checker past the targetPoint
        int targetIndex = copiedPoints.IndexOf(targetPoint);

        int checkedIndex = 11;
        while(targetIndex != checkedIndex)
        {
            if(copiedPoints[checkedIndex].ContainsCheckerColor(_enemyColor))
            {
                return true;
            }
            checkedIndex -= 1;
            if(checkedIndex == -1) checkedIndex = 23; 
        }

        return false;
    }
    private void UnhighlightAllPoints()
    {
        foreach (var point in Points)
        {
            point.SetHighlight(false);
        }
        remover.SetRemovalAllowed(false);
    }

    public bool AreAllCheckersInLastSix(List<Point> copiedPoints, Checker.CheckerColor _player)
    {
        int lastIndex = copiedPoints.Count - 1;
        int countCheckersInLastSix = 0;
        int totalPlayerCheckers = 0;

        for (int i = lastIndex; i > lastIndex - 6; i--)
        {
            foreach (var checker in copiedPoints[i].checkersStack)
            {
                Checker checkerScript = checker.GetComponent<Checker>();
                if (checkerScript != null && checkerScript.checkerColor == _player)
                {
                    countCheckersInLastSix++;
                }
            }
        }

        // Count total checkers of the player on the board
        foreach (var point in copiedPoints)
        {
            foreach (var checker in point.checkersStack)
            {
                Checker checkerScript = checker.GetComponent<Checker>();
                if (checkerScript != null && checkerScript.checkerColor == _player)
                {
                    totalPlayerCheckers++;
                }
            }
        }

        return countCheckersInLastSix == totalPlayerCheckers;
    }

    public void RemoveCheckerFromSelectedPoint()
    {
        if (selectedPoint != null && selectedPoint.HasCheckers())
        {
            lastMoveDone = selectedPoint.possibleMoves.FirstOrDefault(move => move.TargetPoint == null && move.PrevMove == lastMoveDone);
            MovesDone.Add(lastMoveDone);
            List<int> UsedDice = lastMoveDone.DiceUsed;
            moveNr += UsedDice.Count;
            for(int i = 0; i < UsedDice.Count; i++)
                {
                    if(UsedDice[i] == 1) diceUseCount1 -= 1;
                    else diceUseCount2-= 1;
                }
            GameObject checkerToRemove = selectedPoint.RemoveTopChecker();
            if (checkerToRemove != null)
            {
                // Handle the removed checker (e.g., disable it, return to a pool, etc.)
                Checker checkerComponent = checkerToRemove.GetComponent<Checker>();
                checkerComponent.MakeCheckerInvisible();
                
                selectedPoint = null;
                if(moveNr == maxMovesPossible + 1) MakeButtonInteractable(true);
                UnhighlightAllPoints();
                
            }
        }
    }

    public void RemoveChecker(Point startPoint)
    {
        if (startPoint != null && startPoint.HasCheckers())
        {
            GameObject checkerToRemove = startPoint.RemoveTopChecker();
            if (checkerToRemove != null)
            {
                // Handle the removed checker (e.g., disable it, return to a pool, etc.)
                Checker checkerComponent = checkerToRemove.GetComponent<Checker>();
                checkerComponent.MakeCheckerInvisible();
            }
        }
    }

    private void OnEnable() 
    {
        Dice.OnDiceRolled += HandleDiceRoll;
    }

    private void OnDisable() 
    {
        Dice.OnDiceRolled -= HandleDiceRoll;
    }
    private void HandleDiceRoll(int side1, int side2) 
    {
        diceRollResult1 = side1; // Assuming a 6-sided die
        diceRollResult2 = side2;
        // Set use counts based on whether a double was rolled
        if(diceRollResult1 == diceRollResult2)
        {
            diceUseCount1 = 4; 
            diceUseCount2 = 0;
        }else
        {
            diceUseCount1 = 1; 
            diceUseCount2 = 1;
        }
        
        die1.DisableDice();
        MovesDone = new();
        CalculateAllowedMoves(Points, Checker.CheckerColor.Player, Checker.CheckerColor.Enemy, diceUseCount1, diceUseCount2, removalStage);
        diceRolled = true;
    }
    public void DisableDice()
    {
        die1.DisableDice();
    }

      public void EnableDice()
    {
        die1.EnableDice();
    }

    public void SetFinaleDice(int _final1, int _final2)
    {
        die1.finalSide = _final1 - 1;
        die1.finalSide2 = _final2 - 1;

        die2.finalSide2 = _final1 - 1;
        die2.finalSide = _final2 - 1;
    }
    public void RollDice()
    {
        die1.StartRollDice();
    }


    public void EndTurn()
    {
        moveOfTheHeadCount = 0;
        
        foreach (var point in Points) {
            point.possibleMoves.Clear();
        }
        moveNr = 1;
        lastMoveDone = null;
        firstTurnOfTheGame =false;
        diceRolled = false;
        highestDiePossible =false;
        MakeButtonInteractable(false);
        //die1.EnableDice();
        ClientSend.EndTurn(diceRollResult1, diceRollResult2);
    }

    void MakeButtonInteractable(bool isInteractable)
    {
        endTurnButton.interactable = isInteractable;
        endturnButtonText.color = isInteractable ? new Color32(115, 243, 121, 255) : new Color32(224, 224, 224, 255); // Change as needed
    }

    public void Surrender()
    {
        UI.interactable = false;
        board.interactable = false;
        ClientSend.Surrender();
    }

    public void StopSpectating()
    {
        UI.interactable = false;
        board.interactable = false;
        
        ClientSend.LeaveLobby();
        Client.instance.player.lobby = null;
        Client.instance.player.lobby.SetStatus(GameState.Menu);
        Client.instance.player.dice1 = 0;
        Client.instance.player.dice2 = 0;
        Client.instance.player.currentPlayerName = null;
        SceneManager.LoadScene("Main");
        
    }

    public void FinishGame(string WinnerName)
    {
        UI.interactable = false;
        board.interactable = false;
        EndPanel.SetActive(true);
        foreach(Point point in Points)
        {
            foreach(GameObject checker in point.checkersStack)
            {
                Checker checkerComponent = checker.GetComponent<Checker>();
                checkerComponent.MakeCheckerInvisible();
            }
        }
        WinnerText.text = WinnerName + " Wins";
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Main");
        Client.instance.player.lobby.SetStatus(GameState.Menu);
    }

    public void UpdateBoard(List<List<int>> moves)
    {
        StartCoroutine(UpdateMovesSequence(moves, 1f, UpdateUI));
    }
    IEnumerator UpdateMovesSequence(List<List<int>> moves, float delayBetweenMoves, Action onComplete)
    {
        foreach (var move in moves)
        {
            if(move[1] != 24)
            {
                MoveChecker(Points[move[0]], Points[move[1]]);
            }
            else
            {
                RemoveChecker(Points[move[0]]);
            }
            yield return new WaitForSeconds(delayBetweenMoves); // Wait before proceeding to the next move
        }
        onComplete?.Invoke();
    }
    public void UpdateUI()
    {

        SwitchTurnText(Client.instance.player.turn);
        
        if(Client.instance.player.turn)
        {
            EnableDice();
            die1.Reset();
            SetFinaleDice(Client.instance.player.dice1, Client.instance.player.dice2);
        }
        else
        {
            DisableDice();
            die1.Reset();
            SetFinaleDice(Client.instance.player.dice1, Client.instance.player.dice2);
            RollDice();
        }
    }
    IEnumerator StartAICalculationWithDelay(float delayInSeconds)
    {
        yield return new WaitForSeconds(delayInSeconds); // Wait for the specified delay
        agent.MakeMove();
    }
    public void WaitForAIToStartCalc()
    {
        StartCoroutine(StartAICalculationWithDelay(2f));
        
    }
    public void SwitchTurnText(bool turn)
    {
        if(Client.instance.player.currentStatus == PlayerStatus.Player)
        {
            if(turn)
            {
                TurnText.text = "Your turn";
                TurnText.color = new Color32(131, 224, 85, 255);
            }
            else
            {
                TurnText.text = "Opponent's turn";
                TurnText.color = new Color32(243, 211, 58, 255);
            }
        }
        else if(Client.instance.player.currentStatus == PlayerStatus.Spectator)
        {
            TurnText.text = Client.instance.player.currentPlayerName + "'s turn";
            TurnText.color = new Color32(243, 211, 58, 255);
        }
        
    }
}

