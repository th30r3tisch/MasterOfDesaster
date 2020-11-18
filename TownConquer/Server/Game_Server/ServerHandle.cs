using SharedLibrary;
using System;
using System.Drawing;
using System.Numerics;

namespace Game_Server {
    class ServerHandle {
        public static void WelcomeReceived(int _fromClient, Packet _packet) {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            Color _color = _packet.ReadColor();

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient} - {_username}.");
            if (_fromClient != _clientIdCheck) {
                Console.WriteLine($"Player \" {_username}\" (ID:{_fromClient}) has assumed the wrong client ID ({_clientIdCheck})!");
            }

            Server.clients[_fromClient].SendIntoGame(_username, _color);
        }

        public static void AttackRequest(int _fromClient, Packet _packet) {
            int _clientIdCheck = _packet.ReadInt();
            Vector3 _atkTown = _packet.ReadVector3();
            Vector3 _deffTown = _packet.ReadVector3();
            DateTime _timeStamp = DateTime.FromBinary(_packet.ReadLong());

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} requested an attack at town {_deffTown}.");

            Server.clients[_fromClient].AttackTown(_atkTown, _deffTown, _timeStamp);
        }

        public static void RetreatRequest(int _fromClient, Packet _packet) {
            int _clientIdCheck = _packet.ReadInt();
            Vector3 _atkTown = _packet.ReadVector3();
            Vector3 _deffTown = _packet.ReadVector3();
            DateTime _timeStamp = DateTime.FromBinary(_packet.ReadLong());

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} requested an retreat of troops from town {_deffTown}.");

            Server.clients[_fromClient].RetreatTown(_atkTown, _deffTown, _timeStamp);
        }

        public static void ConquerRequest(int _fromClient, Packet _packet) {
            int _clientId = _packet.ReadInt();
            Vector3 _deffTown = _packet.ReadVector3();
            DateTime _timeStamp = DateTime.FromBinary(_packet.ReadLong());

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} requested to conquer {_deffTown}.");

            Server.clients[_fromClient].ConquerTown(_clientId, _deffTown, _timeStamp);
        }
    }
}
