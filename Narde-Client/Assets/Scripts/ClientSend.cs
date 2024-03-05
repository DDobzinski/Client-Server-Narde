using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    //Prepares packets to be sent 
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    #region Packets
    //Creates packet that is sent to server once Welcome message is received
    public static void WelcomeReceived()
    {
        using(Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);

            SendTCPData(_packet);
        }
    }

    public static void CreateLobbyRequestToServer()
    {
        using(Packet _packet = new Packet((int)ClientPackets.createLobby))
        {
            _packet.Write(UIManager.instance.createLobbyScript.lobbyNameInput.text);
            _packet.Write(Mathf.RoundToInt(UIManager.instance.createLobbyScript.SpectatorSlider.value));
            
            _packet.Write(UIManager.instance.createLobbyScript.TypeDropDown.options[UIManager.instance.createLobbyScript.TypeDropDown.value].text);

            SendTCPData(_packet);
        }
    }

    public static void LeaveLobby()
    {
        using(Packet _packet = new Packet((int)ClientPackets.leaveLobby))
        {
            _packet.Write(Client.instance.myId);
            SendTCPData(_packet);
        }
    }

    public static void RequestLobbies()
    {
        using(Packet _packet = new Packet((int)ClientPackets.getLobbies))
        {
            _packet.Write(Client.instance.myId);
            SendTCPData(_packet);
        }
    }

    public static void JoinLobby(int lobbyid, bool spectator)
    {
        using(Packet _packet = new Packet((int)ClientPackets.joinLobby))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(lobbyid);
            _packet.Write(spectator);
            SendTCPData(_packet);
        }
    }

    public static void StartGame()
    {
        using Packet _packet = new((int)ClientPackets.startGame);
        _packet.Write(Client.instance.myId);

        SendTCPData(_packet);
    }

    public static void EndTurn(int dice1, int dice2)
    {
        using Packet _packet = new((int)ClientPackets.AIEndTurn);
        Debug.Log("SendEndTurn");
        _packet.Write(Client.instance.myId);
        _packet.Write(GameManager.Instance.MovesDone.Count);
        foreach(var move in GameManager.Instance.MovesDone)
        {
            _packet.Write(move.StartingPoint.id);
            if(move.TargetPoint != null)
            {
                _packet.Write(move.TargetPoint.id);
            }
            else
            {
                _packet.Write(24);
            }
            _packet.Write(move.DiceUsed.Count);
            for(int i = 0; i < move.DiceUsed.Count; i++)
                    {
                        if(move.DiceUsed[i] == 1)
                        {
                            _packet.Write(dice1);
                        } 
                        else 
                        {
                            _packet.Write(dice2);
                        }
                    }
        }

        SendTCPData(_packet);
    }

    public static void EndTurnAI(int dice1, int dice2)
    {
        using Packet _packet = new((int)ClientPackets.endTurn);
        Debug.Log("Here");
        _packet.Write(Client.instance.myId);
        _packet.Write(GameManager.Instance.agent.finalMoves.Count);
        foreach(var move in GameManager.Instance.agent.finalMoves)
        {
            _packet.Write(move.StartingPoint.id);
            if(move.TargetPoint != null)
            {
                _packet.Write(move.TargetPoint.id);
            }
            else
            {
                _packet.Write(24);
            }
            _packet.Write(move.DiceUsed.Count);
            for(int i = 0; i < move.DiceUsed.Count; i++)
                    {
                        if(move.DiceUsed[i] == 1)
                        {
                            _packet.Write(dice1);
                        } 
                        else 
                        {
                            _packet.Write(dice2);
                        }
                    }
        }

        SendTCPData(_packet);
    }
    public static void Surrender()
    {
        using Packet _packet = new((int)ClientPackets.surrender);
        _packet.Write(Client.instance.myId);
        SendTCPData(_packet);
    }
    #endregion
}
