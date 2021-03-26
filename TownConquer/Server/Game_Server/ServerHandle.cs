using SharedLibrary;
using System;
using System.Drawing;
using System.Numerics;

/// <summary>
/// BASIERT AUF DEM CODE VON TOM WEILAND SIEHE:
/// Weiland, Tom, 8 Dec 2019, https://github.com/tom-weiland/tcp-udp-networking/blob/tutorial-part5/GameServer/GameServer/ServerHandle.cs [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

namespace Game_Server {
    class ServerHandle {
        public static void WelcomeReceived(int fromClient, Packet packet) {
            int clientId = packet.ReadInt();
            string username = packet.ReadString();
            Color color = packet.ReadColor();

            Client client = Server.games[-1].clients[fromClient];
            Console.WriteLine($"{client.tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient} - {username}.");
            if (fromClient != clientId) {
                Console.WriteLine($"Player \" {username}\" (ID:{fromClient}) has assumed the wrong client ID ({clientId})!");
            }

            client.SetupUser(username, color, client.game.gm.sw.ElapsedMilliseconds);
            client.SendIntoGame();
        }

        public static void InteractionRequest(int fromClient, Packet packet) {
            int clientId = packet.ReadInt();
            Vector3 atkTown = packet.ReadVector3();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Client client = Server.games[-1].clients[fromClient];
            Console.WriteLine($"{client.tcp.socket.Client.RemoteEndPoint} requested an attack at town {deffTown}.");

            client.InteractWithTown(atkTown, deffTown);
        }

        public static void RetreatRequest(int fromClient, Packet packet) {
            int clientId = packet.ReadInt();
            Vector3 atkTown = packet.ReadVector3();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Client client = Server.games[-1].clients[fromClient];
            Console.WriteLine($"{client.tcp.socket.Client.RemoteEndPoint} requested an retreat of troops from town {deffTown}.");

            client.RetreatFromTown(atkTown, deffTown);
        }

        public static void ConquerRequest(int fromClient, Packet packet) {
            int clientId = packet.ReadInt();
            Vector3 deffTown = packet.ReadVector3();
            DateTime timeStamp = DateTime.FromBinary(packet.ReadLong());

            Client client = Server.games[-1].clients[fromClient];
            Console.WriteLine($"{client.tcp.socket.Client.RemoteEndPoint} requested to conquer {deffTown}.");

            client.ConquerTown(deffTown);
        }
    }
}
