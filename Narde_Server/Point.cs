namespace Narde_Server
{

    class Point
    {
        public int checkerCount;

        public int playerID;

        public Point(int amountOfCheckers = 0, int _playerID = 0)
        {
            checkerCount = amountOfCheckers;
            playerID = _playerID;
        }

        public void AddCheckers(int checkerAmount, int _playerID)
        {
            checkerCount += checkerAmount;
            playerID = _playerID;
        }

        public void RemoveCheckers(int checkerAmount)
        {
            if(checkerCount >= checkerAmount)
            {
                checkerCount -= checkerAmount;

                if(checkerCount == 0) playerID = 0;
            }
        }

        public Point ShallowCopy()
        {
            return (Point)this.MemberwiseClone();
        }
    }
}