using System.Collections.Generic;

namespace Narde_Server
{
    public enum LobbyType
    {
        PvP, // Player vs Player
        PvAI, // Player vs AI
        AIvAI // AI vs AI
    }

    public enum LobbyStatus
    {
        Open, // Lobby has open slots for players
        PlayersOnly, // Lobby has no spots for spectators
        SpectatorsOnly, // Lobby is full, but spectator slots are available
        Full, // Lobby is full, but spectator slots are available
    }

    public enum GameState
    {
        Menu, // Lobby has open slots for players
        InGame // Lobby is in a game, no slots available
    }

    class Lobby
    {
        public int lobbyId { get; set; }
        public string? lobbyName { get; set; }
        public List<Client> PlayerClients { get; set; }
        public List<Client> SpectatorClients { get; set; }
        public int spectatorLimit { get; set; }
        public LobbyType? type {get; set;}
        public LobbyStatus? status { get; set; }
        public GameState? gameState { get; set; }
        public Game? game { get; set; }
        public Lobby(int lobbyId)
        {
            this.lobbyId = lobbyId;
            this.lobbyName = null;
            PlayerClients = new List<Client>();
            SpectatorClients = new List<Client>();
            this.spectatorLimit = 0;
            this.type = null;
            this.status = null;
            gameState = null;
            game = null;
        }

        public void AddPlayer(Client player)
        {
            PlayerClients.Add(player);
            if((type == LobbyType.PvP && PlayerClients.Count == 2) || (type == LobbyType.PvAI && PlayerClients.Count == 1))
            {
                if(status == LobbyStatus.PlayersOnly)
                {
                    status = LobbyStatus.Full;
                }
                else
                {
                    status = LobbyStatus.SpectatorsOnly;
                }
            }
            player.player?.JoinLobby(this, PlayerStatus.Player);
        }

        // Method to remove a player from the lobby
        public void RemovePlayer(Client player, bool temp = false)
        {
            PlayerClients.Remove(player);
            if(!temp)
            {
                player.player?.LeaveLobby();
            }
            
            if(type == LobbyType.PvP && gameState == GameState.InGame)
            {
                if(PlayerClients[0].player == null) return;
                ServerSend.EndGame(PlayerClients[0].id, PlayerClients[0]?.player?.username);
                

                foreach(var client in SpectatorClients)
                {
                    ServerSend.EndGame(client.id, PlayerClients[0].player.username);
                }
            }
            else if(type == LobbyType.PvAI && gameState == GameState.InGame)
            {
                foreach(var client in SpectatorClients)
                {
                    ServerSend.EndGame(client.id, "AI");
                }
            }
            if(status == LobbyStatus.SpectatorsOnly)
            {
                status = LobbyStatus.Open;
            }
            else if(status == LobbyStatus.Full)
            {
                status = LobbyStatus.PlayersOnly;
            }
            if(PlayerClients.Count == 0 && SpectatorClients.Count == 0) Reset();
            else if(!temp)
            {
                foreach(var client in Server.lobbies[lobbyId].PlayerClients)
                        {   
                             ServerSend.UpdateLobby(client.id, lobbyId);

                        }
                foreach(var client in Server.lobbies[lobbyId].SpectatorClients)
                        {   

                            ServerSend.UpdateLobby(client.id, lobbyId);
                        }
            }
        }

        public void AddSpectator(Client spectator)
        {
            SpectatorClients?.Add(spectator);
            if(SpectatorClients?.Count == spectatorLimit)
            {
                if(status == LobbyStatus.SpectatorsOnly)
                {
                    status = LobbyStatus.Full;
                }
                else
                {
                    status = LobbyStatus.PlayersOnly;
                }
            }
            spectator.player?.JoinLobby(this, PlayerStatus.Spectator);
        }

        
        public void RemoveSpectator(Client spectator, bool temp = false)
        {
            
            SpectatorClients?.Remove(spectator);
            if(!temp)
            {
                spectator?.player?.LeaveLobby();
            }
            
            
            if(status == LobbyStatus.PlayersOnly)
            {
                status = LobbyStatus.Open;
            }
            else if(status == LobbyStatus.Full)
            {
                status = LobbyStatus.SpectatorsOnly;
            }
            
            if(PlayerClients?.Count == 0 && SpectatorClients?.Count == 0) Reset();
            else if(!temp && Server.lobbies[lobbyId].PlayerClients !=null)
            {
                   
                foreach(var client in Server.lobbies[lobbyId].PlayerClients)
                {   
                    ServerSend.UpdateLobby(client.id, lobbyId);

                }
                foreach(var client in Server.lobbies[lobbyId].SpectatorClients)
                {   

                    ServerSend.UpdateLobby(client.id, lobbyId);
                }

                if(type == LobbyType.AIvAI && gameState == GameState.InGame && spectator?.id == game?.hostID)
                {
                    foreach(var client in Server.lobbies[lobbyId].SpectatorClients)
                    {   

                        ServerSend.HostLeft(client.id);
                    }
                }
            }
        }

        public void Reset()
        {
            Console.WriteLine($"Lobby ID {lobbyId} is reset.");
            this.lobbyName = null;
            this.spectatorLimit = 0;
            this.type = null;
            this.status = null;
        }

        public void SetSpectatorLimit(int limit)
        {
            if(limit == 0)
            {
                status = LobbyStatus.PlayersOnly;
            }
            else if(type == LobbyType.AIvAI)
            {
                status = LobbyStatus.SpectatorsOnly;
            }
            else
            {
                status = LobbyStatus.Open;
            }
            spectatorLimit = limit;
        }
    }
}