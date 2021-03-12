using UnityEngine;

public class ServerHandle 
{
    public static void WelcomeReceived(int _fromclient, Packet _packet)
    {
        int clientid = _packet.ReadInt();
        string username = _packet.ReadString();
            
        Debug.Log($"{Server.Clients[_fromclient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromclient}");
        if (_fromclient != clientid)
        {
            Debug.Log($"Player \"{username}\" (ID: {_fromclient}) has assumed the wrong client ID ({clientid})");
        }
        Server.Clients[_fromclient].SendIntoGame(username);
    }

    public static void PlayerMovement(int _fromclient, Packet _packet)
    {
        bool[] _inputs = new bool[_packet.ReadInt()];
        for (int i = 0; i < _inputs.Length; i++)
        {
            _inputs[i] = _packet.ReadBool();
        }

        Quaternion rotation = _packet.ReadQuanternion();

        Server.Clients[_fromclient].Player.SetInput(_inputs, rotation);
    }

    public static void PlayerAttack(int _fromclient, Packet _packet)
    {
        Vector3 direction = _packet.ReadVector3();
        Server.Clients[_fromclient].Player.Attack(direction);
    }
    
}
