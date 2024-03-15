using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Narde_Server
{
    public class Move {
        public int TargetPoint;
        public int StartingPoint;
        public List<int> DiceUsed; // Contains 1, 2, or both for the dice roll(s) enabling this move
        //Class for possible moves of dice
        public Move(int startingPoint, int targetPoint, List<int> diceUsed) 
            {
                StartingPoint = startingPoint;
                TargetPoint = targetPoint;
                DiceUsed = diceUsed;
            }
    }

    class Game
    {
        public List<Point> Board = new();
        public int player1ID;
        public int player2ID;
        public int hostID;
        public int player1InvalidMoveCount;
        public int player2InvalidMoveCount;
        public int player1CheckerCount;
        public int player2CheckerCount;
        public int currentPlayerID;
        public int diceResult1;
        public int diceResult2;
        public bool removingP1 = false;
        public bool removingP2 = false;
        public bool firstTurn = true;
        private bool canHigherDieBeUsed = false;
        public Game(int _player1ID, int _player2ID, int _dice1, int _dice2, int _currentPlayerID, int _hostID = 0)
        {
            hostID =_hostID;
            player1ID = _player1ID;
            player2ID = _player2ID;
            player1InvalidMoveCount = 0;
            player2InvalidMoveCount = 0;
            diceResult1 = _dice1;
            diceResult2 = _dice2;
            currentPlayerID = _currentPlayerID;
            for(int i = 0; i < 24; i++)
            {
                Board.Add(new Point());
            }
            Board[0].AddCheckers(15, player1ID);
            Board[12].AddCheckers(15, player2ID);
            player1CheckerCount = 15;
            player2CheckerCount = 15;
        }

        public bool ValidateTurn(List<Move> moves)
        {
            int maxMoves = diceResult1 == diceResult2 ? 4 : 2;
            int dice1Uses = 0;
            int dice2Uses = 0;
            int movesFromHead = 0;
            //Console.WriteLine($"Total Moves : {moves.Count}");
            bool removingP1Temp = removingP1;
            bool removingP2Temp = removingP2;
            List<Point> BoardCopy = new();
            foreach (Point item in Board)
            {
                BoardCopy.Add(item.ShallowCopy());
            }

            if(maxMoves == 4)
            {
                dice1Uses = 4;
            }
            else
            {
                dice1Uses = 1;
                dice2Uses = 1;
            }

            int lastIndex = 23;
            int headIndex = 0;
            if(currentPlayerID == player2ID)
                {
                    lastIndex = (lastIndex + 12) % 24;
                    headIndex = (headIndex + 12) % 24;
                }
            foreach(var move in moves)
            {
                if(currentPlayerID == player2ID)
                {
                    move.StartingPoint = (move.StartingPoint + 12) % 24;
                    if(move.TargetPoint != 24)
                    {
                        move.TargetPoint = (move.TargetPoint + 12) % 24;
                    }
                    
                }

                if(move.StartingPoint == headIndex)
                {
                    movesFromHead += 1;
                    if(movesFromHead > 1 && !(firstTurn && diceResult1 == diceResult2 && (diceResult1 == 3 || diceResult1 == 4 || diceResult1 == 6)))
                    {
                        Console.WriteLine($"Move from head issue, blocked");
                        return false;
                    }
                    else if(movesFromHead > 2)
                    {
                        Console.WriteLine($"Move from head issue more than 2, blocked");
                        return false;
                    }

                } 
                
                bool moveAllowed = ValidateMove(BoardCopy, move, dice1Uses, dice2Uses, lastIndex, removingP1Temp, removingP2Temp);
                if(!moveAllowed)
                {
                    Console.WriteLine($"Invalid Move");
                    return false;
                }

                BoardCopy[move.StartingPoint].RemoveCheckers(1);
                if(move.TargetPoint < 24)
                {
                    BoardCopy[move.TargetPoint].AddCheckers(1, currentPlayerID);
                }

                for(int i = 0; i < move.DiceUsed.Count; i++)
                {
                    if(move.DiceUsed[i] == diceResult1)
                    {
                        dice1Uses -= 1;
                    }
                    else
                    {
                        dice2Uses -= 1;
                    }
                }

                if(currentPlayerID == player1ID)
                {
                    if(!removingP1Temp && move.TargetPoint <= lastIndex && move.TargetPoint > lastIndex - 6)
                    {
                        removingP1Temp = AreAllCheckersInLastSix(BoardCopy, lastIndex);
                    }
                }
                else
                {
                    if(!removingP2Temp &&move.TargetPoint <= lastIndex && move.TargetPoint > lastIndex - 6)
                    {
                        removingP2Temp = AreAllCheckersInLastSix(BoardCopy, lastIndex);
                    }
                }
                
            }
            
            if(firstTurn && diceResult1 == diceResult2 && (diceResult1 == 3 || diceResult1 == 4 || diceResult1 == 6))
            {
                if(diceResult1 == 3 && BoardCopy[headIndex + 3].checkerCount != 1 && BoardCopy[headIndex + 9].checkerCount != 1)
                {
                    Console.WriteLine($"Exception 3 not followed!");
                    return false;
                }
                else if(diceResult1 == 4 && BoardCopy[headIndex + 8].checkerCount != 2 && Board[headIndex + 8].checkerCount == 0)
                {
                    Console.WriteLine($"Exception 4 not followed!");
                    return false;
                }
                else if(diceResult1 == 6 && BoardCopy[headIndex + 6].checkerCount != 2)
                {
                    Console.WriteLine($"Exception 6 not followed!");
                    return false;
                }
            }

            if(dice1Uses != 0 && dice2Uses != 0)
            {
                int movesUsed = diceResult1 == diceResult2? 4 - dice1Uses: 1 - dice1Uses + 1 - dice2Uses;
                int dice1UsesSim = 0;
                int dice2UsesSim = 0;
                if(maxMoves == 4)
                {
                    dice1UsesSim = 4;
                }
                else
                {
                    dice1UsesSim = 1;
                    dice2UsesSim = 1;
                }

                int movesPossible = 0;
                if(currentPlayerID == player1ID)
                {
                    movesPossible = CalculateAllowedMoves(BoardCopy, dice1UsesSim, dice2UsesSim, headIndex, lastIndex, removingP1, movesPossible, maxMoves);
                }
                else
                {
                    movesPossible = CalculateAllowedMoves(BoardCopy, dice1UsesSim, dice2UsesSim, headIndex, lastIndex, removingP2, movesPossible, maxMoves);
                    
                }
                //Console.WriteLine($"Moves Possible : {movesPossible}");
                if(movesUsed != movesPossible)
                {
                    Console.WriteLine($"More moves possible");
                    return false;
                } 
                else if(diceResult1 != diceResult2)
                {
                    if(diceResult1 > diceResult2 && dice1Uses != 0 && canHigherDieBeUsed) 
                    {
                        Console.WriteLine($"Higher not used! Dice1");
                        return false;
                    } 
                    if(diceResult2> diceResult1 && dice2Uses != 0 && canHigherDieBeUsed) {
                        Console.WriteLine($"Higher not used! Dice2");
                        return false;
                    } 
                }
            }
            //Console.WriteLine($"Success");
            FinalizeTurn(moves, removingP1Temp, removingP2Temp);
            return true;
        }

        public bool ValidateMove(List<Point> Board,Move move, int dice1Uses, int dice2Uses, int lastIndex, bool removingP1Temp, bool removingP2Temp)
        {
            //Console.WriteLine($"Start({move.StartingPoint}), target({move.TargetPoint}), dice used {move.DiceUsed.Count}");
            int totalDice = 0;
            int enemyPlayerId = currentPlayerID == player1ID? player2ID: player1ID;
            if(move.StartingPoint < 24 && move.StartingPoint >= 0)
            {
                if(Board[move.StartingPoint].playerID != currentPlayerID)
                {
                    Console.WriteLine($"StartPoint({move.StartingPoint}) playerId({Board[move.StartingPoint].playerID}) does not match current player({currentPlayerID})");
                    return false;
                } 
            }
            else return false;
            if(move.TargetPoint < 24)
            {
                if(Board[move.TargetPoint].playerID == enemyPlayerId)
                {
                    Console.WriteLine($"Target playerId has enemyPlayerId");
                    return false;
                }
                int currentPoint = move.StartingPoint;

                for(int i = 0; i < move.DiceUsed.Count; i++)
                {
                    int midpoint;
                    if(move.DiceUsed[i] == diceResult1)
                    {
                        if(dice1Uses < 1)
                        {
                            Console.WriteLine($"Dice1Used more than allowed");
                            return false;
                        }
                        dice1Uses -= 1;
                        midpoint = (currentPoint + diceResult1) % 24;
                    }
                    else if(move.DiceUsed[i] == diceResult2)
                    {
                        if(dice2Uses < 1)
                        {
                            Console.WriteLine($"Dice2Used more than allowed");
                            return false;
                        }
                        dice2Uses -= 1;
                        midpoint = (currentPoint + diceResult2) % 24;
                        
                    }
                    else
                        {
                            Console.WriteLine($"Dice value used does not exist");
                            return false;
                        }
                    
                    if(midpoint < 24 && Board[midpoint].playerID == enemyPlayerId) 
                        {
                            Console.WriteLine($"MidPoint is blocked");
                            return false;
                        }
                    if(midpoint < 24 && CreatesRowOfSix(Board, currentPoint, midpoint))
                    {
                        if(!HasOpponentsCheckerPast(Board, midpoint, enemyPlayerId))
                        {
                            Console.WriteLine($"Creates blockade");
                            return false;
                        }
                    }
                    totalDice += move.DiceUsed[i];
                    currentPoint = midpoint;
                }
                if((move.StartingPoint + totalDice) % 24 != move.TargetPoint) 
                {
                    Console.WriteLine($"Total dice used is not equal distance to Target. Start({move.StartingPoint}), total({totalDice}), target({move.TargetPoint})");
                    return false;
                }

            }
            else if ( move.TargetPoint == 24)
            {
                int currentPoint = move.StartingPoint;

                for(int i = 0; i < move.DiceUsed.Count; i ++)
                {
                    int midpoint;
                    if(move.DiceUsed[i] == diceResult1)
                    {
                        if(dice1Uses < 1)
                        {
                            Console.WriteLine($"Dice1Used more than allowed");
                            return false;
                        }
                        dice1Uses -= 1;
                        if(currentPoint + diceResult1 >= lastIndex + 1 && currentPoint <= lastIndex)
                        {
                            midpoint = 24;
                        }
                        else
                        {
                            midpoint = (currentPoint + diceResult1) % 24;
                        }
                    }
                    else if(move.DiceUsed[i] == diceResult2)
                    {
                        if(dice2Uses < 1)
                        {
                            Console.WriteLine($"Dice2Used more than allowed");
                            return false;
                        }
                        dice2Uses -= 1;
                        if(currentPoint + diceResult2 >= lastIndex + 1  && currentPoint <= lastIndex)
                        {
                            midpoint = 24;
                        }
                        else
                        {
                            midpoint = (currentPoint + diceResult2) % 24;
                        }
                        
                    }
                    else
                        {
                            Console.WriteLine($"Dice value used does not exist");
                            return false;
                        }
                    
                    if(midpoint < 24 && Board[midpoint].playerID == enemyPlayerId) 
                        {
                            Console.WriteLine($"MidPoint is blocked");
                            return false;
                        }
                    if(midpoint < 24 && CreatesRowOfSix(Board, currentPoint, midpoint))
                    {
                        if(!HasOpponentsCheckerPast(Board, midpoint, enemyPlayerId))
                        {
                            Console.WriteLine($"Creates blockade");
                            return false;
                        }
                    }
                    if(midpoint == 24)
                    {
                        if(!(currentPoint <= lastIndex && currentPoint > lastIndex - 6))
                        {
                            Console.WriteLine($"Not Removal Point");
                            return false;
                        }
                        if(currentPlayerID == player1ID)
                        {   
                            if(!removingP1Temp) 
                            {
                                Console.WriteLine($"Not removal phase p1");
                                return false;
                            }
                        }
                        else
                        {
                            if(!removingP2Temp)
                            {
                                Console.WriteLine($"Not removal phase p2");
                                return false;
                            }
                        }
                        bool succesfulRemoval = RemovalCheck(Board, currentPoint, move.DiceUsed[i], lastIndex);

                        if(!succesfulRemoval)
                        {
                            Console.WriteLine($"Removal Check blocked");
                            return false;
                        }
                    }
                    totalDice += move.DiceUsed[i];
                    currentPoint = midpoint;
                }
            }

            return true;
        }

        private bool CreatesRowOfSix(List<Point> Board, int startIndex,int targetIndex) 
        {
            // logic to check if placing a checker on targetPoint would create a row of 6
            int rowLengthCount = 1;
            bool checkRight = true;
            bool checkLeft = true;
            int leftindex = targetIndex;
            int rightindex = targetIndex;
            while((checkRight || checkLeft) && rowLengthCount < 6)
            {
                if(checkRight)
                {
                    rightindex -= 1;
                    if(rightindex < 0)
                    {
                        rightindex = 23;
                    }
                    if(Board[rightindex].checkerCount > 0 && Board[rightindex].playerID == currentPlayerID)
                    {   
                        if(rightindex != startIndex ||(rightindex == startIndex && Board[startIndex].checkerCount > 1))
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
                    if(Board[leftindex].checkerCount > 0 && Board[leftindex].playerID == currentPlayerID)
                    {
                        if(leftindex != startIndex || (leftindex == startIndex && Board[startIndex].checkerCount > 1))
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


        bool HasOpponentsCheckerPast(List<Point> Board, int targetIndex, int enemyPlayerId) 
        {
            // logic to check for an opponent's checker past the targetPoint
            int checkedIndex = 11;
            if(currentPlayerID == player2ID)
                {
                    checkedIndex = (checkedIndex + 12) % 24;
                }
            while(targetIndex != checkedIndex)
            {
                if(Board[checkedIndex].playerID == enemyPlayerId)
                {
                    return true;
                }
                checkedIndex -= 1;
                if(checkedIndex == -1 && currentPlayerID == player1ID) checkedIndex = 23; 
                else if(checkedIndex == -1) return false;
            }

            return false;
        }

        public bool AreAllCheckersInLastSix(List<Point> Board, int lastIndex)
        {
            
            int countCheckersInLastSix = 0;
            int totalPlayerCheckers = 0;

            for (int i = 23; i >= 0; i--)
            {
                if( Board[i].playerID == currentPlayerID)
                {
                    if(i <= lastIndex && i > lastIndex - 6)
                    {
                        countCheckersInLastSix += Board[i].checkerCount;
                    }
                    
                    totalPlayerCheckers += Board[i].checkerCount;
                }
            }

            return countCheckersInLastSix == totalPlayerCheckers;
        }
        
        private bool RemovalCheck(List<Point> Board,int startPoint, int moveDistance, int lastIndex)
        {
            if(lastIndex + 1- startPoint == moveDistance)
            {
                return true;
            }
            else if( lastIndex + 1- startPoint < moveDistance)
            {
                bool previousAreEmpty = true;
                for(int i = startPoint-1; i > lastIndex - 6; i--)
                {
                    if(Board[i].checkerCount > 0 && Board[i].playerID == currentPlayerID) 
                    {
                        previousAreEmpty = false;
                        break;
                    } 
                }
                if(previousAreEmpty)
                {
                    return true;
                }
            }
        
            return false;
        }


        public void FinalizeTurn(List<Move> moves, bool removingP1Temp, bool removingP2Temp)
        {
            foreach(var move in moves)
            {
                Board[move.StartingPoint].RemoveCheckers(1);
                if(move.TargetPoint < 24)
                {
                    Board[move.TargetPoint].AddCheckers(1, currentPlayerID);
                }
                else
                {
                    if(currentPlayerID == player1ID)
                    {
                        player1CheckerCount -= 1;
                    }
                    else
                    {
                        player2CheckerCount -= 1;
                    }
                }
            }
            removingP1 = removingP1Temp;
            removingP2 = removingP2Temp;
            canHigherDieBeUsed = false;
            
            if(firstTurn && currentPlayerID == player2ID) firstTurn = false;
            currentPlayerID = currentPlayerID == player1ID? player2ID: player1ID;
            Random rand = new();
            diceResult1 = rand.Next(1, 7);
            diceResult2 = rand.Next(1, 7);
        }

        private int CalculateAllowedMoves(List<Point> BoardCopy,int dice1uses, int dice2uses, int head, int lastIndex, bool simRemovalStage, int movesPossible, int maxMoves, int CalculateHeadMoveCount = 0, int CalculateHeadMoveLimit = 1)
        {
        
            int movesPossibleFromPos = movesPossible;
            if(firstTurn && diceResult1 == diceResult2 && (diceResult1 == 3 || diceResult1 == 4 || diceResult1 == 6)) CalculateHeadMoveLimit = 2;
            if(dice1uses == 0 && dice2uses == 0)
            {
                canHigherDieBeUsed = true;
                return  maxMoves;
            }

            for(int i = 0; i < 24; i++)
            {
                //Debug.Log(copiedPoints.IndexOf(point));
                if(currentPlayerID == player2ID)
                {
                    i = (i + 12) % 24;
                }
                if(BoardCopy[i].checkerCount > 0 && BoardCopy[i].playerID == currentPlayerID)
                {   
                    
                    //Debug.Log(copiedPoints.IndexOf(point));
                    if(i == head && CalculateHeadMoveCount == CalculateHeadMoveLimit) continue;  

                    bool dieMove1Possible = false;
                    bool dieMove2Possible = false;
                    int calculateHeadMoveCountTemp = CalculateHeadMoveCount;
                    bool removalStageTemp = simRemovalStage;
                    if(dice1uses > 0)
                    {
                        Move move;
                        List<int> list =new() { diceResult1};
                        if(i + diceResult1 > lastIndex && i <= lastIndex)
                        {
                            move = new(i, 24, list);
                        }
                        else
                        {
                             move = new(i, (i + diceResult1) % 24, list);
                        }
                       
                        dieMove1Possible = ValidateMove(BoardCopy, move, dice1uses, dice2uses, lastIndex, simRemovalStage, simRemovalStage);
                        if(dieMove1Possible)
                        {
                            if(move.StartingPoint == head)
                            {
                                calculateHeadMoveCountTemp += 1;
                            }
                            if(move.TargetPoint < lastIndex + 1 && move.TargetPoint > lastIndex -6 && !removalStageTemp)
                            {
                                removalStageTemp = AreAllCheckersInLastSix(BoardCopy, lastIndex);
                            }

                            BoardCopy[move.StartingPoint].RemoveCheckers(1);
                            if(move.TargetPoint < 24)
                            {
                                BoardCopy[move.TargetPoint].AddCheckers(1, currentPlayerID);
                            }
                            
                            
                            int movesPossibleD1 = CalculateAllowedMoves(BoardCopy, dice1uses - 1, dice2uses, head, lastIndex, removalStageTemp, movesPossible + 1, maxMoves, calculateHeadMoveCountTemp, CalculateHeadMoveLimit);
                            if(movesPossibleD1 == maxMoves)
                            {
                                canHigherDieBeUsed = true;
                                return maxMoves;
                            }
                            movesPossibleFromPos = movesPossibleFromPos > movesPossibleD1? movesPossibleFromPos: movesPossibleD1;
                            
                            if(move.TargetPoint < 24)
                            {
                                BoardCopy[move.TargetPoint].RemoveCheckers(1);
                            }
                            BoardCopy[move.StartingPoint].AddCheckers(1, currentPlayerID);
                            if(diceResult1 > diceResult2)
                            {
                                canHigherDieBeUsed = true;
                            }
                        }
                        
                    } 
                    if(dice2uses > 0)
                    {
                        calculateHeadMoveCountTemp = CalculateHeadMoveCount;
                        removalStageTemp = simRemovalStage;
                        
                        List<int> list = new() { diceResult2};

                        Move move;
                        if(i + diceResult2 > lastIndex && i <= lastIndex)
                        {
                            move = new(i, 24, list);
                        }
                        else
                        {
                             move = new(i, (i + diceResult2) % 24, list);
                        }
                        dieMove2Possible = ValidateMove(BoardCopy, move, dice1uses, dice2uses, lastIndex, simRemovalStage, simRemovalStage);
                        if(dieMove2Possible)
                        {
                            if(move.StartingPoint == head)
                            {
                                calculateHeadMoveCountTemp += 1;
                            }
                            if(move.TargetPoint < lastIndex + 1 && move.TargetPoint > lastIndex -6  && !removalStageTemp)
                            {
                                removalStageTemp = AreAllCheckersInLastSix(BoardCopy, lastIndex);
                            }

                            BoardCopy[move.StartingPoint].RemoveCheckers(1);
                            if(move.TargetPoint < 24)
                            {
                                BoardCopy[move.TargetPoint].AddCheckers(1, currentPlayerID);
                            }
                            
                            
                            int movesPossibleD2 = CalculateAllowedMoves(BoardCopy, dice1uses, dice2uses - 1, head, lastIndex, removalStageTemp, movesPossible + 1, maxMoves, calculateHeadMoveCountTemp, CalculateHeadMoveLimit);
                            if(movesPossibleD2 == maxMoves)
                            {
                                canHigherDieBeUsed = true;
                                return maxMoves;
                            }
                            movesPossibleFromPos = movesPossibleFromPos > movesPossibleD2? movesPossibleFromPos: movesPossibleD2;
                            
                            if(move.TargetPoint < 24)
                            {
                                BoardCopy[move.TargetPoint].RemoveCheckers(1);
                            }
                            BoardCopy[move.StartingPoint].AddCheckers(1, currentPlayerID);
                            if(diceResult1 < diceResult2)
                            {
                                canHigherDieBeUsed = true;
                            }
                        }
                    } 
                }
            }
        
            return movesPossibleFromPos;
        }
    
        public int CheckForWin()
        {
            if(!removingP1 && !removingP2) return 0;
            if(player1CheckerCount == 0) return 1;
            if(player2CheckerCount == 0) return 2;
            return 0;
        }
    }

    
}