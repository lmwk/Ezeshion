using UnityEngine;

public class ServerSend
{
    public static void Welcome(int _toclient, string msg)
    {
        using (Packet _packet = new Packet((int) ServerPackets.welcome))
        {
            _packet.Write(msg);
            _packet.Write(_toclient);
            SendTCPData(_toclient, _packet);
        }
    }

    public static void SendUDPData(int _toclient, Packet _packet)
    {
        _packet.WriteLength();
        Server.Clients[_toclient].udp.SendData(_packet);
    }

    private static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.Maxplayers; i++)
        {
            Server.Clients[i].udp.SendData(_packet);
        }
    }

    private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.Maxplayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.Clients[i].udp.SendData(_packet);
            }
        }
    }

    private static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.Maxplayers; i++)
        {
            Server.Clients[i].tcp.SendData(_packet);
        }
    }

    private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.Maxplayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.Clients[i].tcp.SendData(_packet);
            }
        }
    }

    private static void SendTCPData(int toclient, Packet packet)
    {
        packet.WriteLength();
        Server.Clients[toclient].tcp.SendData(packet);
    }

    public static void SpawnPlayer(int id, Player clientPlayer)
    {
        using (Packet packet = new Packet((int) ServerPackets.spawnplayer))
        {
            packet.Write(clientPlayer.id);
            packet.Write(clientPlayer.username);
            packet.Write(clientPlayer.transform.position);
            packet.Write(clientPlayer.transform.rotation);

            SendTCPData(id, packet);
        }
    }

    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }
    }

    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            SendUDPDataToAll(player.id, packet);
        }
    }

    public static void PlayerDisconnected(int _playerId)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerDisconnected))
        {
            packet.Write(_playerId);

            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerHealth(Player _player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerHealth))
        {
            packet.Write(_player.id);
            packet.Write(_player.health);
            SendTCPDataToAll(packet);
        }
    }

    public static void PlayerRespawned(Player _player)
    {
        using (Packet packet = new Packet((int) ServerPackets.playerRespawned))
        {
            packet.Write(_player.id);

            SendTCPDataToAll(packet);
        }
    }

    public static void CreateItemSpawner(int _toClient, int _spawnerId, Vector3 _spawnerposition, bool _hasItem)
    {
        using (Packet packet = new Packet((int)ServerPackets.createItemSpawner))
        {
            packet.Write(_spawnerId);
            packet.Write(_spawnerposition);
            packet.Write(_hasItem);
            
            SendTCPData(_toClient, packet);
        }
    }

    public static void ItemSpawned(int _spawnerId)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemSpawned))
        {
            packet.Write(_spawnerId);
            
            SendTCPDataToAll(packet);
        }
    }    
    public static void ItemPickedUp(int _spawnerId, int _byPlayer)
    {
        using (Packet packet = new Packet((int)ServerPackets.itemPickedUp))
        {
            packet.Write(_spawnerId);
            packet.Write(_byPlayer);
            
            SendTCPDataToAll(packet);
        }
    }

    public static void SpawnEnemy(EnemyMob _enemyMob)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPDataToAll(SpawnEnemy_Data(_enemyMob, packet));
        }
    }
    public static void SpawnEnemy(int _toClient,EnemyMob _enemyMob)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnEnemy))
        {
            SendTCPData(_toClient, SpawnEnemy_Data(_enemyMob, packet));
        }
    }

    private static Packet SpawnEnemy_Data(EnemyMob _enemyMob, Packet _packet)
    {
        _packet.Write(_enemyMob.id);
        _packet.Write(_enemyMob.transform.position);

        return _packet;
    }

    public static void EnemyPosition(EnemyMob _enemyMob)
    {
        using (Packet packet = new Packet((int)ServerPackets.enemyPosition))
        {
            packet.Write(_enemyMob.id);
            packet.Write(_enemyMob.transform.position);
            
            SendUDPDataToAll(packet);
        }
    }

    public static void EnemyHealth(EnemyMob _enemyMob)
    {
        using (Packet packet = new Packet((int)ServerPackets.enemyHealth))
        {
            packet.Write(_enemyMob.id);
            packet.Write(_enemyMob.health);
            
            SendTCPDataToAll(packet);
        }
    }
}