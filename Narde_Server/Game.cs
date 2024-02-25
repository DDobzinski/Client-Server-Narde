namespace Narde_Server
{

    class Game
    {
        public List<Point> Board = new();
        public int player1ID;
        public int player2ID;
        public int currentPlayerID;
        public int diceResult1;
        public int diceResult2;
        public Game(int _player1ID, int _player2ID, int _dice1, int _dice2, int _currentPlayerID)
        {

            player1ID = _player1ID;
            player2ID = _player2ID;
            diceResult1 = _dice1;
            diceResult2 = _dice2;
            currentPlayerID = _currentPlayerID;
            for(int i = 0; i < 23; i++)
            {
                Board.Add(new Point());
            }
            Board[0].AddCheckers(15, player1ID);
            Board[11].AddCheckers(15, player2ID);
        }
    }
}