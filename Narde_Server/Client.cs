using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Narde_Server
{
    class Client
    {
        public static int dataBufferSize = 4096;//4MB
        public int id;
        public TCP tcp;
        public Player? player; // Add this line
        public Client(int _clientid)
        {
            id = _clientid;
            tcp = new TCP(id);
        }

        public class TCP
        {
            public TcpClient? socket;//stores instance from ConnectCallback in Server.cs
            private readonly int id;//clients id
            private NetworkStream? stream;
            private Packet? receivedData;
            private byte[]? receiveBuffer;
            public TCP(int _id)
            {
                id = _id;
            }
            //On connect
            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                //Setting size to connect and receive buffers
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome! You have connected to the server!");
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
                    Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if(_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }
                    byte[] _data = new byte[_byteLength];

                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch(Exception _ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {_ex}");
                    Server.clients[id].Disconnect();
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
                            Server.packetHandlers[_packetId](id, _packet);
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

            public void Disconnect()
            {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        private void Disconnect()
        {
            if(player != null && player.currentStatus == PlayerStatus.Player) player.currentLobby.RemovePlayer(Server.clients[id]);
            else if(player != null && player.currentStatus == PlayerStatus.Spectator) player.currentLobby.RemoveSpectator(Server.clients[id]);
            player = null;
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            tcp.Disconnect();
            
        }
    }
}