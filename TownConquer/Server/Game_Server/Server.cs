using Game_Server.EA;
using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Game_Server {
    class Server {

        public delegate void PacketHandler(int fromClient, Packet packet);

        public static GameManager gm { get; private set; }
        public static int maxPlayers { get; private set; }
        public static int port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<int, User> kis = new Dictionary<int, User>();
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
                    if (clients[clientId].udp.endPoint == null) {
                        clients[clientId].udp.Connect(clientEndPoint);
                        return;
                    }

                    if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString()) {
                        clients[clientId].udp.HandleData(packet);
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
                if (clients[i] != null && clients[i].tcp.socket == null) {
                    clients[i].tcp.Connect(client);
                    return;
                }
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void InitializeServerData() {

            gm = new GameManager();

            if (Constants.TRAININGS_MODE == true) {
                //new KnapSack_EA();
                EA_1_Algo ea = new EA_1_Algo();
                //EA_2_Algo ea2 = new EA_2_Algo();
            }

            for (int i = 1; i <= maxPlayers; i++) {
                clients.Add(i, new Client(i));
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
