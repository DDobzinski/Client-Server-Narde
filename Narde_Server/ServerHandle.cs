using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Narde_Server
{
    class ServerHandle
    {
        //
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket?.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            if(_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                //Add Functionality to request Client to change ID
                Server.clients[_clientIdCheck].Disconnect();
            }
            else
            {
                Server.clients[_fromClient].player = new Player(_fromClient, _username);
                //Maybe add functionality so that names have to be unique, thus need to get back to client to say that name not allowed.
            }
        }

        public static void CreateLobby(int _fromClient, Packet _packet)
        {
            string _lobbyName = _packet.ReadString();
            int _spectatorCount = _packet.ReadInt();
            string _lobbyType= _packet.ReadString();
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            if(_lobbyType.Equals("AIvAI") && _spectatorCount < 1) return;
            if(_spectatorCount > 4 || _spectatorCount< 0) return;
            for (int i = 1; i <= Server.MaxLobbies; i++)
            {
                if(Server.lobbies[i].lobbyName == null)
                {
                    Server.lobbies[i].lobbyName = _lobbyName;
                    Server.lobbies[i].gameState = GameState.Menu;
                    Server.lobbies[i].type = _lobbyType switch
                    {
                        "PvAI" => (LobbyType?)LobbyType.PvAI,
                        "AIvAI" => (LobbyType?)LobbyType.AIvAI,
                        _ => (LobbyType?)LobbyType.PvP,
                    };
                    Server.lobbies[i].SetSpectatorLimit(_spectatorCount);
                    
                    if (Server.lobbies[i].type != LobbyType.AIvAI)
                    {
                        Server.lobbies[i].AddPlayer(Server.clients[_fromClient]);
                    }
                    else
                    {
                        Server.lobbies[i].AddSpectator(Server.clients[_fromClient]);
                    }
                    
                    //Server.clients[_fromClient].player.JoinLobby(Server.lobbies[i], PlayerStatus.Player);
                    Console.WriteLine($"{_fromClient} created a lobby {i}.");
                    ServerSend.LobbyCreated(_fromClient, $"Lobby {i} created!");
                    break;
                }
            }
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        public static void LeaveLobby(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                Player? player = Server.clients[_fromClient].player;
                if(player == null || lobby == null) Console.WriteLine($"Null lobby!");
                if(player?.currentStatus == PlayerStatus.Player) lobby?.RemovePlayer(Server.clients[_fromClient]);
                else lobby?.RemoveSpectator(Server.clients[_fromClient]);
                int? lobbyID = lobby?.lobbyId;
                player?.LeaveLobby();
                Console.WriteLine($"ID: {_fromClient}) left lobby ID ({lobbyID})!");
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        public static void GetLobbies(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                ServerSend.SendLobbies(_id);
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

         public static void JoinLobby(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                int _lobbyID = _packet.ReadInt();
                bool _asSpectator = _packet.ReadBool();

                if(_asSpectator)
                {
                    if((Server.lobbies[_lobbyID].status == LobbyStatus.Open || Server.lobbies[_lobbyID].status == LobbyStatus.SpectatorsOnly)&& Server.lobbies[_lobbyID].gameState == GameState.Menu) 
                    {
                        Console.WriteLine($"{_fromClient} joined lobby {_lobbyID} as spectator.");
                        Server.lobbies[_lobbyID].AddSpectator(Server.clients[_id]);
                        ServerSend.ConfirmJoin(_id, _lobbyID);
                        foreach(var client in Server.lobbies[_lobbyID].PlayerClients)
                        {   
                            if(client.id != _id)
                            {
                                ServerSend.UpdateLobby(client.id, _lobbyID);
                            }
                        }
                        foreach(var client in Server.lobbies[_lobbyID].SpectatorClients)
                        {   
                            if(client.id != _id)
                            {
                                ServerSend.UpdateLobby(client.id, _lobbyID);
                            }
                        }
                    }
                    else
                    {
                        ServerSend.RejectLobby(_id);
                    }
                }
                else
                {
                    if((Server.lobbies[_lobbyID].status == LobbyStatus.Open || Server.lobbies[_lobbyID].status == LobbyStatus.PlayersOnly) && Server.lobbies[_lobbyID].gameState == GameState.Menu) 
                    {
                        Console.WriteLine($"{_fromClient} created joined lobby {_lobbyID} as player.");
                        Server.lobbies[_lobbyID].AddPlayer(Server.clients[_id]);
                        ServerSend.ConfirmJoin(_id, _lobbyID);

                        foreach(var client in Server.lobbies[_lobbyID].PlayerClients)
                        {   
                            if(client.id != _id)
                            {
                                ServerSend.UpdateLobby(client.id, _lobbyID);
                            }
                        }
                        foreach(var client in Server.lobbies[_lobbyID].SpectatorClients)
                        {   
                            if(client.id != _id)
                            {
                                ServerSend.UpdateLobby(client.id, _lobbyID);
                            }
                        }
                    }
                    else
                    {
                        ServerSend.RejectLobby(_id);
                    }
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

         public static void SwitchStatus(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                PlayerStatus? status = Server.clients[_fromClient].player?.currentStatus;
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                if(status == PlayerStatus.Player && lobby?.gameState == GameState.Menu)
                {
                    if(lobby.status == LobbyStatus.Open || lobby.status == LobbyStatus.SpectatorsOnly) 
                    {
                        Console.WriteLine($"{_fromClient} switched to spectator.");
                        lobby.AddSpectator(Server.clients[_id]);
                        lobby.RemovePlayer(Server.clients[_id], true);
                        Server.clients[_fromClient].player?.SetStatus(PlayerStatus.Spectator);

                        ServerSend.ConfirmSwitch(_id);
                        foreach(var client in lobby.PlayerClients)
                        {   
                            
                            ServerSend.UpdateLobby(client.id, lobby.lobbyId);
                            
                        }
                        foreach(var client in lobby.SpectatorClients)
                        {   
                            ServerSend.UpdateLobby(client.id, lobby.lobbyId);
                            
                        }
                    }
                    else
                    {
                        ServerSend.DenySwitch(_fromClient);
                    }
                }
                else if(status == PlayerStatus.Spectator && lobby?.gameState == GameState.Menu)
                {
                    if(lobby.status == LobbyStatus.Open || lobby.status == LobbyStatus.PlayersOnly) 
                    {
                        Console.WriteLine($"{_fromClient} switched to player.");
                        lobby.AddPlayer(Server.clients[_id]);
                        lobby.RemoveSpectator(Server.clients[_id], true);
                        if(Server.clients[_fromClient].player != null)
                        {
                            Server.clients[_fromClient].player?.SetStatus(PlayerStatus.Player);
                        }
                        
                        ServerSend.ConfirmSwitch(_id);

                        foreach(var client in lobby.PlayerClients)
                        {   
                            
                            ServerSend.UpdateLobby(client.id, lobby.lobbyId);
                            
                        }
                        foreach(var client in lobby.SpectatorClients)
                        {   
                            
                            ServerSend.UpdateLobby(client.id, lobby.lobbyId);
                            
                        }
                    }
                    else
                    {
                       ServerSend.DenySwitch(_fromClient);
                    }
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }
        
         public static void ForwardMessage(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            string message = _packet.ReadString();
            if(_id == _fromClient)
            {
                PlayerStatus? status = Server.clients[_fromClient].player?.currentStatus;
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                if((status == PlayerStatus.Player || status == PlayerStatus.Spectator ) && lobby?.gameState == GameState.Menu)
                {
                    string? username = Server.clients[_fromClient].player?.username;
                    bool player = status == PlayerStatus.Player;
            
                    
                    foreach(var client in lobby.PlayerClients)
                    {   
                        if(username == null)
                        {
                            ServerSend.ForwardMessage(client.id, "username", message, player);
                        }
                        else
                        {
                            ServerSend.ForwardMessage(client.id, username, message, player);
                        }   
                        
                        
                    }
                    foreach(var client in lobby.SpectatorClients)
                    {   
                        if(username == null)
                        {
                            ServerSend.ForwardMessage(client.id, "username", message, player);
                        }
                        else
                        {
                            ServerSend.ForwardMessage(client.id, username, message, player);
                        } 
                        
                    }
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
        }

        public static void StartGame(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                Player? player = Server.clients[_fromClient].player;
                if(player == null || lobby == null) Console.WriteLine($"Null lobby or player!");

                if((player?.currentStatus == PlayerStatus.Player || player?.currentStatus == PlayerStatus.Spectator) && lobby?.gameState == GameState.Menu)
                {
                    if(lobby.status == LobbyStatus.SpectatorsOnly || lobby.status == LobbyStatus.Full)
                    {
                        lobby.gameState = GameState.InGame;
                        
                        Random rand = new Random();
                        int diceRoll1 = rand.Next(1, 7);
                        int diceRoll2 = rand.Next(1, 7);
                        while(diceRoll1 == diceRoll2)
                        {
                            diceRoll1 = rand.Next(1, 7);
                            diceRoll2 = rand.Next(1, 7);
                        }
                        int playDiceRoll1 = rand.Next(1, 7);
                        int playDiceRoll2 = rand.Next(1, 7);
                        bool p1 = diceRoll1 > diceRoll2;
                        
                        if(p1)
                        {
                            if(lobby.type == LobbyType.PvP)
                            {
                                lobby.game = new Game(lobby.PlayerClients[0].id, lobby.PlayerClients[1].id, playDiceRoll1, playDiceRoll2, lobby.PlayerClients[0].id);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, true, playDiceRoll1, playDiceRoll2);
                                ServerSend.AllowGame(lobby.PlayerClients[1].id, false, playDiceRoll1, playDiceRoll2);
                            }
                            else if(lobby.type == LobbyType.PvAI)
                            {
                                lobby.game = new Game(lobby.PlayerClients[0].id, -1, playDiceRoll1, playDiceRoll2, lobby.PlayerClients[0].id);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, true, playDiceRoll1, playDiceRoll2);
                            }
                            else if(lobby.type == LobbyType.AIvAI)
                            {
                                lobby.game = new Game(-1, -2, playDiceRoll1, playDiceRoll2, -1, _fromClient);
                                ServerSend.AllowGame(_fromClient, true, playDiceRoll1, playDiceRoll2, true);
                            }
                        }
                        else
                        {
                            if(lobby.type == LobbyType.PvP)
                            {
                                lobby.game = new Game(lobby.PlayerClients[1].id, lobby.PlayerClients[0].id, playDiceRoll1, playDiceRoll2, lobby.PlayerClients[1].id);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, false, playDiceRoll1, playDiceRoll2);
                                ServerSend.AllowGame(lobby.PlayerClients[1].id, true, playDiceRoll1, playDiceRoll2);
                            }
                            else if(lobby.type == LobbyType.PvAI)
                            {
                                lobby.game = new Game(-1, lobby.PlayerClients[0].id, playDiceRoll1, playDiceRoll2, -1);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, false, playDiceRoll1, playDiceRoll2);
                            }
                            else if(lobby.type == LobbyType.AIvAI)
                            {
                                lobby.game = new Game(-2, -1, playDiceRoll1, playDiceRoll2, -2, _fromClient);
                                ServerSend.AllowGame(_fromClient, true, playDiceRoll1, playDiceRoll2, false);
                            }
                        }

                        foreach(var client in lobby.SpectatorClients)
                        {
                            if(lobby.type == LobbyType.AIvAI)
                            {
                                if(_fromClient != client.id)
                                {
                                    ServerSend.AllowGame(client.id, false, playDiceRoll1, playDiceRoll2, p1);
                                }
                            }
                            else
                            {
                                ServerSend.AllowGame(client.id, false, playDiceRoll1, playDiceRoll2);
                            }
                           
                        }
                    }
                    else
                    {
                        ServerSend.DenyGame(_fromClient);
                    }
                    
                    
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        public static void PlayerSurrender(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                Player? player = Server.clients[_fromClient].player;

                if(player?.currentStatus == PlayerStatus.Player)
                {
                    if(lobby?.gameState == GameState.InGame)
                    {
                        string winnerName = "";
                        lobby.gameState = GameState.Menu;
                        lobby.game = null;
                        if(lobby.type == LobbyType.PvP)
                        {
                            foreach(var client in lobby.PlayerClients)
                            {
                                if(client.id != _fromClient)
                                {
                                    if(client.player != null)
                                    {
                                        winnerName = client.player.username;
                                    }
                                    else
                                    {
                                        winnerName = "username";
                                    }
                                    break;
                                }
                            }
                        }else 
                        {
                            winnerName = "AI";
                        }
                        
                        foreach(var client in lobby.PlayerClients)
                        {
                            ServerSend.EndGame(client.id, winnerName);
                        }

                        foreach(var client in lobby.SpectatorClients)
                        {
                            ServerSend.EndGame(client.id, winnerName);
                        }
                    }
                    //Deny if lobby not ready
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        public static void PlayerEndTurn(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            
            if(_id == _fromClient)
            {
                Lobby? lobby = Server.clients[_fromClient].player?.currentLobby;
                Player? player = Server.clients[_fromClient].player;

                if(player?.currentStatus == PlayerStatus.Player)
                {
                    if(lobby?.gameState == GameState.InGame && (lobby.game?.currentPlayerID == _fromClient || lobby.type == LobbyType.PvAI && (_fromClient == lobby.game.player1ID || _fromClient == lobby.game.player2ID)))
                    {
                        
                        int moveCount = _packet.ReadInt();
                        
                        List<Move> moves = new();
                        for(int i = 0; i < moveCount; i++)
                        {
                            int startPointID = _packet.ReadInt();
                            int targetPointID = _packet.ReadInt();
                            int diceCount = _packet.ReadInt();
                            
                            List<int> DiceUsed = new();
                            for(int j = 0; j < diceCount; j++)
                            {
                                DiceUsed.Add(_packet.ReadInt());
                            }
                            moves.Add(new(startPointID, targetPointID, DiceUsed));
                        }
                        
                        bool valid = Server.clients[_fromClient].player.currentLobby.game.ValidateTurn(moves);
                        if(valid)
                        {
                            int? playerWon = Server.clients[_fromClient].player?.currentLobby?.game?.CheckForWin();

                            if(playerWon == 1)
                            {
                                if(lobby.type == LobbyType.PvP)
                                {
                                    foreach(var client in lobby.PlayerClients)
                                    {
                                        ServerSend.EndGame(client.id,  Server.clients[Server.clients[_fromClient].player.currentLobby.game.player1ID].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {
                                        ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player1ID].player.username);
                                    }
                                }
                                else if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.player1ID == -1)
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                    }
                                    else
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player1ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                        }
                                    }
                                }

                                
                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                            else if(playerWon == 2)
                            {
                                if(lobby.type == LobbyType.PvP)
                                {
                                    foreach(var client in lobby.PlayerClients)
                                    {
                                        ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player2ID].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {
                                        ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player2ID].player.username);
                                    }
                                }
                                else if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.player2ID == -1)
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                    }
                                    else
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player2ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                        }
                                    }
                                }
                                

                                
                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                            else
                            {
                                if(lobby.type == LobbyType.PvP)
                                {
                                    foreach(var client in lobby.PlayerClients)
                                    {
                                        if(client.id == lobby.game.currentPlayerID)
                                        {
                                            ServerSend.UpdateGame(client.id, true, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                        }
                                        else
                                        {
                                            ServerSend.UpdateGame(client.id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                        }
                                    }
                                }
                                else if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.currentPlayerID == -1)
                                    {
                                        ServerSend.UpdateGame(lobby.PlayerClients[0].id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                    }
                                    else
                                    {
                                        ServerSend.UpdateGame(lobby.PlayerClients[0].id, true, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                    }
                                }
                                

                                foreach(var client in lobby.SpectatorClients)
                                {
                                    ServerSend.UpdateGame(client.id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                }
                            }
                        }
                        else
                        {
                            Game? game = Server.clients[_fromClient].player?.currentLobby?.game;
                            if(game?.player1ID == game?.currentPlayerID)
                            {
                                game.player1InvalidMoveCount ++;
                                if(game.player1InvalidMoveCount > 2)
                                {
                                    if(lobby.type == LobbyType.PvP)
                                    {
                                        foreach(var client in lobby.PlayerClients)
                                        {
                                            ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player2ID].player.username);
                                        }
                                        foreach(var client in lobby.SpectatorClients)
                                        {
                                            ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player2ID].player.username);
                                        }
                                    }
                                    else if(lobby.type == LobbyType.PvAI)
                                    {
                                        if(lobby.game.player2ID == -1)
                                        {
                                            ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                        }
                                        foreach(var client in lobby.SpectatorClients)
                                        {   
                                            if(lobby.game.player2ID == -1)
                                            {
                                                ServerSend.EndGame(client.id, "AI");
                                            }
                                            else
                                            {
                                                ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                            }
                                        }
                                    }

                                    lobby.game = null;
                                    lobby.gameState = GameState.Menu;
                                }
                                else
                                {
                                    ServerSend.InvalidTurn(_fromClient);
                                }
                            }
                            else
                            {
                                game.player2InvalidMoveCount ++;
                                if(game.player2InvalidMoveCount > 2)
                                {
                                    if(lobby.type == LobbyType.PvP)
                                    {
                                        foreach(var client in lobby.PlayerClients)
                                        {
                                            ServerSend.EndGame(client.id,  Server.clients[Server.clients[_fromClient].player.currentLobby.game.player1ID].player.username);
                                        }
                                        foreach(var client in lobby.SpectatorClients)
                                        {
                                            ServerSend.EndGame(client.id, Server.clients[Server.clients[_fromClient].player.currentLobby.game.player1ID].player.username);
                                        }
                                    }
                                    else if(lobby.type == LobbyType.PvAI)
                                    {
                                        if(lobby.game.player1ID == -1)
                                        {
                                            ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                        }
                                        foreach(var client in lobby.SpectatorClients)
                                        {   
                                            if(lobby.game.player1ID == -1)
                                            {
                                                ServerSend.EndGame(client.id, "AI");
                                            }
                                            else
                                            {
                                                ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                            }
                                        }
                                    }
                                    
                                    lobby.game =null;
                                    lobby.gameState = GameState.Menu;
                                }
                                else
                                {
                                    ServerSend.InvalidTurn(_fromClient);
                                }
                                
                            }  
                        }
                    }
                    
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }

        public static void AIEndTurn(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            
            if(_id == _fromClient)
            {
                Lobby lobby = Server.clients[_fromClient].player.currentLobby;
                Player player = Server.clients[_fromClient].player;

                if(player.currentStatus == PlayerStatus.Player|| (player.currentStatus == PlayerStatus.Spectator && lobby.type == LobbyType.AIvAI && _fromClient == lobby.game.hostID))
                {
                    if(lobby.gameState == GameState.InGame &&  ((lobby.type == LobbyType.PvAI && (_fromClient == lobby.game.player1ID || _fromClient == lobby.game.player2ID)) || (lobby.type == LobbyType.AIvAI && _fromClient == lobby.game.hostID)))
                    {
                        
                        int moveCount = _packet.ReadInt();
                        
                        List<Move> moves = new();
                        for(int i = 0; i < moveCount; i++)
                        {
                            int startPointID = _packet.ReadInt();
                            int targetPointID = _packet.ReadInt();
                            int diceCount = _packet.ReadInt();
                            
                            List<int> DiceUsed = new();
                            for(int j = 0; j < diceCount; j++)
                            {
                                DiceUsed.Add(_packet.ReadInt());
                            }
                            moves.Add(new(startPointID, targetPointID, DiceUsed));
                        }
                        
                        bool valid = Server.clients[_fromClient].player.currentLobby.game.ValidateTurn(moves);
                        if(valid)
                        {
                            int playerWon = Server.clients[_fromClient].player.currentLobby.game.CheckForWin();

                            if(playerWon == 1)
                            {
                                if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.player1ID == -1)
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                    }
                                    else
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player1ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                        }
                                    }
                                }
                                else if(lobby.type == LobbyType.AIvAI)
                                {
                    
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player1ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "Advanced Agent");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, "Random Agent");
                                        }
                                    }
                                }


                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                            else if(playerWon == 2)
                            {
                                if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.player2ID == -1)
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, "AI");
                                    }
                                    else
                                    {
                                        ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                    }
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player2ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "AI");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                        }
                                    }
                                }
                                else if(lobby.type == LobbyType.AIvAI)
                                {
                    
                                    foreach(var client in lobby.SpectatorClients)
                                    {   
                                        if(lobby.game.player2ID == -1)
                                        {
                                            ServerSend.EndGame(client.id, "Advanced Agent");
                                        }
                                        else
                                        {
                                            ServerSend.EndGame(client.id, "Random Agent");
                                        }
                                    }
                                }
                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                            else
                            {
                                
                                if(lobby.type == LobbyType.PvAI)
                                {
                                    if(lobby.game.currentPlayerID == -1)
                                    {
                                        ServerSend.UpdateGame(lobby.PlayerClients[0].id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                    }
                                    else
                                    {
                                        ServerSend.UpdateGame(lobby.PlayerClients[0].id, true, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                    }

                                    foreach(var client in lobby.SpectatorClients)
                                    {
                                        ServerSend.UpdateGame(client.id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves);
                                    }
                                }
                                else if(lobby.type == LobbyType.AIvAI)
                                {

                                    foreach(var client in lobby.SpectatorClients)
                                    {  
                                        if(lobby.game.currentPlayerID == -1)
                                        {
                                            if(client.id ==lobby.game.hostID)
                                            {
                                                ServerSend.UpdateGame(client.id, true, lobby.game.diceResult1, lobby.game.diceResult2, moves, true);
                                            }
                                            else
                                            {
                                                ServerSend.UpdateGame(client.id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves, true);
                                            }
                                            
                                        }
                                        else
                                        {
                                            if(client.id ==lobby.game.hostID)
                                            {
                                                ServerSend.UpdateGame(client.id, true, lobby.game.diceResult1, lobby.game.diceResult2, moves, false);
                                            }
                                            else
                                            {
                                                ServerSend.UpdateGame(client.id, false, lobby.game.diceResult1, lobby.game.diceResult2, moves, false);
                                            }
                                        }
                                        
                                    }
                                }

                                
                            }
                        }
                        else 
                        {
                            if(lobby.type == LobbyType.PvAI)
                            {
                                ServerSend.EndGame(lobby.PlayerClients[0].id, lobby.PlayerClients[0].player.username);
                                foreach(var client in lobby.SpectatorClients)
                                {
                                    ServerSend.EndGame(client.id, lobby.PlayerClients[0].player.username);
                                }
                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                            if(lobby.type == LobbyType.AIvAI)
                            {
                                string winnerName;
                                if(lobby.game.currentPlayerID == -1)
                                {
                                    winnerName = "Random Agent";
                                }
                                else
                                {
                                    winnerName = "Advanced Agent";
                                }
                                foreach(var client in lobby.SpectatorClients)
                                {
                                    ServerSend.EndGame(client.id, winnerName);
                                }
                                lobby.game =null;
                                lobby.gameState = GameState.Menu;
                            }
                        }
                    }
                    
                }
            }
            else
            {
                Console.WriteLine($"ID: {_fromClient}) has assumed the wrong client ID ({_id})!");
            }
            //Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            
            
            //Server.clients[_fromClient].player = new Player(_fromClient, _username);
        }
    }
}