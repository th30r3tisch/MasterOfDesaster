using SharedLibrary.Models;
using SharedLibrary;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Game_Server {
    class ServerSend {

        public static void Welcome(int _toClient, string _msg) {
            using (Packet _packet = new Packet((int)ServerPackets.welcome)) {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void CreateWorld(int _toClient, Player _player, int _seed, Town _town) {
            using (Packet _packet = new Packet((int)ServerPackets.createWorld)) {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.color);
                _packet.Write(_seed);
                _packet.Write(_town.position);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void UpdateWorld(int _toClient, Player _player, int townNumber, List<Town> _towns) {
            using (Packet _packet = new Packet((int)ServerPackets.updateWorld)) {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.color);
                _packet.Write(townNumber);
                foreach (Town _town in _towns) {
                    _packet.Write(_town.position);
                }
                SendTCPData(_toClient, _packet);
            }
        }

        public static void GrantedAttack(int _toClient, Vector3 _atkTown, Vector3 _deffTown) {
            using (Packet _packet = new Packet((int)ServerPackets.grantedAttack)) {
                _packet.Write(_atkTown);
                _packet.Write(_deffTown);
                SendTCPData(_toClient, _packet);
                Console.WriteLine($"Attack from Town {_atkTown} to {_deffTown} is GRANTED.");
            }
        }

        public static void GrantedRetreat(int _toClient, Vector3 _atkTown, Vector3 _deffTown) {
            using (Packet _packet = new Packet((int)ServerPackets.grantedRetreat)) {
                _packet.Write(_atkTown);
                _packet.Write(_deffTown);
                SendTCPData(_toClient, _packet);
                Console.WriteLine($"Retreat from Town {_atkTown} to {_deffTown} is GRANTED.");
            }
        }

        public static void GrantedConquer(int _toClient, Player _player, Vector3 _deffTown) {
            using (Packet _packet = new Packet((int)ServerPackets.grantedConquer)) {
                _packet.Write(_player.id);
                _packet.Write(_deffTown);
                SendTCPData(_toClient, _packet);
                Console.WriteLine($"Conquer of {_deffTown} is GRANTED.");
            }
        }

        public static void PlayerDisconneced(int _playerId) {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected)) {
                _packet.Write(_playerId);
                SendTCPDataToAll(_packet);
                Console.WriteLine($"Player with ID: {_playerId} disconnected.");
            }
        }

        #region TCP

        private static void SendTCPData(int _toClient, Packet _packet) {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++) {
                Server.clients[i].tcp.SendData(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++) {
                if (i != _exceptClient) {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        #endregion
        #region UDP

        private static void SendUDPData(int _toClient, Packet _packet) {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        private static void SendUDPDataToAll(Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++) {
                Server.clients[i].udp.SendData(_packet);
            }
        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet) {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++) {
                if (i != _exceptClient) {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #endregion
    }
}
