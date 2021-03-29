using Game_Server.EA;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// BASIERT AUF DEM CODE VON TOM WEILAND SIEHE:
/// Weiland, Tom, 8 Dec 2019, https://github.com/tom-weiland/tcp-udp-networking/blob/tutorial-part5/GameServer/GameServer/Server.cs [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

namespace Game_Server {
    class Server {

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static int maxPlayers { get; private set; }
        public static int maxGames { get; private set; }
        public static int port { get; private set; }

        public static Dictionary<int, Game> games = new Dictionary<int, Game>();
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener _tcpListener;
        private static UdpClient _udpListener;

        public static void Start(int maxPlayers, int port) {
            Server.maxPlayers = maxPlayers;
            Server.port = port;

            InitializeServerData();

            _tcpListener = new TcpListener(IPAddress.Any, Server.port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            _udpListener = new UdpClient(Server.port);
            _udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on {Server.port}.");
        }

        private static void UDPReceiveCallback(IAsyncResult result) {
            try {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);
                _udpListener.BeginReceive(UDPReceiveCallback, null);

                if (data.Length < 4) {
                    return;
                }

                using (Packet packet = new Packet(data)) {
                    int clientId = packet.ReadInt();

                    if (clientId == 0) {
                        return;
                    }

                    // new connection with empty packet to open clients port
                    if (games[-1].clients[clientId].udp.endPoint == null) {
                        games[-1].clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (games[-1].clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString()) {
                        games[-1].clients[clientId].udp.HandleData(packet);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine($"Error receiving UDP data: {e}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet) {
            try {
                if (clientEndPoint != null) {
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception _e) {
                Console.WriteLine($"Error sending data to {clientEndPoint} via UDP: {_e}");
            }
        }

        private static void TCPConnectCallback(IAsyncResult result) {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}.");

            for (int i = 1; i <= maxPlayers; i++) {
                if (games[-1].clients[i] != null && games[-1].clients[i].tcp.socket == null) {
                    games[-1].clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData() {

            games.Add(-1, new Game(-1));

            if (Constants.TRAININGS_MODE == true) {
                //EA_1_Algo ea = new EA_1_Algo();
                //EA_2_Algo ea2 = new EA_2_Algo();
                EA_3_Algo ea3 = new EA_3_Algo();
                Console.Read();
            }

            for (int i = 1; i <= maxPlayers; i++) {
                games[-1].clients.Add(i, new Client(i, games[-1]));
            }

            packetHandlers = new Dictionary<int, PacketHandler>() {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.interactionRequest, ServerHandle.InteractionRequest },
                {(int)ClientPackets.retreatRequest, ServerHandle.RetreatRequest },
                {(int)ClientPackets.conquerRequest, ServerHandle.ConquerRequest },
            };
            Console.WriteLine("Initialized packets and created World.");
        }
    }
}
