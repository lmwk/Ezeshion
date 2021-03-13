using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
        public static int Maxplayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);

        public static Dictionary<int, PacketHandler> PacketHandlers;
        
        private static TcpListener TcpListener;
        private static UdpClient UdpListener;

        public static void Start(int _maxplayers, int _port)
        {
            Maxplayers = _maxplayers;
            Port = _port;
            
            Debug.Log("Starting server...");
            InitializeServerData();
            
            TcpListener = new TcpListener(IPAddress.Any, Port);
            TcpListener.Start();
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectionCallback), null);

            UdpListener = new UdpClient(Port);
            UdpListener.BeginReceive(UDPReceiveCallback, null);
            
            Debug.Log($"Server started on port {Port}");

        }

        private static void UDPReceiveCallback(IAsyncResult ar)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = UdpListener.EndReceive(ar, ref _clientEndPoint);
                UdpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet packet = new Packet(_data))
                {
                    int _clientid = packet.ReadInt();

                    if (_clientid == 0)
                    {
                        return;
                    }

                    if (Clients[_clientid].udp.EndPoint == null)
                    {
                        Clients[_clientid].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (Clients[_clientid].udp.EndPoint.ToString() == _clientEndPoint.ToString())
                    {
                        Clients[_clientid].udp.HandleData(packet);
                    }
                }
                
            }
            catch (Exception e)
            {
                Debug.Log($"Error receiving UDP data: {e}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndpoint, Packet _packet)
        {
            try
            {
                if (_clientEndpoint != null)
                {
                    UdpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndpoint, null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error sending data to {_clientEndpoint} via UDP: {e}");
            }
        }

        private static void TCPConnectionCallback(IAsyncResult ar)
        {
            TcpClient _client = TcpListener.EndAcceptTcpClient(ar);
            TcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectionCallback), null);
            Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= Maxplayers; i++)
            {
                if (Clients[i].tcp.socket == null)
                {
                    Clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: server full");
            
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= Maxplayers; i++)
            {
                Clients.Add(i, new Client(i));
            }

            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.playerMovement, ServerHandle.PlayerMovement},
                { (int)ClientPackets.playerAttack, ServerHandle.PlayerAttack}
            };
            Debug.Log("Initialized Packets.");
        }

        public static void Stop()
        {
            TcpListener.Stop();
            UdpListener.Close();
        }
}
