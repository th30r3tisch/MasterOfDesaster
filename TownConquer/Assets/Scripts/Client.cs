using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

/// <summary>
/// BASIERT AUF DEM CODE VON TOM WEILAND SIEHE:
/// Weiland, Tom, 22 Dec 2019, https://github.com/tom-weiland/tcp-udp-networking/blob/682f3fe3dd0b9a7d74e13896eabbfa48d3c95e20/GameClient/Assets/Scripts/Client.cs [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = Constants.SERVER_PORT;

    public int myId = 0;
    public Player me;
    public List<Town> towns;
    public List<Player> enemies;

    public TCP tcp;
    public UDP udp;

    private bool _isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start() {
        tcp = new TCP();
        udp = new UDP();
        ConnectToServer();
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    private void Disconnect() {
        if (_isConnected) {
            _isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();

            Debug.Log("Disconnected from server.");
        }
    }

    public void ConnectToServer() {
        InitializeClientData();
        _isConnected = true;
        tcp.Connect();
    }

    public class TCP {
        public TcpClient socket;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect() {
            socket = new TcpClient {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void Disconnect() {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }

        private void ConnectCallback(IAsyncResult result) {
            socket.EndConnect(result);

            if (!socket.Connected) {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet packet) {
            try {
                if (socket != null) {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e) {
                Debug.Log($"Error sending data to Server via TCP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) {
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch {
                Disconnect();
            }
        }

        private bool HandleData(byte[] data) {
            int packetLength = 0;

            receivedData.SetBytes(data);

            //has receivedData more than 4 unread bytes? If yes its the start of a packet.
            //because the first data, of any packet, sent is an int with the length of the packet
            if (receivedData.UnreadLength() >= 4) {
                packetLength = receivedData.ReadInt();

                if (packetLength <= 0) {
                    return true;
                }
            }

            // while this is true data is received
            while (packetLength > 0 && packetLength <= receivedData.UnreadLength()) {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        _packetHandlers[packetId](packet);
                    }
                });

                packetLength = 0;

                if (receivedData.UnreadLength() >= 4) {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0) {
                        return true;
                    }
                }
            }

            if (packetLength <= 1) {
                return true;
            }

            return false;
        }
    }

    public class UDP {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort) {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet()) {
                SendData(_packet);
            }
        }

        private void Disconnect() {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }

        public void SendData(Packet packet) {
            try {
                packet.InsertInt(instance.myId);
                if (socket != null) {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception e) {
                Debug.Log($"Error sending data to server via UDP: {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult result) {
            try {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4) {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch {
                Disconnect();
            }

        }

        private void HandleData(byte[] data) {
            using (Packet packet = new Packet(data)) {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() => {
                using (Packet packet = new Packet(data)) {
                    int packetId = packet.ReadInt();
                    _packetHandlers[packetId](packet);
                }
            });
        }
    }

    private void InitializeClientData() {
        _packetHandlers = new Dictionary<int, PacketHandler>() {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },
            { (int)ServerPackets.createWorld, ClientHandle.CreateWorld },
            { (int)ServerPackets.updateWorld, ClientHandle.UpdateWorld },
            { (int)ServerPackets.grantedInteraction, ClientHandle.GrantedInteraction },
            { (int)ServerPackets.grantedRetreat, ClientHandle.GrantedRetreat },
            { (int)ServerPackets.grantedConquer, ClientHandle.GrantedConquer },
            { (int)ServerPackets.playerDisconnected, ClientHandle.PlayerDisconnected },
        };

        Debug.Log("Initialized packets.");
    }
}
