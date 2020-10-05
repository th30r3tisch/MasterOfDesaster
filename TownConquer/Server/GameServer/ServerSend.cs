using GameServer.Models;
using SharedLibrary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GameServer {
    class ServerSend {

        public static void Welcome(int _toClient, string _msg) {
            using (Packet _packet = new Packet((int)ServerPackets.welcome)) {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void CreateWorld(int _toClient, Player _player) {
            using (Packet _packet = new Packet((int)ServerPackets.createWorld)) {
                List<TreeNode> _t = Server.world.GetQuadtree().GetAllContent(Server.world.GetQuadtree(), 0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT);
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.color);

                MemoryStream memorystream = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memorystream, _t);
                byte[] yourBytesToDb = memorystream.ToArray();
                _packet.Write(yourBytesToDb);

                SendTCPData(_toClient, _packet);
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
