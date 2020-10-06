﻿using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;

namespace Game_Server {
    class Client {
        public static int dataBufferSize = 4096;
        public Player player;
        public int id;
        public TCP tcp;
        public UDP udp;

        public Client(int _clientId) {
            id = _clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private readonly int id;

            public TCP(int _id) {
                id = _id;
            }

            public void Connect(TcpClient _socket) {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet _packet) {
                try {
                    if(socket != null) {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _e) {
                    Console.WriteLine($"Error sending data to Player {id} via TCP: {_e}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result) {
                try {
                    int _byteLength = stream.EndRead(_result);
                    if(_byteLength <= 0) {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    receivedData.Reset(HandleData(_data));
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _e){
                    Console.WriteLine($"Error receiving TCP data: {_e}");
                    Server.clients[id].Disconnect();
                }
            }


            //same as in client
            private bool HandleData(byte[] _data) {
                int _packetLength = 0;

                receivedData.SetBytes(_data);

                //has receivedData more than 4 unread bytes? If yes its the start of a packet.
                //because the first data, of any packet, sent is an int with the length of the packet
                if (receivedData.UnreadLength() >= 4) {
                    _packetLength = receivedData.ReadInt();

                    if (_packetLength <= 0) {
                        return true;
                    }
                }

                // while this is true data is received
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength()) {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() => {
                        using (Packet _packet = new Packet(_packetBytes)) {
                            int _packetId = _packet.ReadInt();
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;

                    if (receivedData.UnreadLength() >= 4) {
                        _packetLength = receivedData.ReadInt();

                        if (_packetLength <= 0) {
                            return true;
                        }
                    }
                }

                if (_packetLength <= 1) {
                    return true;
                }

                return false;
            }

            public void Disconnect() {
                socket.Close();
                stream = null;
                receivedData = null;
                receiveBuffer = null;
                socket = null;
            }
        }

        public class UDP {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id) {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint) {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet) {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData) {
                int _packetLength = _packetData.ReadInt();
                byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet _packet = new Packet(_packetBytes)) {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        public void SendIntoGame(string _playerName, Color _color) {
            player = new Player(id, _playerName, _color);

            // send every already connected player to the new player
            foreach (Client _client in Server.clients.Values) {
                if (_client.player != null) {
                    if (_client.id != id) {
                        ServerSend.CreateWorld(id, _client.player, Constants.RANDOM_SEED);
                    }
                }
            }
             // send the new players info to all connected players
            foreach (Client _client in Server.clients.Values) {
                if (_client.player != null) {
                    ServerSend.CreateWorld(_client.id, player, Constants.RANDOM_SEED);
                }
            }
        }

        public void Disconnect() {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            player = null;
            tcp.Disconnect();
            udp.Disconnect();
        }
    }
}
