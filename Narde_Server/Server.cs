// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Dynamic;

namespace Narde_Server
{
    class Server
    {
        public static int MaxPlayers{get; private set;}// can only be set from within the class
        public static int MaxLobbies{get; private set;}// can only be set from within the class
        //Unnocupied port number
        public static int Port{get; private set;}
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();//Dictionary of clients, id is used as keys
        public static Dictionary<int, Lobby> lobbies = new Dictionary<int, Lobby>();//Dictionary of lobbies, id is used as keys
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler>? packetHandlers;

        //Handles most of the connection stuff 
        private static TcpListener? tcpListener;
        
        //Sets up Server on the start
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            MaxLobbies = _maxPlayers;
            Port = _port;

            Console.WriteLine($"Starting server...");
            InitializeServerData();

            //Starting TcpListener
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}.");
        }
        
        //Called when client connects

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if(tcpListener == null)
            {
                Console.WriteLine($"Null tcpListener");
                return;
            } 
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);//holds clients info
            
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Connecting {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)//id is empty, means unnocupied
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }
            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData()
        {
            for( int i = 1; i <= MaxPlayers; i++)//1 is first id
            {
                clients.Add(i, new Client(i));
                lobbies.Add(i, new Lobby(i));
            }
            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived},
                {(int)ClientPackets.createLobby, ServerHandle.CreateLobby},
                {(int)ClientPackets.leaveLobby, ServerHandle.LeaveLobby},
                {(int)ClientPackets.getLobbies, ServerHandle.GetLobbies},
                {(int)ClientPackets.joinLobby, ServerHandle.JoinLobby},
                {(int)ClientPackets.startGame, ServerHandle.StartGame},
                {(int)ClientPackets.surrender, ServerHandle.PlayerSurrender},
                {(int)ClientPackets.endTurn, ServerHandle.PlayerEndTurn},
                {(int)ClientPackets.endTurnAI, ServerHandle.AIEndTurn},
                {(int)ClientPackets.switchStatus, ServerHandle.SwitchStatus},
                {(int)ClientPackets.sendMessage, ServerHandle.ForwardMessage}
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}
