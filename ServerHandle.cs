using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        int _clientIdCheck = _packet.ReadInt();
        string _username = _packet.ReadString();

        Debug.Log($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
        if (_fromClient != _clientIdCheck)
        {
            Debug.Log($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
        }
        Server.clients[_fromClient].SendIntoGame(_username);
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }
        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
    }

    public static void PlayerStartGrappling(int _fromClient, Packet _packet)
    {
        Vector3 _shootDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.StartGrappling(_shootDirection);
    }

    public static void PlayerStopGrappling(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.StopGrappling();
    }

    public static void PlayerShoot(int _fromClient, Packet _packet)
    {
        Vector3 _throwDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.Shoot(_throwDirection);
    }

    public static void PlayerStopShooting(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.StopShooting();
    }

    public static void PlayerStartSword(int _fromClient, Packet _packet)
    {
        Vector3 _attackDirection = _packet.ReadVector3();

        Server.clients[_fromClient].player.SwordAttack();
    }

    public static void PlayerStopSword(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.StopSwordAttack(); //Hacer que el bool sea 0
    }

    public static void PlayerStartTPose(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.StartTPose();
    }

    public static void PlayerStopTPose(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.StopTPose();
    }

    public static void PlayerSwordActivateCollider(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.ActivateSwordCollider();
    }

    public static void PlayerSwordDeactivateCollider(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.DeactivateSwordCollider();
    }

    public static void PlayerConchetumare(int _fromClient, Packet _packet)
    {
        Server.clients[_fromClient].player.Conchetumare();
    }
}