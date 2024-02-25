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
            
            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected and is now player {_fromClient}.");
            if(_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
                //Add Functionality to request Client to change ID
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
            
            for (int i = 1; i <= Server.MaxLobbies; i++)
            {
                if(Server.lobbies[i].lobbyName == null)//id is empty, means unnocupied
                {
                    Server.lobbies[i].lobbyName = _lobbyName;
                    Server.lobbies[i].gameState = GameState.Menu;
                    Server.lobbies[i].SetSpectatorLimit(_spectatorCount);
                    switch(_lobbyType) 
                    {
                        case "PvAI":
                            // code block
                            Server.lobbies[i].type = LobbyType.PvAI;
                            break;
                        case "AIvAI":
                            // code block
                            Server.lobbies[i].type = LobbyType.AIvAI;
                            break;
                        default:
                            Server.lobbies[i].type = LobbyType.PvP;
                            break;
                    }
                    
                    Server.lobbies[i].AddPlayer(Server.clients[_fromClient]);
                    
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
                Lobby lobby = Server.clients[_fromClient].player.currentLobby;
                Player player = Server.clients[_fromClient].player;
                if(player == null || lobby == null) Console.WriteLine($"Null lobby!");
                if(player.currentStatus == PlayerStatus.Player) lobby.RemovePlayer(Server.clients[_fromClient]);
                else lobby.RemoveSpectator(Server.clients[_fromClient]);
                int lobbyID = lobby.lobbyId;
                player.LeaveLobby();
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
                    if(Server.lobbies[_lobbyID].status == LobbyStatus.Open || Server.lobbies[_lobbyID].status == LobbyStatus.SpectatorsOnly) 
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
                    if(Server.lobbies[_lobbyID].status == LobbyStatus.Open || Server.lobbies[_lobbyID].status == LobbyStatus.PlayersOnly) 
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

        public static void StartGame(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                Lobby lobby = Server.clients[_fromClient].player.currentLobby;
                Player player = Server.clients[_fromClient].player;
                if(player == null || lobby == null) Console.WriteLine($"Null lobby or player!");

                if(player.currentStatus == PlayerStatus.Player || player.currentStatus == PlayerStatus.Spectator)
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
                        
                        if(lobby.type == LobbyType.PvP)
                        {
                            if(diceRoll1 > diceRoll2)
                            {
                                lobby.game = new Game(lobby.PlayerClients[0].id, lobby.PlayerClients[1].id, playDiceRoll1, playDiceRoll2, lobby.PlayerClients[0].id);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, true, playDiceRoll1, playDiceRoll2);
                                ServerSend.AllowGame(lobby.PlayerClients[1].id, false, playDiceRoll1, playDiceRoll2);
                            }
                            else
                            {
                                lobby.game = new Game(lobby.PlayerClients[0].id, lobby.PlayerClients[1].id, playDiceRoll1, playDiceRoll2, lobby.PlayerClients[1].id);
                                ServerSend.AllowGame(lobby.PlayerClients[0].id, false, playDiceRoll1, playDiceRoll2);
                                ServerSend.AllowGame(lobby.PlayerClients[1].id, true, playDiceRoll1, playDiceRoll2);
                            }
                            
                        }

                        foreach(var client in lobby.SpectatorClients)
                        {
                            ServerSend.AllowGame(client.id, false, playDiceRoll1, playDiceRoll2);
                        }
                    }
                    ServerSend.DenyGame(_fromClient);
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

        public static void PlayerSurrender(int _fromClient, Packet _packet)
        {
            int _id = _packet.ReadInt();
            if(_id == _fromClient)
            {
                Lobby lobby = Server.clients[_fromClient].player.currentLobby;
                Player player = Server.clients[_fromClient].player;

                if(player.currentStatus == PlayerStatus.Player)
                {
                    if(lobby.gameState == GameState.InGame)
                    {
                        string winnerName = "";
                        lobby.gameState = GameState.Menu;
                        if(lobby.type == LobbyType.PvP)
                        {
                            foreach(var client in lobby.PlayerClients)
                            {
                                if(client.id != _fromClient)
                                {
                                    winnerName = client.player.username;
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

                        foreach(var client in lobby.PlayerClients)
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
    }
}