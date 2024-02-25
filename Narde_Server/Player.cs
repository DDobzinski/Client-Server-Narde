namespace Narde_Server
{
    public enum PlayerStatus
    {
        Menu, // Player vs Player
        Player, // Player vs AI
        Spectator // AI vs AI
    }

    class Player
    {
        public int id;
        public string username;
        public PlayerStatus currentStatus = PlayerStatus.Menu; 
        public Lobby? currentLobby;

        public Player(int id, string username)
        {
            this.id = id;
            this.username = username;
            currentLobby = null;
        }

        // Method for entering a lobby
        public void JoinLobby(Lobby lobby, PlayerStatus status)
        {
            currentLobby = lobby;
            currentStatus = status;
            // Additional logic for joining the lobby
        }

        // Method for leaving the current lobby
        public void LeaveLobby()
        {
            // Logic for leaving the lobby
            currentLobby = null;
            currentStatus = PlayerStatus.Menu;
        }
    }
    
}