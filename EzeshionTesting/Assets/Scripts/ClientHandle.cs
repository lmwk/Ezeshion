using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string msg = _packet.ReadString();
        int _myid = _packet.ReadInt();
        
        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = _myid;
        ClientSend.WelcomeReceived();
        
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuanternion();
        
        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int id = _packet.ReadInt();
        Vector3 position = _packet.ReadVector3();

        if (GameManager.players.TryGetValue(id, out PlayerManager _player))
        {
            _player.transform.position = position;   
        }
    }
    public static void PlayerRotation(Packet _packet)
    {
        int id = _packet.ReadInt();
        Quaternion rotation = _packet.ReadQuanternion();

        if (GameManager.players.TryGetValue(id, out PlayerManager _player))
        {
            _player.transform.rotation = rotation;
        }
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int id = _packet.ReadInt();
        
        Destroy(GameManager.players[id].gameObject);
        GameManager.players.Remove(id);
    }

    public static void PlayerHealth(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float health = _packet.ReadFloat();
        
        GameManager.players[_id].SetHealth(health);
    }

    public static void PlayerRespawned(Packet _packet)
    {
        int _id = _packet.ReadInt();
        
        GameManager.players[_id].Respawn();
    }

    public static void CreateItemsSpawner(Packet _packet)
    {
        int _spawnerid = _packet.ReadInt();
        Vector3 _spawnerPosition = _packet.ReadVector3();
        bool _hasItem = _packet.ReadBool();
        
        GameManager.instance.CreateItemSpawner(_spawnerid, _spawnerPosition, _hasItem);
    }

    public static void ItemSpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        
        GameManager.ItemSpawners[_spawnerId].ItemSpawned();
    }
    public static void ItemPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _byPlayer = _packet.ReadInt();
        
        GameManager.ItemSpawners[_spawnerId].ItemPickedUp();
        GameManager.players[_byPlayer].itemCount++;
    }

    public static void SpawnEnemy(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        
        GameManager.instance.SpawnEnemy(_enemyId, _position);
    }
    public static void EnemyPosition(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.enemies.TryGetValue(_enemyId, out EnemyManager _enemy))
        {
            _enemy.transform.position = _position;
        }

    }

    public static void EnemyHealth(Packet _packet)
    {
        int _enemyId = _packet.ReadInt();
        float _health = _packet.ReadFloat();

        GameManager.enemies[_enemyId].SetHealth(_health);
    }
}
