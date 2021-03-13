using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBuffersize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myId = 0;
    public TCP tcp;
    public UDP udp;

    private bool Isconnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer()
    {
        tcp = new TCP();
        udp = new UDP();
        
        InitializeClientData();

        Isconnected = true;
        tcp.Connect();
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint EndPoint;

        public UDP()
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localport)
        {
            socket = new UdpClient(_localport);
            
            socket.Connect(EndPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                    
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data via UDP: {e}");
            }
        }
        
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                byte[] _data = socket.EndReceive(ar, ref EndPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet _packet = new Packet(data))
            {
                int _packetlength = _packet.ReadInt();
                data = _packet.ReadBytes(_packetlength);
            }
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            EndPoint = null;
            socket = null;
        }
        
    }
    
    public class TCP
    {
        public TcpClient socket;

        private NetworkStream NetworkStream;
        private byte[] receiveBuffer;
        private Packet receivedData;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBuffersize,
                SendBufferSize = dataBuffersize
            };

            receiveBuffer = new byte[dataBuffersize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }

            NetworkStream = socket.GetStream();

            receivedData = new Packet();
            
            NetworkStream.BeginRead(receiveBuffer, 0, dataBuffersize, ReceiveCallback, null);
        }
        
        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    NetworkStream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }
        
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int _bytelength = NetworkStream.EndRead(result);
                if (_bytelength <= 0)
                {
                    instance.Disconnect();
                    return;

                }

                byte[] _data = new byte[_bytelength];
                Array.Copy(receiveBuffer, _data, _bytelength);
                
                receivedData.Reset(HandleData(_data));
                NetworkStream.BeginRead(receiveBuffer, 0, dataBuffersize, ReceiveCallback, null);
            }
            catch 
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetlength = 0;
            
            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetlength = receivedData.ReadInt();
                if (_packetlength <= 0)
                {
                    return true;
                }
            }

            while (_packetlength > 0 && _packetlength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetlength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(_packetBytes))
                    {
                        int _packetid = packet.ReadInt();
                        packetHandlers[_packetid](packet);
                    }
                });
                
                _packetlength = 0;

                if (receivedData.UnreadLength() >= 4)
                {
                    _packetlength = receivedData.ReadInt();
                    if (_packetlength <= 0)
                    {
                        return true;
                    }
                }
                
            }

            if (_packetlength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            NetworkStream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
        
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            {(int)ServerPackets.welcome, ClientHandle.Welcome},
            {(int)ServerPackets.spawnplayer, ClientHandle.SpawnPlayer},
            {(int)ServerPackets.playerPosition, ClientHandle.PlayerPosition},
            {(int)ServerPackets.playerRotation, ClientHandle.PlayerRotation},
            {(int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected},
            {(int)ServerPackets.playerHealth, ClientHandle.PlayerHealth},
            {(int)ServerPackets.playerRespawned, ClientHandle.PlayerRespawned},
            {(int)ServerPackets.createItemSpawner, ClientHandle.CreateItemsSpawner},
            {(int)ServerPackets.itemSpawned, ClientHandle.ItemSpawned},
            {(int)ServerPackets.itemPickedUp, ClientHandle.ItemPickedUp},
            {(int)ServerPackets.spawnEnemy, ClientHandle.SpawnEnemy},
            {(int)ServerPackets.enemyPosition, ClientHandle.EnemyPosition},
            {(int)ServerPackets.enemyHealth, ClientHandle.EnemyHealth},
        };
        Debug.Log("Initialized packets.");
    }

    private void Disconnect()
    {
        if (Isconnected)
        {
            Isconnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            
            Debug.Log("Disconnected from server");
        }
    }
    
}
