using SharedLibrary;
using System;
using System.Drawing;

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
    }
}
