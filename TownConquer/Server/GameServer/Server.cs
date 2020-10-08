﻿using GameServer.Models;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace GameServer {
    class Server {

        public static World world { get; private set; }
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port) {
            MaxPlayers = _maxPlayers;
            Port = _port;

            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {Port}.");
        }

        private static void UDPReceiveCallback(IAsyncResult _result) {
            try {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if(_data.Length < 4) {
                    return;
                }

                using (Packet _packet = new Packet(_data)) {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0) {
                        return;
                    }

                    // new connection with empty packet to open clients port
                    if (clients[_clientId].udp.endPoint == null) {
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString()) {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _e) {
                Console.WriteLine($"Error receiving UDP data: {_e}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet) {
            try {
                if (_clientEndPoint != null) {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _e) {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_e}");
            }
        }

        private static void TCPConnectCallback(IAsyncResult _result) {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}.");

            for (int i = 1; i <= MaxPlayers; i++) {
                if(clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData() {

            world = GameLogic.GenereateInitialMap();
            for (int i = 1; i <= MaxPlayers; i++) {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>() {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
            };
            Console.WriteLine("Initialized packets.");
        }
    }
}