using System;
using System.Collections.Generic;
using System.Text;

namespace Narde_Server
{
    class ServerSend
    {
        //Prepares packet to be sent and sends it
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        //Sends data to all clients
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 0; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
            
        }

        //Sends data to all clients except one specific
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 0; i <= Server.MaxPlayers; i++)
            {
                if(i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }  
            }
            
        }
        //Creates Welcom Packet
        //Input: id of client to send packet, string with message to send
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void LobbyCreated(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.lobbyCreated))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                Lobby lobby = Server.clients[_toClient].player.currentLobby;
                _packet.Write(lobby.lobbyId);
                _packet.Write(lobby.lobbyName);
                switch(lobby.type) 
                    {
                        case LobbyType.PvP:
                            // code block
                            _packet.Write(1);//PvP
                            break;
                        case LobbyType.PvAI:
                            // code block
                            _packet.Write(2);//PvAI
                            break;
                        default:
                            _packet.Write(3);//AIvAI
                            break;
                    }

                switch(lobby.status) 
                {
                    case LobbyStatus.Open:
                        // code block
                        _packet.Write(1);
                        break;
                    case LobbyStatus.PlayersOnly:
                        // code block
                        _packet.Write(2);
                        break;
                    case LobbyStatus.SpectatorsOnly:
                        // code block
                        _packet.Write(3);
                        break;
                    default:
                        _packet.Write(4);//SpectatorsOnly
                        break;
                }
                SendTCPData(_toClient, _packet);
            }
        }

        public static void SendLobbies(int _toClient)
        {
            List<Lobby> allowedLobbies = new List<Lobby>();
            for(int i = 1; i <= Server.MaxLobbies; i++)
            {
                Lobby lobby = Server.lobbies[i];
                if(lobby.status != null && lobby.gameState != GameState.InGame && lobby.status != LobbyStatus.Full)
                {
                    allowedLobbies.Add(lobby);
                }
            }   
            using (Packet _packet = new Packet((int)ServerPackets.sendLobbies))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                _packet.Write(allowedLobbies.Count);
                foreach (var lobby in allowedLobbies)
                {
                    _packet.Write(lobby.lobbyId);
                    _packet.Write(lobby.lobbyName);
                    switch(lobby.type) 
                    {
                        case LobbyType.PvP:
                            // code block
                            _packet.Write(1);//PvP
                            break;
                        case LobbyType.PvAI:
                            // code block
                            _packet.Write(2);//PvAI
                            break;
                        default:
                            _packet.Write(3);//AIvAI
                            break;
                    }

                    switch(lobby.status) 
                    {
                        case LobbyStatus.Open:
                            // code block
                            _packet.Write(1);
                            break;
                        case LobbyStatus.PlayersOnly:
                            // code block
                            _packet.Write(2);
                            break;
                        default:
                            _packet.Write(3);//SpectatorsOnly
                            break;
                    }
                }
                SendTCPData(_toClient, _packet);
                Console.WriteLine($"Sent to ID: {_toClient}) lobbies ({allowedLobbies.Count})!");
            }
        }

        public static void RejectLobby(int _toClient)
        {
            List<Lobby> allowedLobbies = new List<Lobby>();
            for(int i = 1; i <= Server.MaxLobbies; i++)
            {
                Lobby lobby = Server.lobbies[i];
                if(lobby.status != null && lobby.gameState != GameState.InGame && lobby.status != LobbyStatus.Full)
                {
                    allowedLobbies.Add(lobby);
                }
            }   
            using (Packet _packet = new Packet((int)ServerPackets.sendLobbies))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                _packet.Write(allowedLobbies.Count);
                foreach (var lobby in allowedLobbies)
                {
                    _packet.Write(lobby.lobbyId);
                    _packet.Write(lobby.lobbyName);
                    switch(lobby.type) 
                    {
                        case LobbyType.PvP:
                            // code block
                            _packet.Write(1);//PvP
                            break;
                        case LobbyType.PvAI:
                            // code block
                            _packet.Write(2);//PvAI
                            break;
                        default:
                            _packet.Write(3);//AIvAI
                            break;
                    }

                    switch(lobby.status) 
                    {
                        case LobbyStatus.Open:
                            // code block
                            _packet.Write(1);
                            break;
                        case LobbyStatus.PlayersOnly:
                            // code block
                            _packet.Write(2);
                            break;
                        default:
                            _packet.Write(3);//SpectatorsOnly
                            break;
                    }
                }
                SendTCPData(_toClient, _packet);
            }
        }

        public static void ConfirmJoin(int _toClient, int _lobbyID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.confirmJoin))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                Lobby lobby = Server.lobbies[_lobbyID];
                _packet.Write(lobby.lobbyId);
                _packet.Write(lobby.lobbyName);
                switch(lobby.type) 
                {
                    case LobbyType.PvP:
                        // code block
                        _packet.Write(1);//PvP
                        break;
                    case LobbyType.PvAI:
                        // code block
                        _packet.Write(2);//PvAI
                        break;
                    default:
                        _packet.Write(3);//AIvAI
                        break;
                }

                switch(lobby.status) 
                {
                    case LobbyStatus.Open:
                        // code block
                        _packet.Write(1);
                        break;
                    case LobbyStatus.PlayersOnly:
                        // code block
                        _packet.Write(2);
                        break;
                    case LobbyStatus.SpectatorsOnly:
                        // code block
                        _packet.Write(3);
                        break;
                    default:
                        _packet.Write(4);//SpectatorsOnly
                        break;
                }
                if(Server.clients[_toClient].player.currentStatus == PlayerStatus.Player) _packet.Write(1);
                else _packet.Write(2);
                _packet.Write(lobby.PlayerClients.Count);
                foreach(var client in lobby.PlayerClients)
                {
                    _packet.Write(client.player.username);
                }
                _packet.Write(lobby.SpectatorClients.Count);
                foreach(var client in lobby.SpectatorClients)
                {
                    _packet.Write(client.player.username);
                }
                SendTCPData(_toClient, _packet);
            }
        }

        public static void UpdateLobby(int _toClient, int _lobbyID)
        {
            using (Packet _packet = new Packet((int)ServerPackets.updateLobby))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                Lobby lobby = Server.lobbies[_lobbyID];
                _packet.Write(lobby.lobbyId);
                _packet.Write(lobby.lobbyName);
                switch(lobby.type) 
                {
                    case LobbyType.PvP:
                        // code block
                        _packet.Write(1);//PvP
                        break;
                    case LobbyType.PvAI:
                        // code block
                        _packet.Write(2);//PvAI
                        break;
                    default:
                        _packet.Write(3);//AIvAI
                        break;
                }

                switch(lobby.status) 
                {
                    case LobbyStatus.Open:
                        // code block
                        _packet.Write(1);
                        break;
                    case LobbyStatus.PlayersOnly:
                        // code block
                        _packet.Write(2);
                        break;
                    case LobbyStatus.SpectatorsOnly:
                        // code block
                        _packet.Write(3);
                        break;
                    default:
                        _packet.Write(4);//SpectatorsOnly
                        break;
                }
                _packet.Write(lobby.PlayerClients.Count);
                foreach(var client in lobby.PlayerClients)
                {
                    _packet.Write(client.player.username);
                }
                _packet.Write(lobby.SpectatorClients.Count);
                foreach(var client in lobby.SpectatorClients)
                {
                    _packet.Write(client.player.username);
                }
                SendTCPData(_toClient, _packet);
            }
        }

        public static void AllowGame(int _toClient, bool firstMove, int dice1, int dice2)
        {
            using (Packet _packet = new Packet((int)ServerPackets.allowGame))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                _packet.Write(firstMove);
                _packet.Write(dice1);
                _packet.Write(dice2);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void EndGame(int _toClient, string WinnerName)
        {
            using (Packet _packet = new Packet((int)ServerPackets.endGame))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                _packet.Write(WinnerName);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void DenyGame(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.denyGame))//using block makes so that packet is automatically disposed
            {
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }
    }
}