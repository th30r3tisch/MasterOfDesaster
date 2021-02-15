using SharedLibrary.Models;
using SharedLibrary;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Game_Server {
    class ServerSend {

        public static void Welcome(int toClient, string msg) {
            using (Packet packet = new Packet((int)ServerPackets.welcome)) {
                packet.Write(msg);
                packet.Write(toClient);
                SendTCPData(toClient, packet);
            }
        }

        public static void CreateWorld(int toClient, Player player, int seed, Town town) {
            using (Packet packet = new Packet((int)ServerPackets.createWorld)) {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.color);
                packet.Write(player.creationTime.ToBinary());
                packet.Write(seed);
                packet.Write(town.position);

                SendTCPData(toClient, packet);
            }
        }

        public static void UpdateWorld(int toClient, Player player, int townNumber, List<Town> towns) {
            using (Packet packet = new Packet((int)ServerPackets.updateWorld)) {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.color);
                packet.Write(player.creationTime.ToBinary());
                packet.Write(townNumber);
                foreach (Town _town in towns) {
                    packet.Write(_town.position);
                }
                SendTCPData(toClient, packet);
            }
        }

        public static void GrantedAction(int toClient, Vector3 atkTown, Vector3 deffTown) {
            using (Packet packet = new Packet((int)ServerPackets.grantedInteraction)) {
                packet.Write(atkTown);
                packet.Write(deffTown);
                SendTCPData(toClient, packet);
            }
        }

        public static void GrantedRetreat(int toClient, Vector3 atkTown, Vector3 deffTown) {
            using (Packet packet = new Packet((int)ServerPackets.grantedRetreat)) {
                packet.Write(atkTown);
                packet.Write(deffTown);
                SendTCPData(toClient, packet);
            }
        }

        public static void GrantedConquer(int toClient, Player player, Vector3 deffTown) {
            using (Packet packet = new Packet((int)ServerPackets.grantedConquer)) {
                packet.Write(player.id);
                packet.Write(deffTown);
                SendTCPData(toClient, packet);
                Console.WriteLine($"Conquer of {deffTown} is GRANTED.");
            }
        }

        public static void PlayerDisconneced(int playerId) {
            using (Packet packet = new Packet((int)ServerPackets.playerDisconnected)) {
                packet.Write(playerId);
                SendTCPDataToAll(packet);
                Console.WriteLine($"Player with ID: {playerId} disconnected.");
            }
        }

        #region TCP

        private static void SendTCPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.games[-1].clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet) {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                Server.games[-1].clients[i].tcp.SendData(packet);
            }
        }
        private static void SendTCPDataToAll(int exceptClient, Packet packet) {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                if (i != exceptClient) {
                    Server.games[-1].clients[i].tcp.SendData(packet);
                }
            }
        }

        #endregion
        #region UDP

        private static void SendUDPData(int toClient, Packet packet) {
            packet.WriteLength();
            Server.games[-1].clients[toClient].udp.SendData(packet);
        }

        private static void SendUDPDataToAll(Packet packet) {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                Server.games[-1].clients[i].udp.SendData(packet);
            }
        }
        private static void SendUDPDataToAll(int exceptClient, Packet packet) {
            packet.WriteLength();
            for (int i = 1; i <= Server.maxPlayers; i++) {
                if (i != exceptClient) {
                    Server.games[-1].clients[i].udp.SendData(packet);
                }
            }
        }

        #endregion
    }
}
