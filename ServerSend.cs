﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    /// <summary>Sends a packet to a client via TCP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].tcp.SendData(_packet);
    }

    /// <summary>Sends a packet to a client via UDP.</summary>
    /// <param name="_toClient">The client to send the packet the packet to.</param>
    /// <param name="_packet">The packet to send to the client.</param>
    private static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.clients[_toClient].udp.SendData(_packet);
    }

    /// <summary>Sends a packet to all clients via TCP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via TCP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
    }

    /// <summary>Sends a packet to all clients via UDP.</summary>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(_packet);
        }
    }
    /// <summary>Sends a packet to all clients except one via UDP.</summary>
    /// <param name="_exceptClient">The client to NOT send the data to.</param>
    /// <param name="_packet">The packet to send.</param>
    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }
    }

    #region Packets
    /// <summary>Sends a welcome message to the given client.</summary>
    /// <param name="_toClient">The client to send the packet to.</param>
    /// <param name="_msg">The message to send.</param>
    public static void Welcome(int _toClient, string _msg)
    {
        using (Packet _packet = new Packet((int)ServerPackets.welcome))
        {
            _packet.Write(_msg);
            _packet.Write(_toClient);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Tells a client to spawn a player.</summary>
    /// <param name="_toClient">The client that should spawn the player.</param>
    /// <param name="_player">The player to spawn.</param>
    public static void SpawnPlayer(int _toClient, Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);

            SendTCPData(_toClient, _packet);
        }
    }

    /// <summary>Sends a player's updated position to all clients.</summary>
    /// <param name="_player">The player whose position to update.</param>
    public static void PlayerPosition(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.position);

            SendUDPDataToAll(_packet);
        }
    }

    /// <summary>Sends a player's updated rotation to all clients except to himself (to avoid overwriting the local player's rotation).</summary>
    /// <param name="_player">The player whose rotation to update.</param>
    public static void PlayerRotation(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRotation))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.transform.rotation);

            SendUDPDataToAll(_player.id, _packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
        {
            _packet.Write(_playerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerHealth(Player _player, string _playerDoingDamage)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerHealth))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);
            _packet.Write(_playerDoingDamage);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerRespawned))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }

    public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _spawnerPosition, bool _hasItem)
    {
        using (Packet _packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_spawnerPosition);
            _packet.Write(_hasItem);

            SendTCPData(_toClient, _packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.itemSpawned))
        {
            _packet.Write(_spawnerId);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    {
        using(Packet _packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            _packet.Write(_spawnerId);
            _packet.Write(_byPlayer);

            SendTCPDataToAll(_packet);
        }
    }

    public static void SpawnProjectile(Projectile _projectile, int _thrownByPlayer)
    {
        using (Packet _packet = new Packet((int)ServerPackets.spawnProjectile))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);
            _packet.Write(_thrownByPlayer);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ProjectilePosition(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectilePosition))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            SendTCPDataToAll(_packet);
        }
    }

    public static void ProjectileExploded(Projectile _projectile)
    {
        using (Packet _packet = new Packet((int)ServerPackets.projectileExploded))
        {
            _packet.Write(_projectile.id);
            _packet.Write(_projectile.transform.position);

            SendTCPDataToAll(_packet);
        }
    }

    public static void GrapplePoint(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.grapplePoint))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.currentGrapplePosition);

            SendTCPDataToAll(_packet);
        }
    }

    public static void Joint(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.joint))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.lr.positionCount);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerCrouching(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerCrouching))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.crouching);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerSpeed(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerSpeed))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.rb.velocity);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerGrounded(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerGrounded))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.grounded);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerShooting(Player _player)
    {
        using(Packet _packet = new Packet((int)ServerPackets.playerShooting))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.shootKeyPressed);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerSwordAttack(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerSwordAttack))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.swordAttack);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerTPose(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerTPose))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.tPose);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerDamaged(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerDamaged))
        {
            _packet.Write(_player.id);
            _packet.Write(_player.health);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RoundCurrentTime(TimeManager timeManager)
    {
        using (Packet _packet = new Packet((int)ServerPackets.roundCurrentTime))
        {
            _packet.Write(timeManager.currentTime);

            SendTCPDataToAll(_packet);
        }
    }

    public static void RoundFinished(TimeManager timeManager)
    {
        using (Packet _packet = new Packet((int)ServerPackets.roundFinished))
        {
            _packet.Write(timeManager.winnerUserName);

            SendTCPDataToAll(_packet);
        }
    }

    public static void PlayerConchetumareReceived(Player _player)
    {
        using (Packet _packet = new Packet((int)ServerPackets.playerConchetumareReceived))
        {
            _packet.Write(_player.id);

            SendTCPDataToAll(_packet);
        }
    }
    #endregion
}