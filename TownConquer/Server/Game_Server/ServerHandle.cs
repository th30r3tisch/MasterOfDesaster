using SharedLibrary;
using System;
using System.Drawing;
using System.Numerics;

namespace Game_Server {
    class ServerHandle {
        public static void WelcomeReceived(int fromClient, Packet packet) {
            int clientIdCheck = packet.ReadInt();
            string username = packet.ReadString();
            Color color = packet.ReadColor();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient} - {username}.");
            if (fromClient != clientIdCheck) {
                Console.WriteLine($"Player \" {username}\" (ID:{fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[fromClient].SendIntoGame(username, color);
        }

        public static void AttackRequest(int fromClient, Packet packet) {
            int clientIdCheck = packet.ReadInt();
            Vector3 atkTown = packet.ReadVector3();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} requested an attack at town {deffTown}.");

            Server.clients[fromClient].AttackTown(atkTown, deffTown, timeStamp);
        }

        public static void RetreatRequest(int fromClient, Packet packet) {
            int clientIdCheck = packet.ReadInt();
            Vector3 atkTown = packet.ReadVector3();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} requested an retreat of troops from town {deffTown}.");

            Server.clients[fromClient].RetreatTown(atkTown, deffTown, timeStamp);
        }

        public static void ConquerRequest(int fromClient, Packet packet) {
            int clientId = packet.ReadInt();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} requested to conquer {deffTown}.");

            Server.clients[fromClient].ConquerTown(clientId, deffTown, timeStamp);
        }
    }
}
