using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SocialPlatforms.Impl;
public class Agent
{
    private readonly bool advanced;
    private List<AgentPoint> Board = new();
    private int dice1;
    private int dice2;
    private bool firstTurnOfTheGame = true;
    private int maxMovesPossible = 0;
    public List<AgentMoveOption> finalMoves = new();
    private readonly AgentWeights weights = new();
    private List<int> AIaddWeights = new();
    private List<int> AIplLast = new();
    private List<int> AIplFirst = new();
    private bool hasInHome =false;
    private bool gameStart = true;
    private int totalCheckerCount = 15;//Needed for weight calc, how to do, give to move?, total wont work, need to adjust func
    private bool removalStage = false;
    private int dice1Uses;
    private int dice2Uses;
    private double bestScore;
    private bool highestDiePossible;
    private AgentMoveOption bestMove;
    private List<AgentMoveOption> legalMoves;
    public Agent(bool _advanced)
    {
        advanced = _advanced;
        for(int i = 0; i < 24; i++)
        {
            AgentPoint point;   
            if(i == 0)
            {
                point = new(i, 15, false, Checker.CheckerColor.Player);
            }
            else if(i == 12)
            {
                point = new(i, 15, false, Checker.CheckerColor.Enemy);
            }
            else if(i <= 23 && i >= 18)
            {
                point = new(i, 0, true);
            }
            else
            {
                point = new(i);
            }
            Board.Add(point);
            
        }
    }

    public void MakeMove()
    {
        
        AIaddWeights = new();
        finalMoves = new();
        maxMovesPossible = 0;
        bestScore = -1000000;
        bestMove = null;
        highestDiePossible = false;
        legalMoves = new();
        foreach (var point in Board) {
            point.possibleMoves.Clear();
        }
        ProcessPreWeight();
        if(dice1 == dice2)
        {
            dice1Uses = 4;
            dice2Uses = 0; 
        }
        else
        {
            dice1Uses = 1;
            dice2Uses = 1;
        }
        //Make separate for first where dice1 and dice2 are equal to 33 44 66
        if(firstTurnOfTheGame && dice1 == dice2 && (dice1 == 3 || dice1 == 4 || dice1 == 6))
        {
            if(dice1 == 3)
            {
                AgentMoveOption move1 = new(Board[0], Board[9], new List<int> { 1, 1, 1}, 3, null, true);
                finalMoves.Add(move1);
                AgentMoveOption move2 = new(Board[0], Board[3], new List<int> { 1}, 4, move1, true);
                finalMoves.Add(move2);
            }
            else if(dice1 == 4)
            {
                if(!Board[8].HasCheckers())
                {
                    AgentMoveOption move1 = new(Board[0], Board[8], new List<int> { 1, 1}, 2, null, true);
                    finalMoves.Add(move1);
                    AgentMoveOption move2 = new(Board[0], Board[8], new List<int> { 1, 1}, 4, move1, true);
                    finalMoves.Add(move2);
                }
                else
                {
                    AgentMoveOption move1 = new(Board[0], Board[4], new List<int> { 1}, 1, null, true);
                    finalMoves.Add(move1);
                }
                
            }
            else if(dice1 == 6)
            {
                AgentMoveOption move1 = new(Board[0], Board[6], new List<int> { 1}, 1, null, true);
                finalMoves.Add(move1);
                AgentMoveOption move2 = new(Board[0], Board[6], new List<int> { 1}, 2, move1, true);
                finalMoves.Add(move2);
            }
            foreach(var move in finalMoves)
            {
                move.StartingPoint.RemoveTopChecker();
                move.TargetPoint.AddChecker(Checker.CheckerColor.Player);
            }
        }
        else
        {
            
            CalculateAllowedMoves(Board, Checker.CheckerColor.Player, Checker.CheckerColor.Enemy, dice1Uses, dice2Uses, removalStage, totalCheckerCount);
            
            if(dice1Uses + dice2Uses != maxMovesPossible && (bestMove == null || (!advanced && legalMoves.Count == 0)))
            {
                foreach (var point in Board)
                {
                    foreach(var move in point.possibleMoves)
                    {
                        if(move.MoveNumber != maxMovesPossible)
                        {
                            continue;
                        }
                        if(advanced)
                        {
                            List<AgentMoveOption> moves = new();
                            CollectMovesRecursive(move, moves);
                            foreach(var moveOrdered in moves)
                            {
                                moveOrdered.StartingPoint.RemoveTopChecker();
                                moveOrdered.TargetPoint?.AddChecker(Checker.CheckerColor.Player);
                            }
                            double score = AIWeightFunc(move);
                            if(bestMove != null)
                            {
                                if(score > bestScore)
                                {
                                    bestScore = score;
                                    bestMove = move;
                                }
                            }
                            else
                            {
                                bestScore = score;
                                bestMove = move;
                            }
                            
                            for (int i = moves.Count - 1; i >= 0; i--)
                            {
                                moves[i].TargetPoint?.RemoveTopChecker();
                                moves[i].StartingPoint.AddChecker(Checker.CheckerColor.Player);
                            }
                        }
                        else
                        {
                            legalMoves.Add(move);
                        }
                        
                    }
                }
            }
            if(!advanced && legalMoves.Count > 0)
            {
                int randomMoveID = new System.Random().Next(0, legalMoves.Count);
                bestMove = legalMoves[randomMoveID];
            }
            
            if(bestMove != null)
            {
                List<AgentMoveOption> moves = new();
                CollectMovesRecursive(bestMove, moves);
                finalMoves = moves;
                totalCheckerCount = bestMove.totalCheckerCount;
                foreach(var move in finalMoves)
                {
                    move.StartingPoint.RemoveTopChecker();
                    move.TargetPoint?.AddChecker(Checker.CheckerColor.Player);
                    if(move.TargetPoint != null && move.TargetPoint.CanRemoveChecker() && !removalStage)
                    {
                        removalStage = AreAllCheckersInLastSix(Board, Checker.CheckerColor.Player);
                    }
                }
            }
        }
        
        
        firstTurnOfTheGame = false;
        ClientSend.EndTurnAI(this, dice1, dice2);
    }
    private void CollectMovesRecursive(AgentMoveOption move, List<AgentMoveOption> moves)
    {
        if (move.PrevMove != null)
        {
            CollectMovesRecursive(move.PrevMove, moves);
        }
        moves.Add(move);
    }
    public void SetDice(int _dice1, int _dice2)
    {
        dice1 = _dice1;
        dice2 = _dice2;
        
    }
    
    private void CalculateAllowedMoves(List<AgentPoint> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor, int dice1uses, int dice2uses, bool simRemovalStage, int simtotalCheckerCount,int CalculateHeadMoveCount = 0, int CalculateHeadMoveLimit = 1, AgentMoveOption moveCalculated = null)
    {
        List<AgentMoveOption> possibleMoves = new();
        bool allMovesPossible = false;
        if(firstTurnOfTheGame && dice1 == dice2 && (dice1 == 3 || dice1 == 4 || dice1 == 6)) CalculateHeadMoveLimit = 2;
        if(dice1uses == 0 && dice2uses == 0)
        {
            if(advanced)
            {
                double score = AIWeightFunc(moveCalculated);
            
                if(bestMove != null)
                {
                    if(score > bestScore)
                    {
                        bestScore = score;
                        bestMove = moveCalculated;
                    }
                }
                else
                {
                    bestScore = score;
                    bestMove = moveCalculated;
                    
                }
            }
            else
            {
                legalMoves.Add(moveCalculated);
            }
            
            
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
                if(dice1uses > 0) dieMove1Possible = CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, dice1, new List<int> { 1 }, dice1uses, dice2uses, simRemovalStage, simtotalCheckerCount, moveCalculated);
                if(dice2uses > 0) dieMove2Possible = CalculatetMoveOptions(copiedPoints, _playerColor, _enemyColor, possibleMoves, point, dice2, new List<int> { 2 }, dice1uses, dice2uses, simRemovalStage, simtotalCheckerCount, moveCalculated);
                if(dice1 >= dice2 && dieMove1Possible && !highestDiePossible) highestDiePossible = true;
                else if(dice2 >= dice1  && dieMove2Possible && !highestDiePossible) highestDiePossible = true;
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
            if(move.DiceUsed.Count == 2 && dice1 != dice2)
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
                if(advanced)
                {
                    double score = AIWeightFunc(move);
                
                    if(bestMove != null)
                    {
                        if(score > bestScore)
                        {
                            bestScore = score;
                            bestMove = move;
                        }
                    }
                    else
                    {
                        bestScore = score;
                        bestMove = move;
                        
                    }
                }
                else
                {
                    legalMoves.Add(move);
                }
            }
            else if(move.TargetPoint == null)
            {
                move.StartingPoint.RemoveTopChecker();
                CalculateAllowedMoves(copiedPoints, _playerColor, _enemyColor, tempDice1Uses, tempDice2Uses, removalStageTemp, move.totalCheckerCount, calculateHeadMoveCountTemp, CalculateHeadMoveLimit, move);
                
                if (move.usesAllDice)
                {
                    //Maybe here something about evaluate board
                    if(moveCalculated != null) moveCalculated.usesAllDice = true;
                    allMovesPossible = true;
                }
                move.StartingPoint.AddChecker(Checker.CheckerColor.Player);
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

                CalculateAllowedMoves(copiedPoints, _playerColor, _enemyColor, tempDice1Uses, tempDice2Uses, removalStageTemp, move.totalCheckerCount, calculateHeadMoveCountTemp, CalculateHeadMoveLimit, move);
                
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
                    if(dice1 >= dice2 && highestDiePossible)
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
            
        }
    }

    private bool CalculatetMoveOptions(List<AgentPoint> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor, List<AgentMoveOption> possibleMoves, AgentPoint startPoint, int moveDistance, List<int> diceUsed,  int dice1uses, int dice2uses, bool simRemovalStage, int simtotalCheckerCount,AgentMoveOption prevMove = null)
    {
        
        int startIndex = copiedPoints.IndexOf(startPoint);

        if(simRemovalStage && startPoint.CanRemoveChecker() && diceUsed.Count == 1)
        {
            if(24 - startIndex == moveDistance)
            {
                //Debug.Log("Remove move added for startIndex:" + startIndex);
                AgentMoveOption newMoveOption = new(startPoint, null, diceUsed, dice1Uses + dice2Uses - dice1uses - dice2uses + 1, prevMove, false, simtotalCheckerCount - 1);
                possibleMoves.Add(newMoveOption);
                return true;
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
                    AgentMoveOption newMoveOption = new(startPoint, null, diceUsed, dice1Uses + dice2Uses - dice1uses - dice2uses + 1, prevMove, false, simtotalCheckerCount - 1);
                    possibleMoves.Add(newMoveOption);
                    return true;
                }
                //Debug.Log("Prev are empty" + previousAreEmpty);
            }
        }
        
        int targetIndex = startIndex + moveDistance; 
        if(targetIndex > copiedPoints.Count -1) return false;
        AgentPoint targetPoint = copiedPoints[targetIndex];
        
        // Check if the target point is open and intermediate points are open if needed
        if (IsMoveValid(startPoint, targetPoint, copiedPoints, _playerColor, _enemyColor))
        {   
            AgentMoveOption newMoveOption = new(startPoint, targetPoint, diceUsed,  dice1Uses + dice2Uses - dice1uses - dice2uses + 1, prevMove);
            possibleMoves.Add(newMoveOption);
            return true;
        }
        
        return false;
    }

    bool IsMoveValid(AgentPoint startPoint, AgentPoint targetPoint, List<AgentPoint> copiedPoints, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor)
    {
        // Implement logic to check if a move is valid based on Narde rules
        // This includes checking if the target point is open and if intermediate points are open

        if(targetPoint.ContainsCheckerColor(_enemyColor))
        {
            return false;
        }
        if (CreatesRowOfSix(copiedPoints, startPoint, targetPoint, _playerColor, _enemyColor)) {
            // Check if there's an opponent's checker past the target point
            
            if (!HasOpponentsCheckerPast(copiedPoints, targetPoint, _enemyColor)) {
                return false; // Invalidate move if no opponent's checker is past the row of 6
            }
        }
        return true; // Placeholder return
    }
    
    bool CreatesRowOfSix(List<AgentPoint> copiedPoints, AgentPoint startPoint, AgentPoint targetPoint, Checker.CheckerColor _playerColor, Checker.CheckerColor _enemyColor) 
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
                    if(rightindex != startIndex ||(rightindex == startIndex && startPoint.checkerCount > 1))
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
                    if(copiedPoints[leftindex] != startPoint || (leftindex == startIndex && startPoint.checkerCount > 1))
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

    bool HasOpponentsCheckerPast(List<AgentPoint> copiedPoints, AgentPoint targetPoint, Checker.CheckerColor _enemyColor) 
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

    public bool AreAllCheckersInLastSix(List<AgentPoint> copiedPoints, Checker.CheckerColor _player)
    {
        int lastIndex = copiedPoints.Count - 1;
        int countCheckersInLastSix = 0;
        int totalPlayerCheckers = 0;

        for (int i = lastIndex; i > lastIndex - 6; i--)
        {
            if (copiedPoints[i].checkerCount > 0 && copiedPoints[i].ContainsCheckerColor(_player))
                {
                    countCheckersInLastSix+= copiedPoints[i].checkerCount;
                }
        }

        // Count total checkers of the player on the board
        foreach (var point in copiedPoints)
        {
            if (point.checkerCount > 0 && point.ContainsCheckerColor(_player))
                {
                    totalPlayerCheckers+= point.checkerCount;
                }
        }

        return countCheckersInLastSix == totalPlayerCheckers;
    }

    private void SimulateMoveChecker(AgentPoint startingPoint, AgentPoint targetPoint)
    {
        startingPoint.RemoveTopChecker();
        targetPoint.AddChecker(Checker.CheckerColor.Player);
    }

    private double AIWeightFunc(AgentMoveOption move)
    {
        double score = 0;
        Checker.CheckerColor prev = Checker.CheckerColor.None;
        int last = -1;
        int subhole = 0;
        //int subhole2 = 0;
        //int holes = 0;
        int pair = 0;
        int first = -1;
        int i;
        int i2;
        //bool has = false;
        double buf = 0;
        int chainDist = 0;
        int lastSecondPos = 0;
        bool hasDanger = false;
        bool iWin = false;
        Checker.CheckerColor secondPlayer = Checker.CheckerColor.Enemy;
        
        if (Board[0].checkerCount > 1) score = score - weights.head_mul * Board[0].checkerCount * Board[0].checkerCount - weights.head * Board[0].checkerCount;
        int startChips = Board[0].checkerCount;
        int countInHome = 0;
        int secondFirst = AIplFirst[1];// Add determining the first pos of enemy player, and maybe ai, have to make it from player pov
        List<double> fs = weights.field_start;
        List<double> fm = weights.field_middle;
        AgentPoint point;
        for (int k = 0; k <= 23; k++)
        {
            if (k == 12) prev = Checker.CheckerColor.None;
            i = k;
            i2 = (k + 12) % 24;
            point = Board[i];
            if (point.ContainsCheckerColor(Checker.CheckerColor.Player))
            {
                score += weights.holes * subhole * (subhole + 2);
                subhole = 0;
                //subhole2 = 0;
                last = k;// last checker
                if (first == -1)
                {
                    first = k;
                }
                buf = weights.fill + point.checkerCount * weights.tower;
                if (k > 11)
                {//Find A WAY to change game start, there is how it is determined in board pre pass
                    if (gameStart) score += weights.onenemybase * point.checkerCount;
                    else score += weights.onenemybase_e * point.checkerCount;
                }
                if (k < 18)
                {
                    if (k == first && point.checkerCount < 3)
                    {
                        buf /= 10;
                    }
                    if (!gameStart)
                    {
                        buf /= 2;
                    }
                    if (i2 > secondFirst)//second first should be in terms of pov from opponent
                    {
                        score += buf;
                    }
                    else
                    {
                        score += buf / 100;
                    }
                    score += weights.nearHome * point.checkerCount * (k+1);
                }
                else
                {
                    if (i2 > secondFirst)
                    {
                        if (gameStart) score += buf;
                        else score += buf / 10;
                    }
                    else
                    {
                        score += buf / 100;
                    }
                    countInHome += point.checkerCount;
                }
                if (gameStart)
                {
                    score += fs[k];//this might be wrong but prolly not
                }
                else
                {
                    score += fm[k];
                }//Create calculating this before moves, in board preprocess

                if (AIaddWeights[k] > 0 && k != AIplFirst[0] && (i2 > secondFirst || AIaddWeights[k] > 4) && (k != 11 || (k == 11 && Board[11].ContainsCheckerColor(secondPlayer))))
                {
                    score += AIaddWeights[k] * AIaddWeights[k] * (startChips > 7 ? weights.danger_start : weights.danger_end);
                    if (AIaddWeights[k] > 3)
                    {
                        buf = 0;
                        for (int j = k + 1; j <= k + 4; j++)
                        {
                            if (j > 23) break;
                            if (Board[j].ContainsCheckerColor(secondPlayer)) buf++;
                        }
                        if (buf == 4)
                        {
                            score += (point.checkerCount * 2 + AIaddWeights[k]) * (point.checkerCount * 2 + AIaddWeights[k]) * weights.danger_add;
                            hasDanger = true;
                        }
                    }
                }
            }
            else
            {
                if (point.ContainsCheckerColor(secondPlayer))
                {
                    lastSecondPos = k;
                    if (first != -1 && i2 > secondFirst) subhole++;
                }
            }
            
            if (point.ContainsCheckerColor(Checker.CheckerColor.Player) && i2 > secondFirst)
            {
                pair++;
            }
            else
            {
                chainDist = pair;
                if ((pair > 1 && gameStart) || (pair > 5 && !gameStart))
                {
                    if (pair >= 6)
                    {
                        pair = 10 + (pair - 5) / 100;
                        iWin = true;
                    }
                    else if (k > 11 && k < 19)
                    {
                        pair -= 2;
                    }//Why 20 - countinHOme?
                    score += pair * pair * weights.pair * (20 - countInHome) * 0.05 + (k+1) * 0.002;
                    score -= (k - lastSecondPos - chainDist) * weights.chainDist;//Check if pos is used give +1
                }
                pair = 0;
            }
            prev = point.color;
        }
        
        chainDist = pair;
        if (pair > 1)
        {
            int k = 24;
            if (pair >= 6)
            {
                pair = 10 + (pair - 5) / 100;
                iWin = true;
            }
            else if (k > 11 && k < 19)
            {
                pair -= 2;
            }
            score += pair * pair * weights.pair * (20 - countInHome) * 0.05 + (k+1) * 0.002;
            score -= (k - lastSecondPos - chainDist) * weights.chainDist;
        }

        AgentMoveOption tempMove = move;
        //This part considers the moves, and will likely need to be rewritten
        int buf2;
        while(tempMove != null)
        {
            if(tempMove.TargetPoint != null) buf2 = tempMove.TargetPoint.id;
            else buf2 = 24;
            i = tempMove.StartingPoint.id;
            i2 = buf2;
            if (iWin && i < 12)
            {
                score += (i2 - i) * 0.4;
            }
            if (i > 17 && !gameStart)
            {
                if (!((buf2 + 12) % 24 > secondFirst && buf2 !=  24 && Board[buf2].checkerCount == 1) || iWin)
                {
                    score += (i2 - i) * weights.movInHome;
                }
            }
            if (gameStart && i > 11)
            {
                bool subhole2 = true;
                for (int k = i2 - 4; k <= i2 - 1; k++)
                {
                    if (Board[k].ContainsCheckerColor(Checker.CheckerColor.Player))
                    {
                        subhole2 = false;
                        break;
                    }
                }
                if (subhole2) score -= 1.5;
            }
            //points for skipping opponents checkers
            if (!gameStart && buf2 < 24 && (Board[i].checkerCount > 0 || (i + 12) % 24 <= secondFirst) && i2 < 18)
            {
                prev = Checker.CheckerColor.None;
                pair = 0;
                for (int k = i; k <= i2; k++)
                {
                    Checker.CheckerColor col = Board[k].color;
                    if (col == secondPlayer && prev == col)
                    {
                        pair += pair + 1;
                    }
                    prev = col;
                }
                score += pair * pair * weights.pass;
            }
            tempMove = tempMove.PrevMove;
        }
        
        
        if (last > 17)
        {
            last = 17;
        }
        if (!hasInHome && countInHome < 15 && first != -1 && last != -1)
        {
            score += (last - first) * weights.length;
        }
        if (hasDanger)
        {
            if (countInHome > 11) countInHome = 11;
            countInHome /= 2;
        }
        if (hasInHome)
        {
            if (gameStart) score += countInHome * weights.home;
            else score += countInHome * weights.home_middle;
        }
        else
        {
            score += countInHome * weights.home_end;
        }
        score += (15 - move.totalCheckerCount) * weights.throw_;
        if (iWin) score += 10000;
        if (weights.rand > 0) score += new System.Random().NextDouble() * weights.rand;
        return score;
    }

    private void ProcessPreWeight()
    {

        AgentPoint pointEnemyHome = Board[12];
        gameStart = pointEnemyHome.checkerCount > 2 && pointEnemyHome.ContainsCheckerColor(Checker.CheckerColor.Enemy);
        if (!Board[0].ContainsCheckerColor(Checker.CheckerColor.Player)) gameStart = false;

        if(AIplLast.Count == 0)
        {
            AIplLast.Add(0);
            AIplLast.Add(0);
            AIplFirst.Add(24);
            AIplFirst.Add(24);
        }
        
        int AIenemyBottomPos = -1;
        int AIenemyTopPos = 24;
        int pairCount;
        hasInHome =false;
        for (int i = 0; i <= 23; i++)
        {
            AgentPoint point = Board[i];
            int k = i;
            AIaddWeights.Add(0);
            if (point.ContainsCheckerColor(Checker.CheckerColor.Player) ||  point.ContainsCheckerColor(Checker.CheckerColor.None))
            {
                pairCount = 0;
                if (i < 23)
                {
                    for (int j = i + 1; j <= 23; j++)
                    {
                        if (Board[j].ContainsCheckerColor(Checker.CheckerColor.Enemy))
                        {
                            pairCount++;
                        }
                        else break;
                    }
                }
                if (i > 0)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (Board[j].ContainsCheckerColor(Checker.CheckerColor.Enemy))
                        {
                            pairCount++;
                        }
                        else break;
                    }
                }
                if (pairCount > 1)
                {
                    AIaddWeights[i] = pairCount;
                }
            }
            if (point.checkerCount > 0)
            {
                if(point.ContainsCheckerColor(Checker.CheckerColor.Player))
                {
                    AIplLast[0] = Math.Max(i, AIplLast[0]);
                    AIplFirst[0] = Math.Min(i, AIplFirst[0]);
                }
                else if(point.ContainsCheckerColor(Checker.CheckerColor.Enemy))
                {
                    AIplLast[1] = Math.Max((i + 12) % 24, AIplLast[1]);
                    AIplFirst[1] = Math.Min((i + 12) % 24, AIplFirst[1]);
                }
            }
            if (point.ContainsCheckerColor(Checker.CheckerColor.Enemy))
            {
                if (i > 11 && AIenemyTopPos > i) AIenemyTopPos = i;
                if (i < 12 && AIenemyBottomPos < i) AIenemyBottomPos = i;
            }
            else if (point.ContainsCheckerColor(Checker.CheckerColor.Player))
            {
                if (i < 6)
                {
                    hasInHome = true;
                }
            }
        }
    }
    
    public void UpdateBoard(int start, int end)
    {
        if(end != 24)
        {
            Board[start].RemoveTopChecker();
            Board[end].AddChecker(Checker.CheckerColor.Enemy);
        }
        else
        {
            Board[start].RemoveTopChecker();
        }
    } 
}
