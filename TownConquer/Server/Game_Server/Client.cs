using Game_Server.KI;
using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace Game_Server {
    class Client {
        private static int _dataBufferSize = 4096;

        public Player player;
        public int id;
        public TCP tcp;
        public UDP udp;

        public Client(int clientId) {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP {
            public TcpClient socket;

            private NetworkStream stream;
            private Packet receivedData;
            private byte[] receiveBuffer;
            private readonly int id;

            public TCP(int id) {
                this.id = id;
            }

            public void Connect(TcpClient socket) {
                this.socket = socket;
                this.socket.ReceiveBufferSize = _dataBufferSize;
                this.socket.SendBufferSize = _dataBufferSize;

                stream = this.socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[_dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet) {
                try {
                    if (socket != null) {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception _e) {
                    Console.WriteLine($"Error sending data to Player {id} via TCP: {_e}");
                }
            }

            private void ReceiveCallback(IAsyncResult result) {
                try {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0) {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedData.Reset(HandleData(data));
                    stream.BeginRead(receiveBuffer, 0, _dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e) {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    Server.clients[id].Disconnect();
                }
            }


            //same as in client
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
                            Server.packetHandlers[packetId](id, packet);
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

            private int _id;

            public UDP(int id) {
                _id = id;
            }

            public void Connect(IPEndPoint endPoint) {
                this.endPoint = endPoint;
            }

            public void SendData(Packet packet) {
                Server.SendUDPData(endPoint, packet);
            }

            public void HandleData(Packet packetData) {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() => {
                    using (Packet packet = new Packet(packetBytes)) {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](_id, packet);
                    }
                });
            }

            public void Disconnect() {
                endPoint = null;
            }
        }

        public void SendIntoGame(string playerName, Color color) {

            if (Server.gm.game.kis.Count == 0) Server.gm.CreateKis();

            player = new Player(id, playerName, color, DateTime.Now);
            Town t = Server.gm.CreateTown(player);

            ServerSend.CreateWorld(id, Server.clients[id].player, Constants.RANDOM_SEED, t); // create the world for new player

            foreach (KI_base ki in Server.gm.game.kis) {
                ServerSend.UpdateWorld(id, ki.player, ki.player.towns.Count, ki.player.towns);// send every KI player to the new player
            }

            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    if (client.id != id) {
                        ServerSend.UpdateWorld(id, client.player, client.player.towns.Count, client.player.towns);// send every already connected player to the new player
                        ServerSend.UpdateWorld(client.id, player, player.towns.Count, player.towns);// send the new players info to all connected players
                    }
                }
            }
        }

        public void AttackTown(Vector3 atkTown, Vector3 deffTown, DateTime timeStamp) {
            if (!Server.gm.IsIntersecting(atkTown, deffTown)) {
                Server.gm.AddAttackToTown(atkTown, deffTown, timeStamp);
                foreach (Client client in Server.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedAttack(client.id, atkTown, deffTown);
                    }
                }
            }
        }

        public void RetreatTown(Vector3 atkTown, Vector3 deffTown, DateTime timeStamp) {
            Server.gm.RemoveAttackFromTown(atkTown, deffTown, timeStamp);
            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    ServerSend.GrantedRetreat(client.id, atkTown, deffTown);
                }
            }
        }

        public void ConquerTown(int attackerId, Vector3 deffTown, DateTime timeStamp) {
            Player conquerer = Server.clients[attackerId].player;
            Server.gm.ConquerTown(conquerer, deffTown, timeStamp);
            foreach (Client client in Server.clients.Values) {
                if (client.player != null) {
                    ServerSend.GrantedConquer(client.id, conquerer, deffTown);
                }
            }
        }

        public void Disconnect() {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");
            player = null;
            tcp.Disconnect();
            udp.Disconnect();

            ServerSend.PlayerDisconneced(id);
        }
    }
}
