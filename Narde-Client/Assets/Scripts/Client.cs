using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public static Client instance;//for singleton implementation
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";//localhost ip
    public int port = 21224;
    public int myId = 0;//local clients id
    public TCP tcp;
    private bool isConnnected = false;


    public Player player;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    //initializes singleton
    private void Awake()
    {
        if(instance == null)//ensures only one instance of the client class exists
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Makes this GameObject persist across scenes
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        isConnnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            
            socket.EndConnect(_result);

            if(!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();
            
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
             
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if(socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch(Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            
            try
            {
                
                int _byteLength = stream.EndRead(_result);
                if(_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }
                byte[] _data = new byte[_byteLength];

                Array.Copy(receiveBuffer, _data, _byteLength);

                
                //Whether receivedData gets reset depends on value of HandleData.
                ///This needs to be done because of the way TCP works, it adds packet to a larger list fof bytes,
                ///and when enough bytes accumulate it sends them.
                ///Because of this data can be split in two. Because of that we can't always reset.
                receivedData.Reset(HandleData(_data));

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
                
            }
        }
        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);
            ///Check if receivedData has more than 4 unread bytes
            ///If it does then we have start of a packet, because int is 4 bytes, and first thing sent is length of packet
            if(receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();

                if(_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using(Packet _packet = new Packet(_packetBytes)) 
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;

                if(receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();

                    if(_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    
        private void Disconnect()
        {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }
    //Initializes dictionary of packetHandlers
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int) ServerPackets.welcome, ClientHandle.Welcome},
            {(int) ServerPackets.lobbyCreated, ClientHandle.LobbyCreated},
            {(int) ServerPackets.sendLobbies, ClientHandle.ReceiveLobbies},
            {(int) ServerPackets.rejectJoin, ClientHandle.RejectJoin},
            {(int) ServerPackets.confirmJoin, ClientHandle.ConfirmJoin},
            {(int) ServerPackets.updateLobby, ClientHandle.UpdateLobby},
            {(int) ServerPackets.allowGame, ClientHandle.AllowGame},
            {(int) ServerPackets.denyGame, ClientHandle.DenyGame},
            {(int) ServerPackets.endGame, ClientHandle.EndGame},
            {(int) ServerPackets.updateGame, ClientHandle.UpdateGame},
            {(int) ServerPackets.invalidTurn, ClientHandle.InvalidTurn},
            {(int) ServerPackets.hostLeft, ClientHandle.HostLeft},
            {(int) ServerPackets.confirmSwitch, ClientHandle.ConfirmSwitch},
            {(int) ServerPackets.denySwitch, ClientHandle.DenySwitch},
            {(int) ServerPackets.forwardMessage, ClientHandle.UpdateChat}
        };
        Debug.Log("Initialized packets.");
    }

    public void Disconnect()
    {
        if(isConnnected)
        {
            isConnnected = false;
            tcp.socket.Close();
            player = null;
            SceneManager.LoadScene("Main");
            Debug.Log("Disconnected from server.");
        }
    }
}
