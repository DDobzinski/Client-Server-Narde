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
    }
}