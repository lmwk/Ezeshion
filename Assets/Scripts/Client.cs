using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
        public static int databuffersize = 4096;
        
        public int id;
        public Player Player;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientid)
        {
            id = _clientid;
            tcp = new TCP(id);
            udp = new UDP(id);
        }
        
        public class TCP
        {
            public TcpClient socket;
            
            private readonly int id;
            private NetworkStream NetworkStream;
            private Packet recievedData;
            private byte[] recieveBuffer;

            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient tcpClient)
            {
                socket = tcpClient;
                socket.ReceiveBufferSize = databuffersize;
                socket.SendBufferSize = databuffersize;

                NetworkStream = socket.GetStream();

                recievedData = new Packet();
                recieveBuffer = new byte[databuffersize];
                NetworkStream.BeginRead(recieveBuffer, 0, databuffersize, ReceiveCallback, null);
                ServerSend.Welcome(id, "Welcome to the server!");
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
                catch (Exception e)
                {
                    Debug.Log($"Error sending data to player {id} via TCP: {e}");
                }
            }
            
            public void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int _bytelength = NetworkStream.EndRead(result);
                    if (_bytelength <= 0)
                    {
                        Server.Clients[id].Disconnect();
                        return;

                    }

                    byte[] _data = new byte[_bytelength];
                    Array.Copy(recieveBuffer, _data, _bytelength);
                    
                    recievedData.Reset(HandleData(_data));
                    NetworkStream.BeginRead(recieveBuffer, 0, databuffersize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error receiving TCP data: {e}");
                    Server.Clients[id].Disconnect();
                }
            }

            public void Disconnect()
            {
                socket.Close();
                NetworkStream = null;
                recievedData = null;
                recieveBuffer = null;
                socket = null;
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                recievedData.SetBytes(_data);

                if (recievedData.UnreadLength() >= 4)
                {
                    _packetLength = recievedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= recievedData.UnreadLength())
                {
                    byte[] _packetBytes = recievedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.PacketHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (recievedData.UnreadLength() >= 4)
                    {
                        _packetLength = recievedData.ReadInt();
                        if (_packetLength <= 0)
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


        }

        public class UDP
        {
            public IPEndPoint EndPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                EndPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(EndPoint, _packet);
            }

            public void HandleData(Packet _packet)
            {
                int _packetlength = _packet.ReadInt();
                byte[] _packetBytes = _packet.ReadBytes(_packetlength);
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.PacketHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect()
            {
                EndPoint = null;
            }
        }

        public void SendIntoGame(string _playername)
        {
            Player = NetworkManager.instance.InstantiatePlayer();
            Player.Initialize(id, _playername);
            
            foreach (Client _client in Server.Clients.Values)
            {
                if (_client.Player != null)
                {
                    if (_client.id != id)
                    {
                        ServerSend.SpawnPlayer(id, _client.Player);
                    }
                }
            }

            foreach (Client _client in Server.Clients.Values)
            {
                if (_client.Player != null)
                {
                    ServerSend.SpawnPlayer(_client.id, Player);
                }
            }

            foreach (ItemSpawner _itemSpawner in ItemSpawner.Spawners.Values)
            {
                ServerSend.CreateItemSpawner(id, _itemSpawner.spawnerId, _itemSpawner.transform.position, _itemSpawner.hasItem);
            }

            foreach (EnemyMob _enemyMob in EnemyMob.enemies.Values)
            {
                ServerSend.SpawnEnemy(id, _enemyMob);
            }
        }

        private void Disconnect()
        {
            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            
            ThreadManager.ExecuteOnMainThread(() =>
            {
                UnityEngine.Object.Destroy(Player.gameObject);
                Player = null;
            });

            tcp.Disconnect();
            udp.Disconnect();
            
            ServerSend.PlayerDisconnected(id);
        }
}
