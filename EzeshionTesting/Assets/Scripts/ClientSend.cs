using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }
    
    #region Packets

    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.instance.usernamenfield.text);
            SendTCPData(packet);
        }
    }
    #endregion

    public static void PlayerMovement(bool[] inputs)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);
            foreach (bool input in inputs)
            {
                packet.Write(input);
            }
            packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            
            SendUDPData(packet);
        }
    }

    public static void PlayerAttack(Vector3 facing)
    {
        using (Packet packet = new Packet((int)ClientPackets.playerAttack))
        {
            packet.Write(facing);
            Debug.Log("Hit attack send");
            SendTCPData(packet);
        }
    }
}
